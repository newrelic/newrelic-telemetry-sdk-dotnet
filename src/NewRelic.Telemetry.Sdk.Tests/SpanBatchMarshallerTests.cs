using NUnit.Framework;
using System.Collections.Generic;

namespace NewRelic.Telemetry.Sdk.Tests
{
    class SpanBatchMarshallerTests
    {
        [Test]
        public void ToJson_EmptySpanBatch() 
        {
            var expectedString = @"[{""common"":{""trace.id"":""traceId""}}]";
            var spanBatch = new SpanBatch(new List<Span>(), new Dictionary<string, object>(), "traceId");
            var marshaller = new SpanBatchMarshaller();
            var jsonString = marshaller.ToJson(spanBatch);
            Assert.AreEqual(expectedString, jsonString);
        }

        [Test]
        public void ToJson_NullSpanBatch()
        {
            var expectedString = @"[{}]";
            var marshaller = new SpanBatchMarshaller();
            var jsonString = marshaller.ToJson(null);
            Assert.AreEqual(expectedString, jsonString);
        }

        [Test]
        public void ToJson_NonEmptySpanBatch()
        {
            var expectedString = @"[{""common"":{""trace.id"":""traceId""},""spans"":[{""id"":""span1"",""trace.id"":""traceId"",""timestamp"":1,""service.name"":""serviceName"",""duration.ms"":67,""name"":""name"",""parent.id"":""parentId"",""error"":true}]}]";

            var spanBuilder = Span.GetSpanBuilder("span1");
            var span = spanBuilder.TraceId("traceId").TimeStamp(1L)
                .ServiceName("serviceName").DurationMs(67d).Name("name")
                .ParentId("parentId").Error(true).Build();

            var spanBatch = new SpanBatch(new List<Span>() {span}, new Dictionary<string, object>(), "traceId");
            var marshaller = new Sdk.SpanBatchMarshaller();
            var jsonString = marshaller.ToJson(spanBatch);
            Assert.AreEqual(expectedString, jsonString);
        }

        [Test]
        public void ToJson_SpanBatchWithAttributes()
        {
            var customAttributes = new Dictionary<string, object>
            {
                { "customAtt1", "hello" },
                { "customAtt2", 1 },
                { "customAtt3", 1.2D },
                { "customAtt4", true }
            };

            var expectedString = @"[{""common"":{""trace.id"":""traceId"",""attributes"":{""customAtt1"":""hello"",""customAtt2"":1,""customAtt3"":1.2,""customAtt4"":true}},""spans"":[{""id"":""span1"",""trace.id"":""traceId"",""timestamp"":1,""service.name"":""serviceName"",""duration.ms"":67,""name"":""name"",""parent.id"":""parentId"",""error"":true,""attributes"":{""customAtt1"":""hello"",""customAtt2"":1,""customAtt3"":1.2,""customAtt4"":true}}]}]";

            var spanBuilder = Span.GetSpanBuilder("span1");
            var span = spanBuilder.TraceId("traceId").TimeStamp(1L)
                .ServiceName("serviceName").DurationMs(67d).Name("name")
                .ParentId("parentId").Error(true).Attributes(customAttributes).Build();

            var spanBatch = new SpanBatch(new List<Span>() { span }, customAttributes, "traceId");
            var marshaller = new SpanBatchMarshaller();
            var jsonString = marshaller.ToJson(spanBatch);
            Assert.AreEqual(expectedString, jsonString);
        }

        [Test]
        public void ToJson_OmitNullValueProperty()
        {
            var marshaller = new SpanBatchMarshaller();

            var expectedString1 = @"[{""spans"":[{""id"":""spanId""}]}]";
            var spanBuilder1 = Span.GetSpanBuilder("spanId");
            var span1 = spanBuilder1.Build();
            var spanBatch1 = new SpanBatch(new List<Span>() { span1 }, null, null);
            var jsonString1 = marshaller.ToJson(spanBatch1);
            Assert.AreEqual(expectedString1, jsonString1);

            var expectedString2 = @"[{}]";
            var spanBatch2 = new SpanBatch(null, null, null);
            var jsonString2 = marshaller.ToJson(spanBatch2);
            Assert.AreEqual(expectedString2, jsonString2);

            var expectedString3 = @"[{}]";
            var spanBatch3 = new SpanBatch(new List<Span>(), null, null);
            var jsonString3 = marshaller.ToJson(spanBatch3);
            Assert.AreEqual(expectedString3, jsonString3);
        }

        [Test]
        public void ToJson_NullSpans() 
        {
            var marshaller = new SpanBatchMarshaller();

            var expectedString1 = @"[{""spans"":[{""id"":""spanId""}]}]";
            var spanBatch1 = new SpanBatch(
                new List<Span>() 
                { 
                    Span.GetSpanBuilder(null).Build(),
                    Span.GetSpanBuilder(null).Build(),
                    Span.GetSpanBuilder("spanId").Build() 
                }, null, null);
            var jsonString1 = marshaller.ToJson(spanBatch1);
            Assert.AreEqual(expectedString1, jsonString1);

            var expectedString2 = @"[{}]";
            var spanBatch2 = new SpanBatch(
                new List<Span>()
                {
                    Span.GetSpanBuilder(null).Build(),
                    Span.GetSpanBuilder(null).Build()
                }, null, null);
            var jsonString2 = marshaller.ToJson(spanBatch2);
            Assert.AreEqual(expectedString2, jsonString2);
        }

    }
}
