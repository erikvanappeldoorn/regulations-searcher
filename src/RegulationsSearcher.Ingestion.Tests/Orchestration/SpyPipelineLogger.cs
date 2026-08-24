using RegulationsSearcher.Ingestion.Orchestration;

namespace RegulationsSearcher.Ingestion.Tests.Orchestration;

internal sealed class SpyPipelineLogger : IPipelineLogger
{
    public List<(string DocumentName, string StepName)> SucceededSteps { get; } = [];
    public List<(string DocumentName, string StepName, Exception Exception)> FailedSteps { get; } = [];

    public void LogStepSucceeded(string documentName, string stepName) => SucceededSteps.Add((documentName, stepName));

    public void LogStepFailed(string documentName, string stepName, Exception exception) => FailedSteps.Add((documentName, stepName, exception));
}
