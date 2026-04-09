using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using PromptHQ.AI.Configuration;
using PromptHQ.AI.Services;
using PromptHQ.Application.Interfaces;

namespace PromptHQ.AI;

public static class DependencyInjection
{
    public static IServiceCollection AddAILayer(this IServiceCollection services, IConfiguration configuration)
    {
        services.Configure<AIProvidersConfig>(configuration.GetSection("AIProvidersConfig"));

        // Register the HttpClient factory for AI provider calls
        services.AddHttpClient("AIProvider");

        // Register AI services
        services.AddScoped<IAIService, AIFactoryService>();
        services.AddSingleton<IAIProviderListService, AIProviderListService>();

        return services;
    }
}
