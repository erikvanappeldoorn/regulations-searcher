using System.ClientModel;
using Azure;
using Azure.AI.OpenAI;
using Azure.Identity;
using Azure.Search.Documents;
using Azure.Search.Documents.Indexes;
using Microsoft.Extensions.AI;
using RegulationsSearcher.Ingestion.Configuration;
using RegulationsSearcher.Ingestion.Embeddings;

namespace RegulationsSearcher.Ingestion.Clients;

public static class AzureClientFactory
{
    /// <summary>Creates a Search index client using DefaultAzureCredential, falling back to an API key from user-secrets when configured.</summary>
    public static SearchIndexClient CreateSearchIndexClient(AzureSearchOptions options)
    {
        var endpoint = new Uri(options.Endpoint);
        return string.IsNullOrEmpty(options.ApiKey)
            ? new SearchIndexClient(endpoint, new DefaultAzureCredential())
            : new SearchIndexClient(endpoint, new AzureKeyCredential(options.ApiKey));
    }

    public static SearchClient CreateSearchClient(AzureSearchOptions options)
    {
        var endpoint = new Uri(options.Endpoint);
        return string.IsNullOrEmpty(options.ApiKey)
            ? new SearchClient(endpoint, options.IndexName, new DefaultAzureCredential())
            : new SearchClient(endpoint, options.IndexName, new AzureKeyCredential(options.ApiKey));
    }

    /// <summary>Creates an Azure AI Foundry (Azure OpenAI) client using DefaultAzureCredential, falling back to an API key from user-secrets when configured.</summary>
    public static AzureOpenAIClient CreateFoundryClient(FoundryOptions options)
    {
        var endpoint = new Uri(options.Endpoint);
        return string.IsNullOrEmpty(options.ApiKey)
            ? new AzureOpenAIClient(endpoint, new DefaultAzureCredential())
            : new AzureOpenAIClient(endpoint, new ApiKeyCredential(options.ApiKey));
    }

    public static IEmbeddingGenerator<string, Embedding<float>> CreateEmbeddingGenerator(AzureOpenAIClient foundryClient, FoundryOptions options)
    {
        var embeddingGenerator = foundryClient.GetEmbeddingClient(options.EmbeddingDeploymentName).AsIEmbeddingGenerator();
        return new RetryingEmbeddingGenerator(embeddingGenerator);
    }
}
