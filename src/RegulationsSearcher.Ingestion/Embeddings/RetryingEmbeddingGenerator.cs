using System.ClientModel;
using Microsoft.Extensions.AI;

namespace RegulationsSearcher.Ingestion.Embeddings;

public sealed class RetryingEmbeddingGenerator : DelegatingEmbeddingGenerator<string, Embedding<float>>
{
    private readonly int _maxAttempts;
    private readonly TimeSpan _initialDelay;

    public RetryingEmbeddingGenerator(
        IEmbeddingGenerator<string, Embedding<float>> innerGenerator,
        int maxAttempts = 3,
        TimeSpan? initialDelay = null)
        : base(innerGenerator)
    {
        if (maxAttempts <= 0)
        {
            throw new ArgumentOutOfRangeException(nameof(maxAttempts), maxAttempts, "Max attempts must be greater than zero.");
        }

        _maxAttempts = maxAttempts;
        _initialDelay = initialDelay ?? TimeSpan.FromSeconds(1);
    }

    public override async Task<GeneratedEmbeddings<Embedding<float>>> GenerateAsync(
        IEnumerable<string> values,
        EmbeddingGenerationOptions? options = null,
        CancellationToken cancellationToken = default)
    {
        var inputValues = values as IReadOnlyCollection<string> ?? values.ToList();
        var delay = _initialDelay;

        for (var attempt = 1; ; attempt++)
        {
            try
            {
                return await base.GenerateAsync(inputValues, options, cancellationToken);
            }
            catch (Exception exception) when (attempt < _maxAttempts && IsTransient(exception))
            {
                await Task.Delay(delay, cancellationToken);
                delay += delay;
            }
        }
    }

    private static bool IsTransient(Exception exception) => exception switch
    {
        ClientResultException clientResultException => clientResultException.Status == 429 || clientResultException.Status >= 500,
        TimeoutException => true,
        HttpRequestException => true,
        _ => false,
    };
}
