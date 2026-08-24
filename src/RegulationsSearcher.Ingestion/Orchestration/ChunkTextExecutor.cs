using Microsoft.Agents.AI.Workflows;
using RegulationsSearcher.Ingestion.Chunking;

namespace RegulationsSearcher.Ingestion.Orchestration;

public sealed class ChunkTextExecutor : Executor<ExtractedDocument, IReadOnlyList<TextChunk>>
{
    private readonly TextChunker _textChunker;
    private readonly IPipelineLogger _logger;

    public ChunkTextExecutor(TextChunker textChunker, IPipelineLogger logger)
        : base(nameof(ChunkTextExecutor))
    {
        _textChunker = textChunker;
        _logger = logger;
    }

    public override ValueTask<IReadOnlyList<TextChunk>> HandleAsync(ExtractedDocument message, IWorkflowContext context, CancellationToken cancellationToken)
    {
        try
        {
            var chunks = _textChunker.Chunk(message.DocumentName, message.Pages);
            _logger.LogStepSucceeded(message.DocumentName, Id);
            return new ValueTask<IReadOnlyList<TextChunk>>(chunks);
        }
        catch (Exception exception)
        {
            _logger.LogStepFailed(message.DocumentName, Id, exception);
            throw;
        }
    }
}
