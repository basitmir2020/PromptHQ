using PromptHQ.Application.DTOs;
using PromptHQ.Domain.Enums;
using System.Threading.Tasks;

namespace PromptHQ.Application.Interfaces;

public interface IAIService
{
    Task<AiRefinementResponseDto> RefinePromptAsync(string rawPrompt, PromptQualityLevel qualityLevel, long chatId);
}
