namespace RegulationsSearcher.Ingestion.Configuration;

public sealed class AzureSearchOptions
{
    public const string SectionName = "AzureSearch";

    public required string Endpoint { get; init; }
    public required string IndexName { get; init; }

    /// <summary>Optional API-key fallback, expected to come from user-secrets. When null, DefaultAzureCredential is used.</summary>
    public string? ApiKey { get; init; }
}
