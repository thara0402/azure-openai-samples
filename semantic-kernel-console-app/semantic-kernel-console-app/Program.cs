using Microsoft.Extensions.Configuration;
using Microsoft.SemanticKernel;
using semantic_kernel_console_app;

var settings = new ConfigurationBuilder()
    .AddUserSecrets<Program>()
    .Build()
    .GetSection(nameof(AzureOpenAISettings)).Get<AzureOpenAISettings>() ?? throw new NullReferenceException();

var builder = Kernel.CreateBuilder();
builder.AddAzureOpenAIChatCompletion(settings.DeploymentName, settings.Endpoint, settings.ApiKey);
var kernel = builder.Build();

// Running prompts with input parameters
Console.WriteLine("Running prompts with input parameters...");
var summarizePrompt = @"{{$input}}

One line TLDR with the fewest words.";
var summarize = kernel.CreateFunctionFromPrompt(summarizePrompt);

string text = @"
We fell just short in our hopes of winning the Premier League title but Kai Havertz’s late goal ensured we ended a season to remember on a high by beating Everton.
Despite having nothing to play for, the Toffees proved to be tricky opponents and took the lead through Idrissa Gueye’s deflected free-kick, but we instantly replied when Takehiro Tomiyasu found the bottom corner.
We kept pressing for the goal that would give us any hope of the title and it came with a minute left through the German striker, but Manchester City’s 3-1 win against West Ham meant that our result was immaterial as they collected a fourth-straight league title.
They finished just two points ahead of ourselves despite us hitting record hauls for wins and goals in the competition, and amassing 89 points - just one short of the Invincibles’ record 20 years ago.";

var summarizeResult = await kernel.InvokeAsync(summarize, new() { ["input"] = text });
Console.WriteLine(summarizeResult);

//// Prompt chaining
//Console.WriteLine("Prompt chaining...");
//var translationPrompt = @"{{$input}}

//Translate the following English text into Japanese.";
//var translator = kernel.CreateFunctionFromPrompt(translationPrompt);

//Console.WriteLine(await kernel.RunAsync(text, summarize, translator));

// Translate
Console.WriteLine("Translate...");
var translationPrompt = @"{{$input}}

Translate the following English text into Japanese.";
var translator = kernel.CreateFunctionFromPrompt(translationPrompt);

var translationResult = await kernel.InvokeAsync(translator, new() { ["input"] = summarizeResult });
Console.WriteLine(translationResult);
