using Microsoft.Extensions.Configuration;
using aoai_on_your_data_console_app;
using MyApiClient;

var settings = new ConfigurationBuilder()
    .AddUserSecrets<Program>()
    .Build()
    .GetSection(nameof(AzureOpenAISettings)).Get<AzureOpenAISettings>() ?? throw new NullReferenceException();

Console.WriteLine("アシスタントのセットアップ中・・・"); 
var body = new ExtensionsChatCompletionsRequest
{
    DataSources = new[] {
            new DataSource {
                Type = "AzureCognitiveSearch",
                Parameters = new {
                    endpoint = settings.CognitiveSearchEndpoint,
                    key = settings.CognitiveSearchKey,
                    indexName = settings.CognitiveSearchIndexName
                }
            }
        },
    Messages = new[] {
            new Message {
                Role = MessageRole.System,
                Content = """
                    あなたは社内の総務担当です。就業規則をもとにして、ユーザーからの質問に回答してください。
                    回答の際には出典を出力してください。
                    回答が分からない場合は、「分かりません」と回答してください。
                """
            },
            new Message {
                Role = MessageRole.User,
                Content = "社員が結婚するときの特別休暇は何日ですか？"
            }
        },
    Max_tokens = 800,
    Temperature = 0d,
    Frequency_penalty = 0d,
    Presence_penalty = 0d,
};

Console.WriteLine("チャットを開始する");
foreach (var message in body.Messages)
{
    Console.WriteLine($"{message.Role}: {message.Content}");
}

using (var httpClient = new HttpClient())
{
    httpClient.DefaultRequestHeaders.Add("api-key", settings.ApiKey);
    var client = new ExtensionsChatCompletionsClient(httpClient);
    client.BaseUrl = settings.Endpoint;

    Console.WriteLine("就業規則を確認中・・・");
    var result = await client.CreateAsync(settings.DeploymentId, settings.ApiVersion, body);
    foreach (var choice in result.Choices)
    {
        foreach (var message in choice.Messages)
        {
            if (message.Role == MessageRole.Assistant)
            {
                Console.WriteLine($"{message.Role}: {message.Content}");
            }
        }
    }
}
