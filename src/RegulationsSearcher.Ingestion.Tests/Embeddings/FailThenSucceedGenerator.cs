using Microsoft.Extensions.AI;

namespace RegulationsSearcher.Ingestion.Tests.Embeddings;

internal sealed class FailThenSucceedGenerator : IEmbeddingGenerator<string, Embedding<float>>
{
    private readonly Queue<Exception> _failures;

    public int CallCount { get; private set; }

    public FailThenSucceedGenerator(params Exception[] failures)
    {
        _failures = new Queue<Exception>(failures);
    }

    public Task<GeneratedEmbeddings<Embedding<float>>> GenerateAsync(
        IEnumerable<string> values,
        EmbeddingGenerationOptions? options = null,
        CancellationToken cancellationToken = default)
    {
        CallCount++;

        if (_failures.Count > 0)
        {
            throw _failures.Dequeue();
        }

        return Task.FromResult(new GeneratedEmbeddings<Embedding<float>>(
            values.Select(_ => new Embedding<float>(new ReadOnlyMemory<float>([1f])))));
    }

    public object? GetService(Type serviceType, object? serviceKey = null) => null;

    public void Dispose()
    {
    }
}
