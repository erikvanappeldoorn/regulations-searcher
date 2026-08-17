using Microsoft.Extensions.AI;

namespace RegulationsSearcher.Ingestion.Tests.Embeddings;

internal sealed class FakeEmbeddingGenerator : IEmbeddingGenerator<string, Embedding<float>>
{
    public List<string> ReceivedValues { get; } = [];
    public int CallCount { get; private set; }

    public Task<GeneratedEmbeddings<Embedding<float>>> GenerateAsync(
        IEnumerable<string> values,
        EmbeddingGenerationOptions? options = null,
        CancellationToken cancellationToken = default)
    {
        CallCount++;
        var inputs = values.ToList();
        ReceivedValues.AddRange(inputs);

        var embeddings = new GeneratedEmbeddings<Embedding<float>>(
            inputs.Select(value => new Embedding<float>(new ReadOnlyMemory<float>([value.Length]))));

        return Task.FromResult(embeddings);
    }

    public object? GetService(Type serviceType, object? serviceKey = null) => null;

    public void Dispose()
    {
    }
}
