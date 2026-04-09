using System.Collections.Generic;
using System.Linq;
using Microsoft.Extensions.Options;
using PromptHQ.AI.Configuration;
using PromptHQ.Application.Interfaces;

namespace PromptHQ.AI.Services;

public class AIProviderListService : IAIProviderListService
{
    private readonly AIProvidersConfig _config;

    public AIProviderListService(IOptions<AIProvidersConfig> config)
    {
        _config = config.Value;
    }

    public Dictionary<string, string> GetProviders()
    {
        return _config.Providers.ToDictionary(p => p.Key, p => p.Value.DisplayName);
    }

    public string GetDefaultProviderKey()
    {
        return _config.DefaultProvider;
    }
}
