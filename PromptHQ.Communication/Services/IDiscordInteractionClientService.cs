using PromptHQ.Application.Interfaces;

namespace PromptHQ.Communication.Services;

public interface IDiscordInteractionClientService : IChatClientService
{
    void UseInteraction(string applicationId, string interactionToken);
}
