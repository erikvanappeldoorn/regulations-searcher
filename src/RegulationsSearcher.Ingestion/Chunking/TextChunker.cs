using Microsoft.ML.Tokenizers;
using RegulationsSearcher.Ingestion.Extraction;

namespace RegulationsSearcher.Ingestion.Chunking;

public sealed record TextChunk(string DocumentName, string Content, int PageStart, int PageEnd, int ChunkIndex);

public sealed class TextChunker
{
    private readonly Tokenizer _tokenizer;
    private readonly int _chunkSizeInTokens;
    private readonly int _overlapSizeInTokens;

    public TextChunker(int chunkSizeInTokens, int overlapSizeInTokens)
    {
        if (chunkSizeInTokens <= 0)
        {
            throw new ArgumentOutOfRangeException(nameof(chunkSizeInTokens), chunkSizeInTokens, "Chunk size must be greater than zero.");
        }

        if (overlapSizeInTokens < 0 || overlapSizeInTokens >= chunkSizeInTokens)
        {
            throw new ArgumentOutOfRangeException(nameof(overlapSizeInTokens), overlapSizeInTokens, "Overlap size must be non-negative and less than the chunk size.");
        }

        _tokenizer = TiktokenTokenizer.CreateForModel("gpt-4");
        _chunkSizeInTokens = chunkSizeInTokens;
        _overlapSizeInTokens = overlapSizeInTokens;
    }

    public IReadOnlyList<TextChunk> Chunk(string documentName, IReadOnlyList<PdfPageText> pages)
    {
        var tokenIds = new List<int>();
        var tokenPageNumbers = new List<int>();

        foreach (var page in pages)
        {
            var pageTokenIds = _tokenizer.EncodeToIds(page.Text);
            tokenIds.AddRange(pageTokenIds);
            tokenPageNumbers.AddRange(Enumerable.Repeat(page.PageNumber, pageTokenIds.Count));
        }

        if (tokenIds.Count == 0)
        {
            return [];
        }

        var chunks = new List<TextChunk>();
        var step = _chunkSizeInTokens - _overlapSizeInTokens;
        var chunkIndex = 0;

        for (var start = 0; start < tokenIds.Count; start += step)
        {
            var length = Math.Min(_chunkSizeInTokens, tokenIds.Count - start);
            var content = _tokenizer.Decode(tokenIds.Skip(start).Take(length));
            var pageStart = tokenPageNumbers[start];
            var pageEnd = tokenPageNumbers[start + length - 1];
            chunks.Add(new TextChunk(documentName, content, pageStart, pageEnd, chunkIndex));
            chunkIndex++;

            if (start + length >= tokenIds.Count)
            {
                break;
            }
        }

        return chunks;
    }
}
