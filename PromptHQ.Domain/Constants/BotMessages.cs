namespace PromptHQ.Domain.Constants;

public static class BotMessages
{
    public const string WelcomeMessage =
        "<b>Welcome to PromptHQ!</b>\n\n" +
        "I am an expert AI prompt engineer. Send me your rough prompt and I will transform it into an expert-level prompt.\n\n" +
        "<b>Commands:</b>\n" +
        "- /improve &lt;your prompt&gt; - Get an advanced, highly effective prompt.\n" +
        "- /elite &lt;your prompt&gt; - Get a masterful prompt designed for complex reasoning tasks.\n" +
        "- /model - Choose which AI model to use.";

    public const string ProcessingMessage = "Processing your prompt. Please wait...";

    public const string ErrorMessage = "Sorry, I encountered an error while processing your request. Please try again later.";

    public const string InvalidCommandMessage = "Please use a valid command like /improve or /elite followed by your prompt.";

    public const string EmptyPromptMessage = "Please provide a prompt after the command. E.g., /improve write me an essay.";
}
