using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace aoai_on_your_data_console_app
{
    internal class AzureOpenAISettings
    {
        public required string CognitiveSearchEndpoint { get; set; }
        public required string CognitiveSearchKey { get; set; }
        public required string CognitiveSearchIndexName { get; set; }

        public required string Endpoint { get; set; }
        public required string ApiVersion { get; set; }
        public required string ApiKey { get; set; }
        public required string DeploymentId { get; set; }
    }
}
