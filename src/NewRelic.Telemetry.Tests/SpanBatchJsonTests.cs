using NUnit.Framework;
using NewRelic.Telemetry.Spans;
using System.Linq;

namespace NewRelic.Telemetry.Tests
{ 
    class SpanBatchJsonTests
    {
        [Test]
        public void ToJson_EmptySpanBatch() 
        {
            // Arrange
            var spanBatch = SpanBatchBuilder.Create()
                .WithTraceId("traceId")
                .Build();
            
            // Act
            var jsonString = spanBatch.ToJson();

            //Assert
            var resultSpanBatch = TestHelpers.DeserializeArrayFirstOrDefault(jsonString);
            var resultCommonProps = TestHelpers.DeserializeObject(resultSpanBatch["common"]);

            TestHelpers.AssertForAttribValue(resultCommonProps, "trace.id", "traceId");
       }

        [Test]
        public void ToJson_NonEmptySpanBatch()
        {
            // Arrange
            var spanBatch = SpanBatchBuilder.Create()
                .WithTraceId("traceId")
                .WithSpan(SpanBuilder.Create("span1")
                    .WithTraceId("traceId")
                    .WithTimestamp(1L)
                    .WithServiceName("serviceName")
                    .WithDurationMs(67)
                    .WithName("name")
                    .WithParentId("parentId")
                    .HasError(true).Build())
                .Build();

            // Act
            var jsonString = spanBatch.ToJson();

            // Assert
            var resultSpanBatches = TestHelpers.DeserializeArray(jsonString);

            TestHelpers.AssertForCollectionLength(resultSpanBatches, 1);

            var resultSpanBatch = resultSpanBatches.First();
            
            var resultCommonProps = TestHelpers.DeserializeObject(resultSpanBatch["common"]);

            TestHelpers.AssertForAttribValue(resultCommonProps, "trace.id", "traceId");

            var resultSpans = TestHelpers.DeserializeArray(resultSpanBatch["spans"]);

            TestHelpers.AssertForCollectionLength(resultSpans, 1);
            
            var resultSpan = resultSpans.FirstOrDefault();

            TestHelpers.AssertForAttribValue(resultSpan, "id", "span1");
            TestHelpers.AssertForAttribValue(resultSpan, "trace.id", "traceId");
            TestHelpers.AssertForAttribValue(resultSpan, "timestamp", 1);
            TestHelpers.AssertForAttribValue(resultSpan, "error", true);
            TestHelpers.AssertForAttribCount(resultSpan, 5);

            var resultSpanAttribs = TestHelpers.DeserializeObject(resultSpan["attributes"]);

            TestHelpers.AssertForAttribValue(resultSpanAttribs, "duration.ms", 67);
            TestHelpers.AssertForAttribValue(resultSpanAttribs, "name", "name");
            TestHelpers.AssertForAttribValue(resultSpanAttribs, "service.name", "serviceName");
            TestHelpers.AssertForAttribValue(resultSpanAttribs, "parent.id", "parentId");
            TestHelpers.AssertForAttribCount(resultSpanAttribs, 4);
        }

        [Test]
        public void ToJson_SpanBatchWithAttributes()
        {
            // Arrange
            var spanBatch = SpanBatchBuilder.Create()
                .WithTraceId("traceId")
                .WithAttribute("customAtt1", "hello")
                .WithAttribute("customAtt2", 1)
                .WithAttribute("customAtt3", (decimal)1.2)
                .WithAttribute("customAtt4", true)
                .WithSpan(SpanBuilder.Create("span1")
                    .WithTraceId("traceId")
                    .WithTimestamp(1)
                    .WithServiceName("serviceName")
                    .WithDurationMs(67)
                    .WithName("name")
                    .WithParentId("parentId")
                    .HasError(true)
                    .Build())
                .Build();


            // Act
            var jsonString = spanBatch.ToJson();

            // Assert
            var resultSpanBatches = TestHelpers.DeserializeArray(jsonString);

            TestHelpers.AssertForCollectionLength(resultSpanBatches, 1);

            var resultSpanBatch = resultSpanBatches.First();

            var resultCommonProps = TestHelpers.DeserializeObject(resultSpanBatch["common"]);

            TestHelpers.AssertForAttribValue(resultCommonProps, "trace.id", "traceId");

            var resultCommonPropAttribs = TestHelpers.DeserializeObject(resultCommonProps["attributes"]);

            TestHelpers.AssertForAttribCount(resultCommonPropAttribs, 4);
            TestHelpers.AssertForAttribValue(resultCommonPropAttribs, "customAtt1", "hello");
            TestHelpers.AssertForAttribValue(resultCommonPropAttribs, "customAtt2", 1);
            TestHelpers.AssertForAttribValue(resultCommonPropAttribs, "customAtt3", (decimal)1.2);
            TestHelpers.AssertForAttribValue(resultCommonPropAttribs, "customAtt4", true);

            var resultSpans = TestHelpers.DeserializeArray(resultSpanBatch["spans"]);

            TestHelpers.AssertForCollectionLength(resultSpans, 1);

            var resultSpan = resultSpans.FirstOrDefault();

            TestHelpers.AssertForAttribValue(resultSpan, "id", "span1");
            TestHelpers.AssertForAttribValue(resultSpan, "trace.id", "traceId");
            TestHelpers.AssertForAttribValue(resultSpan, "timestamp", 1);
            TestHelpers.AssertForAttribValue(resultSpan, "error", true);
            TestHelpers.AssertForAttribCount(resultSpan, 5);

            var resultSpanAttribs = TestHelpers.DeserializeObject(resultSpan["attributes"]);

            TestHelpers.AssertForAttribValue(resultSpanAttribs, "duration.ms", 67D);
            TestHelpers.AssertForAttribValue(resultSpanAttribs, "name", "name");
            TestHelpers.AssertForAttribValue(resultSpanAttribs, "service.name", "serviceName");
            TestHelpers.AssertForAttribValue(resultSpanAttribs, "parent.id", "parentId");
            TestHelpers.AssertForAttribCount(resultSpanAttribs, 4);
        }
    }
}
