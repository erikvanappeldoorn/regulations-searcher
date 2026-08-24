using RegulationsSearcher.Ingestion.Orchestration;

namespace RegulationsSearcher.Ingestion.Tests.Orchestration;

internal sealed class NoOpPipelineLogger : IPipelineLogger
{
    public void LogStepSucceeded(string documentName, string stepName)
    {
    }

    public void LogStepFailed(string documentName, string stepName, Exception exception)
    {
    }
}
