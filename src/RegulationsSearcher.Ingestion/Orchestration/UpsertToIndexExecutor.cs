using Microsoft.Agents.AI.Workflows;
using RegulationsSearcher.Ingestion.Embeddings;
using RegulationsSearcher.Ingestion.Indexing;

namespace RegulationsSearcher.Ingestion.Orchestration;

public sealed class UpsertToIndexExecutor : Executor<IReadOnlyList<EmbeddedChunk>>
{
    private readonly ChunkUploader _chunkUploader;

    public UpsertToIndexExecutor(ChunkUploader chunkUploader)
        : base(nameof(UpsertToIndexExecutor))
    {
        _chunkUploader = chunkUploader;
    }

    public override async ValueTask HandleAsync(IReadOnlyList<EmbeddedChunk> message, IWorkflowContext context, CancellationToken cancellationToken)
    {
        await _chunkUploader.UploadAsync(message, cancellationToken);
        await context.YieldOutputAsync(message, cancellationToken);
    }
}
