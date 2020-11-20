# NewRelic.OpenTelemetry Distributed Tracing Demo Application

Note: this is based on https://github.com/open-telemetry/opentelemetry-dotnet/tree/master/examples/MicroserviceExample

This set of projects is an example distributed application comprised of two
components:

1. An ASP.NET Core Web API
2. A background Worker Service

The application demonstrates a number of OpenTelemetry concepts:

* OpenTelemetry APIs for distributed context propagation.
* Basic conventions of how messaging systems are handled in OpenTelemetry.

The Web API publishes messages to RabbitMQ which the Worker Service consumes.
Distributed context propagation is achieved using OpenTelemetry APIs to inject
and extract trace context in the headers of the published messages.

The WebAPI and WorkerService applications are configured to use the New Relic OpenTelemetry exporter from this repository.

You will need your New Relic Insights insert API key set in your environment like this:

```shell
export NEWRELIC_APIKEY=<your_api_key>
```

## Running the example

A running instance of RabbitMQ is required, which can easily be
spun up in a Docker container.

The `WebApi` and `WorkerService` projects can be run from this directory as
follows:

```shell
dotnet run --project WebApi
dotnet run --project WorkerService
```

Instead of running the projects individually, if you are using Docker Desktop,
a `docker-compose` file is provided. This makes standing up the RabbitMQ dependency easy, as well as starting both applications.

To run the example using `docker-compose`, run the following from this
directory:

```shell
docker-compose up --build
```

With everything running:

* [Invoke the Web API](http://localhost:5000/SendMessage) to send a message.
* View your traces in New Relic by finding the `WebApi` and/or `WorkerService` entities in the Entity Explorer, then
  choosing `Distributed Tracing` from the left-hand menu.
* If you have run RabbitMQwith default settings:
  * Manage RabbitMQ [here](http://localhost:15672/)
    * user = guest
    * password = guest

## References

* [New Relic OpenTelemetry](https://docs.newrelic.com/docs/integrations/open-source-telemetry-integrations/open-source-telemetry-integration-list/opentelemetry-exporter)
* [Docker Desktop](https://www.docker.com/products/docker-desktop)
* [OpenTelemetry Project](https://opentelemetry.io/)
* [RabbitMQ](https://www.rabbitmq.com/)
* [Worker Service](https://docs.microsoft.com/en-us/azure/azure-monitor/app/worker-service)
