using System.Text.Json;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using PromptHQ.Application.Interfaces;
using PromptHQ.Communication.Services;

namespace PromptHQ.Communication.Controllers;

[ApiController]
[Route("api/discord")]
public class DiscordInteractionsController(
    IServiceScopeFactory scopeFactory,
    DiscordRequestVerifier requestVerifier,
    ILogger<DiscordInteractionsController> logger)
    : ControllerBase
{
    [HttpPost("interactions")]
    public async Task<IActionResult> Post()
    {
        using var streamReader = new StreamReader(Request.Body);
        var bodyText = await streamReader.ReadToEndAsync();

        if (!requestVerifier.Verify(Request.Headers, bodyText))
        {
            return Unauthorized();
        }

        using var document = JsonDocument.Parse(bodyText);
        var root = document.RootElement;
        var type = root.GetProperty("type").GetInt32();

        if (type == 1)
        {
            return Ok(new { type = 1 });
        }

        if (type == 2)
        {
            var commandText = BuildCommandText(root);
            if (commandText is null)
            {
                return Ok(MessageResponse("Unsupported Discord command."));
            }

            QueueProcessing(root, commandText);
            return Ok(new { type = 5 });
        }

        if (type == 3)
        {
            var customId = root.GetProperty("data").GetProperty("custom_id").GetString() ?? string.Empty;
            QueueComponentProcessing(root, customId);
            return Ok(new { type = 6 });
        }

        return Ok(MessageResponse("Unsupported Discord interaction."));
    }

    private void QueueProcessing(JsonElement root, string commandText)
    {
        var applicationId = root.GetProperty("application_id").GetString() ?? string.Empty;
        var interactionToken = root.GetProperty("token").GetString() ?? string.Empty;
        var chatId = ExtractUserId(root);

        _ = Task.Run(async () =>
        {
            using var scope = scopeFactory.CreateScope();
            var processor = scope.ServiceProvider.GetRequiredService<IPromptProcessingService>();
            var discordClient = scope.ServiceProvider.GetRequiredService<IDiscordInteractionClientService>();
            discordClient.UseInteraction(applicationId, interactionToken);

            try
            {
                await processor.ProcessWebhookUpdateAsync(commandText, chatId, discordClient);
            }
            catch (Exception ex)
            {
                logger.LogError(ex, "Discord command processing failed for user {UserId}", chatId);
            }
        });
    }

    private void QueueComponentProcessing(JsonElement root, string customId)
    {
        var applicationId = root.GetProperty("application_id").GetString() ?? string.Empty;
        var interactionToken = root.GetProperty("token").GetString() ?? string.Empty;
        var interactionId = root.GetProperty("id").GetString() ?? string.Empty;
        var chatId = ExtractUserId(root);

        _ = Task.Run(async () =>
        {
            using var scope = scopeFactory.CreateScope();
            var processor = scope.ServiceProvider.GetRequiredService<IPromptProcessingService>();
            var discordClient = scope.ServiceProvider.GetRequiredService<IDiscordInteractionClientService>();
            discordClient.UseInteraction(applicationId, interactionToken);

            try
            {
                await processor.ProcessCallbackQueryAsync(chatId, interactionId, customId, discordClient);
            }
            catch (Exception ex)
            {
                logger.LogError(ex, "Discord component processing failed for user {UserId}", chatId);
            }
        });
    }

    private static string? BuildCommandText(JsonElement root)
    {
        var data = root.GetProperty("data");
        var commandName = data.GetProperty("name").GetString();

        if (commandName is "model" or "settings" or "start")
        {
            return "/" + commandName;
        }

        if (commandName is not ("improve" or "elite"))
        {
            return null;
        }

        var prompt = string.Empty;
        if (data.TryGetProperty("options", out var options))
        {
            foreach (var option in options.EnumerateArray())
            {
                if (option.GetProperty("name").GetString() == "prompt" &&
                    option.TryGetProperty("value", out var value))
                {
                    prompt = value.GetString() ?? string.Empty;
                    break;
                }
            }
        }

        return $"/{commandName} {prompt}".Trim();
    }

    private static long ExtractUserId(JsonElement root)
    {
        if (root.TryGetProperty("member", out var member) &&
            member.TryGetProperty("user", out var memberUser) &&
            TryReadSnowflake(memberUser, "id", out var memberUserId))
        {
            return memberUserId;
        }

        if (root.TryGetProperty("user", out var user) &&
            TryReadSnowflake(user, "id", out var userId))
        {
            return userId;
        }

        return TryReadSnowflake(root, "channel_id", out var channelId) ? channelId : 0;
    }

    private static bool TryReadSnowflake(JsonElement element, string propertyName, out long value)
    {
        value = 0;
        return element.TryGetProperty(propertyName, out var property) &&
            long.TryParse(property.GetString(), out value);
    }

    private static object MessageResponse(string content)
    {
        return new
        {
            type = 4,
            data = new
            {
                content,
                allowed_mentions = new { parse = Array.Empty<string>() }
            }
        };
    }
}
