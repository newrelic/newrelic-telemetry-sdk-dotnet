using Microsoft.Extensions.Configuration;
using NewRelic.Telemetry;
using NewRelic.Telemetry.Tracing;
using NewRelic.Telemetry.Extensions;
using System;
using System.Collections.Generic;
using System.IO;
using System.Threading;
using System.Threading.Tasks;

namespace BasicConsoleApplication
{
    class Program
    {
        private static TraceDataSender _dataSvc;

        static void Main(string[] args)
        {
            Console.WriteLine("Enter to begin...");
            Console.ReadLine();

            Configuration();
            
            Console.WriteLine("Example_SpanBatchForSingleTrace");
            Example_SpanBatchForSingleTrace().Wait();

            Console.WriteLine();
            Console.WriteLine();

            Console.WriteLine("Example_SpanBatchForMultipleTraces");
            Example_SpanBatchForMultipleTraces().Wait();
        }


        /// <summary>
        /// At startup, read configuration settings and instantiate the data service
        /// which will communicate with the New Relic endpoint.
        /// </summary>
        private static void Configuration()
        {
            // obtain application settings from appsettings.json file
            // Most importantly, the config file will contain the New Relic API Key
            var config = new ConfigurationBuilder()
                .SetBasePath(Directory.GetCurrentDirectory())
                .AddJsonFile("appsettings.json")
                .Build();

            var telemetryConfiguration = new TelemetryConfiguration(config);
            
            Console.WriteLine($"{telemetryConfiguration.TraceUrl}");

            // The SpanDataSender handles all communication with the New Relic end point
            _dataSvc = new TraceDataSender(telemetryConfiguration, null);
        }

        /// <summary>
        /// In this example, all of the spans are for the same trace.  Accordingly, the
        /// TraceId is set in the SpanBatch.
        /// </summary>
        private static async Task Example_SpanBatchForSingleTrace()
        {
            var traceId = System.Guid.NewGuid().ToString();

            // The collection of Spans
            var spans = new List<NewRelicSpan>();

            // Perform 10 units of work as part of this trace/spanBatch
            for (var spanIdx = 0; spanIdx < 5; spanIdx++)
            {
                var spanStartTime = DateTime.UtcNow;
                var spanAttribs = new Dictionary<string, object>();

                try
                {
                    // Attempt to perform a unit of work
                    DoWork($"Hello World #{spanIdx}");
                }
                catch (Exception ex)
                {
                    // In the event of an exception, mark the span 
                    // as having an error and record a custom attribute 
                    // with the details about the exception.
                    spanAttribs[NewRelicConsts.Tracing.AttribName_HasError] = true;
                    spanAttribs[NewRelicConsts.Tracing.AttribName_HasError] = ex;
                }
                finally
                {
                    spanAttribs[NewRelicConsts.Tracing.AttribName_Name] =  $"{traceId} - {spanIdx}";
                    spanAttribs[NewRelicConsts.Tracing.AttribName_DurationMs] = DateTime.UtcNow.Subtract(spanStartTime).TotalMilliseconds;

                    // Create a new Span assigning it a random guid as the spanId
                    var span = new NewRelicSpan(
                        traceId: null,         //Not supplying TraceID here because Batch will have common properties with TraceID
                        spanId: Guid.NewGuid().ToString(),
                        parentSpanId: null,
                        timestamp: DateTime.UtcNow.ToUnixTimeMilliseconds(),
                        attributes: spanAttribs);

                    spans.Add(span);
                }
            }

            var spanBatchCommonProps = new NewRelicSpanBatchCommonProperties(traceId, null);

            var spanBatch = new NewRelicSpanBatch(spans, spanBatchCommonProps);

            // Send the SpanBatch to the New Relic endpoint.
            await SendDataToNewRelic(spanBatch);
        }


        /// <summary>
        /// In this example, multiple traces will be reported in the same batch.
        /// Accordingly, the TraceId is applied to the individual spans, and NOT on
        /// the SpanBatch.
        /// </summary>
        private static async Task Example_SpanBatchForMultipleTraces()
        {
            // The collection of Spans
            var spans = new List<NewRelicSpan>();

            for (var traceIdx = 0; traceIdx < 3; traceIdx++)
            {
                var traceId = Guid.NewGuid().ToString();

                // Perform 10 units of work as part of this trace/spanBatch
                for (var spanIdx = 0; spanIdx < 5; spanIdx++)
                {
                    var spanStartTime = DateTime.UtcNow;
                    var spanAttribs = new Dictionary<string, object>();

                    try
                    {
                        // Attempt to perform a unit of work
                        DoWork($"Hello World #{spanIdx}");
                    }
                    catch (Exception ex)
                    {
                        // In the event of an exception, mark the span 
                        // as having an error and record a custom attribute 
                        // with the details about the exception.
                        spanAttribs[NewRelicConsts.Tracing.AttribName_HasError] = true;
                        spanAttribs[NewRelicConsts.Tracing.AttribName_HasError] = ex;
                    }
                    finally
                    {
                        spanAttribs[NewRelicConsts.Tracing.AttribName_Name] = $"{traceId} - {spanIdx}";
                        spanAttribs[NewRelicConsts.Tracing.AttribName_DurationMs] = DateTime.UtcNow.Subtract(spanStartTime).TotalMilliseconds;

                        // Create a new Span assigning it a random guid as the spanId
                        var span = new NewRelicSpan(
                            traceId: traceId,         //Since we're mixing traces in the same batch, the trace id is supplied on each span
                            spanId: Guid.NewGuid().ToString(),
                            parentSpanId: null,
                            timestamp: DateTime.UtcNow.ToUnixTimeMilliseconds(),
                            attributes: spanAttribs);

                        spans.Add(span);
                    }
                }
            }

            var spanBatch = new NewRelicSpanBatch(spans, null);

            // Send the SpanBatch to the New Relic endpoint.
            await SendDataToNewRelic(spanBatch);
        }


        /// <summary>
        /// Sends the data to New Relic endpoint.
        /// </summary>
        /// <param name="spanBatch"></param>
        /// <returns></returns>
        private static async Task SendDataToNewRelic(NewRelicSpanBatch spanBatch)
        {
            var result = await _dataSvc.SendDataAsync(spanBatch);
            Console.WriteLine("Send Data to New Relic");
            Console.WriteLine($"{"Result",-20}: {result.ResponseStatus}");
            Console.WriteLine($"{"Http Status",-20}: {result.HttpStatusCode}");
            Console.WriteLine($"{"Message",-20}: {result.Message}");
        }

        private static void DoWork(string value)
        {
            Thread.Sleep(TimeSpan.FromMilliseconds(200));
            Console.WriteLine(value);
        }

    }
}
