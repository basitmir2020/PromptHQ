using Microsoft.Extensions.Logging;
using PromptHQ.Application.Interfaces;
using Telegram.Bot;
using Telegram.Bot.Types.Enums;
using Telegram.Bot.Types.ReplyMarkups;

namespace PromptHQ.Communication.Services;

public class TelegramBotClientService : IChatClientService
{
    private readonly ITelegramBotClient _botClient;
    private readonly ILogger<TelegramBotClientService> _logger;

    public TelegramBotClientService(
        ITelegramBotClient botClient,
        ILogger<TelegramBotClientService> logger)
    {
        _botClient = botClient;
        _logger = logger;
    }

    public async Task SendMessageAsync(long chatId, string text)
    {
        try
        {
            await _botClient.SendMessage(
                chatId: chatId,
                text: text,
                parseMode: ParseMode.Html
            );
        }
        catch (Telegram.Bot.Exceptions.ApiRequestException ex) when (ex.ErrorCode == 400)
        {
            _logger.LogWarning(ex, "HTML parsing failed. Falling back to plain text for ChatId {ChatId}", chatId);
            try
            {
                await _botClient.SendMessage(chatId: chatId, text: text, parseMode: ParseMode.None);
            }
            catch (Exception innerEx)
            {
                _logger.LogError(innerEx, "Failed to send plain text fallback message to {ChatId}", chatId);
            }
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Failed to send message to {ChatId}", chatId);
        }
    }

    public async Task SendInlineKeyboardAsync(
        long chatId,
        string text,
        IEnumerable<IEnumerable<(string Text, string CallbackData)>> buttons)
    {
        try
        {
            var inlineKeyboard = new InlineKeyboardMarkup(
                buttons.Select(row =>
                    row.Select(btn => InlineKeyboardButton.WithCallbackData(btn.Text, btn.CallbackData))
                )
            );

            await _botClient.SendMessage(
                chatId: chatId,
                text: text,
                parseMode: ParseMode.Html,
                replyMarkup: inlineKeyboard
            );
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Failed to send inline keyboard to {ChatId}", chatId);
        }
    }

    public async Task AnswerCallbackQueryAsync(string callbackQueryId, string text)
    {
        try
        {
            await _botClient.AnswerCallbackQuery(callbackQueryId, text);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Failed to answer callback query {QueryId}", callbackQueryId);
        }
    }
}

