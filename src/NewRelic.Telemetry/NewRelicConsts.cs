namespace NewRelic.Telemetry
{
    public static class NewRelicConsts
    {
        public const string AttribName_InstrumentationProvider = "instrumentation.provider";

        public static class Tracing
        {
            public const string AttribName_ServiceName = "service.name";
            public const string AttribName_DurationMs = "duration.ms";
            public const string AttribName_Name = "name";
            public const string AttribName_ParentId = "parent.id";
            public const string AttribName_HasError = "error";
            public const string AttribName_ErrorMsg = "error.message";
        }
    }


}
