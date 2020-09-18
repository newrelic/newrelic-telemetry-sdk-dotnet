using NUnit.Framework;
using NewRelic.Telemetry.Tracing;
using System.Linq;
using System.Collections.Generic;

namespace NewRelic.Telemetry.Tests
{ 
    class SpanBatchJsonTests
    {
        [Test]
        public void ToJson_EmptySpanBatch() 
        {
            // Arrange
            var spanBatch = new NewRelicSpanBatch(
                spans: new NewRelicSpan[0],
                commonProperties: new NewRelicSpanBatchCommonProperties(
                    traceId: "traceId",
                    attributes: null));

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
            var spanBatch = new NewRelicSpanBatch(
                commonProperties: new NewRelicSpanBatchCommonProperties(
                    traceId: "traceId",
                    attributes: null),
                spans: new NewRelicSpan[]
                {
                    new NewRelicSpan(
                        spanId: "span1",
                        traceId: "traceId",
                        timestamp: 1L,
                        parentSpanId : "parentId",
                        attributes : new Dictionary<string, object>()
                        {
                            { NewRelicConsts.Tracing.AttribName_DurationMs, 67 },
                            { NewRelicConsts.Tracing.AttribName_ServiceName, "serviceName" },
                            { NewRelicConsts.Tracing.AttribName_Name, "name" },
                            { NewRelicConsts.Tracing.AttribName_HasError, true }
                        })
                });

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
            var spanBatch = new NewRelicSpanBatch(
                commonProperties: new NewRelicSpanBatchCommonProperties(
                    traceId: "traceId",
                    attributes: new Dictionary<string, object>()
                    {
                         { "customAtt1", "hello" },
                         { "customAtt2", 1 },
                         { "customAtt3", (decimal)1.2 },
                         { "customAtt4", true }
                    }),
                spans: new NewRelicSpan[]
                {
                    new NewRelicSpan(
                        spanId: "span1",
                        traceId: "traceId1",
                        timestamp: 1L,
                        parentSpanId : "parentId1",
                        attributes : new Dictionary<string, object>()
                        {
                            { NewRelicConsts.Tracing.AttribName_DurationMs, 100 },
                            { NewRelicConsts.Tracing.AttribName_ServiceName, "serviceName1" },
                            { NewRelicConsts.Tracing.AttribName_Name, "name1" },
                            { NewRelicConsts.Tracing.AttribName_HasError, true }
                        }),
                    new NewRelicSpan(
                        spanId: "span2",
                        traceId: "traceId2",
                        timestamp: 2L,
                        parentSpanId : "parentId2",
                        attributes : new Dictionary<string, object>()
                        {
                            { NewRelicConsts.Tracing.AttribName_DurationMs, 200 },
                            { NewRelicConsts.Tracing.AttribName_ServiceName, "serviceName2" },
                            { NewRelicConsts.Tracing.AttribName_Name, "name2" },
                        }),
                });

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
            var spanBatch = new NewRelicSpanBatch(
                commonProperties: new NewRelicSpanBatchCommonProperties(
                    traceId: "traceId",
                    attributes: new Dictionary<string, object>()
                    {
                         { "customAtt1", "hello" },
                         { "customAtt2", 1 },
                         { "customAtt3", (decimal)1.2 },
                         { "customAtt4", true }
                    }),
                spans: new NewRelicSpan[]
                {
                    new NewRelicSpan(
                        spanId: "span1",
                        traceId: "traceId",
                        timestamp: 1L,
                        parentSpanId : "parentId",
                        attributes : new Dictionary<string, object>()
                        {
                            { NewRelicConsts.Tracing.AttribName_DurationMs, 67 },
                            { NewRelicConsts.Tracing.AttribName_ServiceName, "serviceName" },
                            { NewRelicConsts.Tracing.AttribName_Name, "name" },
                            { NewRelicConsts.Tracing.AttribName_HasError, true }
                        })
                });

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
    }
}
