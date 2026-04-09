using System.Collections.Concurrent;
using PromptHQ.Application.Interfaces;

namespace PromptHQ.Infrastructure.Services;

public class UserStateService : IUserStateService
{
    private readonly ConcurrentDictionary<long, string> _userProviderMap = new();

    public string GetActiveProvider(long chatId)
    {
        return _userProviderMap.TryGetValue(chatId, out var provider) ? provider : string.Empty;
    }

    public void SetActiveProvider(long chatId, string providerKey)
    {
        _userProviderMap.AddOrUpdate(chatId, providerKey, (_, _) => providerKey);
    }
}
