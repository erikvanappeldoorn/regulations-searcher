using RegulationsSearcher.Ingestion.Orchestration;

namespace RegulationsSearcher.Ingestion.Tests.Orchestration;

public class ConsolePipelineLoggerTests
{
    [Fact]
    public void LogStepSucceeded_WritesStepNameToConsole()
    {
        var logger = new ConsolePipelineLogger();
        var originalOut = Console.Out;
        using var writer = new StringWriter();
        Console.SetOut(writer);

        try
        {
            logger.LogStepSucceeded("ExtractTextExecutor");
        }
        finally
        {
            Console.SetOut(originalOut);
        }

        Assert.Contains("ExtractTextExecutor", writer.ToString());
        Assert.Contains("succeeded", writer.ToString());
    }

    [Fact]
    public void LogStepFailed_WritesStepNameAndExceptionMessageToConsole()
    {
        var logger = new ConsolePipelineLogger();
        var originalOut = Console.Out;
        using var writer = new StringWriter();
        Console.SetOut(writer);

        try
        {
            logger.LogStepFailed("ExtractTextExecutor", new InvalidOperationException("boom"));
        }
        finally
        {
            Console.SetOut(originalOut);
        }

        Assert.Contains("ExtractTextExecutor", writer.ToString());
        Assert.Contains("failed", writer.ToString());
        Assert.Contains("boom", writer.ToString());
    }
}
