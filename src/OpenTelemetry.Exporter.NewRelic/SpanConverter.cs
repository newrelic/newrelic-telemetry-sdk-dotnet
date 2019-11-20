using NRSpans = NewRelic.Telemetry.Spans;
using OpenTelemetry.Trace;
using System;
using OpenTelemetry.Trace.Export;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;

namespace OpenTelemetry.Exporter.NewRelic
{
    internal static class SpanConverter
    {
        private const string _attribName_url = "http.url";
        private const string _NewRelicTraceEndpoint = "https://trace-api.newrelic.com/trace/v1";        //Make this injected

        public static NRSpans.Span ToNewRelicSpan(Span openTelemetrySpan, string serviceName)
        {
            if (openTelemetrySpan == null) throw new ArgumentNullException(nameof(openTelemetrySpan));
            if (openTelemetrySpan.Context == null) throw new NullReferenceException($"{nameof(openTelemetrySpan)}.Context");
            if (openTelemetrySpan.Context.SpanId == null) throw new NullReferenceException($"{nameof(openTelemetrySpan)}.Context.SpanId");
            if (openTelemetrySpan.Context.TraceId == null) throw new NullReferenceException($"{nameof(openTelemetrySpan)}.Context.TraceId");
            if (openTelemetrySpan.StartTimestamp == null) throw new NullReferenceException($"{nameof(openTelemetrySpan)}.StartTimestamp");
           
            var newRelicSpanBuilder = NRSpans.SpanBuilder.Create(openTelemetrySpan.Context.SpanId.ToHexString())
                   .WithTraceId(openTelemetrySpan.Context.TraceId.ToHexString())
                   .WithExecutionTimeInfo(openTelemetrySpan.StartTimestamp, openTelemetrySpan.EndTimestamp)   //handles Nulls
                   .HasError(!openTelemetrySpan.Status.IsOk)
                   .WithName(openTelemetrySpan.Name);       //Handles Nulls


            if (!string.IsNullOrWhiteSpace(serviceName))
            {
                newRelicSpanBuilder.WithServiceName(serviceName);
            }

            if(openTelemetrySpan.ParentSpanId != null)
            {
                newRelicSpanBuilder.WithParentId(openTelemetrySpan.ParentSpanId.ToHexString());
            }

            if (openTelemetrySpan.Attributes != null)
            {
                foreach (var spanAttrib in openTelemetrySpan.Attributes)
                {
                    if(string.Equals(spanAttrib.Key, _attribName_url,StringComparison.OrdinalIgnoreCase) 
                        && string.Equals(spanAttrib.Value?.ToString(),_NewRelicTraceEndpoint, StringComparison.OrdinalIgnoreCase))
                    {
                        return null;
                    }

                    newRelicSpanBuilder.WithAttribute(spanAttrib.Key, spanAttrib.Value);
                }
            }

            return newRelicSpanBuilder.Build();
        }
    }
}
