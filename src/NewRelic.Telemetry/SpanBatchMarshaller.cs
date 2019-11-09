using System.Collections.Generic;
using System.IO;
using System.Text;
using System.Text.Json;
using System.Runtime.CompilerServices;

[assembly: InternalsVisibleTo("NewRelic.Telemetry.Tests, PublicKey=0024000004800000940000000602000000240000525341310004000001000100198f2915b649f8774e7937c4e37e39918db1ad4e83109623c1895e386e964f6aa344aeb61d87ac9bd1f086a7be8a97d90f9ad9994532e5fb4038d9f867eb5ed02066ae24086cf8a82718564ebac61d757c9cbc0cc80f69cc4738f48f7fc2859adfdc15f5dde3e05de785f0ed6b6e020df738242656b02c5c596a11e628752bd0")]

namespace NewRelic.Telemetry.Spans
{
    public interface ISpanBatchMarshaller
    {
        string ToJson(SpanBatch spanBatch);
    }

    internal class SpanBatchMarshaller : ISpanBatchMarshaller
    {
        internal SpanBatchMarshaller()
        {
        }

        public string ToJson(SpanBatch batch) 
        {
            var options = new JsonWriterOptions
            {
                Indented = false
            };

            using (var stream = new MemoryStream())
            {
                using (var writer = new Utf8JsonWriter(stream, options))
                {
                    writer.WriteStartArray();
                    writer.WriteStartObject();

                    if (batch != null)
                    {
                        if (!string.IsNullOrEmpty(batch.TraceId) || batch.Attributes?.Count > 0)
                        {
                            BuildCommonBlock(writer, batch);
                        }

                        BuildSpansBlock(writer, batch);
                    }

                    writer.WriteEndObject();
                    writer.WriteEndArray();
                }
                return Encoding.UTF8.GetString(stream.ToArray());
            }
        }

        private void BuildCommonBlock(Utf8JsonWriter writer, SpanBatch batch)
        {
            writer.WritePropertyName("common");
            writer.WriteStartObject();
            if (!string.IsNullOrEmpty(batch.TraceId))
            {
                writer.WriteString("trace.id", batch.TraceId);
            }

            var customAttributes = batch.Attributes;
            if (customAttributes?.Count > 0)
            {
                BuildAttributesBlock(writer, customAttributes);
            }

            writer.WriteEndObject();
        }

        private void BuildAttributesBlock(Utf8JsonWriter writer, IDictionary<string, object> attributes)
        {

            writer.WritePropertyName("attributes");
            writer.WriteStartObject();

            foreach (var attribute in attributes)
            {
                var t = attribute.Value.GetType();
                if (t == typeof(string))
                {
                    writer.WriteString(attribute.Key, (string)attribute.Value);
                }
                else if (t == typeof(int))
                {
                    writer.WriteNumber(attribute.Key, (int)attribute.Value);
                }
                else if (t == typeof(double))
                {
                    writer.WriteNumber(attribute.Key, (double)attribute.Value);
                }
                else if (t == typeof(long))
                {
                    writer.WriteNumber(attribute.Key, (long)attribute.Value);
                }
                else if (t == typeof(float))
                {
                    writer.WriteNumber(attribute.Key, (float)attribute.Value);
                }
                else if (t == typeof(uint))
                {
                    writer.WriteNumber(attribute.Key, (uint)attribute.Value);
                }
                else if (t == typeof(ulong))
                {
                    writer.WriteNumber(attribute.Key, (ulong)attribute.Value);
                }
                else if (t == typeof(decimal))
                {
                    writer.WriteNumber(attribute.Key, (decimal)attribute.Value);
                }
                else if (t == typeof(bool))
                {
                    writer.WriteBoolean(attribute.Key, (bool)attribute.Value);
                }
                else
                {
                    writer.WriteString(attribute.Key, attribute.Value.ToString());
                }
            }

            writer.WriteEndObject();
        }

        private void BuildSpansBlock(Utf8JsonWriter writer, SpanBatch batch)
        {
            if(batch.Spans == null || batch.Spans.Count == 0) 
            {
                return;
            }

            bool didCreateSpansProperty = false;

            foreach (var span in batch.Spans)
            {
                if (!didCreateSpansProperty)
                {
                    writer.WritePropertyName("spans");
                    writer.WriteStartArray();
                    didCreateSpansProperty = true;
                }

                writer.WriteStartObject();

                writer.WriteString("id", span.Id);

                if (!string.IsNullOrEmpty(span.TraceId))
                {
                    writer.WriteString("trace.id", span.TraceId);
                }

                if (span.Timestamp != default(long))
                {
                    writer.WriteNumber("timestamp", span.Timestamp);
                }

                if (span.Error)
                {
                    writer.WriteBoolean("error", span.Error);
                }

                var attributes = span.Attributes;

                if (attributes == null)
                {
                    attributes = new Dictionary<string, object>();
                }

                if (span.DurationMs != default(double))
                {
                    attributes.Add("duration.ms", span.DurationMs);
                }

                if (!string.IsNullOrEmpty(span.Name))
                {
                    attributes.Add("name", span.Name);
                }

                if (!string.IsNullOrEmpty(span.ServiceName))
                {
                    attributes.Add("service.name", span.ServiceName);
                }

                if (!string.IsNullOrEmpty(span.ParentId))
                {
                    attributes.Add("parent.id", span.ParentId);
                }

                if (attributes.Count > 0)
                {
                    BuildAttributesBlock(writer, attributes);
                }

                writer.WriteEndObject();
            }

            if (didCreateSpansProperty)
            {
                writer.WriteEndArray();
            }
        }
    }
}
