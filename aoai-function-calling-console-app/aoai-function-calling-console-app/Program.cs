using aoai_function_calling_console_app;
using Azure;
using Azure.AI.OpenAI;
using Microsoft.Extensions.Configuration;
using System.Net.Http.Json;
using System;
using System.Text.Json;
using System.Text.Json.Serialization;
using Newtonsoft.Json;

var settings = new ConfigurationBuilder()
    .AddUserSecrets<Program>()
    .Build()
    .GetSection(nameof(AzureOpenAISettings)).Get<AzureOpenAISettings>() ?? throw new NullReferenceException();

var chatCompletionsOptions = new ChatCompletionsOptions
{
    MaxTokens = 800,
    Messages =
    {
        new ChatMessage(ChatRole.System, """
            あなたは、ユーザーが紅葉の名所を検索するのを助けるために設計されたAIアシスタントです。
            ユーザーから紅葉の名所を質問されたら、search_autumn_leaves関数を呼び出します。
        """)
    },
    Functions =
    {
        new FunctionDefinition{
            Name = "search_autumn_leaves",
            Description = "引数で指定された場所の紅葉の名所を検索します。",
            Parameters = BinaryData.FromObjectAsJson(new
            {
                Type = "object",
                Properties = new
                {
                    location = new
                    {
                        Type = "string",
                        Description = "紅葉の名所を検索する場所、場所には都道府県や都市の名前を含めること、例：東京、山梨",
                    },
                },
                Required = new[] { "location" },
            },
            new JsonSerializerOptions
            {
                PropertyNamingPolicy = JsonNamingPolicy.CamelCase,
            }),
        }
    },
    FunctionCall = FunctionDefinition.Auto
};

Console.WriteLine("アシスタントのセットアップ中・・・");
var client = new OpenAIClient(new Uri(settings.Endpoint), new AzureKeyCredential(settings.ApiKey));
await client.GetChatCompletionsAsync(settings.DeploymentName, chatCompletionsOptions);

Console.WriteLine("チャットを開始する");
var userMessage = "今年は紅葉を見に行きたいけど、京都でお勧めの紅葉の名所を教えてください。";
Console.WriteLine($"{ChatRole.User}: {userMessage}");
chatCompletionsOptions.Messages.Add(new ChatMessage(ChatRole.User, userMessage));
var result = await client.GetChatCompletionsAsync(settings.DeploymentName, chatCompletionsOptions);
var choice = result.Value.Choices[0];
if (choice.FinishReason == "function_call")
{
    // API 呼び出し
    var functionsResponse = String.Empty;
    switch (choice.Message.FunctionCall.Name)
    {
        case "search_autumn_leaves":
            var parameter = JsonConvert.DeserializeObject<AutumnLeavesParameter>(choice.Message.FunctionCall.Arguments);
            if (parameter == null)
            {
                Console.WriteLine("FunctionCall.Arguments is null");
            }
            else
            {
                functionsResponse = ApiClient.SearchAutumnLeaves(parameter.Location);
            }
            break;
        default:
            Console.WriteLine("function: not found");
            return;
    }

    // API のレスポンスを使ってメッセージを返す
    chatCompletionsOptions.Messages.Add(
        new ChatMessage
        {
            Role = choice.Message.Role,
            Name = choice.Message.FunctionCall.Name,
            Content = choice.Message.FunctionCall.Arguments
        });
    chatCompletionsOptions.Messages.Add(
        new ChatMessage
        {
            Role = ChatRole.Function,
            Name = choice.Message.FunctionCall.Name,
            Content = functionsResponse
        });
    var assistantResult = await client.GetChatCompletionsAsync(settings.DeploymentName, chatCompletionsOptions);
    var assistantChoice = assistantResult.Value.Choices[0];
    Console.WriteLine($"{assistantChoice.Message.Role}: {assistantChoice.Message.Content}");

    Console.WriteLine("---------------------------------");
    Console.WriteLine("Information");
    Console.WriteLine($"- FunctionCall.Name: {choice.Message.FunctionCall.Name}");
    Console.WriteLine($"- FunctionCall.Arguments: {choice.Message.FunctionCall.Arguments}");
}
else
{
    Console.WriteLine($"{choice.Message.Role}: {choice.Message.Content}");
}
