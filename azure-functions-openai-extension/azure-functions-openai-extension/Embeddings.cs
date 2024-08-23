using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Azure.Functions.Worker;
using Microsoft.Azure.Functions.Worker.Extensions.OpenAI.Embeddings;
using Microsoft.Azure.Functions.Worker.Extensions.OpenAI.Search;
using Microsoft.Azure.Functions.Worker.Http;
using Microsoft.Extensions.Logging;
using System.Net;

namespace azure_functions_openai_extension
{
    public class Embeddings
    {
        private readonly ILogger<Embeddings> _logger;

        public Embeddings(ILogger<Embeddings> logger)
        {
            _logger = logger;
        }

        [Function(nameof(GenerateEmbeddings))]
        public async Task GenerateEmbeddings(
            [HttpTrigger(AuthorizationLevel.Function, "post", Route = "embeddings/generate")] HttpRequestData req,
            [EmbeddingsInput("{rawText}", InputType.RawText, Model = "%EMBEDDING_MODEL_DEPLOYMENT_NAME%")] EmbeddingsContext embeddings)
        {
            _logger.LogInformation("Embeddings input binding function processed a request.");

            var requestBody = await req.ReadFromJsonAsync<GenerateEmbeddingsRequest>();
            if (requestBody?.RawText == null)
            {
                throw new ArgumentException("Invalid request body. Make sure that you pass in {\"RawText\": value } as the request body.");
            }

            _logger.LogInformation(
                "Received {count} embedding(s) for input text containing {length} characters.",
                embeddings.Count,
                requestBody?.RawText?.Length);

            // TODO: Store the embeddings into a database or other storage.
        }

        [Function(nameof(IngestEmbeddings))]
        public async Task<EmbeddingsStoreOutputResponse> IngestEmbeddings(
            [HttpTrigger(AuthorizationLevel.Function, "post", Route = "embeddings/ingest")] HttpRequestData req)
        {
            _logger.LogInformation("Embeddings store output binding function processed a request.");

            var requestBody = await req.ReadFromJsonAsync<IngestEmbeddingsRequest>();
            if (requestBody?.RawText == null || requestBody?.Title == null)
            {
                throw new ArgumentException("Invalid request body. Make sure that you pass in {\"RawText\": value, \"Title\": value} as the request body.");
            }

            IActionResult result = new OkObjectResult(new { status = HttpStatusCode.OK });
            return new EmbeddingsStoreOutputResponse
            {
                HttpResponse = result,
                SearchableDocument = new SearchableDocument(requestBody.Title)
            };
        }

        [Function(nameof(SemanticSearch))]
        public IActionResult SemanticSearch(
            [HttpTrigger(AuthorizationLevel.Function, "post", Route = "embeddings/search")] SemanticSearchRequest unused,
            [SemanticSearchInput("AISearchEndpoint", "openai-index", Query = "{Prompt}", ChatModel = "%CHAT_MODEL_DEPLOYMENT_NAME%", EmbeddingsModel = "%EMBEDDING_MODEL_DEPLOYMENT_NAME%")] SemanticSearchContext result)
        {
            _logger.LogInformation("Semantic search input binding function processed a request.");

            return new ContentResult { Content = result.Response, ContentType = "text/plain" };
        }

        public class SemanticSearchRequest
        {
            public string? Prompt { get; set; }
        }

        internal class IngestEmbeddingsRequest
        {
            public string? RawText { get; set; }

            public string? Title { get; set; }
        }

        public class EmbeddingsStoreOutputResponse
        {
            [EmbeddingsStoreOutput("{RawText}", InputType.RawText, "AISearchEndpoint", "openai-index", Model = "%EMBEDDING_MODEL_DEPLOYMENT_NAME%")]
            public required SearchableDocument SearchableDocument { get; init; }

            public IActionResult? HttpResponse { get; set; }
        }

        internal class GenerateEmbeddingsRequest
        {
            public string? RawText { get; set; }
        }
    }
}
