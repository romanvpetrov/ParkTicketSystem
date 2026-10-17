# Azure Application Insights setup

The API uses the Azure Monitor OpenTelemetry distribution. It automatically sends ASP.NET Core requests, dependencies, metrics, exceptions, and application logs. Request and response bodies are not collected.

## Create and connect Application Insights

1. In the Azure portal, create an **Application Insights** resource in the same region as the app when possible. A workspace-based resource is recommended.
2. Open the resource's **Overview** page and copy its **Connection String**.
3. Open the App Service that hosts this API.
4. Go to **Settings > Environment variables > App settings** and add:

   | Name | Value |
   | --- | --- |
   | `APPLICATIONINSIGHTS_CONNECTION_STRING` | The copied Application Insights connection string |
   | `OTEL_SERVICE_NAME` | `TicketSystem.Api` |

5. Save the settings and restart the App Service.
6. Call one or more endpoints. Data normally appears in Application Insights after a few minutes.

This project uses code-based instrumentation. Do not also enable the App Service codeless Application Insights agent, because running both instrumentation paths can produce duplicate telemetry.

The connection string is configured only in the App Service environment; do not commit it to `appsettings.json`.

## Store client IP addresses

The API processes `X-Forwarded-For` and `X-Forwarded-Proto` only when the immediate proxy is in an Azure App Service internal network. The allowlist includes the non-routable Linux App Service link-local range (`169.254.0.0/16`) so platform proxy addresses can rotate, plus both native IPv4 and IPv4-mapped IPv6 forms of the documented private App Service networks. The API then normalizes IPv4-mapped IPv6 client addresses and records the result as the OpenTelemetry `client.address`, which Azure Monitor maps to `requests.client_IP`. Forwarded headers from other sources are ignored to prevent clients from spoofing their address.

Application Insights uses client IP addresses for geolocation but stores `0.0.0.0` by default. To retain the actual address, set `DisableIpMasking` to `true` on the Application Insights resource.

Using Azure PowerShell:

```powershell
Update-AzApplicationInsights `
    -ResourceGroupName "<resource-group>" `
    -Name "<application-insights-resource>" `
    -DisableIPMasking
```

To configure it in the portal:

1. Open the Application Insights resource.
2. Select **Automation > Export template**, then **Deploy**.
3. Select **Edit template**.
4. Add `"DisableIpMasking": true` inside the Application Insights resource's `properties` object without removing its existing properties.
5. Save and deploy the template.

Only telemetry received after this setting is enabled contains the address. IP addresses are personal data in many jurisdictions. Confirm applicable privacy, retention, and access requirements before enabling storage.

## Verify request telemetry

Open the Application Insights resource, select **Logs**, and run:

```kusto
requests
| where timestamp > ago(24h)
| project timestamp, name, url, resultCode, success, duration,
          client_IP, client_City, client_StateOrProvince, client_CountryOrRegion,
          operation_Id
| order by timestamp desc
```

If the Log Analytics workspace shows resource-specific table names, use:

```kusto
AppRequests
| where TimeGenerated > ago(24h)
| project TimeGenerated, Name, Url, ResultCode, Success, DurationMs,
          ClientIP, ClientCity, ClientStateOrProvince, ClientCountryOrRegion,
          OperationId
| order by TimeGenerated desc
```

Use the operation id to correlate a request with exceptions, dependencies, and application logs:

```kusto
let operationId = "paste-operation-id-here";
union requests, traces, exceptions, dependencies
| where operation_Id == operationId
| order by timestamp asc
```

## Sampling

`AzureMonitor:SamplingRatio` in `TicketSystem.Api/appsettings.json` controls the percentage of correlated telemetry sent to Azure. The current value, `1.0`, captures every request. Lower it in high-traffic environments to reduce ingestion cost.

## Local verification

Set the connection string only for the current PowerShell session and start the API:

```powershell
$env:APPLICATIONINSIGHTS_CONNECTION_STRING = "<connection-string>"
$env:OTEL_SERVICE_NAME = "TicketSystem.Api.Local"
dotnet run --project TicketSystem.Api
```

Without `APPLICATIONINSIGHTS_CONNECTION_STRING`, the API continues to use its configured local logging providers and does not send telemetry to Azure.
