using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Azure.Functions.Worker;
using Microsoft.Azure.Functions.Worker.Extensions.OpenAI.TextCompletion;
using Microsoft.Azure.Functions.Worker.Http;
using Microsoft.Extensions.Logging;

namespace azure_functions_openai_extension
{
    public class Text
    {
        private readonly ILogger<Text> _logger;

        public Text(ILogger<Text> logger)
        {
            _logger = logger;
        }

        [Function(nameof(GenerateText))]
        public IActionResult GenerateText(
            [HttpTrigger(AuthorizationLevel.Function, "get", Route = "text/{prompt}")] HttpRequest req,
            [TextCompletionInput("{prompt}", Model = "%CHAT_MODEL_DEPLOYMENT_NAME%")] TextCompletionResponse response)
        {
            _logger.LogInformation("Text completion input binding function processed a request.");

            return new OkObjectResult(response.Content);
        }
    }
}
