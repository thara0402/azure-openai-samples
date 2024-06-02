using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace prompty_console_app
{
    internal class AzureOpenAISettings
    {
        public required string Endpoint { get; set; }
        public required string ApiKey { get; set; }
        public required string DeploymentName { get; set; }
    }
}
