using Azure.AI.OpenAI;
using Microsoft.Azure.Functions.Worker;
using Microsoft.Azure.Functions.Worker.Builder;
using Microsoft.Extensions.Azure;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using System.ClientModel;
using System.Reflection;
using System.Text.Encodings.Web;
using System.Text.Json;
using System.Text.Unicode;
using travel_concierge.Models;

var builder = FunctionsApplication.CreateBuilder(args);

builder.ConfigureFunctionsWebApplication();

builder.Configuration.AddUserSecrets(Assembly.GetExecutingAssembly());

builder.Services
    .AddApplicationInsightsTelemetryWorkerService()
    .ConfigureFunctionsApplicationInsights()
    .Configure<OrchestratorWorkerSettings>(builder.Configuration.GetSection("Function"))
    .AddAzureClients(clientBuilder =>
    {
        clientBuilder.AddClient<AzureOpenAIClient, AzureOpenAIClientOptions>(options =>
        {
            var endpoint = builder.Configuration["Function:AzureOpenAIEndpoint"];
            if (string.IsNullOrEmpty(endpoint)) throw new InvalidOperationException("Function:AzureOpenAIEndpoint is required.");

            var apiKey = builder.Configuration["Function:AzureOpenAIApiKey"];
            if (string.IsNullOrEmpty(apiKey)) throw new InvalidOperationException("Function:AzureOpenAIApiKey is required.");

            return new AzureOpenAIClient(new Uri(endpoint), new ApiKeyCredential(apiKey), options);
        });
    });

builder.Services.Configure<JsonSerializerOptions>(jsonSerializerOptions =>
{
    jsonSerializerOptions.PropertyNamingPolicy = JsonNamingPolicy.CamelCase;
    // オーケスとレーター関数のmetadata.SerializedOutputの日本語がエスケープされないように設定
    // https://learn.microsoft.com/ja-jp/dotnet/standard/serialization/system-text-json/character-encoding
    jsonSerializerOptions.Encoder = JavaScriptEncoder.Create(UnicodeRanges.All);
});

builder.Build().Run();
