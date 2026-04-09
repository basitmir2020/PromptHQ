using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using PromptHQ.Application.Interfaces;
using PromptHQ.Communication.Configuration;
using PromptHQ.Communication.Controllers;
using PromptHQ.Communication.Services;
using Telegram.Bot;

namespace PromptHQ.Communication;

public static class DependencyInjection
{
    public static IServiceCollection AddCommunication(this IServiceCollection services, IConfiguration configuration)
    {
        services.AddControllers()
            .AddApplicationPart(typeof(TelegramWebhookController).Assembly);

        services.Configure<TelegramSettings>(configuration.GetSection("TelegramSettings"));
        services.Configure<DiscordSettings>(configuration.GetSection("DiscordSettings"));

        services.AddHttpClient("TelegramWebhook")
            .AddTypedClient<ITelegramBotClient>((httpClient, sp) =>
            {
                var options = sp.GetRequiredService<Microsoft.Extensions.Options.IOptionsMonitor<TelegramSettings>>().CurrentValue;
                var currentToken = options.BotToken?.Trim();
                if (string.IsNullOrWhiteSpace(currentToken))
                {
                    throw new ArgumentNullException("Telegram BotToken not configured");
                }

                return new TelegramBotClient(currentToken, httpClient);
            });

        services.AddScoped<IChatClientService, TelegramBotClientService>();
        services.AddScoped<TelegramBotClientService>();
        services.AddScoped<DiscordRequestVerifier>();
        services.AddHttpClient<IDiscordInteractionClientService, DiscordInteractionClientService>();

        return services;
    }
}
