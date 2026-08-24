using Microsoft.Extensions.AI;

namespace RegulationsSearcher.Ingestion.Tests.Orchestration;

internal sealed class ThrowingEmbeddingGenerator : IEmbeddingGenerator<string, Embedding<float>>
{
    public Task<GeneratedEmbeddings<Embedding<float>>> GenerateAsync(
        IEnumerable<string> values,
        EmbeddingGenerationOptions? options = null,
        CancellationToken cancellationToken = default) =>
        throw new InvalidOperationException("embedding service unavailable");

    public object? GetService(Type serviceType, object? serviceKey = null) => null;

    public void Dispose()
    {
    }
}
