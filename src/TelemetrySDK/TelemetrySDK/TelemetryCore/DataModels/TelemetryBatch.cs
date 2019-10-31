using System.Collections.Generic;

namespace NewRelic.TelemetryCore.DataModels
{
	public abstract class TelemetryBatch<T> where T:ITelemetryType
	{
		ICollection<T> _telemetryData;

		IDictionary<string, object> _commonAttributes;

		public TelemetryBatch(ICollection<T> telemetryData, IDictionary<string, object> commonAttributes)
		{
			this._telemetryData = telemetryData;
			this._commonAttributes = commonAttributes;
		}

		/**
		* Returns the number of telemetryData items in this collection. If this batch contains more than
		* <tt>Integer.MAX_VALUE</tt> items, returns <tt>Integer.MAX_VALUE</tt>.
		*
		* @return the number of telemetryData items in this batch
		*/
		public int Count => _telemetryData.Count;

		public ICollection<T> Telemetry => _telemetryData;

		public IDictionary<string, object> Attributes => _commonAttributes;
	}
}
