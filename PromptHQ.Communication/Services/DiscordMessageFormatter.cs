using System.Net;
using System.Text.RegularExpressions;

namespace PromptHQ.Communication.Services;

public static partial class DiscordMessageFormatter
{
    public static string FromHtml(string text)
    {
        if (string.IsNullOrWhiteSpace(text))
        {
            return string.Empty;
        }

        var formatted = PreRegex().Replace(text, match =>
        {
            var inner = WebUtility.HtmlDecode(match.Groups[1].Value).Trim();
            return $"```\n{inner}\n```";
        });

        formatted = BoldRegex().Replace(formatted, match => $"**{WebUtility.HtmlDecode(match.Groups[1].Value)}**");
        formatted = StripTagsRegex().Replace(formatted, string.Empty);

        return WebUtility.HtmlDecode(formatted).Trim();
    }

    [GeneratedRegex("<pre>(.*?)</pre>", RegexOptions.IgnoreCase | RegexOptions.Singleline)]
    private static partial Regex PreRegex();

    [GeneratedRegex("<b>(.*?)</b>", RegexOptions.IgnoreCase | RegexOptions.Singleline)]
    private static partial Regex BoldRegex();

    [GeneratedRegex("<.*?>", RegexOptions.Singleline)]
    private static partial Regex StripTagsRegex();
}
