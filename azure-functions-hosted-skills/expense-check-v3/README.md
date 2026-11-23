# Expense Pre-Check API: Tools

This sample builds on [expense-check-v2](../expense-check-v2/) and uses Azure Functions hosted skills and a Microsoft Foundry model for a pre-submission check, not an approval or rejection. The [agent definition](src/expense-check.agent.md) retains the request schema and the [expense-policy skill](src/skills/expense-policy/SKILL.md) as the source of the rules and checking procedure. It adds a [custom Python tool](src/tools/calculate_total.py) that sums all amounts without containing policy rules.

```text
HTTP POST /api/expense-check
  -> HTTP handler and Hosted Skill binding
  -> expense-policy reusable skill + calculate_total Python tool
  -> Microsoft Foundry model
  -> Structured Japanese pre-check findings
```

## Prerequisites

- An Azure subscription and permission to create resources and role assignments.
- [Azure Developer CLI](https://learn.microsoft.com/azure/developer/azure-developer-cli/install-azd) (`azd`) and [Azure CLI](https://learn.microsoft.com/cli/azure/install-azure-cli) (`az`).
- Python 3.13.
- VS Code and the [REST Client extension](https://marketplace.visualstudio.com/items?itemName=humao.rest-client) to test the API.
- [Azure Functions Core Tools 4.x](https://learn.microsoft.com/azure/azure-functions/functions-run-local) and [Docker Desktop](https://www.docker.com/products/docker-desktop/) to run Azurite locally.
- A region that supports Functions Flex Consumption and the model, with sufficient model quota.

Azure Functions hosted skills is in preview. Azure resources and model usage incur charges.

## Deploy to Azure

The following example uses PowerShell. Run it from the repository root, replacing the subscription ID and environment name with your own values.

```powershell
cd azure-functions-hosted-skills\expense-check-v3

az login
azd auth login

$subscriptionId = "<subscription-id>"
az account set --subscription $subscriptionId

azd env new expense-check-v3-demo
azd env set AZURE_SUBSCRIPTION_ID $subscriptionId
azd env set AZURE_LOCATION eastus2

azd provision --preview
azd up
```

`eastus2` is an example. Choose an allowed region from [main.bicep](infra/main.bicep) and check model availability and quota. `azd up` provisions the resources and deploys the app.

The deployment creates these main resources:

- A resource group named `rg-<environment-name>`.
- A Python 3.13 Functions app and a Flex Consumption plan.
- A storage account for Functions and a Blob container for the deployment package.
- A Microsoft Foundry account, project, and model deployment.
- A user-assigned managed identity and required role assignments.
- Application Insights and Log Analytics.

The default model settings are:

| Setting | Default |
|---|---|
| Deployment name `FOUNDRY_MODEL` | `gpt-5.4-mini` |
| Model name `FOUNDRY_MODEL_NAME` | `gpt-5.4-mini` |
| Version `FOUNDRY_MODEL_VERSION` | `2026-03-17` |
| Capacity `FOUNDRY_DEPLOYMENT_CAPACITY` | `200` |

To change a default, use a command such as `azd env set FOUNDRY_MODEL_NAME <model-name>` before `azd up`. Check availability and quota for the chosen model, version, region, and capacity. See [main.parameters.json](infra/main.parameters.json) for the available parameters.

Use `azd deploy` after changing only the app code, or `azd provision` after changing the infrastructure.

## Test the API in Azure

1. In the Azure portal, open the Function App and get a function key from the `expense_check` function. A host key also works, but grants access to all functions in the app.
2. Create a `.env` file in the same `src` folder as [expense-check.http](src/expense-check.http). If it already exists, add or update the values without overwriting the rest of the file.

   ```dotenv
   AZURE_FUNCTION_URL=https://<function-app-name>.azurewebsites.net
   AZURE_FUNCTION_KEY=<function-key>
   ```

   REST Client reads these values for the Azure request. `.env` is excluded from Git and Functions deployment, but it is not encrypted. Do not share or commit the key.
3. Open [expense-check.http](src/expense-check.http) in VS Code and select **Send Request** for the **Azure** request.

The request contains 1–50 expenses. The HTTP handler validates `input_schema` and the [Hosted Skill binding](https://learn.microsoft.com/azure/azure-functions/functions-hosted-skills-bindings) validates structured output against `response_schema`; both schemas reject extra properties. The runtime discovers reusable skills in `src/skills/` and Python tools in `src/tools/`. The agent must pass every amount to `calculate_total`, and `total_amount_jpy` is the integer returned by that tool. The handler checks the binding's tool-call evidence and rejects missing, failed, incomplete, or mismatched calls and mismatched totals with HTTP 502 rather than guessing; malformed requests return HTTP 400. Runtime/model errors are not returned as successful checks.

For the sample request, the total is **6,200 JPY**. The 2,400 JPY meal with a receipt should have no issues; the 3,800 JPY meal should include both an 800 JPY over-limit reason and a missing-receipt reason. Model output can vary; schema validation does not guarantee policy compliance, so review the returned pre-check before submitting an expense. The handler fails closed if tool-call evidence is unavailable from the configured runtime/provider.

Run unit tests from the project directory after installing the dependencies in `src/requirements.txt`. To also run HTTP integration tests against a running local app, set `EXPENSE_CHECK_URL` as below. For an Azure endpoint, use its full `/api/expense-check` URL and set `AZURE_FUNCTION_KEY` to a function key. A successful response also verifies the handler observed the required tool call.

The local test suite also exercises skill loading and tool invocation through the runtime with a scripted model. Its example findings are scripted, not evidence that a real model applied the policy correctly; use the HTTP integration tests to check that behavior with your configured model.

```powershell
$env:EXPENSE_CHECK_URL = "http://localhost:7071/api/expense-check"
python -m unittest discover -s tests -v
```

## Run locally

Local execution also requires a Foundry project and model you can access. If you have deployed to Azure, run the following commands from the project directory to check the settings:

```powershell
az login
```

1. Start Azurite in a separate terminal:

   ```powershell
   docker run --rm -it -p 10000:10000 -p 10001:10001 -p 10002:10002 -v c:/azurite:/data mcr.microsoft.com/azure-storage/azurite:3.38.0
   ```

2. Create `src\local.settings.json` if it does not exist, or update the required values if it does. Replace the endpoint and deployment name with values from your environment.

   ```json
   {
     "IsEncrypted": false,
     "Values": {
       "FUNCTIONS_WORKER_RUNTIME": "python",
       "AzureWebJobsStorage": "UseDevelopmentStorage=true",
       "AZURE_FUNCTIONS_AGENTS_PROVIDER": "foundry",
       "FOUNDRY_PROJECT_ENDPOINT": "https://<foundry-account>.services.ai.azure.com/api/projects/<project>",
       "FOUNDRY_MODEL": "<model-deployment-name>"
     }
   }
   ```

   `local.settings.json` is for local use only. Do not commit secrets.

3. From the project directory, create a virtual environment and start Functions:

   ```powershell
   cd azure-functions-hosted-skills\expense-check-v3\src
   python -m venv .venv
   .venv\Scripts\Activate.ps1
   pip install -r requirements.txt
   func start
   ```

4. Open [expense-check.http](src/expense-check.http) in VS Code and select **Send Request** for the first (local) request. A function key is not required for a normal local Core Tools run.

5. When finished, stop Functions and Azurite with `Ctrl+C` in their respective terminals. Then deactivate the virtual environment in the Functions terminal:

   ```powershell
   deactivate
   ```

## Customize

| What to change | File |
|---|---|
| Sample rules and checking procedure | [SKILL.md](src/skills/expense-policy/SKILL.md) |
| Agent instructions and request and response schemas | [expense-check.agent.md](src/expense-check.agent.md) |
| HTTP route, request validation, and tool-result verification | [function_app.py](src/function_app.py) |
| Total calculation tool | [calculate_total.py](src/tools/calculate_total.py) |
| Shared model configuration and timeout | [agents.config.yaml](src/agents.config.yaml) |
| Infrastructure and Functions settings | [infra](infra/) |
| Python dependencies | [requirements.txt](src/requirements.txt) |

## Troubleshooting

- **The model deployment fails:** Check model and version availability in the region, as well as quota.
- **Storage returns 403 immediately after deployment:** Wait for role assignments to propagate, then retry `azd deploy`.
- **The Azure API returns 401:** Check the function or host key and the `x-functions-key` header.
- **The local app cannot connect to the model:** Check that the `az login` account has Foundry access and that the endpoint and model deployment name are correct.
- **Investigating runtime errors:** Check Application Insights in Azure or the `func start` output locally.

## Clean up

When the environment is no longer needed, run `azd down` from the project directory. Verify the target environment and resources before deletion, especially if the resource group contains other important resources.