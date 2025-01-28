using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Azure.Functions.Worker;
using Microsoft.Azure.Functions.Worker.Http;
using Microsoft.DurableTask.Client;
using Microsoft.Extensions.Logging;
using travel_concierge.Models;
using travel_concierge.Orchestrator;
using FromBodyAttribute = Microsoft.Azure.Functions.Worker.Http.FromBodyAttribute;

namespace travel_concierge
{
    public class Starter
    {
        [Function(nameof(SyncStarter))]
        public async Task<IActionResult> SyncStarter(
            [HttpTrigger(AuthorizationLevel.Anonymous, "post", Route = "invoke/sync")] HttpRequest req,
            [FromBody] Prompt prompt,
            [DurableClient] DurableTaskClient client,
            FunctionContext executionContext)
        {
            var logger = executionContext.GetLogger("SyncStarter");

            if (prompt == null)
            {
                return new BadRequestObjectResult("Please pass a prompt in the request body");
            }

            // Function input comes from the request content.
            string instanceId = await client.ScheduleNewOrchestrationInstanceAsync(nameof(OrchestratorWorker), prompt);

            logger.LogInformation("Started orchestration with ID = '{instanceId}'.", instanceId);

            OrchestrationMetadata metadata = await client.WaitForInstanceCompletionAsync(instanceId, getInputsAndOutputs: true);

            return new OkObjectResult(metadata.SerializedOutput ?? "");
        }

        [Function(nameof(AsyncStarter))]
        public async Task<HttpResponseData> AsyncStarter(
            [HttpTrigger(AuthorizationLevel.Anonymous, "post", Route = "invoke/async")] HttpRequestData req,
            [FromBody] Prompt prompt,
            [DurableClient] DurableTaskClient client,
            FunctionContext executionContext)
        {
            var logger = executionContext.GetLogger("AsyncStarter");

            if (prompt == null)
            {
                var badRequestResponse = req.CreateResponse(System.Net.HttpStatusCode.BadRequest);
                await badRequestResponse.WriteStringAsync("Please pass a prompt in the request body");
                return badRequestResponse;
            }

            // Function input comes from the request content.
            string instanceId = await client.ScheduleNewOrchestrationInstanceAsync(nameof(OrchestratorWorker), prompt);

            logger.LogInformation("Started orchestration with ID = '{instanceId}'.", instanceId);

            // Returns an HTTP 202 response with an instance management payload.
            // See https://learn.microsoft.com/azure/azure-functions/durable/durable-functions-http-api#start-orchestration
            return await client.CreateCheckStatusResponseAsync(req, instanceId);
        }
    }
}
