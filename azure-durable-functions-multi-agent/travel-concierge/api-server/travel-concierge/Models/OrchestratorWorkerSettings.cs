
namespace travel_concierge.Models
{
    internal class OrchestratorWorkerSettings
    {
        public required string AzureOpenAIEndpoint { get; set; }
        public required string AzureOpenAIApiKey { get; set; }
        public required string ModelDeploymentName { get; set; }
    }
}
