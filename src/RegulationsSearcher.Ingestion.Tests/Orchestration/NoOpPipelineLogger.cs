using RegulationsSearcher.Ingestion.Orchestration;

namespace RegulationsSearcher.Ingestion.Tests.Orchestration;

internal sealed class NoOpPipelineLogger : IPipelineLogger
{
    public void LogStepSucceeded(string stepName)
    {
    }

    public void LogStepFailed(string stepName, Exception exception)
    {
    }
}
