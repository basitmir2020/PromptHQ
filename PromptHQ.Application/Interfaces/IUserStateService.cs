namespace PromptHQ.Application.Interfaces;

public interface IUserStateService
{
    string GetActiveProvider(long chatId);
    void SetActiveProvider(long chatId, string providerKey);
}
