using Microsoft.Azure.Functions.Worker;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using pdf_summarizer.Models;

namespace pdf_summarizer.Agents
{
    internal class UploadDocumentAgent : AgentBase
    {
        public UploadDocumentAgent(IOptions<MySettings> optionsAccessor) : base(optionsAccessor)
        {
        }

        [Function(nameof(UploadDocumentAgent))]
        [BlobOutput("samples-workitems-output/{FileName}.txt", Connection = "StorageBindingConnection")]
        public static string RunActivity(
            [ActivityTrigger] UploadDocumentParameter parameter,
            FunctionContext executionContext)
        {
            var logger = executionContext.GetLogger("UploadDocument");
            logger.LogInformation("Uploading document to {parameter.FileName}.txt.", parameter.FileName);
            return parameter.Content;
        }
    }
}