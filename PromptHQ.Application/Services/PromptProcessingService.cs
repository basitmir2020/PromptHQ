using Microsoft.Extensions.Logging;
using PromptHQ.Application.Interfaces;
using PromptHQ.Domain.Constants;
using PromptHQ.Domain.Enums;
using System.Net;
using System.Text;

namespace PromptHQ.Application.Services;

public class PromptProcessingService(
    IAIService aiService,
    IChatClientService defaultChatClientService,
    IUserStateService userStateService,
    IAIProviderListService providerListService,
    ILogger<PromptProcessingService> logger)
    : IPromptProcessingService
{
    // We inject IConfiguration indirectly via a simple provider list interface

    public async Task ProcessWebhookUpdateAsync(string rawMessageText, long chatId, IChatClientService? chatClientService = null)
    {
        if (string.IsNullOrWhiteSpace(rawMessageText))
            return;

        var chatClient = chatClientService ?? defaultChatClientService;

        logger.LogInformation("Processing message for ChatId {ChatId}", chatId);

        var (command, promptText) = ParseCommand(rawMessageText);

        switch (command)
        {
            case CommandType.Start:
                await chatClient.SendMessageAsync(chatId, BotMessages.WelcomeMessage);
                break;

            case CommandType.Model:
                await HandleModelSelectionMenu(chatId, chatClient);
                break;

            case CommandType.Improve:
            case CommandType.Elite:
                if (string.IsNullOrWhiteSpace(promptText))
                {
                    await chatClient.SendMessageAsync(chatId, BotMessages.EmptyPromptMessage);
                    return;
                }

                // Show which model is active
                var activeProvider = userStateService.GetActiveProvider(chatId);
                var providers = providerListService.GetProviders();
                var defaultKey = providerListService.GetDefaultProviderKey();
                var currentKey = string.IsNullOrEmpty(activeProvider) ? defaultKey : activeProvider;
                var currentDisplay = providers.ContainsKey(currentKey) ? providers[currentKey] : currentKey;

                await chatClient.SendMessageAsync(chatId, $"Processing with <b>{WebUtility.HtmlEncode(currentDisplay)}</b>...");

                var quality = command == CommandType.Elite ? PromptQualityLevel.Elite : PromptQualityLevel.Advanced;

                var aiResult = await aiService.RefinePromptAsync(promptText, quality, chatId);

                if (aiResult.IsSuccess)
                {
                    await chatClient.SendMessageAsync(chatId, BuildRefinementReply(aiResult));
                }
                else
                {
                    logger.LogError("AI Refinement failed: {Error}", aiResult.ErrorMessage);
                    await chatClient.SendMessageAsync(chatId, BotMessages.ErrorMessage);
                }
                break;

            default:
                await chatClient.SendMessageAsync(chatId, BotMessages.InvalidCommandMessage);
                break;
        }
    }

    public async Task ProcessCallbackQueryAsync(long chatId, string callbackQueryId, string callbackData, IChatClientService? chatClientService = null)
    {
        var chatClient = chatClientService ?? defaultChatClientService;

        if (!callbackData.StartsWith("model:"))
        {
            await chatClient.AnswerCallbackQueryAsync(callbackQueryId, "Unknown action.");
            return;
        }

        var providerKey = callbackData.Substring("model:".Length);
        var providers = providerListService.GetProviders();

        if (!providers.ContainsKey(providerKey))
        {
            await chatClient.AnswerCallbackQueryAsync(callbackQueryId, "Provider not found.");
            return;
        }

        userStateService.SetActiveProvider(chatId, providerKey);
        var displayName = providers[providerKey];

        logger.LogInformation("User {ChatId} switched AI model to {ProviderKey} ({DisplayName})", chatId, providerKey, displayName);

        await chatClient.AnswerCallbackQueryAsync(callbackQueryId, $"Switched to {displayName}!");
        await chatClient.SendMessageAsync(chatId, $"AI model changed to <b>{WebUtility.HtmlEncode(displayName)}</b>.\n\nAll your future prompts will now be processed by this model.");
    }

    private async Task HandleModelSelectionMenu(long chatId, IChatClientService chatClientService)
    {
        var providers = providerListService.GetProviders();
        var activeProvider = userStateService.GetActiveProvider(chatId);
        var defaultKey = providerListService.GetDefaultProviderKey();
        var currentKey = string.IsNullOrEmpty(activeProvider) ? defaultKey : activeProvider;

        var buttons = providers.Select(p =>
        {
            var label = p.Key == currentKey ? $"Selected: {p.Value}" : p.Value;
            return new List<(string Text, string CallbackData)>
            {
                (label, $"model:{p.Key}")
            };
        }).ToList();

        await chatClientService.SendInlineKeyboardAsync(
            chatId,
            "<b>Select AI Model</b>\n\nChoose which AI model to use for prompt generation:",
            buttons
        );
    }

    private static string BuildRefinementReply(PromptHQ.Application.DTOs.AiRefinementResponseDto aiResult)
    {
        var reply = new StringBuilder();

        reply.AppendLine("<b>Refined Prompt</b>");
        reply.AppendLine();
        reply.AppendLine($"<pre>{WebUtility.HtmlEncode(aiResult.RefinedPrompt)}</pre>");
        reply.AppendLine();
        reply.AppendLine("<b>Input Quality</b>");
        reply.AppendLine(FormatMultilineHtml(aiResult.InputQualityAssessment));
        reply.AppendLine();
        reply.AppendLine("<b>Improvements</b>");
        reply.Append(FormatMultilineHtml(aiResult.Feedback));

        return reply.ToString();
    }

    private static string FormatMultilineHtml(string value)
    {
        if (string.IsNullOrWhiteSpace(value))
        {
            return "Not provided.";
        }

        var lines = value
            .Split('\n', StringSplitOptions.RemoveEmptyEntries | StringSplitOptions.TrimEntries)
            .Select(line => WebUtility.HtmlEncode(NormalizeBullet(line)));

        return string.Join('\n', lines);
    }

    private static string NormalizeBullet(string line)
    {
        var trimmed = line.Trim();
        if (trimmed.StartsWith("- ") || trimmed.StartsWith("* "))
        {
            return "- " + trimmed[2..].Trim();
        }

        return trimmed;
    }

    private (CommandType, string) ParseCommand(string text)
    {
        var span = text.Trim();
        if (span.StartsWith("/start"))
            return (CommandType.Start, string.Empty);

        if (span.StartsWith("/model") || span.StartsWith("/settings"))
            return (CommandType.Model, string.Empty);

        if (span.StartsWith("/improve"))
        {
            var prompt = span.Length > 8 ? span.Substring(8).Trim() : string.Empty;
            return (CommandType.Improve, prompt);
        }

        if (span.StartsWith("/elite"))
        {
            var prompt = span.Length > 6 ? span.Substring(6).Trim() : string.Empty;
            return (CommandType.Elite, prompt);
        }

        return (CommandType.Unknown, string.Empty);
    }
}
