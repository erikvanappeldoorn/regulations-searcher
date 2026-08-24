namespace RegulationsSearcher.Ingestion.Orchestration;

public interface IPipelineLogger
{
    void LogStepSucceeded(string documentName, string stepName);

    void LogStepFailed(string documentName, string stepName, Exception exception);
}
