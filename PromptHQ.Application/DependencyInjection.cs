using Microsoft.Extensions.DependencyInjection;
using PromptHQ.Application.Interfaces;
using PromptHQ.Application.Services;

namespace PromptHQ.Application;

public static class DependencyInjection
{
    public static IServiceCollection AddApplicationCore(this IServiceCollection services)
    {
        services.AddScoped<IPromptProcessingService, PromptProcessingService>();
        return services;
    }
}
