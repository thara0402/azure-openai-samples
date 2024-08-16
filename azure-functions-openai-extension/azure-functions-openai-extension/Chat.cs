using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Azure.Functions.Worker;
using Microsoft.Azure.Functions.Worker.Extensions.OpenAI.Assistants;
using Microsoft.Azure.Functions.Worker.Http;
using Microsoft.Extensions.Logging;

namespace azure_functions_openai_extension
{
    public class Chat
    {
        private readonly ILogger<Chat> _logger;

        public Chat(ILogger<Chat> logger)
        {
            _logger = logger;
        }

        [Function(nameof(CreateAssistant))]
        public CreateAssistantOutput CreateAssistant(
            [HttpTrigger(AuthorizationLevel.Function, "put", Route = "chat/{assistantId}")] HttpRequest req,
            string assistantId)
        {
            _logger.LogInformation("Assistant create output binding function processed a request.");

            var responseJson = new { assistantId };
            var instructions = """
                You are an AI assistant that helps people find information.
                """;

            return new CreateAssistantOutput
            {
                HttpResponse = new OkObjectResult(responseJson) { StatusCode = 201 },
                AssistantCreateRequest = new AssistantCreateRequest(assistantId, instructions)
            };
        }

        [Function(nameof(PostUserMessage))]
        public IActionResult PostUserMessage(
            [HttpTrigger(AuthorizationLevel.Anonymous, "post", Route = "chat/{assistantId}")] HttpRequestData req,
            string assistantId,
            [AssistantPostInput("{assistantId}", "{userMessage}", Model = "%CHAT_MODEL_DEPLOYMENT_NAME%")] AssistantState state)
        {
            _logger.LogInformation("Assistant post input binding function processed a request.");

            return new OkObjectResult(state.RecentMessages.LastOrDefault()?.Content ?? "No response returned.");
        }

        [Function(nameof(GetChatState))]
        public IActionResult GetChatState(
           [HttpTrigger(AuthorizationLevel.Anonymous, "get", Route = "chat/{assistantId}")] HttpRequestData req,
           string assistantId,
           [AssistantQueryInput("{assistantId}", TimestampUtc = "{timestampUtc}")] AssistantState state)
        {
            _logger.LogInformation("Assistant query input binding function processed a request.");

            return new OkObjectResult(state);
        }

        public class CreateAssistantOutput
        {
            [AssistantCreateOutput()]
            public AssistantCreateRequest? AssistantCreateRequest { get; set; }

            [HttpResult]
            public IActionResult? HttpResponse { get; set; }
        }

    }
}
