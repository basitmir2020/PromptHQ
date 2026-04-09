using System.Text;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using NSec.Cryptography;
using PromptHQ.Communication.Configuration;

namespace PromptHQ.Communication.Services;

public class DiscordRequestVerifier(
    IOptions<DiscordSettings> options,
    ILogger<DiscordRequestVerifier> logger)
{
    private const string SignatureHeader = "X-Signature-Ed25519";
    private const string TimestampHeader = "X-Signature-Timestamp";

    public bool Verify(IHeaderDictionary headers, string body)
    {
        if (string.IsNullOrWhiteSpace(options.Value.PublicKey))
        {
            logger.LogWarning("Discord PublicKey is not configured.");
            return false;
        }

        if (!headers.TryGetValue(SignatureHeader, out var signatureHeader) ||
            !headers.TryGetValue(TimestampHeader, out var timestampHeader))
        {
            return false;
        }

        try
        {
            var publicKeyBytes = Convert.FromHexString(options.Value.PublicKey);
            var signatureBytes = Convert.FromHexString(signatureHeader.ToString());
            var messageBytes = Encoding.UTF8.GetBytes(timestampHeader.ToString() + body);
            var algorithm = SignatureAlgorithm.Ed25519;
            var publicKey = PublicKey.Import(algorithm, publicKeyBytes, KeyBlobFormat.RawPublicKey);

            return algorithm.Verify(publicKey, messageBytes, signatureBytes);
        }
        catch (Exception ex)
        {
            logger.LogWarning(ex, "Discord request signature verification failed.");
            return false;
        }
    }
}
