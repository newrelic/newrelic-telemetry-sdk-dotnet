using System.Collections.Generic;
using System.Text;
using Utf8Json;

namespace NewRelic.Telemetry.Spans
{
    internal interface ISpanBatchMarshaller
    {
        string ToJson(SpanBatch spanBatch);
    }

    internal class SpanBatchMarshaller : ISpanBatchMarshaller
    {
        public string ToJson(SpanBatch batch)
        {
            var writer = new JsonWriter();
            
            writer.WriteBeginArray();
            writer.WriteBeginObject();

            if (batch != null)
            {
                var hasCommonBlock = false;
                if (!string.IsNullOrEmpty(batch.TraceId) || batch.Attributes?.Count > 0)
                {
                    BuildCommonBlock(ref writer, batch);
                    hasCommonBlock = true;
                }

                if (batch.Spans != null && batch.Spans.Count > 0)
                {
                    if (hasCommonBlock)
                    {
                        writer.WriteValueSeparator();
                    }

                    BuildSpansBlock(ref writer, batch);
                }
            }

            writer.WriteEndObject();
            writer.WriteEndArray();

            return writer.ToString();
        }

        private void BuildCommonBlock(ref JsonWriter writer, SpanBatch batch)
        {
            writer.WritePropertyName("common");
            writer.WriteBeginObject();

            var hasTraceID = false;
            if (!string.IsNullOrEmpty(batch.TraceId))
            {
                WriteAttribute(ref writer, "trace.id", batch.TraceId);
                hasTraceID = true;
            }

            var customAttributes = batch.Attributes;
            if (customAttributes?.Count > 0)
            {
                if(hasTraceID)
                {
                    writer.WriteValueSeparator();
                }
                BuildAttributesBlock(ref writer, customAttributes);
            }

            writer.WriteEndObject();
        }

        private void BuildAttributesBlock(ref JsonWriter writer, IDictionary<string, object> attributes)
        {
            writer.WritePropertyName("attributes");
            writer.WriteBeginObject();

            bool firstValue = true;
            foreach (var attribute in attributes)
            {
                if(firstValue)
                {
                    firstValue = false;
                }
                else
                {
                    writer.WriteValueSeparator();
                }

                WriteAttribute(ref writer, attribute);
            }

            writer.WriteEndObject();
        }

        private void WriteAttribute(ref JsonWriter writer, KeyValuePair<string, object> attribute)
        {
            //includes the seperator
            writer.WritePropertyName(attribute.Key);

            if(attribute.Value == null)
            {
                writer.WriteNull();
                return;
            }

            var t = attribute.Value.GetType();
            if (t == typeof(string))
            {
                writer.WriteString((string)attribute.Value);
                return;
            }
            
            if (t == typeof(int))
            {
                writer.WriteInt32((int)attribute.Value);
                return;
            }

            if (t == typeof(double))
            {
                writer.WriteDouble((double)attribute.Value);
                return;
            }

            if (t == typeof(long))
            {
                writer.WriteInt64((long)attribute.Value);
                return;
            }

            if (t == typeof(float))
            {
                writer.WriteSingle((float)attribute.Value);
                return;
            }

            if (t == typeof(uint))
            {
                writer.WriteUInt32((uint)attribute.Value);
                return;
            }

            if (t == typeof(ulong))
            {
                writer.WriteUInt64((ulong)attribute.Value);
                return;
            }

            if (t == typeof(decimal))
            {
                var decvValBytes = Encoding.UTF8.GetBytes(attribute.Value.ToString());
                writer.WriteRaw(decvValBytes);
                return;
            }

            if (t == typeof(bool))
            {
                writer.WriteBoolean((bool)attribute.Value);
                return;
            }

            writer.WriteString(attribute.Value.ToString());
        }

        private void WriteAttribute(ref JsonWriter writer, string key, string value)
        {
            writer.WritePropertyName(key);

            if (value == null)
            {
                writer.WriteNull();
                return;
            }

            writer.WriteString(value);
        }

        private void WriteAttribute(ref JsonWriter writer, string key, long value)
        {
            writer.WritePropertyName(key);
            writer.WriteInt64(value);
        }

        private void WriteAttribute(ref JsonWriter writer, string key, bool value)
        {
            writer.WritePropertyName(key);
            writer.WriteBoolean(value);
        }

        private void BuildSpansBlock(ref JsonWriter writer, SpanBatch batch)
        {
            if (batch.Spans == null || batch.Spans.Count == 0)
            {
                return;
            }

            writer.WritePropertyName("spans");
            writer.WriteBeginArray();

            for (var i = 0; i < batch.Spans.Count; i++)
            {
                var span = batch.Spans[i];

                //For anything other than the first, write the seperator
                if (i > 0)
                {
                    writer.WriteValueSeparator();
                }

                writer.WriteBeginObject();

                WriteAttribute(ref writer, "id", span.Id);

                if (!string.IsNullOrEmpty(span.TraceId))
                {
                    writer.WriteValueSeparator();
                    WriteAttribute(ref writer, "trace.id", span.TraceId);
                }

                if (span.Timestamp != default(long))
                {
                    writer.WriteValueSeparator();
                    WriteAttribute(ref writer, "timestamp", span.Timestamp);
                }

                if (span.Error)
                {
                    writer.WriteValueSeparator();
                    WriteAttribute(ref writer, "error", span.Error);
                }

                var attributes = span.Attributes ?? new Dictionary<string, object>();

                if (span.DurationMs != default)
                {
                    attributes["duration.ms"] = span.DurationMs;
                }

                if (!string.IsNullOrEmpty(span.Name))
                {
                    attributes["name"] = span.Name;
                }

                if (!string.IsNullOrEmpty(span.ServiceName))
                {
                    attributes["service.name"] = span.ServiceName;
                }

                if (!string.IsNullOrEmpty(span.ParentId))
                {
                    attributes["parent.id"] = span.ParentId;
                }

                if (attributes.Count > 0)
                {
                    writer.WriteValueSeparator();
                    BuildAttributesBlock(ref writer, attributes);
                }

                writer.WriteEndObject();
            }
            writer.WriteEndArray();
        }
    }
}
