namespace PromptHQ.Application.DTOs;

public class AiRefinementResponseDto
{
    public string RefinedPrompt { get; set; } = string.Empty;
    public string InputQualityAssessment { get; set; } = string.Empty;
    public string Feedback { get; set; } = string.Empty;
    public bool IsSuccess { get; set; }
    public string ErrorMessage { get; set; } = string.Empty;
}
