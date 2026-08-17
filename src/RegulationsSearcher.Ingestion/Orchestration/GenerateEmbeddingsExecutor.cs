using Microsoft.Agents.AI.Workflows;
using RegulationsSearcher.Ingestion.Chunking;
using RegulationsSearcher.Ingestion.Embeddings;

namespace RegulationsSearcher.Ingestion.Orchestration;

public sealed class GenerateEmbeddingsExecutor : Executor<IReadOnlyList<TextChunk>, IReadOnlyList<EmbeddedChunk>>
{
    private readonly ChunkEmbedder _chunkEmbedder;
    private readonly IPipelineLogger _logger;

    public GenerateEmbeddingsExecutor(ChunkEmbedder chunkEmbedder, IPipelineLogger logger)
        : base(nameof(GenerateEmbeddingsExecutor))
    {
        _chunkEmbedder = chunkEmbedder;
        _logger = logger;
    }

    public override async ValueTask<IReadOnlyList<EmbeddedChunk>> HandleAsync(IReadOnlyList<TextChunk> message, IWorkflowContext context, CancellationToken cancellationToken)
    {
        try
        {
            var embeddedChunks = await _chunkEmbedder.EmbedAsync(message, cancellationToken);
            _logger.LogStepSucceeded(Id);
            return embeddedChunks;
        }
        catch (Exception exception)
        {
            _logger.LogStepFailed(Id, exception);
            throw;
        }
    }
}
