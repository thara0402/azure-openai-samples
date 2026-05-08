using Azure;
using Azure.AI.OpenAI;
using maf_durable_agent;
using Microsoft.Agents.AI.Hosting.AzureFunctions;
using Microsoft.Azure.Functions.Worker;
using Microsoft.Azure.Functions.Worker.Builder;
using Microsoft.Extensions.AI;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using OpenAI.Chat;

var endpoint = Environment.GetEnvironmentVariable("AZURE_OPENAI_ENDPOINT")
    ?? throw new InvalidOperationException("AZURE_OPENAI_ENDPOINT is not set.");
var deploymentName = Environment.GetEnvironmentVariable("AZURE_OPENAI_DEPLOYMENT_NAME")
    ?? throw new InvalidOperationException("AZURE_OPENAI_DEPLOYMENT_NAME is not set.");
var azureOpenAiKey = Environment.GetEnvironmentVariable("AZURE_OPENAI_KEY")
    ?? throw new InvalidOperationException("AZURE_OPENAI_KEY is not set.");

var aoaiClient = new AzureOpenAIClient(
    new Uri(endpoint),
    new AzureKeyCredential(azureOpenAiKey));

// Set up an AI agent following the standard Microsoft Agent Framework pattern.
const string SummaryName = "SummaryAgent";
const string SummaryInstructions = "One line TLDR with the fewest words.";
var summaryAgent = aoaiClient.GetChatClient(deploymentName).AsAIAgent(SummaryInstructions, SummaryName);

const string TranslatorName = "TranslatorAgent";
const string TranslatorInstructions = "Translate the following English text into Japanese.";
var translatorAgent = aoaiClient.GetChatClient(deploymentName).AsAIAgent(TranslatorInstructions, TranslatorName);

const string HelloName = "HelloAgent";
const string HelloInstructions = "Use the SayHello tool to greet the name provided by the user.";
var helloAgent = aoaiClient.GetChatClient(deploymentName).AsAIAgent(
    HelloInstructions,
    HelloName,
    tools: [AIFunctionFactory.Create(HelloAgent.SayHello)]);

var builder = FunctionsApplication.CreateBuilder(args);

builder.ConfigureFunctionsWebApplication();

// Configure the function app to host the AI agent.
builder.ConfigureDurableAgents(options =>
    {
        options.AddAIAgent(summaryAgent, enableHttpTrigger: false, enableMcpToolTrigger: false);
        options.AddAIAgent(translatorAgent, enableHttpTrigger: false, enableMcpToolTrigger: false);
        options.AddAIAgent(helloAgent, enableHttpTrigger: false, enableMcpToolTrigger: false);
    });

builder.Services
    .AddApplicationInsightsTelemetryWorkerService()
    .ConfigureFunctionsApplicationInsights();

builder.Build().Run();
