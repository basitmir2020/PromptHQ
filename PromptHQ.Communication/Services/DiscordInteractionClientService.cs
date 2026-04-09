using System.Net.Http.Json;
using Microsoft.Extensions.Logging;

namespace PromptHQ.Communication.Services;

public class DiscordInteractionClientService(
    HttpClient httpClient,
    ILogger<DiscordInteractionClientService> logger)
    : IDiscordInteractionClientService
{
    private string _applicationId = string.Empty;
    private string _interactionToken = string.Empty;

    public void UseInteraction(string applicationId, string interactionToken)
    {
        _applicationId = applicationId;
        _interactionToken = interactionToken;
    }

    public async Task SendMessageAsync(long chatId, string text)
    {
        foreach (var chunk in SplitMessage(DiscordMessageFormatter.FromHtml(text)))
        {
            await SendFollowupAsync(new
            {
                content = chunk,
                allowed_mentions = new { parse = Array.Empty<string>() }
            });
        }
    }

    public async Task SendInlineKeyboardAsync(
        long chatId,
        string text,
        IEnumerable<IEnumerable<(string Text, string CallbackData)>> buttons)
    {
        var components = buttons
            .Take(5)
            .Select(row => new
            {
                type = 1,
                components = row.Take(5).Select(button => new
                {
                    type = 2,
                    style = 1,
                    label = button.Text,
                    custom_id = button.CallbackData
                }).ToArray()
            })
            .ToArray();

        await SendFollowupAsync(new
        {
            content = DiscordMessageFormatter.FromHtml(text),
            components,
            allowed_mentions = new { parse = Array.Empty<string>() }
        });
    }

    public Task AnswerCallbackQueryAsync(string callbackQueryId, string text)
    {
        return Task.CompletedTask;
    }

    private async Task SendFollowupAsync(object payload)
    {
        if (string.IsNullOrWhiteSpace(_applicationId) || string.IsNullOrWhiteSpace(_interactionToken))
        {
            logger.LogError("Discord interaction context was not initialized.");
            return;
        }

        var response = await httpClient.PostAsJsonAsync(
            $"https://discord.com/api/v10/webhooks/{_applicationId}/{_interactionToken}",
            payload);

        if (!response.IsSuccessStatusCode)
        {
            var body = await response.Content.ReadAsStringAsync();
            logger.LogError("Discord followup failed with {StatusCode}: {Body}", response.StatusCode, body);
        }
    }

    private static IEnumerable<string> SplitMessage(string message)
    {
        if (message.Length <= 1900)
        {
            yield return message;
            yield break;
        }

        for (var start = 0; start < message.Length; start += 1900)
        {
            yield return message.Substring(start, Math.Min(1900, message.Length - start));
        }
    }
}
