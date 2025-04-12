using Microsoft.Azure.Functions.Worker;
using Microsoft.DurableTask;
using travel_concierge.Models;
using Microsoft.Extensions.Logging;

namespace travel_concierge.Orchestrator
{
    public class OrchestratorWorker
    {
        [Function(nameof(OrchestratorWorker))]
        public static async Task<SynthesizedResult> RunOrchestrator(
            [OrchestrationTrigger] TaskOrchestrationContext context)
        {
            var logger = context.CreateReplaySafeLogger(nameof(OrchestratorWorker));

            var prompt = context.GetInput<Prompt>();
            ArgumentNullException.ThrowIfNull(prompt);

            var options = TaskOptions.FromRetryPolicy(new RetryPolicy(
                maxNumberOfAttempts: 3,
                firstRetryInterval: TimeSpan.FromSeconds(5)));

            // Agent Desider
            var agentsToRun = await context.CallGetAgentsToRunAsync(prompt, options);
            if (!agentsToRun.IsAgentCall)
            {
                logger.LogInformation("No agent call happened.");
                return new SynthesizedResult { Content = agentsToRun.Content };
            }

            // Agent Call
            logger.LogInformation("Agent call happened.");
            var parallelAgentCall = new List<Task<string>>();
            foreach (var agentCall in agentsToRun.AgentCalls)
            {
                parallelAgentCall.Add(context.CallActivityAsync<string>(agentCall.AgentName, agentCall.Arguments, options));
            }
            await Task.WhenAll(parallelAgentCall);

            // Synthesize
            var agentCallResults = new AgentCallResults
            {
                Results = parallelAgentCall.Select(x => x.Result).ToList(),
                Prompt = prompt,
                CalledAgentNames = agentsToRun.AgentCalls.Select(x => x.AgentName).ToList()
            };
            return await context.CallSynthesizeAgentCallResultsAsync(agentCallResults, options);
        }
    }
}
