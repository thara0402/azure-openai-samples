using Microsoft.Extensions.AI;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using ModelContextProtocol.Client;
using travel_concierge.Models;

namespace travel_concierge.Orchestrator
{
    public class OrchestratorWorker(IChatClient chatClient, IOptions<TravelConciergeSettings> settings, ILogger<OrchestratorWorker> logger) : IOrchestratorWorker
    {
        private readonly IChatClient _chatClient = chatClient;
        private readonly TravelConciergeSettings _settings = settings.Value;
        private readonly ILogger<OrchestratorWorker> _logger = logger;

        public async Task<OrchestratorWorkerResult> RunOrchestrator(Prompt prompt)
        {
            var messages = prompt.Messages.ConvertToChatMessageArray();
            ChatMessage[] allMessages = [
                new ChatMessage(ChatRole.System, OrchestratorWorkerPrompt.SystemPrompt),
                .. messages,
            ];

            // Create the MCP client
            // Configure it to start and connect to MCP server.
            var mcpClient = await CreateMcpClientAsync();

            // List all available tools from the MCP server.
            _logger.LogInformation("Available tools:");
            var tools = await mcpClient.ListToolsAsync();
            foreach (var tool in tools)
            {
                _logger.LogInformation("Tool: {ToolName} - {ToolDescription}", tool.Name, tool.Description);
            }

            // Conversation that can utilize the tools via prompts.
            var response = await _chatClient.GetResponseAsync(allMessages, new() { Tools = [.. tools] });
            return new OrchestratorWorkerResult
            {
                Content = response.Text,
                CalledAgentNames = ExtractFunctionCallNames(response.Messages)
            };
        }

        private async Task<IMcpClient> CreateMcpClientAsync()
        {
            var options = new SseClientTransportOptions
            {
                Endpoint = new Uri(_settings.MCPServerEndpoint),
                TransportMode = HttpTransportMode.Sse,
                Name = "Travel Concierge MCP Server",
                AdditionalHeaders = new Dictionary<string, string>
                {
                    {"x-functions-key", _settings.MCPExtensionSystemKey}
                }
            };
            var transport = new SseClientTransport(options);
            return await McpClientFactory.CreateAsync(transport);
        }

        private List<string> ExtractFunctionCallNames(IList<ChatMessage> messages)
        {
            return messages
                .Where(m => m.Role == ChatRole.Assistant && m.Contents != null && m.Contents.All(c => c is FunctionCallContent))
                .SelectMany(m => m.Contents.OfType<FunctionCallContent>().Select(c => c.Name))
                .ToList();
        }
    }
}

internal static class OrchestratorWorkerPrompt
{
    public const string SystemPrompt = """
        あなたは、人々が情報を見つけるのを助ける 旅行 AI アシスタントです。
        アシスタントとして、ユーザーからの問いについて必要なツールを利用して回答してください。
        あなたの知識にないことや、使えるツールがない場合は「わかりません」と答えてください。
        使えるツールがあるが、情報が足りない時はユーザーにその情報を質問してください。
        また、旅行以外の話題については答えないでください。
        """;
}

public interface IOrchestratorWorker
{
    Task<OrchestratorWorkerResult> RunOrchestrator(Prompt prompt);
}
