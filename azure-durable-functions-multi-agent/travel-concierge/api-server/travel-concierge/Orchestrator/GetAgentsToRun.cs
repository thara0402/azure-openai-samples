using Azure.AI.OpenAI;
using Microsoft.Azure.Functions.Worker;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using OpenAI.Chat;
using System.Text.Json;
using travel_concierge.Agents;
using travel_concierge.Models;

namespace travel_concierge.Orchestrator
{
    internal class GetAgentsToRun(AzureOpenAIClient openAIClient, IOptions<OrchestratorWorkerSettings> settings)
    {
        private readonly AzureOpenAIClient _openAIClient = openAIClient;
        private readonly OrchestratorWorkerSettings _settings = settings.Value;

        [Function(nameof(GetAgentsToRun))]
        public async Task<AgentsToRun> RunActivityAsync([ActivityTrigger] Prompt prompt, FunctionContext executionContext)
        {
            var logger = executionContext.GetLogger(nameof(GetAgentsToRun));
            logger.LogInformation("Run GetAgentsToRun Activity.");

            var messages = prompt.Messages.ConvertToChatMessageArray();
            ChatMessage[] allMessages = [
                new SystemChatMessage(GetAgentsToRunPrompt.SystemPrompt),
                .. messages,
            ];
            var options = new ChatCompletionOptions
            {
                Tools = {
                    AgentDefinition.GetDestinationSuggestAgent,
                    AgentDefinition.GetClimateAgent,
                    AgentDefinition.GetSightseeingSpotAgent,
                    AgentDefinition.GetHotelAgent,
                    AgentDefinition.SubmitReservationAgent
                }
            };

            var chatClient = _openAIClient.GetChatClient(_settings.ModelDeploymentName);
            var chatResult = await chatClient.CompleteChatAsync(allMessages, options);
            if (chatResult.Value.FinishReason == ChatFinishReason.ToolCalls)
            {
                var result = new AgentsToRun
                {
                    IsAgentCall = true,
                    AgentCalls = chatResult.Value.ToolCalls.Select(toolCall => new AgentCall
                    {
                        AgentName = toolCall.FunctionName,
                        Arguments = JsonDocument.Parse(toolCall.FunctionArguments)
                    }).ToArray()
                };
                return result;
            }
            else
            {
                return new AgentsToRun
                {
                    IsAgentCall = false,
                    Content = chatResult.Value.Content.First().Text
                };
            }
        }
    }

    internal static class GetAgentsToRunPrompt
    {
        public const string SystemPrompt = """
        あなたは、人々が情報を見つけるのを助ける 旅行 AI アシスタントです。
        アシスタントとして、ユーザーからの問いについて必要なツールを選択してください。
        あなたの知識にないことや、使えるツールがない場合は「わかりません」と答えてください。
        使えるツールがあるが、情報が足りない時はユーザーにその情報を質問してください。
        また、旅行以外の話題については答えないでください。
        """;
    }
}
