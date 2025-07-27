# Travel Concierge
This repository is a sample application for implementing the Orchestrator-Workers pattern introduced in Anthropic's blog "[Building effective agents](https://www.anthropic.com/research/building-effective-agents)" using Azure Functions MCP extension.

## Sequence Diagram
Case of synchronous endpoint:

```mermaid
sequenceDiagram
    participant Client
    participant Starter
    participant OrchestratorWorker (MCP Client)
    participant LLM
    participant Agents (MCP Server)

    Client->>Starter: Sends a Web API request
    Starter->>OrchestratorWorker (MCP Client): Initiates the process
    OrchestratorWorker (MCP Client) ->> Agents (MCP Server): Requests a list of available tools from the server
    Agents (MCP Server) ->> OrchestratorWorker (MCP Client): Returns the list of available tools
    OrchestratorWorker (MCP Client) ->> LLM: Sends chat messages
    LLM ->> OrchestratorWorker (MCP Client): Returns the response
    OrchestratorWorker (MCP Client) ->> Agents (MCP Server): Calls tools
    Agents (MCP Server) ->> OrchestratorWorker (MCP Client): Returns the results of the tool calls
    OrchestratorWorker (MCP Client) ->> LLM: Sends tool results for final completion
    LLM ->> OrchestratorWorker (MCP Client): Returns the final user-facing message
    OrchestratorWorker (MCP Client) ->> Starter: Sends the answer
    Starter ->> Client: Returns the Web API response
```

## API Server
### Preparation
local.settings.json
```json
{
  "IsEncrypted": false,
  "Values": {
    "AzureWebJobsStorage": "UseDevelopmentStorage=true",
    "FUNCTIONS_WORKER_RUNTIME": "dotnet-isolated"
  }
}
```

secret.json
```json
{
  "Function": {
    "AzureOpenAIEndpoint": "https://xxx-xxx-eastus2.cognitiveservices.azure.com/",
    "AzureOpenAIApiKey": "xxx",
    "ModelDeploymentName": "gpt-4o",
    "MCPServerEndpoint": "http://localhost:xxxx/runtime/webhooks/mcp/sse",
    "MCPExtensionSystemKey": "xxx"
  }
}
```
### Azurite
```shell
$ docker run --rm -it -p 10000:10000 -p 10001:10001 -p 10002:10002 -v c:/azurite:/data mcr.microsoft.com/azure-storage/azurite:3.33.0
```

## Chat Client
### Create an environment using venv
```shell-session
$ cd chat-client
$ python -m venv .venv
```

### Activate environment
```shell-session
$ .venv\Scripts\activate.bat
```

### Install Streamlit in environment
```shell-session
$ pip install streamlit openai requests
```

### Run Streamlit app
```shell-session
$ streamlit run app.py
```

### Return to normal shell
```shell-session
$ deactivate
```
