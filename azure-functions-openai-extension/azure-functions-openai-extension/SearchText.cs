using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Azure.Functions.Worker;
using Microsoft.Azure.Functions.Worker.Extensions.OpenAI.TextCompletion;
using Microsoft.Azure.Functions.Worker.Http;
using Microsoft.Extensions.Logging;

namespace azure_functions_openai_extension
{
    public class SearchText
    {
        private readonly ILogger<SearchText> _logger;

        public SearchText(ILogger<SearchText> logger)
        {
            _logger = logger;
        }

        [Function(nameof(SearchText))]
        public IActionResult Run(
            [HttpTrigger(AuthorizationLevel.Function, "get", Route = "completion/{text}")] HttpRequest req,
            [TextCompletionInput("{text}", Model = "%CHAT_MODEL_DEPLOYMENT_NAME%")] TextCompletionResponse response)
        {
            _logger.LogInformation("Text completion input binding function processed a request.");

            return new OkObjectResult(response.Content);
        }
    }
}
