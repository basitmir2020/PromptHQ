using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using PromptHQ.Application.Interfaces;
using PromptHQ.Communication.Configuration;
using Telegram.Bot.Types;
using Telegram.Bot.Types.Enums;

namespace PromptHQ.Communication.Controllers;

[ApiController]
[Route("api/telegram")]
public class TelegramWebhookController(
    IPromptProcessingService promptProcessingService,
    IOptions<TelegramSettings> telegramOptions,
    ILogger<TelegramWebhookController> logger)
    : ControllerBase
{
    private readonly TelegramSettings _telegramSettings = telegramOptions.Value;

    [HttpPost("webhook")]
    public async Task<IActionResult> Post()
    {
        using var streamReader = new StreamReader(Request.Body);
        var bodyText = await streamReader.ReadToEndAsync();

        Update update;
        try
        {
            var options = new System.Text.Json.JsonSerializerOptions
            {
                PropertyNamingPolicy = System.Text.Json.JsonNamingPolicy.SnakeCaseLower
            };
            update = System.Text.Json.JsonSerializer.Deserialize<Update>(bodyText, options) ?? throw new Exception();
        }
        catch (Exception ex)
        {
            logger.LogWarning(ex, "Failed to deserialize Telegram update");
            return BadRequest();
        }

        if (!string.IsNullOrEmpty(_telegramSettings.SecretToken))
        {
            if (!Request.Headers.TryGetValue("X-Telegram-Bot-Api-Secret-Token", out var secretTokenHeader) ||
                secretTokenHeader != _telegramSettings.SecretToken)
            {
                logger.LogWarning("Unauthorized webhook request. Mismatched or missing secret token.");
                return Unauthorized();
            }
        }

        if (update.Type == UpdateType.Message && update.Message?.Text != null)
        {
            _ = Task.Run(async () =>
            {
                try
                {
                    await promptProcessingService.ProcessWebhookUpdateAsync(update.Message.Text, update.Message.Chat.Id);
                }
                catch (Exception ex)
                {
                    logger.LogError(ex, "Background processing failed for chat {ChatId}", update.Message.Chat.Id);
                }
            });
        }
        else if (update.Type == UpdateType.CallbackQuery && update.CallbackQuery != null)
        {
            var callbackQuery = update.CallbackQuery;
            var chatId = callbackQuery.Message?.Chat?.Id ?? 0;
            var callbackQueryId = callbackQuery.Id;
            var callbackData = callbackQuery.Data ?? string.Empty;

            if (chatId > 0)
            {
                _ = Task.Run(async () =>
                {
                    try
                    {
                        await promptProcessingService.ProcessCallbackQueryAsync(chatId, callbackQueryId, callbackData);
                    }
                    catch (Exception ex)
                    {
                        logger.LogError(ex, "Callback query processing failed for chat {ChatId}", chatId);
                    }
                });
            }
        }

        return Ok();
    }
}

