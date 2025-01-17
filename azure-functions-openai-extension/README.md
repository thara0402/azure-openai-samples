# Azure Functions の Azure OpenAI Extension を使ったサンプルアプリケーション

## 前準備
local.settings.json
```json
{
  "IsEncrypted": false,
  "Values": {
    "AzureWebJobsStorage": "UseDevelopmentStorage=true",
    "FUNCTIONS_WORKER_RUNTIME": "dotnet-isolated",
    "AZURE_OPENAI_ENDPOINT": "https://xxx-xxx-eastus2.cognitiveservices.azure.com/",
    "AZURE_OPENAI_KEY": "xxx",
    "CHAT_MODEL_DEPLOYMENT_NAME": "gpt-4o",
    "EMBEDDING_MODEL_DEPLOYMENT_NAME": "text-embedding-ada-002",
    "AISearchEndpoint": "https://xxx.search.windows.net",
    "SearchAPIKey": "xxx"
  }
}
```

## ユースケース
- [Text completion input binding を使ってコンテンツを要約する](./azure-functions-openai-extension/Text.cs)
- [Chat completion binding を使ってチャットアシスタントを構築する](./azure-functions-openai-extension/Chat.cs)
- [Assistant trigger を使ってチャットアシスタントにカスタムスキルを追加する](./azure-functions-openai-extension/AssistantSkills.cs)
- [Embeddings binding を使ってベクトル検索を構築する](./azure-functions-openai-extension/Embeddings.cs)

