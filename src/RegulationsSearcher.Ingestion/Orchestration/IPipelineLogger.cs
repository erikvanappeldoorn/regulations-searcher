namespace RegulationsSearcher.Ingestion.Orchestration;

public interface IPipelineLogger
{
    void LogStepSucceeded(string stepName);

    void LogStepFailed(string stepName, Exception exception);
}
