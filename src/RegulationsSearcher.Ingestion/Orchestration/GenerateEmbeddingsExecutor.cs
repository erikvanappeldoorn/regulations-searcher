using Microsoft.Agents.AI.Workflows;
using RegulationsSearcher.Ingestion.Chunking;
using RegulationsSearcher.Ingestion.Embeddings;

namespace RegulationsSearcher.Ingestion.Orchestration;

public sealed class GenerateEmbeddingsExecutor : Executor<IReadOnlyList<TextChunk>, IReadOnlyList<EmbeddedChunk>>
{
    private readonly ChunkEmbedder _chunkEmbedder;

    public GenerateEmbeddingsExecutor(ChunkEmbedder chunkEmbedder)
        : base(nameof(GenerateEmbeddingsExecutor))
    {
        _chunkEmbedder = chunkEmbedder;
    }

    public override async ValueTask<IReadOnlyList<EmbeddedChunk>> HandleAsync(IReadOnlyList<TextChunk> message, IWorkflowContext context, CancellationToken cancellationToken) =>
        await _chunkEmbedder.EmbedAsync(message, cancellationToken);
}
