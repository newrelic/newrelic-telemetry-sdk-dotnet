using NUnit.Framework;
using Microsoft.Extensions.Configuration;
using System.IO;
using System.Collections.Generic;
using System.Text.Json;

namespace NewRelic.Telemetry.Tests
{
    /// <summary>
    /// TelemetryConfiguration is a hierarchical cofiguration.  Discovering the value for a 
    /// configuration setting occurs at 3-levels.  If a configuration section exists for a 
    /// specific product, those values will be used.  Else, if a NewRelic configuration section 
    /// exists, those values will be used.  Else, hard-coded default values will be used.
    /// </summary>
    public class ConfigTests
    {
        const string NRConfigSection = "NewRelic";
        const string ProductName = "TestProduct";
        const string AltProductName = "DifferentProduct";
        const string MissingProductName = "MissingProduct";

        const string ApiKey_ProdValue = "987654";
        const string ApiKey_NewRelicValue = "123456";
        const string ApiKey_DiffProductvalue = "ABCDEFG";
        const string ApiKey_DefaultValue = null;

        const int SendTimeoutSeconds_ProdValue = 124;
        const int SendTimeoutSeconds_DefaultValue = 5;
        const int SendTimeoutSeconds_DiffProdValue = 500;

        const bool AuditLoggingEnabled_ProdValue = true;
        const bool AuditLoggingEnabled_NewRelicValue = false;
        const bool AuditLoggingEnabled_DiffProdValue = true;

        const string ServiceName_NewRelicValue = NRConfigSection + "Service";
        const string ServiceName_DefaultValue = null;

        const int BackoffMaxSeconds_DefaultValue = 80;

        [SetUp]
        public void Setup()
        {
            
        }

        /// <summary>
        /// Uses a dictionary to produce an example appsettings.json configuration.  This 
        /// scenraio does NOT contain a New Relic Config Section.  Although the attribute values, 
        /// such as ApiKey, match the New Relic Config spec, they should be ignored because they
        //  are not part of a New Relic Config section.
        /// </summary>
        public IConfiguration ConfigExample_NewRelicConfigMissing
        {
            //  {
            //      "NotNewRelic": {
            //          "ApiKey": "DifferentProductAPIKey",
            //          "ServiceName": "DifferentProductServiceName",
            //          "AuditLoggingEnabled": false,
            //          "TestProduct": {
            //                          "ApiKey": "987654",
            //          "AuditLoggingEnabled": true,
            //          "SendTimeoutSeconds": 124
            //          }
            //      }
            //  }
            get
            {
                #region Build JSON
                var productConfigSection = new Dictionary<string, object>()
                {
                    { "ApiKey", ApiKey_ProdValue },
                    { "AuditLoggingEnabled", AuditLoggingEnabled_ProdValue },
                    { "SendTimeoutSeconds", SendTimeoutSeconds_ProdValue }
                };

                var nonNewRelicConfigSection = new Dictionary<string, object>()
                {
                    { "ApiKey", "DifferentProductAPIKey" },
                    { "ServiceName", "DifferentProductServiceName" },
                    { "AuditLoggingEnabled", false },
                    { ProductName, productConfigSection }
                };

                var configObj = new Dictionary<string, object>()
                {
                    { "NotNewRelic" , nonNewRelicConfigSection }
                };
                #endregion

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
        /// Uses a dictionary to produce an example appsettings.json configuration.  This 
        /// scenraio contains a New Relic Config Section and the config sections for two 
        //  example products, "TestProduct" and "DifferentProduct".  Various assertions
        /// will be made testing for a product specific configuration and/or a general New
        /// Relic configuration.
        /// </summary>
        public IConfiguration ConfigExample_NewRelicConfig
        {
            //  {
            //      "NewRelic": {
            //      "ApiKey": "123456",
            //      "ServiceName": "NewRelicService",
            //      "AuditLoggingEnabled": false,
            //      "TestProduct": {
            //          "ApiKey": "987654",
            //          "AuditLoggingEnabled": true,
            //          "SendTimeoutSeconds": 124
            //      },
            //      "DifferentProduct": {
            //          "ApiKey": "ABCDEFG",
            //          "AuditLoggingEnabled": true,
            //          "SendTimeoutSeconds": 500
            //      }
            //      }
            //  }

            get
            {
                #region build JSON
                var productConfigSection = new Dictionary<string, object>()
                {
                    { "ApiKey", ApiKey_ProdValue },
                    { "AuditLoggingEnabled", AuditLoggingEnabled_ProdValue },
                    { "SendTimeoutSeconds", SendTimeoutSeconds_ProdValue }
                };

                var altProductConfig = new Dictionary<string, object>()
                {
                    { "ApiKey", ApiKey_DiffProductvalue },
                    { "AuditLoggingEnabled", AuditLoggingEnabled_DiffProdValue },
                    { "SendTimeoutSeconds", SendTimeoutSeconds_DiffProdValue }
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
                #endregion
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
        /// With a valid New Relic Config Section and Product Specific config Sections, 
        /// test Configuration the configuration for a specific product.  Testing to make sure that 
        /// when a config value for a product exists that it is used before the overall New Relic value
        /// and lastly the Default Value (in the class)
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
        /// With a valid New Relic Config Section and Product specific config Sections, 
        /// tests general Configuration without a specific product.  Testing to make sure that 
        /// the product configuration values are not used and that New Relic Values only override default values.
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


        /// <summary>
        /// With a valid New Relic Config Section and Product specific config Sections, 
        /// tests configuration scenario for a specific product where specific product section does not exist
        /// but there is a New Relic Section.  In this case, the values from New Relic section should
        /// override default values (as defined the in TelemetryConfiguration class)
        /// </summary>
        [Test]
        public void MissingProductConfig()
        {
            var telemetryConfig = new TelemetryConfiguration(ConfigExample_NewRelicConfig, MissingProductName);

            Assert.AreEqual(ApiKey_NewRelicValue, telemetryConfig.ApiKey);
            Assert.AreEqual(SendTimeoutSeconds_DefaultValue, telemetryConfig.SendTimeout);
            Assert.AreEqual(ServiceName_NewRelicValue, telemetryConfig.ServiceName);
            Assert.AreEqual(BackoffMaxSeconds_DefaultValue, telemetryConfig.BackoffMaxSeconds);
        }

        /// <summary>
        /// With a configuration that does NOT contain a New Relic section, 
        /// tests to ensure that only default values should be used (as defined the in TelemetryConfiguration class)
        /// </summary>
        [Test]
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
