using Azure.AI.OpenAI;
using Microsoft.Azure.Functions.Worker;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using OpenAI.Chat;
using travel_concierge.Models;

namespace travel_concierge.Synthesizer
{
    internal class SynthesizeAgentCallResults(AzureOpenAIClient openAIClient, IOptions<OrchestratorWorkerSettings> settings)
    {
        private readonly AzureOpenAIClient _openAIClient = openAIClient;
        private readonly OrchestratorWorkerSettings _settings = settings.Value;

        [Function(nameof(SynthesizeAgentCallResults))]
        public async Task<SynthesizedResult> RunActivityAsync(
            [ActivityTrigger] AgentCallResults agentCallResults, FunctionContext executionContext)
        {
            var logger = executionContext.GetLogger(nameof(SynthesizeAgentCallResults));
            logger.LogInformation("Run SynthesizeAgentCallResults Activity.");

            var systemMessageTemplate = SynthesizerPrompt.SystemPrompt;
            var systemMessage = $"{systemMessageTemplate}¥n{string.Join("¥n", agentCallResults.Results)}";

            ChatMessage[] allMessages = [
                new SystemChatMessage(systemMessage),
                .. agentCallResults.Prompt.Messages.ConvertToChatMessageArray(),
            ];

            var chatClient = _openAIClient.GetChatClient(_settings.ModelDeploymentName);
            var chatResult = await chatClient.CompleteChatAsync(allMessages);

            return new SynthesizedResult
            {
                Content = chatResult.Value.Content.First().Text,
                CalledAgentNames = agentCallResults.CalledAgentNames
            };
        }
    }

    internal static class SynthesizerPrompt
    {
        // Orchestrator Agent functions
        public const string SystemPrompt = """
        あなたは、ユーザーの質問に対して答えを作成する役割を持っています。
        - ユーザーの質問に対して**以下の参考情報のみを用いて**回答を生成してください。
        - 一部分のみ回答できる場合はその部分のみ回答してください。
        - 質問内容に対して全く情報がない場合は「情報がありません」と回答してください。
        - 回答は見やすく簡潔に。Markdown形式で記述することができます。
        # 参考情報
        """;
    }
}
