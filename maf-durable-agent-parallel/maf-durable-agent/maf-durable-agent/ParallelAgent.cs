using Microsoft.Agents.AI.DurableTask;
using Microsoft.Azure.Functions.Worker;
using Microsoft.Azure.Functions.Worker.Http;
using Microsoft.DurableTask;
using Microsoft.DurableTask.Client;
using Microsoft.Extensions.Logging;
using System;
using System.Collections.Generic;
using System.Net;
using System.Text;

namespace maf_durable_agent
{
    public static class ParallelAgent
    {
        // Define a strongly-typed response structure for agent outputs
        public sealed record TextResponse(string Text);

        [Function(nameof(ParallelAgent))]
        public static async Task<List<string>> RunOrchestrator(
            [OrchestrationTrigger] TaskOrchestrationContext context)
        {
            ILogger logger = context.CreateReplaySafeLogger(nameof(ParallelAgent));
            logger.LogInformation("Parallel Agent.");

            // Get the prompt from the orchestration input
            var prompt = context.GetInput<string>() ?? throw new InvalidOperationException("Prompt is required");
            var outputs = new List<string>();

            // Get both agents
            var englishTranslatorAgent = context.GetAgent("EnglishTranslatorAgent");
            var italianTranslatorAgent = context.GetAgent("ItalianTranslatorAgent");

            // Run both agents in parallel
            var englishTask = englishTranslatorAgent.RunAsync<TextResponse>(message: prompt);
            var italianTask = italianTranslatorAgent.RunAsync<TextResponse>(message: prompt);

            // Wait for both tasks to complete
            await Task.WhenAll(englishTask, italianTask);

            // Collect the results
            outputs.Add((await englishTask).Result.Text);
            outputs.Add((await italianTask).Result.Text);

            return outputs;
        }

        [Function("ParallelAgent_HttpStart")]
        public static async Task<HttpResponseData> HttpStart(
            [HttpTrigger(AuthorizationLevel.Anonymous, "post")] HttpRequestData req,
            [DurableClient] DurableTaskClient client,
            FunctionContext executionContext)
        {
            ILogger logger = executionContext.GetLogger("ParallelAgent_HttpStart");

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
                nameof(ParallelAgent), prompt);

            logger.LogInformation("Started orchestration with ID = '{instanceId}'.", instanceId);

            // Returns an HTTP 202 response with an instance management payload.
            // See https://learn.microsoft.com/azure/azure-functions/durable/durable-functions-http-api#start-orchestration
            return await client.CreateCheckStatusResponseAsync(req, instanceId);
        }
    }
}
