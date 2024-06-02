#pragma warning disable SKEXP0040
using Azure;
using Microsoft.Extensions.Configuration;
using Microsoft.SemanticKernel;
using prompty_console_app;
using static System.Net.Mime.MediaTypeNames;

var settings = new ConfigurationBuilder()
    .AddUserSecrets<Program>()
    .Build()
    .GetSection(nameof(AzureOpenAISettings)).Get<AzureOpenAISettings>() ?? throw new NullReferenceException();

var builder = Kernel.CreateBuilder();
builder.AddAzureOpenAIChatCompletion(settings.DeploymentName, settings.Endpoint, settings.ApiKey);
var kernel = builder.Build();

var prompty = kernel.CreateFunctionFromPromptyFile("assistant.prompty");

while (true)
{
    Console.Write("You: ");
    var text = Console.ReadLine();
    if (string.IsNullOrWhiteSpace(text))
    {
        break;
    }

    Console.WriteLine("Assistant:");
    await foreach (var result in kernel.InvokeStreamingAsync<string>(prompty, new() { ["question"] = text }))
    {
        Console.Write(result);
    }
    Console.WriteLine("");
}
