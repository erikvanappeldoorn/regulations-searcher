using RegulationsSearcher.Ingestion.Chunking;
using RegulationsSearcher.Ingestion.Extraction;

namespace RegulationsSearcher.Ingestion.Tests.Chunking;

public class TextChunkerTests
{
    private static string BuildWords(int start, int count) =>
        string.Join(" ", Enumerable.Range(start, count).Select(i => $"w{i}"));

    [Fact]
    public void Chunk_TextShorterThanOneChunk_ReturnsSingleChunkCoveringAllText()
    {
        var chunker = new TextChunker(chunkSizeInTokens: 20, overlapSizeInTokens: 5);
        var pages = new[] { new PdfPageText(1, BuildWords(0, 3)) };

        var chunks = chunker.Chunk("doc.pdf", pages);

        var chunk = Assert.Single(chunks);
        Assert.Equal("doc.pdf", chunk.DocumentName);
        Assert.Equal(0, chunk.ChunkIndex);
        Assert.Equal(1, chunk.PageStart);
        Assert.Equal(1, chunk.PageEnd);
        Assert.Equal(["w0", "w1", "w2"], chunk.Content.Split(' ', StringSplitOptions.RemoveEmptyEntries));
    }

    [Fact]
    public void Chunk_ExactMultipleOfChunkSize_ProducesEvenChunksWithNoOverlapOrGap()
    {
        var chunker = new TextChunker(chunkSizeInTokens: 10, overlapSizeInTokens: 0);
        var pages = new[] { new PdfPageText(1, BuildWords(0, 20)) };

        var chunks = chunker.Chunk("doc.pdf", pages);

        Assert.Equal(4, chunks.Count);
        Assert.Equal([0, 1, 2, 3], chunks.Select(c => c.ChunkIndex));

        var coveredWords = chunks
            .SelectMany(c => c.Content.Split(' ', StringSplitOptions.RemoveEmptyEntries))
            .ToList();
        Assert.Equal(Enumerable.Range(0, 20).Select(i => $"w{i}"), coveredWords);
    }

    [Fact]
    public void Chunk_WithOverlap_RepeatsOverlapTokensBetweenConsecutiveChunks()
    {
        var chunker = new TextChunker(chunkSizeInTokens: 10, overlapSizeInTokens: 4);
        var pages = new[] { new PdfPageText(1, BuildWords(0, 8)) };

        var chunks = chunker.Chunk("doc.pdf", pages);

        Assert.Equal(2, chunks.Count);
        var firstWords = chunks[0].Content.Split(' ', StringSplitOptions.RemoveEmptyEntries);
        var secondWords = chunks[1].Content.Split(' ', StringSplitOptions.RemoveEmptyEntries);

        Assert.Equal(["w0", "w1", "w2", "w3", "w4"], firstWords);
        Assert.Equal(["w3", "w4", "w5", "w6", "w7"], secondWords);
    }

    [Fact]
    public void Chunk_ChunkSpanningPageBoundary_RecordsFullPageRange()
    {
        var chunker = new TextChunker(chunkSizeInTokens: 14, overlapSizeInTokens: 0);
        var pages = new[]
        {
            new PdfPageText(1, BuildWords(0, 5)),
            new PdfPageText(2, BuildWords(5, 5)),
        };

        var chunks = chunker.Chunk("doc.pdf", pages);

        Assert.Equal(2, chunks.Count);
        Assert.Equal(1, chunks[0].PageStart);
        Assert.Equal(2, chunks[0].PageEnd);
        Assert.Equal(2, chunks[1].PageStart);
        Assert.Equal(2, chunks[1].PageEnd);
    }

    [Fact]
    public void Chunk_NoPages_ReturnsEmptyList()
    {
        var chunker = new TextChunker(chunkSizeInTokens: 10, overlapSizeInTokens: 2);

        var chunks = chunker.Chunk("doc.pdf", []);

        Assert.Empty(chunks);
    }

    [Fact]
    public void Chunk_OnlyEmptyPageText_ReturnsEmptyList()
    {
        var chunker = new TextChunker(chunkSizeInTokens: 10, overlapSizeInTokens: 2);
        var pages = new[] { new PdfPageText(1, string.Empty) };

        var chunks = chunker.Chunk("doc.pdf", pages);

        Assert.Empty(chunks);
    }

    [Theory]
    [InlineData(0)]
    [InlineData(-1)]
    public void Constructor_NonPositiveChunkSize_Throws(int chunkSizeInTokens)
    {
        Assert.Throws<ArgumentOutOfRangeException>(() => new TextChunker(chunkSizeInTokens, overlapSizeInTokens: 0));
    }

    [Theory]
    [InlineData(-1)]
    [InlineData(10)]
    public void Constructor_OverlapOutOfRange_Throws(int overlapSizeInTokens)
    {
        Assert.Throws<ArgumentOutOfRangeException>(() => new TextChunker(chunkSizeInTokens: 10, overlapSizeInTokens));
    }
}
