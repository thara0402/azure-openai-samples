using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Azure.Functions.Worker;
using Microsoft.Extensions.Logging;
using travel_concierge.Models;
using travel_concierge.Orchestrator;
using FromBodyAttribute = Microsoft.Azure.Functions.Worker.Http.FromBodyAttribute;

namespace travel_concierge
{
    public class Starter(IOrchestratorWorker orchestratorWorker, ILogger<Starter> logger)
    {
        private readonly IOrchestratorWorker _orchestratorWorker = orchestratorWorker;
        private readonly ILogger<Starter> _logger = logger;

        [Function(nameof(SyncStarter))]
        public async Task<IActionResult> SyncStarter(
            [HttpTrigger(AuthorizationLevel.Anonymous, "post", Route = "invoke/sync")] HttpRequest req,
            [FromBody] Prompt prompt)
        {
            if (prompt == null)
            {
                return new BadRequestObjectResult("Please pass a prompt in the request body");
            }

            var result = await _orchestratorWorker.RunOrchestrator(prompt);
            return new OkObjectResult(result);
        }
    }
}
