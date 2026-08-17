using Microsoft.Agents.AI.Workflows;
using RegulationsSearcher.Ingestion.Chunking;

namespace RegulationsSearcher.Ingestion.Orchestration;

public sealed class ChunkTextExecutor : Executor<ExtractedDocument, IReadOnlyList<TextChunk>>
{
    private readonly TextChunker _textChunker;

    public ChunkTextExecutor(TextChunker textChunker)
        : base(nameof(ChunkTextExecutor))
    {
        _textChunker = textChunker;
    }

    public override ValueTask<IReadOnlyList<TextChunk>> HandleAsync(ExtractedDocument message, IWorkflowContext context, CancellationToken cancellationToken) =>
        new(_textChunker.Chunk(message.DocumentName, message.Pages));
}
