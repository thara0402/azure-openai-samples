using Azure;
using Azure.AI.OpenAI;
using maf_console_app;
using Microsoft.Agents.AI;
using Microsoft.Extensions.AI;
using Microsoft.Extensions.Configuration;

var settings = new ConfigurationBuilder()
    .AddUserSecrets<Program>()
    .Build()
    .GetSection(nameof(AzureOpenAISettings)).Get<AzureOpenAISettings>() ?? throw new NullReferenceException();

var instructions = """
                You are a helpful assistant.
            """;

var userMessages = new String[] {
    "摂氏20度は華氏何度ですか？また、華氏50度は摂氏何度ですか？",
    "マラソン（42.195キロメートル）は何マイルですか？また、75キログラムは何ポンドですか？"
};

Console.WriteLine("エージェントのセットアップ中・・・");

// --- Skills Provider ---
// Discovers skills from the 'skills' directory containing SKILL.md files.
// The script runner runs file-based scripts (e.g. Python) as local subprocesses.
var skillsProvider = new AgentSkillsProvider(
    Path.Combine(AppContext.BaseDirectory, "skills"),
    SubprocessScriptRunner.RunAsync);

// --- Agent Setup ---
var aoaiClient = new AzureOpenAIClient(
    new Uri(settings.Endpoint),
    new AzureKeyCredential(settings.ApiKey));

var chatClient = aoaiClient.GetChatClient(settings.DeploymentName)
    .AsIChatClient();

var agent = new ChatClientAgent(chatClient, new ChatClientAgentOptions
{
    ChatOptions = new ChatOptions
    {
        Instructions = instructions
    },
    AIContextProviders = [skillsProvider]
});

Console.WriteLine("チャットを開始する");

var session = await agent.CreateSessionAsync();

foreach (var userMessage in userMessages)
{
    Console.WriteLine($"{ChatRole.User}: {userMessage}");

    var result = await agent.RunAsync(userMessage, session);
    Console.WriteLine($"{ChatRole.Assistant}: {result.Text}");

    Console.WriteLine();
}
