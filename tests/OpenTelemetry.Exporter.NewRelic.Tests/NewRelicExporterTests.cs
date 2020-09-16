using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Net;
using Microsoft.Extensions.Configuration;
using NewRelic.Telemetry;
using OpenTelemetry.Trace;
using Xunit;

namespace OpenTelemetry.Exporter.NewRelic.Tests
{
    [Collection("newrelic-exporter")]
    public class NewRelicExporterTests : IDisposable
    {
        private static readonly ConcurrentDictionary<Guid, string> Responses = new ConcurrentDictionary<Guid, string>();

        private readonly IDisposable? testServer;
        private readonly string testServerHost;
        private readonly int testServerPort;

        static NewRelicExporterTests()
        {
            Activity.DefaultIdFormat = ActivityIdFormat.W3C;
            Activity.ForceDefaultIdFormat = true;

            var listener = new ActivityListener
            {
                ShouldListenTo = _ => true,
                GetRequestedDataUsingParentId = (ref ActivityCreationOptions<string> options) => ActivityDataRequest.AllData,
                GetRequestedDataUsingContext = (ref ActivityCreationOptions<ActivityContext> options) => ActivityDataRequest.AllData,
            };

            ActivitySource.AddActivityListener(listener);
        }

        public NewRelicExporterTests()
        {
            this.testServer = TestHttpServer.RunServer(
                ctx => ProcessServerRequest(ctx),
                out this.testServerHost,
                out this.testServerPort);

            static void ProcessServerRequest(HttpListenerContext context)
            {
                context.Response.StatusCode = 200;

                using StreamReader readStream = new StreamReader(context.Request.InputStream);

                string requestContent = readStream.ReadToEnd();

                Responses.TryAdd(
                    Guid.Parse(context.Request.QueryString["requestId"]),
                    requestContent);

                context.Response.OutputStream.Close();
            }
        }

        public void Dispose()
        {
            this.testServer?.Dispose();
        }

        [Fact]
        public void SuppresssesInstrumentation()
        {
            const string ActivitySourceName = "newrelic.test";
            var requestId = Guid.NewGuid();
            var testActivityProcessor = new TestActivityProcessor();

            var endCalledCount = 0;

            testActivityProcessor.EndAction =
                (a) =>
                {
                    endCalledCount++;
                };

            var config = new ConfigurationBuilder()
                .AddInMemoryCollection(new Dictionary<string, string> {
                    { "NewRelic:ServiceName", "test-newrelic" },
                    { "NewRelic:ApiKey", "my-apikey" },
                    { "NewRelic:TraceUrlOverride", $"http://{this.testServerHost}:{this.testServerPort}/trace/v1?requestId={requestId}" },
                })
                .Build();

            var exporterOptions = new TelemetryConfiguration(config);

            var newRelicExporter = new NewRelicTraceExporter(exporterOptions);
            var exportActivityProcessor = new BatchExportActivityProcessor(newRelicExporter);

            var openTelemetrySdk = Sdk.CreateTracerProviderBuilder()
                .AddSource(ActivitySourceName)
                .AddProcessor(testActivityProcessor)
                .AddProcessor(exportActivityProcessor)
                .AddHttpClientInstrumentation()
                .Build();

            var source = new ActivitySource(ActivitySourceName);
            var activity = source.StartActivity("Test Activity");
            activity?.Stop();

            // We call ForceFlush on the exporter twice, so that in the event
            // of a regression, this should give any operations performed in
            // the  exporter itself enough time to be instrumented and loop
            // back through the exporter.
            exportActivityProcessor.ForceFlush();
            exportActivityProcessor.ForceFlush();

            Assert.Equal(1, endCalledCount);
        }
    }
}
