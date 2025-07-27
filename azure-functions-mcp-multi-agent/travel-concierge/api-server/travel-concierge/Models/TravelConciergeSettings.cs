
namespace travel_concierge.Models
{
    public class TravelConciergeSettings
    {
        public required string AzureOpenAIEndpoint { get; set; }
        public required string AzureOpenAIApiKey { get; set; }
        public required string ModelDeploymentName { get; set; }
        public required string MCPServerEndpoint { get; set; }
        public required string MCPServerApiKey { get; set; }
    }
}
