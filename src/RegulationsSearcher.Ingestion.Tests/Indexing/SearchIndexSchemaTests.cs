using Azure.Search.Documents.Indexes.Models;
using RegulationsSearcher.Ingestion.Indexing;

namespace RegulationsSearcher.Ingestion.Tests.Indexing;

public class SearchIndexSchemaTests
{
    [Fact]
    public void BuildIndex_SetsIndexName()
    {
        var index = SearchIndexSchema.BuildIndex("regulation-chunks", vectorSearchDimensions: 1536);

        Assert.Equal("regulation-chunks", index.Name);
    }

    [Fact]
    public void BuildIndex_KeyFieldIsStringKey()
    {
        var index = SearchIndexSchema.BuildIndex("regulation-chunks", vectorSearchDimensions: 1536);

        var keyField = index.Fields.Single(field => field.Name == SearchIndexSchema.KeyFieldName);
        Assert.True(keyField.IsKey);
        Assert.Equal(SearchFieldDataType.String, keyField.Type);
    }

    [Fact]
    public void BuildIndex_ContentFieldIsSearchable()
    {
        var index = SearchIndexSchema.BuildIndex("regulation-chunks", vectorSearchDimensions: 1536);

        var contentField = index.Fields.Single(field => field.Name == SearchIndexSchema.ContentFieldName);
        Assert.True(contentField.IsSearchable);
        Assert.Equal(SearchFieldDataType.String, contentField.Type);
    }

    [Fact]
    public void BuildIndex_ContentVectorFieldHasConfiguredDimensionsAndProfile()
    {
        var index = SearchIndexSchema.BuildIndex("regulation-chunks", vectorSearchDimensions: 1536);

        var vectorField = index.Fields.Single(field => field.Name == SearchIndexSchema.ContentVectorFieldName);
        Assert.Equal(SearchFieldDataType.Collection(SearchFieldDataType.Single), vectorField.Type);
        Assert.Equal(1536, vectorField.VectorSearchDimensions);
        Assert.NotNull(vectorField.VectorSearchProfileName);

        var profile = index.VectorSearch!.Profiles.Single(profile => profile.Name == vectorField.VectorSearchProfileName);
        var algorithm = index.VectorSearch!.Algorithms.Single(algorithm => algorithm.Name == profile.AlgorithmConfigurationName);
        Assert.IsType<HnswAlgorithmConfiguration>(algorithm);
    }

    [Theory]
    [InlineData("documentName")]
    [InlineData("pageStart")]
    [InlineData("pageEnd")]
    [InlineData("chunkIndex")]
    public void BuildIndex_MetadataFieldsAreFilterable(string fieldName)
    {
        var index = SearchIndexSchema.BuildIndex("regulation-chunks", vectorSearchDimensions: 1536);

        var field = index.Fields.Single(field => field.Name == fieldName);
        Assert.True(field.IsFilterable);
    }

    [Fact]
    public void BuildIndex_PageAndChunkIndexFieldsAreInt32()
    {
        var index = SearchIndexSchema.BuildIndex("regulation-chunks", vectorSearchDimensions: 1536);

        Assert.Equal(SearchFieldDataType.Int32, index.Fields.Single(field => field.Name == SearchIndexSchema.PageStartFieldName).Type);
        Assert.Equal(SearchFieldDataType.Int32, index.Fields.Single(field => field.Name == SearchIndexSchema.PageEndFieldName).Type);
        Assert.Equal(SearchFieldDataType.Int32, index.Fields.Single(field => field.Name == SearchIndexSchema.ChunkIndexFieldName).Type);
    }

    [Fact]
    public void BuildIndex_DifferentVectorDimensions_AreReflectedOnTheVectorField()
    {
        var index = SearchIndexSchema.BuildIndex("regulation-chunks", vectorSearchDimensions: 3072);

        var vectorField = index.Fields.Single(field => field.Name == SearchIndexSchema.ContentVectorFieldName);
        Assert.Equal(3072, vectorField.VectorSearchDimensions);
    }
}
