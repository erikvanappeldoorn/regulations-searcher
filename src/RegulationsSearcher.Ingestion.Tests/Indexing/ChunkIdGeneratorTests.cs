using RegulationsSearcher.Ingestion.Indexing;

namespace RegulationsSearcher.Ingestion.Tests.Indexing;

public class ChunkIdGeneratorTests
{
    [Fact]
    public void GenerateId_ReturnsDocumentNameAndChunkIndexJoinedByDash()
    {
        var id = ChunkIdGenerator.GenerateId("regulation-2024", 3);

        Assert.Equal("regulation-2024-3", id);
    }

    [Fact]
    public void GenerateId_IsDeterministicForSameInputs()
    {
        var first = ChunkIdGenerator.GenerateId("doc.pdf", 5);
        var second = ChunkIdGenerator.GenerateId("doc.pdf", 5);

        Assert.Equal(first, second);
    }

    [Fact]
    public void GenerateId_DifferentChunkIndex_ProducesDifferentId()
    {
        var first = ChunkIdGenerator.GenerateId("doc.pdf", 0);
        var second = ChunkIdGenerator.GenerateId("doc.pdf", 1);

        Assert.NotEqual(first, second);
    }

    [Fact]
    public void GenerateId_DocumentNameWithInvalidKeyCharacters_ReplacesThemWithUnderscore()
    {
        var id = ChunkIdGenerator.GenerateId("doc.pdf", 0);

        Assert.Equal("doc_pdf-0", id);
    }

    [Fact]
    public void GenerateId_DocumentNameWithSpacesAndParentheses_ReplacesThemWithUnderscore()
    {
        var id = ChunkIdGenerator.GenerateId("EU Regulation (2024).pdf", 2);

        Assert.Equal("EU_Regulation__2024__pdf-2", id);
    }

    [Fact]
    public void GenerateId_DocumentNameWithAllowedCharacters_LeavesThemUnchanged()
    {
        var id = ChunkIdGenerator.GenerateId("abc_XYZ-123=", 7);

        Assert.Equal("abc_XYZ-123=-7", id);
    }
}
