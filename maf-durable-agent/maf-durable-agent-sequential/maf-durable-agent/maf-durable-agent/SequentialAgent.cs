using Microsoft.Agents.AI;
using Microsoft.Agents.AI.DurableTask;
using Microsoft.Azure.Functions.Worker;
using Microsoft.Azure.Functions.Worker.Http;
using Microsoft.DurableTask;
using Microsoft.DurableTask.Client;
using Microsoft.Extensions.Logging;
using System.Net;

namespace maf_durable_agent;

public static class SequentialAgent
{
    // Define a strongly-typed response structure for agent outputs
    public sealed record TextResponse(string Text);
    
    [Function(nameof(SequentialAgent))]
    public static async Task<List<string>> RunOrchestrator(
        [OrchestrationTrigger] TaskOrchestrationContext context)
    {
        ILogger logger = context.CreateReplaySafeLogger(nameof(SequentialAgent));
        logger.LogInformation("Sequential Agent.");

        // Get the prompt from the orchestration input
        var prompt = context.GetInput<string>() ?? throw new InvalidOperationException("Prompt is required");
        var outputs = new List<string>();

        // Get both agents
        var summaryAgent = context.GetAgent("SummaryAgent");
        var translatorAgent = context.GetAgent("TranslatorAgent");

        // Summarize the prompt using the summary agent
        var summaryResponse = await summaryAgent.RunAsync<TextResponse>(message: prompt);
        var summaryText = summaryResponse.Result.Text;
        outputs.Add(summaryText);

        // Translate the summary using the translator agent
        var translatorResponse = await translatorAgent.RunAsync<TextResponse>(message: summaryText);
        var translatorText = translatorResponse.Result.Text;
        outputs.Add(translatorText);

        return outputs;
    }

    [Function("SequentialAgent_HttpStart")]
    public static async Task<HttpResponseData> HttpStart(
        [HttpTrigger(AuthorizationLevel.Anonymous, "post")] HttpRequestData req,
        [DurableClient] DurableTaskClient client,
        FunctionContext executionContext)
    {
        ILogger logger = executionContext.GetLogger("SequentialAgent_HttpStart");

        // Read the prompt from the request body
        string? prompt = await req.ReadAsStringAsync();
        if (String.IsNullOrWhiteSpace(prompt))
        {
            HttpResponseData badRequestResponse = req.CreateResponse(HttpStatusCode.BadRequest);
            await badRequestResponse.WriteAsJsonAsync(new { error = "Prompt is required" });
            return badRequestResponse;
        }

        // Function input comes from the request content.
        string instanceId = await client.ScheduleNewOrchestrationInstanceAsync(
            nameof(SequentialAgent), prompt);

        logger.LogInformation("Started orchestration with ID = '{instanceId}'.", instanceId);

        // Returns an HTTP 202 response with an instance management payload.
        // See https://learn.microsoft.com/azure/azure-functions/durable/durable-functions-http-api#start-orchestration
        return await client.CreateCheckStatusResponseAsync(req, instanceId);
    }
}
