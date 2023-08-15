using Azure;
using Azure.AI.OpenAI;
using Microsoft.Extensions.Configuration;
using aoai_console_app;

var settings = new ConfigurationBuilder()
    .AddUserSecrets<Program>()
    .Build()
    .GetSection(nameof(AzureOpenAISettings)).Get<AzureOpenAISettings>() ?? throw new NullReferenceException();

var chatCompletionsOptions = new ChatCompletionsOptions
{
    MaxTokens = 200,
    Messages =
        {
            new ChatMessage(ChatRole.System, """
                おれの名前は、トニートニー・チョッパー。
                動物系悪魔の実「ヒトヒトの実」の能力者で、人の言葉を話せるトナカイ。
                海賊「麦わらの一味」の船医。
                夢は、何でも治せる医者になること。
                わたあめ大好き。
                一人称は、おれ。
                口調はフレンドリーで可愛い。
            """)
        }
};

var userMessages = new String[] {
    "こんにちは。",
    "トナカイなのに人の言葉を話せるの？",
    "好きな食べ物は？",
};

Console.WriteLine("アシスタントのセットアップ中・・・");
var client = new OpenAIClient(new Uri(settings.Endpoint), new AzureKeyCredential(settings.ApiKey));
await client.GetChatCompletionsAsync(settings.DeploymentName, chatCompletionsOptions);

Console.WriteLine("チャットを開始する");
foreach (var userMessage in userMessages)
{
    Console.WriteLine($"{ChatRole.User}: {userMessage}");
    chatCompletionsOptions.Messages.Add(new ChatMessage(ChatRole.User, userMessage));
    var result = await client.GetChatCompletionsAsync(settings.DeploymentName, chatCompletionsOptions);

    foreach (var choice in result.Value.Choices)
    {
        Console.WriteLine($"{choice.Message.Role}: {choice.Message.Content}");
        chatCompletionsOptions.Messages.Add(new ChatMessage(choice.Message.Role, choice.Message.Content));
    }
}

