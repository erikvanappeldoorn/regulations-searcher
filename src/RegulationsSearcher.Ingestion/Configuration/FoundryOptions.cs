namespace RegulationsSearcher.Ingestion.Configuration;

public sealed class FoundryOptions
{
    public const string SectionName = "Foundry";

    public required string Endpoint { get; init; }
    public required string EmbeddingDeploymentName { get; init; }
    public required int EmbeddingDimension { get; init; }

    /// <summary>Optional API-key fallback, expected to come from user-secrets. When null, DefaultAzureCredential is used.</summary>
    public string? ApiKey { get; init; }
}
