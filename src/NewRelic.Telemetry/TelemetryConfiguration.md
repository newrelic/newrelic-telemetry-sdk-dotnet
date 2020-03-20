# Telemetry SDK for .NET Configuration Options

The following configuration options are available.

### Common Configuration Options
The following configuration settings are common and should be set for all applications/services.

| Option                   | Required/Default | Description                                                      |
| ------------------------ | ---------------- | ---------------------------------------------------------------- |
| `ApiKey`                 | Required         | An Insert API Key is required when sending telemetry data to New Relic.  For more information about API Keys, refer to [this doc](https://docs.newrelic.com/docs/apis/get-started/intro-apis/types-new-relic-api-keys#event-insert-key)                                                                 |
| `ServiceName`              | Recommended      | This name identifies the service/application generating the telemetry information that is sent to New Relic.  |

### Advanced Configuration Options
The following configuration options are used in special circumstances, generally when instructed to do so by New Relic.

| Option                   | Required/Default | Description                                                      |
| ------------------------ | ---------------- | ---------------------------------------------------------------- |
| `TraceUrlOverride`         | Optional         | Allows overriding the the default endpoint to which Span information is sent.  This setting should not be used unless instructured to do so by New Relic. |
| `SendTimeout` <br/>seconds      | Optional <br/> default 5-sec  | The amount of time the SDK will wait for a response from a New Relic endpoint before determining a request to have failed. |




### Example Configuration
appsettings.json
```JSON
{
  "NewRelic": {
    "ApiKey" : "/* YOUR API KEY GOES HERE */",
    "ServiceName": "/* YOUR APPLICATION/SERVICE NAME GOES HERE */"
  },
  "Logging": {
    "LogLevel": {
      "Default": "Information",
      "Microsoft": "Warning",
      "Microsoft.Hosting.Lifetime": "Information"
    }
  },
  "AllowedHosts": "*",
}
```