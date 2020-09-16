using Microsoft.Extensions.Configuration;
using NewRelic.Telemetry;
using NewRelic.Telemetry.Spans;
using System;
using System.IO;
using System.Threading;
using System.Threading.Tasks;

namespace BasicConsoleApplication
{
    class Program
    {
        private static SpanDataSender _dataSvc;

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
            _dataSvc = new SpanDataSender(telemetryConfiguration);
        }

        /// <summary>
        /// In this example, all of the spans are for the same trace.  Accordingly, the
        /// TraceId is set in the SpanBatch.
        /// </summary>
        private static async Task Example_SpanBatchForSingleTrace()
        {
            var traceId = System.Guid.NewGuid().ToString();

            // The SpanBatch manages a collection of Spans
            var spanBatch = SpanBatch.Create();

            // Since all of the spans in this batch represent a single
            // execution trace, set the TraceId on the SpanBatch instead
            // of on the individual spans.
            spanBatch.WithTraceId(traceId);

            // Perform 10 units of work as part of this trace/spanBatch
            for (var spanIdx = 0; spanIdx < 5; spanIdx++)
            {
                // Create a new Span assigning it a random guid as the spanId
                var span = Span.Create(Guid.NewGuid().ToString());

                //Add a name to the span to better understand it in the New Relic UI.
                span.WithName($"{traceId} - {spanIdx}");

                // Capture the start time for later use in calculating duration.
                var startTime = DateTime.UtcNow;

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
                    span.HasError(true);
                    span.WithAttribute("Exception", ex);
                }
                finally
                {
                    // Calculate the duration of execution and record it
                    var duration = TimeSpan.FromMilliseconds(100);
                    span.WithExecutionTimeInfo(startTime, duration);

                    //Attach the span to the Span Batch.
                    spanBatch.WithSpan(span);
                }
            }

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
            var spanBatch = SpanBatch.Create();

            for (var traceIdx = 0; traceIdx < 3; traceIdx++)
            {
                var traceId = Guid.NewGuid().ToString();

                for (var spanIdx = 0; spanIdx < 5; spanIdx++)
                {
                    
                    var span = Span.Create(Guid.NewGuid().ToString());

                    // Since multiple traces will be reported in the same SpanBatch,
                    // the TraceID needs to be attached to the individual spans.
                    span.WithTraceId(traceId)
                               .WithName($"{traceId} - {spanIdx}");

                    // Capture the start time for later use in calculating duration.
                    var startTime = DateTime.UtcNow;

                    try
                    {
                        // Attempt to perform a unit of work
                        DoWork($"Hello Outer Space Trace={traceId}, Span={spanIdx}");
                    }
                    catch (Exception ex)
                    {
                        // In the event of an exception, mark the span 
                        // as having an error and record a custom attribute 
                        // with the details about the exception.
                        span.HasError(true);
                        span.WithAttribute("Exception", ex);
                    }
                    finally
                    {
                        // Calculate the duration of execution and record it
                        var duration = TimeSpan.FromMilliseconds(100);
                        span.WithExecutionTimeInfo(startTime, duration);

                        //Attach the span to the Span Batch.
                        spanBatch.WithSpan(span);
                    }
                }

                Console.WriteLine();
            }

            // Send the SpanBatch to the New Relic endpoint.
            await SendDataToNewRelic(spanBatch);
        }


        /// <summary>
        /// Sends the data to New Relic endpoint.
        /// </summary>
        /// <param name="spanBatch"></param>
        /// <returns></returns>
        private static async Task SendDataToNewRelic(SpanBatch spanBatch)
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
