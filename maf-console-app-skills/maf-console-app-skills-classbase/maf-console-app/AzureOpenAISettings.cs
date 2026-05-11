using System;
using System.Collections.Generic;
using System.Text;

namespace maf_console_app
{
    internal class AzureOpenAISettings
    {
        public required string Endpoint { get; set; }
        public required string ApiKey { get; set; }
        public required string DeploymentName { get; set; }
    }
}
