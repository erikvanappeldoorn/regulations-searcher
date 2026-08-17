using System.Net;
using RegulationsSearcher.Ingestion.Embeddings;

namespace RegulationsSearcher.Ingestion.Tests.Embeddings;

public class RetryingEmbeddingGeneratorTests
{
    private static HttpRequestException ThrottledException() =>
        new("Too many requests", inner: null, HttpStatusCode.TooManyRequests);

    [Fact]
    public async Task GenerateAsync_TransientFailureThenSuccess_RetriesAndReturnsResult()
    {
        var inner = new FailThenSucceedGenerator(ThrottledException());
        var retrying = new RetryingEmbeddingGenerator(inner, maxAttempts: 3, initialDelay: TimeSpan.FromMilliseconds(1));

        var result = await retrying.GenerateAsync(["a", "b"]);

        Assert.Equal(2, inner.CallCount);
        Assert.Equal(2, result.Count);
    }

    [Fact]
    public async Task GenerateAsync_TransientFailuresExceedMaxAttempts_ThrowsAfterFinalAttempt()
    {
        var inner = new FailThenSucceedGenerator(ThrottledException(), ThrottledException(), ThrottledException());
        var retrying = new RetryingEmbeddingGenerator(inner, maxAttempts: 3, initialDelay: TimeSpan.FromMilliseconds(1));

        await Assert.ThrowsAsync<HttpRequestException>(() => retrying.GenerateAsync(["a"]));
        Assert.Equal(3, inner.CallCount);
    }

    [Fact]
    public async Task GenerateAsync_TimeoutException_IsRetried()
    {
        var inner = new FailThenSucceedGenerator(new TimeoutException());
        var retrying = new RetryingEmbeddingGenerator(inner, maxAttempts: 2, initialDelay: TimeSpan.FromMilliseconds(1));

        var result = await retrying.GenerateAsync(["a"]);

        Assert.Equal(2, inner.CallCount);
        Assert.Single(result);
    }

    [Fact]
    public async Task GenerateAsync_NonTransientFailure_ThrowsWithoutRetrying()
    {
        var inner = new FailThenSucceedGenerator(new InvalidOperationException("not transient"));
        var retrying = new RetryingEmbeddingGenerator(inner, maxAttempts: 3, initialDelay: TimeSpan.FromMilliseconds(1));

        await Assert.ThrowsAsync<InvalidOperationException>(() => retrying.GenerateAsync(["a"]));
        Assert.Equal(1, inner.CallCount);
    }

    [Fact]
    public void Constructor_NonPositiveMaxAttempts_Throws()
    {
        var inner = new FailThenSucceedGenerator();
        Assert.Throws<ArgumentOutOfRangeException>(() => new RetryingEmbeddingGenerator(inner, maxAttempts: 0));
    }
}
