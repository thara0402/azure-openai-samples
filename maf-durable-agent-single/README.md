# The durable task extension for Microsoft Agent Framework

## Preparation
local.settings.json
```json
{
  "IsEncrypted": false,
  "Values": {
    "AzureWebJobsStorage": "UseDevelopmentStorage=true",
    "FUNCTIONS_WORKER_RUNTIME": "dotnet-isolated",
    "DURABLE_TASK_SCHEDULER_CONNECTION_STRING": "Endpoint=http://localhost:8080;Authentication=None",
    "TASKHUB_NAME": "default",
    "AZURE_OPENAI_ENDPOINT": "<AZURE_OPENAI_ENDPOINT>",
    "AZURE_OPENAI_DEPLOYMENT_NAME": "<AZURE_OPENAI_DEPLOYMENT_NAME>",
    "AZURE_OPENAI_KEY": "<AZURE_OPENAI_KEY>"
  }
}
```

## Set up
### Azurite
```shell
$ docker run --rm -it -p 10000:10000 -p 10001:10001 -p 10002:10002 -v c:/azurite:/data mcr.microsoft.com/azure-storage/azurite:3.35.0
```

### Durable task scheduler emulator
```shell
$ docker run --rm -it -p 8080:8080 -p 8082:8082 mcr.microsoft.com/dts/dts-emulator:v0.0.9
```
