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
                あなたは日本の地理に詳しい優秀なアシスタントです。
            """;

var userMessages = new String[] {
    "日本の都道府県のうち、海に面していない都道府県を３つ教えて",
    "その３つの都道府県の県庁所在地を教えて",
};

Console.WriteLine("アシスタントのセットアップ中・・・");

var aoaiClient = new AzureOpenAIClient(
    new Uri(settings.Endpoint),
    new AzureKeyCredential(settings.ApiKey));

var chatClient = aoaiClient.GetChatClient(settings.DeploymentName)
    .AsIChatClient();

var agent = new ChatClientAgent(chatClient, instructions);

Console.WriteLine("チャットを開始する");

var session = await agent.CreateSessionAsync();

foreach (var userMessage in userMessages)
{
    Console.WriteLine($"{ChatRole.User}: {userMessage}");
    var result = await agent.RunAsync(userMessage, session);

    foreach (var message in result.Messages)
    {
        Console.WriteLine($"{message.Role}: {message.Text}");
    }
}
