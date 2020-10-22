// Copyright 2020 New Relic, Inc. All rights reserved.
// SPDX-License-Identifier: Apache-2.0

namespace NewRelic.Telemetry
{
#if INTERNALIZE_TELEMETRY_SDK
    internal
#else
    public
#endif
    static class NewRelicConsts
    {
        public const string AttribNameCollectorName = "collector.name";
        public const string AttribNameInstrumentationProvider = "instrumentation.provider";

        public static class Tracing
        {
            public const string AttribNameServiceName = "service.name";
            public const string AttribSpanKind = "span.kind";
            public const string AttribNameDurationMs = "duration.ms";
            public const string AttribNameName = "name";
            public const string AttribNameParentId = "parent.id";
            public const string AttribNameErrorMsg = "error.message";
            public const string AttribNameHttpUrl = "http.url";
        }
    }
}
