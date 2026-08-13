# Azure Functions Agents Runtime

## Run Locally

### Create local settings
local.settings.json
```json
{
  "IsEncrypted": false,
  "Values": {
    "FUNCTIONS_WORKER_RUNTIME": "python",
    "AzureWebJobsStorage": "UseDevelopmentStorage=true",
    "ACA_SESSION_POOL_ENDPOINT": "",
    "AZURE_FUNCTIONS_AGENTS_PROVIDER": "foundry",
    "FOUNDRY_PROJECT_ENDPOINT": "https://<foundry_name>.services.ai.azure.com/api/projects/<project_name>",
    "FOUNDRY_MODEL": "gpt-5.4"
  }
}
```

### Azure credentials with access to a Microsoft Foundry project
```shell-session
$ az login --tenant <tenant_id>
```

### Start Azurite (local storage emulator)
```shell-session
$ docker run --rm -it -p 10000:10000 -p 10001:10001 -p 10002:10002 -v c:/azurite:/data mcr.microsoft.com/azure-storage/azurite:3.35.0 azurite --skipApiVersionCheck  --blobHost 0.0.0.0 --queueHost 0.0.0.0 --tableHost 0.0.0.0
```

### Install dependencies
```shell-session
$ cd src
$ python -m venv .venv
$ .venv\Scripts\activate.bat
$ pip install -r requirements.txt
```

### Run locally
```shell-session
$ func start
```

### Return to normal shell
```shell-session
$ deactivate
```

## Deploy
### Set environment variables
```shell-session
$ cd basic-chat
$ az login
$ azd auth login
$ azd env new gooner0813
$ azd env set AZURE_LOCATION eastus2
```
### Deploy to Azure
```shell-session
$ azd up
```
### Clean up resources
```shell-session
$ azd down
```
