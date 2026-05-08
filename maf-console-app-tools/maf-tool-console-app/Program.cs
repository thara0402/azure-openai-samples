using Azure;
using Azure.AI.OpenAI;
using maf_tool_console_app;
using Microsoft.Agents.AI;
using Microsoft.Extensions.AI;
using Microsoft.Extensions.Configuration;
using System.ComponentModel;

var settings = new ConfigurationBuilder()
    .AddUserSecrets<Program>()
    .Build()
    .GetSection(nameof(AzureOpenAISettings)).Get<AzureOpenAISettings>() ?? throw new NullReferenceException();

var instructions = """
            あなたは、ユーザーが桜の名所を検索するのを助けるために設計されたAIアシスタントです。
            """;

var userMessages = new String[] {
    "今年は桜を見に行きたいけど、京都でお勧めの桜の名所を教えてください。",
    "東京はどこがいいですか？",
};

[Description("引数で指定された場所の桜の名所を検索します。")]
static string SearchCherryBlossoms([Description("桜の名所を検索する場所、場所には都道府県や都市の名前を含めること、例：東京、山梨")] string location)
{
    if (location.Contains("京都"))
    {
        return $"{location}の桜の名所は清水寺です。";
    }
    return "京都以外の桜の名所は検索できません。";
}

Console.WriteLine("アシスタントのセットアップ中・・・");

var aoaiClient = new AzureOpenAIClient(
    new Uri(settings.Endpoint),
    new AzureKeyCredential(settings.ApiKey));

var chatClient = aoaiClient.GetChatClient(settings.DeploymentName)
    .AsIChatClient();

var agent = new ChatClientAgent(
    chatClient,
    instructions: instructions,
    tools : [AIFunctionFactory.Create(SearchCherryBlossoms)]);

Console.WriteLine("チャットを開始する");

var session = await agent.CreateSessionAsync();

foreach (var userMessage in userMessages)
{
    Console.WriteLine($"{ChatRole.User}: {userMessage}");
    var result = await agent.RunAsync(userMessage, session);

    Console.WriteLine($"{ChatRole.Assistant}: {result.Text}");
}




