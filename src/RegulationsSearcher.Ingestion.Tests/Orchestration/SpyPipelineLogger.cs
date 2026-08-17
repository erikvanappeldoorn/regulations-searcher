using RegulationsSearcher.Ingestion.Orchestration;

namespace RegulationsSearcher.Ingestion.Tests.Orchestration;

internal sealed class SpyPipelineLogger : IPipelineLogger
{
    public List<string> SucceededSteps { get; } = [];
    public List<(string StepName, Exception Exception)> FailedSteps { get; } = [];

    public void LogStepSucceeded(string stepName) => SucceededSteps.Add(stepName);

    public void LogStepFailed(string stepName, Exception exception) => FailedSteps.Add((stepName, exception));
}
