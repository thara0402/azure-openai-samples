using Microsoft.Azure.Functions.Worker;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using pdf_summarizer.Models;

namespace pdf_summarizer.Agents
{
    internal class UploadDocumentAgent(IOptions<MySettings> optionsAccessor)
    {
        protected readonly MySettings _settings = optionsAccessor.Value;

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