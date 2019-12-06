using System;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;
using NewRelic.Telemetry;
using NewRelic.Telemetry.Spans;

namespace ConsoleApplicationWithTracer
{
    /// <summary>
    /// Establishes a simple tracer utility that provides a wrapper that tracks the execution of code 
    /// as a span.  Tracks current state and attaches children spans to their parents.  Lastly,
    /// at the end of a trace, it sends the information to the New Relic endpoint.
    /// </summary>
    public static class SimpleTracer
    {
        private static ThreadLocal<SpanBatchBuilder> _currentSpanBatch = new ThreadLocal<SpanBatchBuilder>(NewTrace);
        private static ThreadLocal<SpanBuilder> _currentSpan = new ThreadLocal<SpanBuilder>();
        private static SpanDataSender _dataSender;
        private static TelemetryConfiguration _currentConfig;
        private static ILoggerFactory _loggerFactory;

        private static bool _isTracingEnabled => _dataSender != null;

        /// <summary>
        /// Configures the TelemetrySDK providing the Configuration Options object
        /// </summary>
        /// <returns></returns>
        public static void WithConfiguration(TelemetryConfiguration config)
        {
            _currentConfig = config;
        }

        /// <summary>
        /// Configures the Telemetry SDK using a Configuration Provider
        /// </summary>
        /// <param name="configProvider"></param>
        public static void WithConfiguration(IConfiguration configProvider)
        {
            _currentConfig = new TelemetryConfiguration(configProvider);
        }

        /// <summary>
        /// Configures the Telemetry SDK with Default Configuration
        /// </summary>
        /// <param name="apiKey">Required: API Key to communicate with New Relic endpoint</param>
        public static void WithDefaultConfiguration(string apiKey)
        {
            _currentConfig = new TelemetryConfiguration().WithAPIKey(apiKey);
        }

        /// <summary>
        /// Configures logging within the Telemetry SDK.
        /// </summary>
        /// <param name="loggerFactory"></param>
        public static void WithLogging(ILoggerFactory loggerFactory)
        {
            _loggerFactory = loggerFactory;
        }

        /// <summary>
        /// Starts Tracing
        /// </summary>
        public static void EnableTracing()
        {
            _dataSender = new SpanDataSender(_currentConfig, _loggerFactory);
        }

        /// <summary>
        /// Disables Tracing
        /// </summary>
        public static void DisableTracing()
        {
            _dataSender = null;
        }

        /// <summary>
        /// Creates a new Trace (SpanBatch) and sets the TraceId
        /// to a Guid
        /// </summary>
        private static SpanBatchBuilder NewTrace()
        {
            var traceId = Guid.NewGuid().ToString();

            Console.WriteLine($"{"SIMPLE TRACER: Trace Started",-30}: {traceId}");

            return SpanBatchBuilder.Create()
                .WithTraceId(traceId);
        }

        /// <summary>
        /// Wraps a unit of work and records it as a span.  It tracking calculates duration and handles exceptions.  
        /// Upon completion, if this the topmost unit of work, the spans will be sent to the New Relic endpoint as 
        /// a SpanBatch/Trace.
        /// </summary>
        /// <param name="action">The work to be executed and tracked as a span</param>
        public static void TrackWork(Action action)
        {
            TrackWork(null, action);
        }

        /// <summary>
        /// Wraps a unit of work and records it as a span.  It tracking calculates duration and handles exceptions.  
        /// Upon completion, if this the topmost unit of work, the spans will be sent to the New Relic endpoint as 
        /// a SpanBatch/Trace.
        /// </summary>
        /// <param name="action">The work to be executed and tracked as a span</param>
        public static void TrackWork(string name, Action action)
        {
            //If Tracing is not enabled, just invoke action and return
            if (!_isTracingEnabled)
            {
                action.Invoke();
                return;
            }

            var parentSpan = _currentSpan.Value;
            
            var newSpanId = Guid.NewGuid().ToString();

            var thisSpan = SpanBuilder.Create(newSpanId);

            if(!string.IsNullOrWhiteSpace(name))
            {
                thisSpan.WithAttribute("Name", name);
            }

            _currentSpan.Value = thisSpan;

            Console.WriteLine($"{"SIMPLE TRACER: Span Started",-30}: Span={newSpanId}, Parent={(parentSpan == null ? "<TOPMOST ITEM>": parentSpan.SpanId)} - {name??"No Name"}");

            // If this unit of work is part of a larger unit of work,
            // this will associate it as a child span of the larger span.
            if (parentSpan != null)
            {
                thisSpan.WithParentId(parentSpan.SpanId);
            }

            // collect the start timestamp
            var startTime = DateTime.UtcNow;

            try
            {
                action?.Invoke();
            }
            catch(Exception ex)
            {
                Console.WriteLine($"{"SIMPLE TRACER: Error Detected",-30}: Span={newSpanId}, Parent={(parentSpan == null ? "<TOPMOST ITEM>" : parentSpan.SpanId)} - {name ?? "No Name"} - {ex.Message}");

                // In the event an unhandled exception occurs, mark the span as "HasError"
                // and record the exception.
                thisSpan.HasError(true);
                thisSpan.WithAttribute("Exception", ex);

                // Rethrow the exception for the caller so as to not change the execution path of the application.
                throw;
            }
            finally
            {
                // In all cases, record the execution duration
                thisSpan.WithExecutionTimeInfo(startTime, DateTime.UtcNow);
             
                // Attach the span to the SpanBatch (Trace).
                var spanBatch = _currentSpanBatch.Value.WithSpan(thisSpan.Build());

                Console.WriteLine($"{"SIMPLE TRACER: Span Completed",-30}: Span={newSpanId}, Parent={(parentSpan == null ? "<TOPMOST ITEM>" : parentSpan.SpanId)} - {name ?? "No Name"}");

                // If this is a topmost unit of work, send the trace to the New Relic endpoint.
                if (parentSpan == null)
                {
                    _currentSpan.Value = null;
                    _currentSpanBatch.Value = null;

                    var sb = spanBatch.Build();

                    Console.WriteLine($"{"SIMPLE TRACER: Trace Completed",-30}: {sb.CommonProperties.TraceId}");

                    SendDataToNewRelic(sb).Wait();

                }
                else
                {
                    _currentSpan.Value = parentSpan;
                }
            }
        }
        /// <summary>
        /// Sends the data to New Relic endpoint.
        /// </summary>
        /// <param name="spanBatch"></param>
        /// <returns></returns>
        private static async Task SendDataToNewRelic(SpanBatch spanBatch)
        {
            var result = await _dataSender.SendDataAsync(spanBatch);
            Console.WriteLine("Send Data to New Relic");
            Console.WriteLine($"{"Result",-20}: {result.ResponseStatus}");
            Console.WriteLine($"{"Http Status",-20}: {result.HttpStatusCode}");
            Console.WriteLine($"{"Message",-20}: {result.Message}");
        }

        /// <summary>
        /// Allows access to the current span allowing the caller to add contextual information to the span.
        /// </summary>
        /// <example>This method could be used to denote a url on a web request</example>
        /// <example>This method could be used to record the object name and operation for a database call</example>
        /// <param name="action">the code that requires access to the current span.  Keeping in mind that it may be null</param>
        public static void CurrentSpan(Action<SpanBuilder> action)
        {
            //If Tracing is not enabled, just invoke action and return
            if (!_isTracingEnabled)
            {
                action.Invoke(null);
                return;
            }

            action.Invoke(_currentSpan.Value);
        }


    }
}