using Microsoft.Agents.AI.DurableTask;
using Microsoft.Azure.Functions.Worker;
using Microsoft.Azure.Functions.Worker.Http;
using Microsoft.DurableTask;
using Microsoft.DurableTask.Client;
using Microsoft.Extensions.Logging;
using System.ComponentModel;

namespace maf_durable_agent;

public static class HelloAgent
{
    public sealed record TextResponse(string Text);

    [Description("Greets the given name and returns a hello message.")]
    public static string SayHello([Description("The name to greet")] string name)
    {
        return $"Hello {name}!";
    }

    [Function(nameof(HelloAgent))]
    public static async Task<List<string>> RunOrchestrator(
        [OrchestrationTrigger] TaskOrchestrationContext context)
    {
        ILogger logger = context.CreateReplaySafeLogger(nameof(HelloAgent));
        logger.LogInformation("Saying hello.");
        var outputs = new List<string>();

        var helloAgent = context.GetAgent("HelloAgent");

        var tokyoResponse = await helloAgent.RunAsync<TextResponse>(message: "Tokyo");
        outputs.Add(tokyoResponse.Result.Text);

        var seattleResponse = await helloAgent.RunAsync<TextResponse>(message: "Seattle");
        outputs.Add(seattleResponse.Result.Text);

        var londonResponse = await helloAgent.RunAsync<TextResponse>(message: "London");
        outputs.Add(londonResponse.Result.Text);

        return outputs;
    }

    [Function("HelloAgent_HttpStart")]
    public static async Task<HttpResponseData> HttpStart(
        [HttpTrigger(AuthorizationLevel.Anonymous, "get", "post")] HttpRequestData req,
        [DurableClient] DurableTaskClient client,
        FunctionContext executionContext)
    {
        ILogger logger = executionContext.GetLogger("HelloAgent_HttpStart");

        // Function input comes from the request content.
        string instanceId = await client.ScheduleNewOrchestrationInstanceAsync(
            nameof(HelloAgent));

        logger.LogInformation("Started orchestration with ID = '{instanceId}'.", instanceId);

        // Returns an HTTP 202 response with an instance management payload.
        // See https://learn.microsoft.com/azure/azure-functions/durable/durable-functions-http-api#start-orchestration
        return await client.CreateCheckStatusResponseAsync(req, instanceId);
    }
}