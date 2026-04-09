using System.Collections.Generic;

namespace PromptHQ.Application.Interfaces;

/// <summary>
/// Provides a read-only view of configured AI providers for the Application layer
/// without coupling it to Infrastructure configuration classes.
/// </summary>
public interface IAIProviderListService
{
    /// <summary>Returns a dictionary of ProviderKey -> DisplayName.</summary>
    Dictionary<string, string> GetProviders();

    /// <summary>Returns the key of the default provider.</summary>
    string GetDefaultProviderKey();
}
