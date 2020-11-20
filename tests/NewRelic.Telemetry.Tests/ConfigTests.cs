// Copyright 2020 New Relic, Inc. All rights reserved.
// SPDX-License-Identifier: Apache-2.0

using System;
using System.Collections.Generic;
using System.IO;
using System.Text.Json;
using Microsoft.Extensions.Configuration;
using Xunit;

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
        private const string NRConfigSection = "NewRelic";
        private const string ProductName = "TestProduct";
        private const string AltProductName = "DifferentProduct";
        private const string MissingProductName = "MissingProduct";

        private const string ApiKeyProdValue = "987654";
        private const string ApiKeyNewRelicValue = "123456";
        private const string ApiKeyDiffProductvalue = "ABCDEFG";
        private const string ApiKeyDefaultValue = null;

        private const int SendTimeoutSecondsDiffProdValue = 500;

        private const bool AuditLoggingEnabledProdValue = true;
        private const bool AuditLoggingEnabledNewRelicValue = false;
        private const bool AuditLoggingEnabledDiffProdValue = true;

        private const string ServiceNameNewRelicValue = NRConfigSection + "Service";
        private const string ServiceNameDefaultValue = "New Relic Telemetry SDK";

        private const int BackoffMaxSecondsDefaultValue = 80;

        private TimeSpan _sendTimeoutSecondsProdValue = TimeSpan.FromSeconds(124);
        private TimeSpan _sendTimeoutSecondsDefaultValue = TimeSpan.FromSeconds(5);

        /// <summary>
        /// Uses a dictionary to produce an example appsettings.json configuration.  This 
        /// scenraio does NOT contain a New Relic Config Section.  Although the attribute values, 
        /// such as ApiKey, match the New Relic Config spec, they should be ignored because they.
        /// are not part of a New Relic Config section.
        /// </summary>
        public IConfiguration ConfigExample_NewRelicConfigMissing
        {
            // {
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
                    { "ApiKey", ApiKeyProdValue },
                    { "AuditLoggingEnabled", AuditLoggingEnabledProdValue },
                    { "SendTimeoutSeconds", _sendTimeoutSecondsProdValue },
                };

                var nonNewRelicConfigSection = new Dictionary<string, object>()
                {
                    { "ApiKey", "DifferentProductAPIKey" },
                    { "ServiceName", "DifferentProductServiceName" },
                    { "AuditLoggingEnabled", false },
                    { ProductName, productConfigSection },
                };

                var configObj = new Dictionary<string, object>()
                {
                    { "NotNewRelic", nonNewRelicConfigSection },
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
        /// scenraio contains a New Relic Config Section and the config sections for two. 
        /// example products, "TestProduct" and "DifferentProduct".  Various assertions
        /// will be made testing for a product specific configuration and/or a general New
        /// Relic configuration.
        /// </summary>
        public IConfiguration ConfigExample_NewRelicConfig
        {
            // {
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
                    { "ApiKey", ApiKeyProdValue },
                    { "AuditLoggingEnabled", AuditLoggingEnabledProdValue },
                    { "SendTimeoutSeconds", _sendTimeoutSecondsProdValue.TotalSeconds },
                };

                var altProductConfig = new Dictionary<string, object>()
                {
                    { "ApiKey", ApiKeyDiffProductvalue },
                    { "AuditLoggingEnabled", AuditLoggingEnabledDiffProdValue },
                    { "SendTimeoutSeconds", SendTimeoutSecondsDiffProdValue },
                };

                var nrConfigSection = new Dictionary<string, object>()
                {
                    { "ApiKey", ApiKeyNewRelicValue },
                    { "ServiceName", ServiceNameNewRelicValue },
                    { "AuditLoggingEnabled", AuditLoggingEnabledNewRelicValue },
                    { ProductName, productConfigSection },
                    { AltProductName, altProductConfig },
                };

                var configObj = new Dictionary<string, object>()
                {
                    { NRConfigSection, nrConfigSection },
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
        /// test Configuration for a specific product.  Testing to make sure that 
        /// when a config value for a product exists that it is used before the overall New Relic value
        /// and lastly the Default Value (in the class).
        /// </summary>
        [Fact]
        public void ProductSpecificConfig()
        {
            var telemetryConfig = new TelemetryConfiguration(ConfigExample_NewRelicConfig, ProductName);
            Assert.Equal(ApiKeyProdValue, telemetryConfig.ApiKey);
            Assert.Equal(_sendTimeoutSecondsProdValue, telemetryConfig.SendTimeout);
            Assert.Equal(ServiceNameNewRelicValue, telemetryConfig.ServiceName);
            Assert.Equal(BackoffMaxSecondsDefaultValue, telemetryConfig.BackoffMaxSeconds);
        }

        /// <summary>
        /// With a valid New Relic Config Section and Product specific config Sections, 
        /// tests general Configuration without a specific product.  Testing to make sure that 
        /// the product configuration values are not used and that New Relic Values only override default values.
        /// </summary>
        [Fact]
        public void NewRelicConfigOnly()
        {
            var telemetryConfig = new TelemetryConfiguration(ConfigExample_NewRelicConfig);

            Assert.Equal(ApiKeyNewRelicValue, telemetryConfig.ApiKey);
            Assert.Equal(_sendTimeoutSecondsDefaultValue, telemetryConfig.SendTimeout);
            Assert.Equal(ServiceNameNewRelicValue, telemetryConfig.ServiceName);
            Assert.Equal(BackoffMaxSecondsDefaultValue, telemetryConfig.BackoffMaxSeconds);
        }

        /// <summary>
        /// With a valid New Relic Config Section and Product specific config Sections, 
        /// tests configuration scenario for a specific product where specific product section does not exist
        /// but there is a New Relic Section.  In this case, the values from New Relic section should
        /// override default values (as defined the in TelemetryConfiguration class).
        /// </summary>
        [Fact]
        public void MissingProductConfig()
        {
            var telemetryConfig = new TelemetryConfiguration(ConfigExample_NewRelicConfig, MissingProductName);

            Assert.Equal(ApiKeyNewRelicValue, telemetryConfig.ApiKey);
            Assert.Equal(_sendTimeoutSecondsDefaultValue, telemetryConfig.SendTimeout);
            Assert.Equal(ServiceNameNewRelicValue, telemetryConfig.ServiceName);
            Assert.Equal(BackoffMaxSecondsDefaultValue, telemetryConfig.BackoffMaxSeconds);
        }

        /// <summary>
        /// With a configuration that does NOT contain a New Relic section, 
        /// tests to ensure that only default values should be used (as defined the in TelemetryConfiguration class).
        /// </summary>
        [Fact]
        public void MissingNewRelicConfig()
        {
            var telemetryConfig = new TelemetryConfiguration(ConfigExample_NewRelicConfigMissing);

            Assert.Equal(ApiKeyDefaultValue, telemetryConfig.ApiKey);
            Assert.Equal(_sendTimeoutSecondsDefaultValue, telemetryConfig.SendTimeout);
            Assert.Equal(ServiceNameDefaultValue, telemetryConfig.ServiceName);
            Assert.Equal(BackoffMaxSecondsDefaultValue, telemetryConfig.BackoffMaxSeconds);
        }
    }
}
