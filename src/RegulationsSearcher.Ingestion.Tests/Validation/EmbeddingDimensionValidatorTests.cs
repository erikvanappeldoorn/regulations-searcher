using Azure.Search.Documents.Indexes.Models;
using RegulationsSearcher.Ingestion.Validation;

namespace RegulationsSearcher.Ingestion.Tests.Validation;

public class EmbeddingDimensionValidatorTests
{
    private static SearchField VectorField(string name, int dimensions) =>
        new(name, SearchFieldDataType.Collection(SearchFieldDataType.Single)) { VectorSearchDimensions = dimensions };

    [Fact]
    public async Task ValidateAsync_IndexDoesNotExist_DoesNotThrow()
    {
        var client = new FakeSearchIndexClient(index: null);
        var validator = new EmbeddingDimensionValidator(client);

        await validator.ValidateAsync("regulations", "contentVector", 1536);
    }

    [Fact]
    public async Task ValidateAsync_DimensionsMatch_DoesNotThrow()
    {
        var index = new SearchIndex("regulations", [VectorField("contentVector", 1536)]);
        var client = new FakeSearchIndexClient(index);
        var validator = new EmbeddingDimensionValidator(client);

        await validator.ValidateAsync("regulations", "contentVector", 1536);
    }

    [Fact]
    public async Task ValidateAsync_DimensionsMismatch_ThrowsInvalidOperationException()
    {
        var index = new SearchIndex("regulations", [VectorField("contentVector", 1536)]);
        var client = new FakeSearchIndexClient(index);
        var validator = new EmbeddingDimensionValidator(client);

        var exception = await Assert.ThrowsAsync<InvalidOperationException>(
            () => validator.ValidateAsync("regulations", "contentVector", 768));

        Assert.Contains("768", exception.Message);
        Assert.Contains("1536", exception.Message);
    }

    [Fact]
    public async Task ValidateAsync_VectorFieldMissing_ThrowsInvalidOperationException()
    {
        var index = new SearchIndex("regulations", [new SearchField("documentName", SearchFieldDataType.String)]);
        var client = new FakeSearchIndexClient(index);
        var validator = new EmbeddingDimensionValidator(client);

        await Assert.ThrowsAsync<InvalidOperationException>(
            () => validator.ValidateAsync("regulations", "contentVector", 1536));
    }

    [Fact]
    public async Task ValidateAsync_FieldExistsButIsNotVector_ThrowsInvalidOperationException()
    {
        var index = new SearchIndex("regulations", [new SearchField("contentVector", SearchFieldDataType.String)]);
        var client = new FakeSearchIndexClient(index);
        var validator = new EmbeddingDimensionValidator(client);

        await Assert.ThrowsAsync<InvalidOperationException>(
            () => validator.ValidateAsync("regulations", "contentVector", 1536));
    }
}
