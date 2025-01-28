using Microsoft.Azure.Functions.Worker;
using Microsoft.DurableTask.Client;
using Microsoft.Extensions.Logging;
using Microsoft.DurableTask;
using Azure.Storage.Blobs;
using pdf_summarizer.Models;

namespace pdf_summarizer
{
    public static class PdfSummarizer
    {
        [Function(nameof(PdfSummarizer))]
        public static async Task RunOrchestrator(
            [OrchestrationTrigger] TaskOrchestrationContext context)
        {
            var logger = context.CreateReplaySafeLogger(nameof(PdfSummarizer));
            logger.LogInformation("Started Pdf Summarizer.");

            var blobUri = context.GetInput<string>();
            if (string.IsNullOrEmpty(blobUri))
            {
                throw new ArgumentNullException(nameof(blobUri), "Blob URI cannot be null or empty.");
            }
  
            var options = TaskOptions.FromRetryPolicy(new RetryPolicy(
                maxNumberOfAttempts: 3,
                firstRetryInterval: TimeSpan.FromSeconds(5)));

            // PDF からテキストを抽出
            var analyzeResult = await context.CallAnalyzePdfAgentAsync(blobUri, options);

            // テキストを日本語で要約
            var summarizeResult = await context.CallSummarizeTextAgentAsync(analyzeResult, options);

            // 要約結果をテキストファイルとしてアップロード
            var parameter = new UploadDocumentParameter
            {
                FileName = GetFileNameWithoutExtension(blobUri),  // URI から拡張子を除いたファイル名
                Content = summarizeResult
            };
            var uploadResult = await context.CallUploadDocumentAgentAsync(parameter, options);
        }

        [Function(nameof(BlobTrigger))]
        public static async Task BlobTrigger(
            [BlobTrigger("samples-workitems/{name}", Connection = "StorageBindingConnection")] BlobClient blobClient, string name,
            [DurableClient] DurableTaskClient client,
            FunctionContext executionContext)
        {
            var logger = executionContext.GetLogger(nameof(BlobTrigger));
            var uri = blobClient.Uri;
            logger.LogInformation($"Blob trigger function Processed blob\n Name: {name} \n URL: {uri}");

            var instanceId = await client.ScheduleNewOrchestrationInstanceAsync(nameof(PdfSummarizer), uri);

            logger.LogInformation("Started orchestration with ID = '{instanceId}'.", instanceId);
        }

        private static string GetFileNameWithoutExtension(string url)
        {
            var uri = new Uri(url);
            var fileName = Path.GetFileName(uri.LocalPath);
            return Path.GetFileNameWithoutExtension(fileName);
        }
    }
}
