using Microsoft.Agents.AI.Workflows;
using RegulationsSearcher.Ingestion.Embeddings;
using RegulationsSearcher.Ingestion.Indexing;

namespace RegulationsSearcher.Ingestion.Orchestration;

public sealed class UpsertToIndexExecutor : Executor<IReadOnlyList<EmbeddedChunk>>
{
    private readonly ChunkUploader _chunkUploader;
    private readonly IPipelineLogger _logger;

    public UpsertToIndexExecutor(ChunkUploader chunkUploader, IPipelineLogger logger)
        : base(nameof(UpsertToIndexExecutor))
    {
        _chunkUploader = chunkUploader;
        _logger = logger;
    }

    public override async ValueTask HandleAsync(IReadOnlyList<EmbeddedChunk> message, IWorkflowContext context, CancellationToken cancellationToken)
    {
        try
        {
            await _chunkUploader.UploadAsync(message, cancellationToken);
            await context.YieldOutputAsync(message, cancellationToken);
            _logger.LogStepSucceeded(Id);
        }
        catch (Exception exception)
        {
            _logger.LogStepFailed(Id, exception);
            throw;
        }
    }
}
