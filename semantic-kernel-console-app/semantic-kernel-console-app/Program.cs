using Microsoft.Extensions.Configuration;
using Microsoft.SemanticKernel;
using semantic_kernel_console_app;

var settings = new ConfigurationBuilder()
    .AddUserSecrets<Program>()
    .Build()
    .GetSection(nameof(AzureOpenAISettings)).Get<AzureOpenAISettings>() ?? throw new NullReferenceException();

var builder = new KernelBuilder();
builder.WithAzureTextCompletionService(settings.DeploymentName, settings.Endpoint, settings.ApiKey);
var kernel = builder.Build();

// Running prompts with input parameters
Console.WriteLine("Running prompts with input parameters...");
var summarizePrompt = @"{{$input}}

One line TLDR with the fewest words.";
var summarize = kernel.CreateSemanticFunction(summarizePrompt);

string text = @"
We rounded off the Premier League season with an emphatic win over Wolves, the highlight of which saw Granit Xhaka net his first brace for the club.
The Swiss midfielder took just 13 minutes of the first half to achieve the feat, with Bukayo Saka adding an excellent third before half-time to mark the week in which he pledged his future in the club in style.
We kept up the pressure in the second half and Gabriel Jesus added a fourth with a header just before the hour mark, and Jakub Kiwior bagged his first in red and white to complete the scoring, as well as a memorable campaign that has brought Champions League football back to the Emirates Stadium.";

Console.WriteLine(await summarize.InvokeAsync(text));

// Prompt chaining
Console.WriteLine("Prompt chaining...");
var translationPrompt = @"{{$input}}

Translate the following English text into Japanese.";
var translator = kernel.CreateSemanticFunction(translationPrompt);

Console.WriteLine(await kernel.RunAsync(text, summarize, translator));
