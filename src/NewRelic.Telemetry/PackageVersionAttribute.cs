// Copyright 2020 New Relic, Inc. All rights reserved.
// SPDX-License-Identifier: Apache-2.0

using System;

namespace NewRelic.Telemetry
{
    [AttributeUsage(AttributeTargets.Assembly)]
    internal class PackageVersionAttribute : Attribute
    {
        public string PackageVersion { get; }

        public PackageVersionAttribute(string version) 
        {
            PackageVersion = version;
        }
    }
}
