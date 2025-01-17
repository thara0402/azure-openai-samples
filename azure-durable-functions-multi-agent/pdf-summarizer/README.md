# PDF を日本語で要約する

## 前準備
local.settings.json
```json
{
  "IsEncrypted": false,
  "Values": {
    "AzureWebJobsStorage": "UseDevelopmentStorage=true",
    "FUNCTIONS_WORKER_RUNTIME": "dotnet-isolated",
    "StorageBindingConnection": "xxx",
    "AZURE_OPENAI_ENDPOINT": "https://xxx-xxx-eastus2.cognitiveservices.azure.com/",
    "AZURE_OPENAI_KEY": "xxx",
    "CHAT_MODEL_DEPLOYMENT_NAME": "gpt-4o"
  }
}
```

secret.json
```json
{
  "Function": {
    "DocumentIntelligenceEndpoint": "https://xxx.cognitiveservices.azure.com/",
    "DocumentIntelligenceApiKey": "xxx"
  }
}```

