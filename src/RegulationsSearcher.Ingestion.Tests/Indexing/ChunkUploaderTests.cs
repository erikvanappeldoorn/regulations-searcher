using Azure;
using Azure.Search.Documents;
using Azure.Search.Documents.Models;
using RegulationsSearcher.Ingestion.Chunking;
using RegulationsSearcher.Ingestion.Embeddings;
using RegulationsSearcher.Ingestion.Indexing;

namespace RegulationsSearcher.Ingestion.Tests.Indexing;

public class ChunkUploaderTests
{
    private sealed class FakeSearchClient : SearchClient
    {
        public List<object> UploadedDocuments { get; } = [];

        public override Task<Response<IndexDocumentsResult>> MergeOrUploadDocumentsAsync<T>(
            IEnumerable<T> documents,
            IndexDocumentsOptions? options = null,
            CancellationToken cancellationToken = default)
        {
            UploadedDocuments.AddRange(documents!.Cast<object>());
            return Task.FromResult(Response.FromValue(
                SearchModelFactory.IndexDocumentsResult([]),
                response: null!));
        }
    }

    [Fact]
    public async Task UploadAsync_NoChunks_DoesNotCallSearchClient()
    {
        var client = new FakeSearchClient();
        var uploader = new ChunkUploader(client);

        await uploader.UploadAsync([]);

        Assert.Empty(client.UploadedDocuments);
    }

    [Fact]
    public async Task UploadAsync_UploadsIndexedChunkPerEmbeddedChunk()
    {
        var client = new FakeSearchClient();
        var uploader = new ChunkUploader(client);
        var embeddedChunks = new[]
        {
            new EmbeddedChunk(
                new TextChunk("doc.pdf", "first chunk content", PageStart: 1, PageEnd: 1, ChunkIndex: 0),
                new ReadOnlyMemory<float>([0.1f, 0.2f])),
            new EmbeddedChunk(
                new TextChunk("doc.pdf", "second chunk content", PageStart: 1, PageEnd: 2, ChunkIndex: 1),
                new ReadOnlyMemory<float>([0.3f, 0.4f])),
        };

        await uploader.UploadAsync(embeddedChunks);

        var uploaded = client.UploadedDocuments.Cast<IndexedChunk>().ToList();
        Assert.Equal(2, uploaded.Count);

        Assert.Equal("doc_pdf-0", uploaded[0].Id);
        Assert.Equal("first chunk content", uploaded[0].Content);
        Assert.Equal(new[] { 0.1f, 0.2f }, uploaded[0].ContentVector.ToArray());
        Assert.Equal("doc.pdf", uploaded[0].DocumentName);
        Assert.Equal(1, uploaded[0].PageStart);
        Assert.Equal(1, uploaded[0].PageEnd);
        Assert.Equal(0, uploaded[0].ChunkIndex);

        Assert.Equal("doc_pdf-1", uploaded[1].Id);
        Assert.Equal(1, uploaded[1].ChunkIndex);
    }
}
