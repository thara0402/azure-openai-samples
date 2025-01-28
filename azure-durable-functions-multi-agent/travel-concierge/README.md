# 旅行コンシェルジュ

## API Server
### 前準備
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
}
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
