namespace PromptHQ.AI.Configuration;

public class AIProviderSettings
{
    public string DisplayName { get; set; } = string.Empty;
    public string ApiKey { get; set; } = string.Empty;
    public string BaseUrl { get; set; } = string.Empty;
    public string ModelName { get; set; } = string.Empty;
    public int MaxTokens { get; set; } = 3000;
}

public class AIProvidersConfig
{
    public string DefaultProvider { get; set; } = string.Empty;
    public Dictionary<string, AIProviderSettings> Providers { get; set; } = new();
}
