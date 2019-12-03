using System;
using System.Diagnostics;
using System.Threading;
using NewRelic.Telemetry;
using NewRelic.Telemetry.Spans;


namespace SampleConsoleApplication
{
    /// <summary>
    /// Helper functions to simplify interaction with the Telemetry SDK.  Wraps units of work to be tracked, recording their exeuction duration and 
    /// recording exceptions that may occur.
    /// </summary>
    public static class SimpleTracer
    {
        private static ThreadLocal<SpanBatchBuilder> _currentSpanBatch = new ThreadLocal<SpanBatchBuilder>(() => SpanBatchBuilder.Create());
        private static ThreadLocal<SpanBuilder> _currentSpan = new ThreadLocal<SpanBuilder>();
        private static SpanDataSender _dataSender = new SpanDataSender(new TelemetryConfiguration().WithAPIKey("Your API Key Here"));

        /// <summary>
        /// Records a unit of work as a span, tracking its duration and handling exceptions.  Upon completion, for a topmost 
        /// unit of work, information will be sent to the New Relic endpoint as a SpanBatch.
        /// </summary>
        /// <param name="action">The work to be executed and tracked as a span</param>
        public static void TrackWork(Action action)
        {
            var parentSpan = _currentSpan.Value;

            var thisSpan = SpanBuilder.Create(Guid.NewGuid().ToString());

            _currentSpan.Value = thisSpan;

            if(parentSpan != null)
            {
                thisSpan.WithParentId(parentSpan.SpanId);
            }

            var startTime = DateTime.UtcNow;

            try
            {
                action?.Invoke();
            }
            catch(Exception ex)
            {
                thisSpan.HasError(true);
                thisSpan.WithAttribute("Exception", ex);
                throw;
            }
            finally
            {

                thisSpan.WithExecutionTimeInfo(startTime, DateTime.UtcNow);
             
                var spanBatch = _currentSpanBatch.Value.WithSpan(thisSpan.Build());

                if(parentSpan != null)
                {
                    _currentSpan.Value = parentSpan;
                }
                else
                {
                    _currentSpan.Value = null;
                    _currentSpanBatch.Value = null;

                    _dataSender.SendDataAsync(spanBatch.Build()).Wait();
                }
            }
        }

        /// <summary>
        /// Allows access to the current span.  Useful for adding attribution to the span
        /// from within your code.
        /// </summary>
        /// <param name="action">The work to be executed and tracked as a span</param>
        public static void CurrentSpan(Action<SpanBuilder> action)
        {

            if(_currentSpan.Value == null)
            {
                return;
            }

            action.Invoke(_currentSpan.Value);
        }
    }
}
