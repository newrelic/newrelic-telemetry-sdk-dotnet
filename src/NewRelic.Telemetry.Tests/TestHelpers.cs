using NUnit.Framework;
using System.Collections.Generic;
using System.Text.Json;
using System.Linq;
using System;

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

        public static void AssertForAttribValue(Dictionary<string, JsonElement> dic, string attribName, object testValue)
        {
            if (!dic.ContainsKey(attribName))
            {
                if (testValue == null)
                {
                    return;     //This is OK
                }

                Assert.Fail($"Attribute {attribName}, expected {testValue}, actual NULL/missing");
            }

            var realVal = dic[attribName];

            if (testValue is string)
            {
                Assert.IsTrue((string)testValue == realVal.GetString(), $"Attribute {attribName}, expected {testValue}, actual {realVal}");
                return;
            }

            if (testValue is int || testValue is long || testValue is short)
            {
                Assert.IsTrue(Convert.ToInt64(testValue) == realVal.GetInt64(), $"Attribute {attribName}, expected {testValue}, actual {realVal}");
                return;
            }

            if (testValue is bool)
            {
                Assert.IsTrue((bool)testValue == realVal.GetBoolean(), $"Attribute {attribName}, expected {testValue}, actual {realVal}");
                return;
            }

            if (testValue is decimal)
            {
                Assert.IsTrue((decimal)testValue == realVal.GetDecimal(), $"Attribute {attribName}, expected {testValue}, actual {realVal}");
                return;
            }

            if (testValue is double || testValue is float)
            {
                Assert.IsTrue((double)testValue == realVal.GetDouble(), $"Attribute {attribName}, expected {testValue}, actual {realVal}");
                return;
            }

            Assert.Fail("Not Implemented");
        }

    }
}
