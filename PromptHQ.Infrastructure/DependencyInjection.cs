using Microsoft.Extensions.DependencyInjection;
using PromptHQ.Application.Interfaces;
using PromptHQ.Infrastructure.Services;

namespace PromptHQ.Infrastructure;

public static class DependencyInjection
{
    public static IServiceCollection AddInfrastructure(this IServiceCollection services)
    {
        // User state is in-memory (Singleton so it persists across requests)
        services.AddSingleton<IUserStateService, UserStateService>();

        return services;
    }
}
