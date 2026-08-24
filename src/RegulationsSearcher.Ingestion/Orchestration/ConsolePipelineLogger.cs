namespace RegulationsSearcher.Ingestion.Orchestration;

public sealed class ConsolePipelineLogger : IPipelineLogger
{
    public void LogStepSucceeded(string documentName, string stepName) =>
        Console.WriteLine($"[{documentName}] [{stepName}] succeeded");

    public void LogStepFailed(string documentName, string stepName, Exception exception) =>
        Console.WriteLine($"[{documentName}] [{stepName}] failed: {exception.Message}");
}
