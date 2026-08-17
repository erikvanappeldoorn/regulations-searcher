namespace RegulationsSearcher.Ingestion.Orchestration;

public sealed class ConsolePipelineLogger : IPipelineLogger
{
    public void LogStepSucceeded(string stepName) =>
        Console.WriteLine($"[{stepName}] succeeded");

    public void LogStepFailed(string stepName, Exception exception) =>
        Console.WriteLine($"[{stepName}] failed: {exception.Message}");
}
