namespace PromptHQ.Communication.Configuration;

public class TelegramSettings
{
    public string BotToken { get; set; } = string.Empty;
    public string SecretToken { get; set; } = string.Empty;
    public string WebhookUrl { get; set; } = string.Empty;
}

