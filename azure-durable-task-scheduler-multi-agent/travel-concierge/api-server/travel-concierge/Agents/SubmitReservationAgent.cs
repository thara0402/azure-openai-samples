using Azure.AI.OpenAI;
using Microsoft.Azure.Functions.Worker;
using Microsoft.Extensions.Options;
using travel_concierge.Models;

namespace travel_concierge.Agents
{
    internal class SubmitReservationAgent(AzureOpenAIClient openAIClient, IOptions<OrchestratorWorkerSettings> settings)
    {
        private readonly AzureOpenAIClient _openAIClient = openAIClient;
        private readonly OrchestratorWorkerSettings _settings = settings.Value;

        [Function(nameof(SubmitReservationAgent))]
        public string Run([ActivityTrigger] SubmitReservationParameter req, FunctionContext executionContext)
        {
            // This is sample code. Replace this with your own logic.
            var result = $"""
            予約番号は {Guid.NewGuid()} です。
            --------------------------------
            ホテル名：{req.Destination}
            チェックイン日：{req.CheckIn}
            チェックアウト日：{req.CheckOut}
            人数：{req.GuestsCount} 名
            --------------------------------
            """;

            return result;
        }
    }
}
