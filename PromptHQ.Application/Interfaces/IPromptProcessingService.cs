using System.Threading.Tasks;

namespace PromptHQ.Application.Interfaces;

public interface IPromptProcessingService
{
    Task ProcessWebhookUpdateAsync(string rawMessageText, long chatId, IChatClientService? chatClientService = null);
    Task ProcessCallbackQueryAsync(long chatId, string callbackQueryId, string callbackData, IChatClientService? chatClientService = null);
}
