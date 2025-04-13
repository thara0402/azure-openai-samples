# 旅行コンシェルジュ

## API Server
### Preparation
local.settings.json
```json
{
  "IsEncrypted": false,
  "Values": {
    "AzureWebJobsStorage": "UseDevelopmentStorage=true",
    "FUNCTIONS_WORKER_RUNTIME": "dotnet-isolated",
    "DURABLE_TASK_SCHEDULER_CONNECTION_STRING": "Endpoint=http://localhost:8080;Authentication=None",
    "TASKHUB_NAME": "default"
  }
}
```

secret.json
```json
{
  "Function": {
    "AzureOpenAIEndpoint": "https://xxx-xxx-eastus2.cognitiveservices.azure.com/",
    "AzureOpenAIApiKey": "xxx",
    "ModelDeploymentName": "gpt-4o"
  }
}
```

### Set up durable task scheduler emulator
```shell-session
$ docker run --rm -it -p 8080:8080 -p 8082:8082 mcr.microsoft.com/dts/dts-emulator:v0.0.5
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
