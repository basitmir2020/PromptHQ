namespace PromptHQ.Application.Interfaces;

public interface IChatClientService
{
    Task SendMessageAsync(long chatId, string text);
    Task SendInlineKeyboardAsync(long chatId, string text, IEnumerable<IEnumerable<(string Text, string CallbackData)>> buttons);
    Task AnswerCallbackQueryAsync(string callbackQueryId, string text);
}

