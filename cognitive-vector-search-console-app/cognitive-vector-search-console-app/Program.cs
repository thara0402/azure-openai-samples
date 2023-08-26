using Azure.AI.OpenAI;
using Azure.Search.Documents.Indexes;
using Azure;
using cognitive_vector_search_console_app;
using Microsoft.Extensions.Configuration;
using Azure.Search.Documents.Indexes.Models;
using Azure.Search.Documents.Models;
using Azure.Search.Documents;
using System.Text.Json;

var settings = new ConfigurationBuilder()
    .AddUserSecrets<Program>()
    .Build()
    .GetSection(nameof(AzureOpenAISettings)).Get<AzureOpenAISettings>() ?? throw new NullReferenceException();

var openAIClient = new OpenAIClient(new Uri(settings.Endpoint), new AzureKeyCredential(settings.ApiKey));
var indexClient = new SearchIndexClient(new Uri(settings.CognitiveSearchEndpoint), new AzureKeyCredential(settings.CognitiveSearchKey));
var searchClient = indexClient.GetSearchClient(settings.CognitiveSearchIndexName);

Console.Write("Would you like to index (y/n)? ");
var indexChoice = Console.ReadLine()?.ToLower() ?? string.Empty;
if (indexChoice == "y")
{
    await indexClient.CreateOrUpdateIndexAsync(CreateSearchIndex(settings.CognitiveSearchIndexName));
    await UploadDocumentAsync(settings.DeploymentOrModelName, searchClient, openAIClient);
}
Console.WriteLine("");

var inputQuery = "暑い日に食べられている京菓子は？";
Console.WriteLine($"Query: {inputQuery}\n");

Console.WriteLine("Choose a query approach:");
Console.WriteLine("1. Vector Search");
Console.WriteLine("2. Keyword Search");
Console.WriteLine("3. Hybrid Search");
Console.Write("Enter the number of the desired approach: ");
var choice = int.Parse(Console.ReadLine() ?? "0");
Console.WriteLine("");

switch (choice)
{
    case 1:
        await VectorSearchAsync(searchClient, openAIClient, settings.DeploymentOrModelName, inputQuery);
        break;
    case 2:
        await KeywordSearchAsync(searchClient, inputQuery);
        break;
    case 3:
        await HybridSearchAsync(searchClient, openAIClient, settings.DeploymentOrModelName, inputQuery);
        break;
    default:
        Console.WriteLine("Invalid choice. Exiting...");
        break;
}


static async Task VectorSearchAsync(SearchClient searchClient, OpenAIClient openAIClient, string deploymentOrModelName, string query)
{
    var queryEmbeddings = await GetEmbeddingsAsync(deploymentOrModelName, query, openAIClient);

    var searchOptions = new SearchOptions
    {
        Vectors = { new SearchQueryVector { Value = queryEmbeddings.ToArray(), KNearestNeighborsCount = 3, Fields = { "contentVector" } } },
        Size = 3,
        Select = { "title", "content", "category" },
    };
    SearchResults<SearchDocument> response = await searchClient.SearchAsync<SearchDocument>(null, searchOptions);

    var count = 0;
    await foreach (var result in response.GetResultsAsync())
    {
        count++;
        Console.WriteLine($"Title: {result.Document["title"]}");
        Console.WriteLine($"Score: {result.Score}");
        Console.WriteLine($"Content: {result.Document["content"]}");
        Console.WriteLine($"Category: {result.Document["category"]}\n");
    }
    Console.WriteLine($"Total Results: {count}");
}

static async Task KeywordSearchAsync(SearchClient searchClient, string query)
{
    var searchOptions = new SearchOptions
    {
        Size = 3,
        Select = { "title", "content", "category" },
    };
    SearchResults<SearchDocument> response = await searchClient.SearchAsync<SearchDocument>(query, searchOptions);

    var count = 0;
    await foreach (var result in response.GetResultsAsync())
    {
        count++;
        Console.WriteLine($"Title: {result.Document["title"]}");
        Console.WriteLine($"Score: {result.Score}");
        Console.WriteLine($"Content: {result.Document["content"]}");
        Console.WriteLine($"Category: {result.Document["category"]}\n");
    }
    Console.WriteLine($"Total Results: {count}");
}

static async Task HybridSearchAsync(SearchClient searchClient, OpenAIClient openAIClient, string deploymentOrModelName, string query)
{
    var queryEmbeddings = await GetEmbeddingsAsync(deploymentOrModelName, query, openAIClient);

    var searchOptions = new SearchOptions
    {
        Vectors = { new SearchQueryVector { Value = queryEmbeddings.ToArray(), KNearestNeighborsCount = 3, Fields = { "contentVector" } } },
        Size = 10,
        Select = { "title", "content", "category" },
    };
    SearchResults<SearchDocument> response = await searchClient.SearchAsync<SearchDocument>(query, searchOptions);

    var count = 0;
    await foreach (var result in response.GetResultsAsync())
    {
        count++;
        Console.WriteLine($"Title: {result.Document["title"]}");
        Console.WriteLine($"Score: {result.Score}");
        Console.WriteLine($"Content: {result.Document["content"]}");
        Console.WriteLine($"Category: {result.Document["category"]}\n");
    }
    Console.WriteLine($"Total Results: {count}");
}

static async Task UploadDocumentAsync(string deploymentOrModelName, SearchClient searchClient, OpenAIClient openAIClient)
{
    var json = File.ReadAllText("./kyogashi.json");
    var sampleDocuments = new List<SearchDocument>();
    foreach (var document in JsonSerializer.Deserialize<List<Dictionary<string, object>>>(json) ?? new List<Dictionary<string, object>>())
    {
        var content = document["content"]?.ToString() ?? string.Empty;
        document["contentVector"] = (await GetEmbeddingsAsync(deploymentOrModelName, content, openAIClient)).ToArray();
        sampleDocuments.Add(new SearchDocument(document));
    }
    await searchClient.IndexDocumentsAsync(IndexDocumentsBatch.Upload(sampleDocuments));
}

static async Task<IReadOnlyList<float>> GetEmbeddingsAsync(string deploymentOrModelName, string text, OpenAIClient openAIClient)
{
    var response = await openAIClient.GetEmbeddingsAsync(deploymentOrModelName, new EmbeddingsOptions(text));
    return response.Value.Data[0].Embedding;
}

static SearchIndex CreateSearchIndex(string indexName)
{
    var vectorSearchConfigName = "vector-config";
    var modelDimensions = 1536;

    return new SearchIndex(indexName)
    {
        VectorSearch = new VectorSearch
        {
            AlgorithmConfigurations =
            {
                new HnswVectorSearchAlgorithmConfiguration(vectorSearchConfigName)
            }
        },
        Fields =
        {
            new SimpleField("id", SearchFieldDataType.String) { IsKey = true, IsFilterable = true, IsSortable = true, IsFacetable = true },
            new SearchableField("title") { IsFilterable = true, IsSortable = true, AnalyzerName = LexicalAnalyzerName.JaLucene },
            new SearchableField("content") { IsFilterable = true, AnalyzerName = LexicalAnalyzerName.JaLucene },
            new SearchField("contentVector", SearchFieldDataType.Collection(SearchFieldDataType.Single))
            {
                IsSearchable = true,
                VectorSearchDimensions = modelDimensions,
                VectorSearchConfiguration = vectorSearchConfigName
            },
            new SearchableField("category") { IsFilterable = true, IsSortable = true, IsFacetable = true, AnalyzerName = LexicalAnalyzerName.JaLucene }
        }
    };
}
