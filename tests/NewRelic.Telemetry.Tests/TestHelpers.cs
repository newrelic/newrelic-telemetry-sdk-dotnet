// Copyright 2020 New Relic, Inc. All rights reserved.
// SPDX-License-Identifier: Apache-2.0

using System;
using System.Collections.Generic;
using System.Linq;
using System.Text.Json;
using NewRelic.Telemetry.Metrics;
using NUnit.Framework;

namespace NewRelic.Telemetry.Tests
{
    public static class TestHelpers
    {
        public static Dictionary<string, JsonElement>[] DeserializeArray(string jsonString)
        {
            var items = JsonSerializer.Deserialize<JsonElement[]>(jsonString);
            var objItems = items.Select(x => JsonSerializer.Deserialize<Dictionary<string, JsonElement>>(x.ToString())).ToArray();

            return objItems;
        }

        public static Dictionary<string, JsonElement>[] DeserializeArray(JsonElement jsonElem)
        {
            return DeserializeArray(jsonElem.ToString());
        }

        public static Dictionary<string, JsonElement> DeserializeArrayFirstOrDefault(string jsonString)
        {
            return DeserializeArray(jsonString).FirstOrDefault();
        }

        public static Dictionary<string, JsonElement> DeserializeArrayFirstOrDefault(JsonElement jsonElem)
        {
            return DeserializeArray(jsonElem).FirstOrDefault();
        }

        public static Dictionary<string, JsonElement> DeserializeObject(string jsonString)
        {
            return JsonSerializer.Deserialize<Dictionary<string, JsonElement>>(jsonString);
        }

        public static Dictionary<string, JsonElement> DeserializeObject(JsonElement jsonElem)
        {
            return DeserializeObject(jsonElem.ToString());
        }

        public static void AssertForCollectionLength(Dictionary<string, JsonElement>[] dic, int length)
        {
            Assert.IsTrue(dic.Length == length, $"There should be {length} items, actual {dic?.Length}.");
        }

        public static void AssertForAttribCount(Dictionary<string, JsonElement> dic, int length)
        {
            Assert.IsTrue(dic.Count == length, $"There should be {length} properties, actual {dic?.Count}.");
        }

        public static void AssertForAttribNotPresent(Dictionary<string, JsonElement> dic, string attribName)
        {
            if (dic.ContainsKey(attribName))
            {
                var realVal = dic[attribName];
                Assert.Fail($"Attribute {attribName}, was NOT expected, but was present with value {realVal}");
            }
        }

        public static void AssertForAttribValue(Dictionary<string, JsonElement> dic, string attribName, object expectedValueObj)
        {
            if (!dic.ContainsKey(attribName))
            {
                if (expectedValueObj == null)
                {
                    return;     // This is OK
                }

                Assert.Fail($"Attribute {attribName}, expected {expectedValueObj}, actual NULL/missing");
            }

            var actualValJson = dic[attribName];

            if (expectedValueObj is string)
            {
                Assert.IsTrue((string)expectedValueObj == actualValJson.GetString(), $"Attribute {attribName}, expected {expectedValueObj}, actual {actualValJson}");
                return;
            }

            if (expectedValueObj is int || expectedValueObj is long || expectedValueObj is short)
            {
                Assert.IsTrue(Convert.ToInt64(expectedValueObj) == actualValJson.GetInt64(), $"Attribute {attribName}, expected {expectedValueObj}, actual {actualValJson}");
                return;
            }

            if (expectedValueObj is bool)
            {
                Assert.IsTrue((bool)expectedValueObj == actualValJson.GetBoolean(), $"Attribute {attribName}, expected {expectedValueObj}, actual {actualValJson}");
                return;
            }

            if (expectedValueObj is decimal)
            {
                Assert.IsTrue((decimal)expectedValueObj == actualValJson.GetDecimal(), $"Attribute {attribName}, expected {expectedValueObj}, actual {actualValJson}");
                return;
            }

            if (expectedValueObj is double || expectedValueObj is float)
            {
                Assert.IsTrue((double)expectedValueObj == actualValJson.GetDouble(), $"Attribute {attribName}, expected {expectedValueObj}, actual {actualValJson}");
                return;
            }

            if (expectedValueObj is MetricSummaryValue)
            {
                var expectedVal = expectedValueObj as MetricSummaryValue;

                foreach (var actualValProp in actualValJson.EnumerateObject())
                {
                    var actualPropValue = actualValProp.Value.GetDouble();
                    double? expectedPropValue = 0;

                    switch (actualValProp.Name)
                    {
                        case "count":
                            expectedPropValue = expectedVal.Count;
                            break;
                        case "sum":
                            expectedPropValue = expectedVal.Sum;
                            break;
                        case "min":
                            expectedPropValue = expectedVal.Min;
                            break;
                        case "max":
                            expectedPropValue = expectedVal.Max;
                            break;
                        default:
                            throw new Exception($"Unexpected property {actualValProp.Name}.");
                    }

                    Assert.AreEqual(expectedPropValue, actualPropValue, $"Mismatch on {actualValProp.Name} property: Expected: {expectedPropValue}, Actual: {actualPropValue}");
                }
                
                return;
            }

            Assert.Fail("Not Implemented");
        }
    }
}
