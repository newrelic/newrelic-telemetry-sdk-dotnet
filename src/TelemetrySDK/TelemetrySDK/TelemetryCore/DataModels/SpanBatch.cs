using System.Collections.Generic;

namespace NewRelic.TelemetryCore.DataModels
{
	public class SpanBatch: TelemetryBatch<Span>
	{
		private string _traceId;

		public SpanBatch(ICollection<Span> spans, IDictionary<string, object> commonAttributes, string traceId): base(spans, commonAttributes)
		{
			_traceId = traceId;
		}

		public string TraceId => _traceId;
	}

}
