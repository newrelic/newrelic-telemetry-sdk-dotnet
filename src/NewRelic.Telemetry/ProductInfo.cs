// Copyright 2020 New Relic, Inc. All rights reserved.
// SPDX-License-Identifier: Apache-2.0

using System.Reflection;

namespace NewRelic.Telemetry
{
    internal class ProductInfo
    {
        public const string Name = "NewRelic-Dotnet-TelemetrySDK";
        public static readonly string Version = Assembly.GetExecutingAssembly().GetCustomAttribute<PackageVersionAttribute>()?.PackageVersion ?? "unspecified";
    }
}
