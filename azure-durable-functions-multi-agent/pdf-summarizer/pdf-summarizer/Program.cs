using Azure;
using Azure.AI.FormRecognizer.DocumentAnalysis;
using Microsoft.Azure.Functions.Worker;
using Microsoft.Azure.Functions.Worker.Builder;
using Microsoft.Extensions.Azure;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using pdf_summarizer.Models;
using System.Reflection;

var builder = FunctionsApplication.CreateBuilder(args);

builder.ConfigureFunctionsWebApplication();

builder.Configuration.AddUserSecrets(Assembly.GetExecutingAssembly());

builder.Services
    .AddApplicationInsightsTelemetryWorkerService()
    .ConfigureFunctionsApplicationInsights()
    .Configure<MySettings>(builder.Configuration.GetSection("Function"))
    .AddAzureClients(clientBuilder =>
    {
        clientBuilder.AddClient<DocumentAnalysisClient, DocumentAnalysisClientOptions>(options =>
        {
            var endpoint = builder.Configuration["Function:DocumentIntelligenceEndpoint"];
            if (string.IsNullOrEmpty(endpoint)) throw new InvalidOperationException("Function:DocumentIntelligenceEndpoint is required.");

            var apiKey = builder.Configuration["Function:DocumentIntelligenceApiKey"];
            if (string.IsNullOrEmpty(apiKey)) throw new InvalidOperationException("Function:DocumentIntelligenceApiKey is required.");

            return new DocumentAnalysisClient(new Uri(endpoint), new AzureKeyCredential(apiKey), options);
        });
    });

builder.Build().Run();
