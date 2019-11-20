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
            TestHelpers.AssertForAttribCount(resultSpan, 4);

            var resultSpanAttribs = TestHelpers.DeserializeObject(resultSpan["attributes"]);

            TestHelpers.AssertForAttribValue(resultSpanAttribs, "duration.ms", 67);
            TestHelpers.AssertForAttribValue(resultSpanAttribs, "name", "name");
            TestHelpers.AssertForAttribValue(resultSpanAttribs, "service.name", "serviceName");
            TestHelpers.AssertForAttribValue(resultSpanAttribs, "parent.id", "parentId");
            TestHelpers.AssertForAttribValue(resultSpanAttribs, "error", true);
            TestHelpers.AssertForAttribCount(resultSpanAttribs, 5);
        }


        [Test]
        public void ToJson_SpanBatchWithMultipleSpans()
        {
            // Arrange
            var spanBatchBuilder = SpanBatchBuilder.Create()
               .WithTraceId("traceId")
               .WithAttribute("customAtt1", "hello")
               .WithAttribute("customAtt2", 1)
               .WithAttribute("customAtt3", (decimal)1.2)
               .WithAttribute("customAtt4", true);



            spanBatchBuilder.WithSpan(SpanBuilder.Create("span1")
                   .WithTraceId("traceId1")
                   .WithTimestamp(1)
                   .WithServiceName("serviceName1")
                   .WithDurationMs(100)
                   .WithName("name1")
                   .WithParentId("parentId1")
                   .HasError(true)
                   .Build());

            spanBatchBuilder.WithSpan(SpanBuilder.Create("span2")
                   .WithTraceId("traceId2")
                   .WithTimestamp(2)
                   .WithServiceName("serviceName2")
                   .WithDurationMs(200)
                   .WithName("name2")
                   .WithParentId("parentId2")
                   .HasError(false)
                   .Build());

            var spanBatch = spanBatchBuilder.Build();

            // Act
            var jsonString = spanBatch.ToJson();

            //Assert
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

            TestHelpers.AssertForCollectionLength(resultSpans, 2);

            var firstSpan = resultSpans[0];

            TestHelpers.AssertForAttribValue(firstSpan, "id", "span1");
            TestHelpers.AssertForAttribValue(firstSpan, "trace.id", "traceId1");
            TestHelpers.AssertForAttribValue(firstSpan, "timestamp", 1);
            TestHelpers.AssertForAttribCount(firstSpan, 4);

            var firstSpanAttribs = TestHelpers.DeserializeObject(firstSpan["attributes"]);

            TestHelpers.AssertForAttribValue(firstSpanAttribs, "duration.ms", 100);
            TestHelpers.AssertForAttribValue(firstSpanAttribs, "name", "name1");
            TestHelpers.AssertForAttribValue(firstSpanAttribs, "service.name", "serviceName1");
            TestHelpers.AssertForAttribValue(firstSpanAttribs, "parent.id", "parentId1");
            TestHelpers.AssertForAttribValue(firstSpanAttribs, "error", true);

            TestHelpers.AssertForAttribCount(firstSpanAttribs, 5);

            var secondSpan = resultSpans[1];

            TestHelpers.AssertForAttribValue(secondSpan, "id", "span2");
            TestHelpers.AssertForAttribValue(secondSpan, "trace.id", "traceId2");
            TestHelpers.AssertForAttribValue(secondSpan, "timestamp", 2);
            TestHelpers.AssertForAttribCount(secondSpan, 4);

            var secondSpanAttribs = TestHelpers.DeserializeObject(secondSpan["attributes"]);

            TestHelpers.AssertForAttribValue(secondSpanAttribs, "duration.ms", 200);
            TestHelpers.AssertForAttribValue(secondSpanAttribs, "name", "name2");
            TestHelpers.AssertForAttribValue(secondSpanAttribs, "service.name", "serviceName2");
            TestHelpers.AssertForAttribValue(secondSpanAttribs, "parent.id", "parentId2");
            TestHelpers.AssertForAttribCount(secondSpanAttribs, 4);
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
            TestHelpers.AssertForAttribCount(resultSpan, 4);

            var resultSpanAttribs = TestHelpers.DeserializeObject(resultSpan["attributes"]);

            TestHelpers.AssertForAttribValue(resultSpanAttribs, "duration.ms", 67D);
            TestHelpers.AssertForAttribValue(resultSpanAttribs, "name", "name");
            TestHelpers.AssertForAttribValue(resultSpanAttribs, "service.name", "serviceName");
            TestHelpers.AssertForAttribValue(resultSpanAttribs, "parent.id", "parentId");
            TestHelpers.AssertForAttribValue(resultSpanAttribs, "error", true);
            TestHelpers.AssertForAttribCount(resultSpanAttribs, 5);
        }


        [Test]
        public void ToJson_DuplicatePropertyValuesKeepsLast()
        {
            //Arrange
            var spanBatch = SpanBatchBuilder.Create()
                .WithTraceId("BadTraceID")
                .WithTraceId("GoodTraceID")
                .WithAttribute("customAtt1", "BadAttr1")
                .WithAttribute("customAtt1", "GoodAttr1")
                .WithAttribute("customAtt2", -1000)
                .WithAttribute("customAtt2", 1000)
                .WithSpan(SpanBuilder.Create("span1")
                    .WithTraceId("BadTraceID")
                    .WithTraceId("GoodTraceID")
                    .WithTimestamp(-100)
                    .WithTimestamp(100)
                    .WithServiceName("BadserviceName")
                    .WithServiceName("GoodServiceName")
                    .WithDurationMs(-500)
                    .WithDurationMs(500)
                    .WithName("BadName")
                    .WithName("GoodName")
                    .WithParentId("BadParentId")
                    .WithParentId("GoodParentId")
                    .HasError(true)
                    .HasError(false)
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

            TestHelpers.AssertForAttribValue(resultCommonProps, "trace.id", "GoodTraceID");

            var resultCommonPropAttribs = TestHelpers.DeserializeObject(resultCommonProps["attributes"]);

            TestHelpers.AssertForAttribCount(resultCommonPropAttribs, 2);
            TestHelpers.AssertForAttribValue(resultCommonPropAttribs, "customAtt1", "GoodAttr1");
            TestHelpers.AssertForAttribValue(resultCommonPropAttribs, "customAtt2", 1000);

            var resultSpans = TestHelpers.DeserializeArray(resultSpanBatch["spans"]);

            TestHelpers.AssertForCollectionLength(resultSpans, 1);

            var resultSpan = resultSpans.FirstOrDefault();

            TestHelpers.AssertForAttribValue(resultSpan, "id", "span1");
            TestHelpers.AssertForAttribValue(resultSpan, "trace.id", "GoodTraceID");
            TestHelpers.AssertForAttribValue(resultSpan, "timestamp", 100);
            TestHelpers.AssertForAttribCount(resultSpan, 4);

            var resultSpanAttribs = TestHelpers.DeserializeObject(resultSpan["attributes"]);

            TestHelpers.AssertForAttribValue(resultSpanAttribs, "duration.ms", 500);
            TestHelpers.AssertForAttribValue(resultSpanAttribs, "name", "GoodName");
            TestHelpers.AssertForAttribValue(resultSpanAttribs, "service.name", "GoodServiceName");
            TestHelpers.AssertForAttribValue(resultSpanAttribs, "parent.id", "GoodParentId");
            TestHelpers.AssertForAttribValue(resultSpanAttribs, "error", true);
            TestHelpers.AssertForAttribCount(resultSpanAttribs, 5);



        }
    }
}
