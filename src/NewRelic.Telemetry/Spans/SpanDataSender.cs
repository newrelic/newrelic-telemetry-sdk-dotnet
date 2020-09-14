using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;
using NewRelic.Telemetry.Transport;
using System.Collections.Generic;
using System.Linq;

namespace NewRelic.Telemetry.Spans
{
    /// <summary>
    /// The SpanDataSender is used to send Span data to New Relic.  It manages the communication 
    /// with the New Relic end points and reports outcomes.
    /// </summary>
    public class SpanDataSender : DataSender<SpanBatch>
    {
        protected override string EndpointUrl => _config.TraceUrl;

        /// <summary>
        /// Creates new SpanDataSender setting the options using an instance of TelemetryConfiguration
        /// to specify settings.
        /// </summary>
        /// <param name="configOptions"></param>
        public SpanDataSender(TelemetryConfiguration configOptions) : base(configOptions)
        {
        }

        /// <summary>
        /// Creates new SpanDataSender setting the options using an instance of TelemetryConfiguration
        /// to specify settings and a Logger Factory that will be used to log information about the
        /// interactions with New Relic endpoints.
        /// </summary>
        /// <param name="configOptions"></param>
        /// <param name="loggerFactory"></param>
        public SpanDataSender(TelemetryConfiguration configOptions, ILoggerFactory? loggerFactory) : base(configOptions, loggerFactory)
        {
        }

        /// <summary>
        /// Creates new SpanDataSender obtaining configuration settings from a Configuration Provider 
        /// that is compatible with <see cref="Microsoft.Extensions.Configuration">Microsoft.Extensions.Configuration.</see>
        /// </summary>
        /// <param name="configProvider"></param>
        public SpanDataSender(IConfiguration configProvider) : base(configProvider)
        {
        }


        /// <summary>
        /// Creates new SpanDataSender obtaining configuration settings from a Configuration Provider 
        /// that is compatible with <see cref="Microsoft.Extensions.Configuration">Microsoft.Extensions.Configuration.</see>
        /// It also accepts a <see cref="Microsoft.Extensions.Logging.ILoggerFactory">logger factory</see> 
        /// that will be used to log information about the interactions with New Relic endpoints.
        /// </summary>
        /// <param name="configProvider"></param>
        /// <param name="loggerFactory"></param>
        public SpanDataSender(IConfiguration configProvider, ILoggerFactory loggerFactory) : base(configProvider, loggerFactory)
        {
        }

        protected override void BeforeDataSend(SpanBatch dataToSend)
        {
            base.BeforeDataSend(dataToSend);

            if(_config.InstrumentationProvider == null || string.IsNullOrWhiteSpace(_config.InstrumentationProvider) || dataToSend.Spans == null)
            {
                return;
            }

            foreach (var span in dataToSend.Spans)
            {
                span.WithAttribute(Span.attribName_InstrumentationProvider, _config.InstrumentationProvider);
            }
        }

        protected override bool ContainsNoData(SpanBatch dataToCheck)
        {
            return (dataToCheck?.Spans?.Count).GetValueOrDefault(0) == 0;
        }
    }
}
