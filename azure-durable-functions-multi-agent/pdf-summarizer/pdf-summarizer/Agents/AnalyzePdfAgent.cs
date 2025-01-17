using Azure.AI.FormRecognizer.DocumentAnalysis;
using Azure;
using Microsoft.Azure.Functions.Worker;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using pdf_summarizer.Models;
using System.Text;

namespace pdf_summarizer.Agents
{
    internal class AnalyzePdfAgent : AgentBase
    {
        public AnalyzePdfAgent(IOptions<MySettings> optionsAccessor) : base(optionsAccessor)
        {
        }

        [Function(nameof(AnalyzePdfAgent))]
        public async Task<string> RunActivityAsync([ActivityTrigger] string blobUri, FunctionContext executionContext)
        {
            var logger = executionContext.GetLogger("AnalyzePdf");
            logger.LogInformation("Analyzing Pdf from {blobUri}.", blobUri);

            var client = new DocumentAnalysisClient(
                new Uri(_settings.DocumentIntelligenceEndpoint),
                new AzureKeyCredential(_settings.DocumentIntelligenceApiKey));

            var operation = await client.AnalyzeDocumentFromUriAsync(WaitUntil.Completed, "prebuilt-read", new Uri(blobUri));

            var result = new StringBuilder();
            foreach (var page in operation.Value.Pages)
            {
                foreach (var line in page.Lines)
                {
                    result.AppendLine(line.Content);
                }
            }
            return result.ToString();
        }
    }
}
