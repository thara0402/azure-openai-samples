using Azure;
using Azure.AI.OpenAI;
using Microsoft.Azure.Functions.Worker;
using Microsoft.Azure.Functions.Worker.Builder;
using Microsoft.Extensions.AI;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using System.Reflection;
using travel_concierge.Models;
using travel_concierge.Orchestrator;

var builder = FunctionsApplication.CreateBuilder(args);

builder.ConfigureFunctionsWebApplication();

builder.Configuration.AddUserSecrets(Assembly.GetExecutingAssembly());

builder.EnableMcpToolMetadata();

builder.Services
    .AddApplicationInsightsTelemetryWorkerService()
    .ConfigureFunctionsApplicationInsights()
    .AddTransient<IOrchestratorWorker, OrchestratorWorker>()
//    .AddTransient<OrchestratorWorker>()
    .Configure<TravelConciergeSettings>(builder.Configuration.GetSection("Function"))
    .AddChatClient(_ => BuildChatClient(builder.Configuration));

builder.Build().Run();

static IChatClient BuildChatClient(IConfiguration configuration)
{
    string GetRequired(string key) =>
        configuration[key] ?? throw new InvalidOperationException($"{key} is required.");

    var endpoint = GetRequired("Function:AzureOpenAIEndpoint");
    var apiKey = GetRequired("Function:AzureOpenAIApiKey");
    var modelDeploymentName = GetRequired("Function:ModelDeploymentName");

    return new ChatClientBuilder(
            new AzureOpenAIClient(new Uri(endpoint), new AzureKeyCredential(apiKey))
                .GetChatClient(modelDeploymentName).AsIChatClient())
        .UseFunctionInvocation()
        .Build();
}
