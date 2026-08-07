using Microsoft.ML.Tokenizers;

namespace RegulationsSearcher.Ingestion.Chunking;

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

    public IReadOnlyList<string> Chunk(string text)
    {
        var tokenIds = _tokenizer.EncodeToIds(text);
        if (tokenIds.Count == 0)
        {
            return [];
        }

        var chunks = new List<string>();
        var step = _chunkSizeInTokens - _overlapSizeInTokens;

        for (var start = 0; start < tokenIds.Count; start += step)
        {
            var length = Math.Min(_chunkSizeInTokens, tokenIds.Count - start);
            chunks.Add(_tokenizer.Decode(tokenIds.Skip(start).Take(length)));

            if (start + length >= tokenIds.Count)
            {
                break;
            }
        }

        return chunks;
    }
}
