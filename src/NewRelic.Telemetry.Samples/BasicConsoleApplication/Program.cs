using Microsoft.Extensions.Configuration;
using NewRelic.Telemetry.Spans;
using System;
using System.IO;
using System.Threading.Tasks;

namespace BasicConsoleApplication
{
	class Program
	{
        private static SpanDataSender _dataSvc;

		static void Main(string[] args)
		{
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

            // The SpanDataSender handles all communication with the New Relic end point
            _dataSvc = new SpanDataSender(config);
        }

        /// <summary>
        /// In this example, all of the spans are for the same trace.  Accordingly, the
        /// TraceId is set in the SpanBatchBuilder 
        /// </summary>
        private static async Task Example_SpanBatchForSingleTrace()
        {
            var traceId = System.Guid.NewGuid().ToString();

            // The SpanBatchBuilder manages a collection of Spans
            var spanBatchBuilder = SpanBatchBuilder.Create();

            // Since all of the spans in this batch represent a single
            // execution trace, set the TraceId on the SpanBatch instead
            // of on the individual spans.
            spanBatchBuilder.WithTraceId(traceId);

            // Perform 10 units of work as part of this trace/spanBatch
            for (var spanIdx = 0; spanIdx < 10; spanIdx++)
            {
                // The SpanBuilder is used to crate a new span.
                // Create a new SpanBuilder assigning it a random guid as the spanId
                var spanBuilder = SpanBuilder.Create(Guid.NewGuid().ToString());

                //Add a name to the span to better understand it in the New Relic UI.
                spanBuilder.WithName($"{traceId} - {spanIdx}");

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
                    spanBuilder.HasError(true);
                    spanBuilder.WithAttribute("Exception", ex);
                }
                finally
                {
                    // Calculate the duration of execution and record it
                    var endTime = DateTime.UtcNow;
                    spanBuilder.WithExecutionTimeInfo(startTime, endTime);

                    //Obtain the completed Span from the SpanBuilder
                    var span = spanBuilder.Build();

                    //Attach the span to the Span Batch.
                    spanBatchBuilder.WithSpan(span);
                }
            }

            // Obtain the SpanBatch from the SpanBatchBuilder
            var spanBatch = spanBatchBuilder.Build();

            // Send the SpanBatch to the New Relic endpoint.
            await _dataSvc.SendDataAsync(spanBatch);
        }


        /// <summary>
        /// In this example, multiple traces will be reported in the same batch.
        /// Accordingly, the TraceId is applied to the individual spans, and NOT on
        /// the SpanBatch.
        /// </summary>
        private static async Task Example_SpanBatchForMultipleTraces()
        {
            var spanBatchBuilder = SpanBatchBuilder.Create();

            for (var traceIdx = 0; traceIdx < 5; traceIdx++)
            {
                var traceId = Guid.NewGuid().ToString();

                for (var spanIdx = 0; spanIdx < 10; spanIdx++)
                {
                    //Add a name to the span to better understand it in the New Relic UI.


                    var spanBuilder = SpanBuilder.Create(Guid.NewGuid().ToString());

                    spanBuilder.WithTraceId(traceId)
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
                        spanBuilder.HasError(true);
                        spanBuilder.WithAttribute("Exception", ex);
                    }
                    finally
                    {
                        // Calculate the duration of execution and record it
                        var endTime = DateTime.UtcNow;
                        spanBuilder.WithExecutionTimeInfo(startTime, endTime);

                        //Obtain the completed Span from the SpanBuilder
                        var span = spanBuilder.Build();

                        //Attach the span to the Span Batch.
                        spanBatchBuilder.WithSpan(span);
                    }
                }

                Console.WriteLine();
            }

            // Obtain the SpanBatch from the SpanBatchBuilder
            var spanBatch = spanBatchBuilder.Build();

            // Send the SpanBatch to the New Relic endpoint.
            await _dataSvc.SendDataAsync(spanBatch);
        }



        private static void DoWork(string value)
        {
            Console.WriteLine(value);
        }

	}
}
