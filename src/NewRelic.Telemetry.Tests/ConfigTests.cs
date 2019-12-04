using NUnit.Framework;
using Microsoft.Extensions.Configuration;
using System.IO;
using System.Collections.Generic;
using System.Text.Json;

namespace NewRelic.Telemetry.Tests
{
    public class ConfigTests
    {
        const string NRConfigSection = "NewRelic";
        const string ProductName = "TestProduct";
        const string AltProductName = "DifferentProduct";
        const string MissingProductName = "MissingProduct";

        const string ApiKey_ProdValue = "987654";
        const string ApiKey_NewRelicValue = "123456";
        const string ApiKey_DefaultValue = null;

        const int SendTimeoutSeconds_ProdValue = 124;
        const int SendTimeoutSeconds_DefaultValue = 5;

        const bool AuditLoggingEnabled_ProdValue = true;
        const bool AuditLoggingEnabled_NewRelicValue = false;

        const string ServiceName_NewRelicValue = NRConfigSection + "Service";
        const string ServiceName_DefaultValue = null;

        const int BackoffMaxSeconds_DefaultValue = 80;

        [SetUp]
        public void Setup()
        {
            
        }

        public IConfiguration ConfigExample_NewRelicConfigMissing
        {
            get
            {
                var productConfigSection = new Dictionary<string, object>()
            {
                { "ApiKey", ApiKey_ProdValue },
                { "AuditLoggingEnabled", AuditLoggingEnabled_ProdValue },
                { "SendTimeoutSeconds", SendTimeoutSeconds_ProdValue }
            };

                var altProductConfig = new Dictionary<string, object>()
            {
                { "ApiKey", "Different Product" },
                { "AuditLoggingEnabled", true },
                { "SendTimeoutSeconds", -10 }
            };

                var DifferentProductConfig = new Dictionary<string, object>()
            {
                { "ApiKey", ApiKey_NewRelicValue },
                { "Service.Name", ServiceName_NewRelicValue },
                { "AuditLoggingEnabled", AuditLoggingEnabled_NewRelicValue },
                { ProductName, productConfigSection },
                { AltProductName, altProductConfig }
            };

                var configObj = new Dictionary<string, object>()
            {
                { "SomethingElse" , DifferentProductConfig }
            };

                var configJson = JsonSerializer.Serialize(configObj);

                using (var stream = new MemoryStream())
                {
                    using (var writer = new StreamWriter(stream))
                    {
                        writer.Write(configJson);
                        writer.Flush();
                        stream.Position = 0;

                        return new ConfigurationBuilder()
                            .AddJsonStream(stream)
                            .Build();
                    }
                }
            }
        }

        public IConfiguration ConfigExample_NewRelicConfig
        {
            get
            {
                var productConfigSection = new Dictionary<string, object>()
                {
                    { "ApiKey", ApiKey_ProdValue },
                    { "AuditLoggingEnabled", AuditLoggingEnabled_ProdValue },
                    { "SendTimeoutSeconds", SendTimeoutSeconds_ProdValue }
                };

                var altProductConfig = new Dictionary<string, object>()
                {
                    { "ApiKey", "Different Product" },
                    { "AuditLoggingEnabled", true },
                    { "SendTimeoutSeconds", -10 }
                };

                var nrConfigSection = new Dictionary<string, object>()
                {
                    { "ApiKey", ApiKey_NewRelicValue },
                    { "ServiceName", ServiceName_NewRelicValue },
                    { "AuditLoggingEnabled", AuditLoggingEnabled_NewRelicValue },
                    { ProductName, productConfigSection },
                    { AltProductName, altProductConfig }
                };

                var configObj = new Dictionary<string, object>()
                {
                    { NRConfigSection, nrConfigSection }
                };

                var configJson = JsonSerializer.Serialize(configObj);

                using (var stream = new MemoryStream())
                {
                    using (var writer = new StreamWriter(stream))
                    {
                        writer.Write(configJson);
                        writer.Flush();
                        stream.Position = 0;

                        return new ConfigurationBuilder()
                            .AddJsonStream(stream)
                            .Build();
                    }
                }
            }
        }

        /// <summary>
        /// When a Product Specific Configuration exists, ensures that the product Specific
        /// Values ovverride New Relic Values which override Default Values.  Further tests 
        /// that multiple products are correctly identified.
        /// </summary>
        [Test]
        public void ProductSpecificConfig()
        {
            var telemetryConfig = new TelemetryConfiguration(ConfigExample_NewRelicConfig, ProductName);
            Assert.AreEqual(ApiKey_ProdValue, telemetryConfig.ApiKey);
            Assert.AreEqual(SendTimeoutSeconds_ProdValue, telemetryConfig.SendTimeout);
            Assert.AreEqual(ServiceName_NewRelicValue, telemetryConfig.ServiceName);
            Assert.AreEqual(BackoffMaxSeconds_DefaultValue, telemetryConfig.BackoffMaxSeconds);

        }

        /// <summary>
        /// When a product specific configuration is not enabled that the New Relic 
        /// Values override default values 
        /// </summary>
        [Test]
        public void NewRelicConfigOnly()
        {
            var telemetryConfig = new TelemetryConfiguration(ConfigExample_NewRelicConfig);

            Assert.AreEqual(ApiKey_NewRelicValue, telemetryConfig.ApiKey);
            Assert.AreEqual(SendTimeoutSeconds_DefaultValue, telemetryConfig.SendTimeout);
            Assert.AreEqual(ServiceName_NewRelicValue, telemetryConfig.ServiceName);
            Assert.AreEqual(BackoffMaxSeconds_DefaultValue, telemetryConfig.BackoffMaxSeconds);
        }

        [Test]
        public void MissingProductConfig()
        {
            var telemetryConfig = new TelemetryConfiguration(ConfigExample_NewRelicConfig, MissingProductName);

            Assert.AreEqual(ApiKey_NewRelicValue, telemetryConfig.ApiKey);
            Assert.AreEqual(SendTimeoutSeconds_DefaultValue, telemetryConfig.SendTimeout);
            Assert.AreEqual(ServiceName_NewRelicValue, telemetryConfig.ServiceName);
            Assert.AreEqual(BackoffMaxSeconds_DefaultValue, telemetryConfig.BackoffMaxSeconds);
        }

        public void MissingNewRelicConfig()
        {
            var telemetryConfig = new TelemetryConfiguration(ConfigExample_NewRelicConfigMissing);

            Assert.AreEqual(ApiKey_DefaultValue, telemetryConfig.ApiKey);
            Assert.AreEqual(SendTimeoutSeconds_DefaultValue, telemetryConfig.SendTimeout);
            Assert.AreEqual(ServiceName_DefaultValue, telemetryConfig.ServiceName);
            Assert.AreEqual(BackoffMaxSeconds_DefaultValue, telemetryConfig.BackoffMaxSeconds);
        }
    }
}
