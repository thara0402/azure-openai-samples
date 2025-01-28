using Microsoft.Azure.Functions.Worker;
using Microsoft.Azure.Functions.Worker.Extensions.OpenAI.TextCompletion;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using pdf_summarizer.Models;

namespace pdf_summarizer.Agents
{
    internal class SummarizeTextAgent(IOptions<MySettings> optionsAccessor)
    {
        protected readonly MySettings _settings = optionsAccessor.Value;

        [Function(nameof(SummarizeTextAgent))]
        public string RunActivity(
            [ActivityTrigger] string text,
            [TextCompletionInput("In one or two sentences, summarize the following in Japanese: {text}", Model = "%CHAT_MODEL_DEPLOYMENT_NAME%")] TextCompletionResponse response,
            FunctionContext executionContext)
        {
            var logger = executionContext.GetLogger("SummarizeText");
            logger.LogInformation("Summarizing Text to {text}.", text);
            return response.Content;
        }
    }
}
