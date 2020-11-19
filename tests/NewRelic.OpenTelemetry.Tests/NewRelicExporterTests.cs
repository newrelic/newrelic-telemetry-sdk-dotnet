// Copyright 2020 New Relic, Inc. All rights reserved.
// SPDX-License-Identifier: Apache-2.0

using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Net;
using OpenTelemetry;
using OpenTelemetry.Trace;
using Xunit;

namespace NewRelic.OpenTelemetry.Tests
{
    [Collection("newrelic-exporter")]
    public class NewRelicExporterTests : IDisposable
    {
        private static readonly ConcurrentDictionary<Guid, IDictionary<string, object>> Responses = new ConcurrentDictionary<Guid, IDictionary<string, object>>();

        private readonly IDisposable? _testServer;
        private readonly string _testServerHost;
        private readonly int _testServerPort;

        static NewRelicExporterTests()
        {
            Activity.DefaultIdFormat = ActivityIdFormat.W3C;
            Activity.ForceDefaultIdFormat = true;

            var listener = new ActivityListener
            {
                ShouldListenTo = _ => true,
                Sample = (ref ActivityCreationOptions<ActivityContext> options) => ActivitySamplingResult.AllData,
            };

            ActivitySource.AddActivityListener(listener);
        }

        public NewRelicExporterTests()
        {
            _testServer = TestHttpServer.RunServer(
                ctx => ProcessServerRequest(ctx),
                out _testServerHost,
                out _testServerPort);

            static void ProcessServerRequest(HttpListenerContext context)
            {
                context.Response.StatusCode = 200;

                using var readStream = new StreamReader(context.Request.InputStream);

                var dictionary = new Dictionary<string, object>
                {
                    { "request-body", readStream.ReadToEnd() },
                    { "user-agent", context.Request.UserAgent },
                };

                Responses.TryAdd(
                    Guid.Parse(context.Request.QueryString["requestId"]),
                    dictionary);

                context.Response.OutputStream.Close();
            }
        }

        public void Dispose()
        {
            _testServer?.Dispose();
        }

        [Fact]
        public void UserAgentStringIsCorrect()
        {
            const string ActivitySourceName = "newrelic.test";
            var requestId = Guid.NewGuid();

            var exporterOptions = new NewRelicExporterOptions()
            {
                ApiKey = "my-apikey",
                ServiceName = "test-newrelic",
                EndpointUrl = new Uri($"http://{_testServerHost}:{_testServerPort}/trace/v1?requestId={requestId}"),
            };

            var newRelicExporter = new NewRelicTraceExporter(exporterOptions);
            var exportActivityProcessor = new BatchExportProcessor<Activity>(newRelicExporter);

            using var openTelemetrySdk = Sdk.CreateTracerProviderBuilder()
                .AddSource(ActivitySourceName)
                .AddProcessor(exportActivityProcessor)
                .Build();

            var source = new ActivitySource(ActivitySourceName);
            var activity = source.StartActivity("Test Activity");
            activity?.Stop();

            exportActivityProcessor.ForceFlush();

            Assert.Equal(
                $"{Telemetry.ProductInfo.Name}/exporter {ProductInfo.Name}/{ProductInfo.Version}",
                Responses[requestId]["user-agent"]);
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

            var exporterOptions = new NewRelicExporterOptions()
            {
                ApiKey = "my-apikey",
                ServiceName = "test-newrelic",
                EndpointUrl = new Uri($"http://{_testServerHost}:{_testServerPort}/trace/v1?requestId={requestId}"),
            };

            var newRelicExporter = new NewRelicTraceExporter(exporterOptions);
            var exportActivityProcessor = new BatchExportProcessor<Activity>(newRelicExporter);

            using var openTelemetrySdk = Sdk.CreateTracerProviderBuilder()
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
