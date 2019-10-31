using System;
using System.Collections.Generic;

namespace NewRelic.TelemetryCore.DataModels
{
	public class Span: ITelemetryType
	{
		private string _id;
		IEnumerable<KeyValuePair<string, object>> _attributes;

		private string _traceId; // trace.id <- top level
		private DateTimeOffset _timestamp; // in epoch ms

		private string _serviceName; // service.name <- goes in attributes
		private TimeSpan _duration; // duration.ms <- goes in attributes
		private string _name; // goes in attributes
		private string _parentId; // parent.id <- goes in attributes
		private bool _error;

		private Span(
			String id,
			IEnumerable<KeyValuePair<string, object>> attributes,
			string traceId,
			DateTimeOffset timestamp,
			string serviceName,
			TimeSpan duration,
			string name,
			string parentId,
			bool error)
			{
				this._id = id;
				this._attributes = attributes;
				this._traceId = traceId;
				this._timestamp = timestamp;
				this._serviceName = serviceName;
				this._duration = duration;
				this._name = name;
				this._parentId = parentId;
				this._error = error;
			}

		/**
		 * @param spanId The ID associated with this span
		 * @return A Builder class that can be used to add variables to a Span object and create a new
		 *     Span instance
		 */
		public static SpanBuilder GetBuilder(string spanId)
		{
			return new SpanBuilder(spanId);
		}

		public string Id => _id;

		public string TraceId => _traceId;

		public DateTimeOffset TimeStamp => _timestamp;

		public IEnumerable<KeyValuePair<string, object>> Attributes => _attributes;

		public string Name => _name;

		public string ParentId => _parentId;

		public string ServiceName => _serviceName;

		public TimeSpan Duration => _duration;

		/**
		 * A class for holding the variables associated with a Span object and creating a new Span object
		 * with those variables
		 */
		public class SpanBuilder
		{

			private string _id;
			private IEnumerable<KeyValuePair<string, object>> _attributes = new Dictionary<string, object>();
			private string _traceId;
			private DateTimeOffset _timestamp = DateTime.Now;
			private string _serviceName;
			private TimeSpan _durationMs;
			private string _name;
			private string _parentId;
			private bool _error = false;

			/** @param spanId The ID associated with the Span object to be created */
			public SpanBuilder(string spanId)
			{
				this._id = spanId;
			}

			/**
			 * @param attributes Dimensional attributes as key-value pairs, associated with the Span object
			 *     to be created. See {@link Attributes}
			 * @return The SpanBuilder object with its attributes variable set to the given attributes
			 *     object
			 */
			public SpanBuilder Attributes(IEnumerable<KeyValuePair<string, object>> attributes)
			{
				this._attributes = attributes;
				return this;
			}

			/**
			 * @param traceId The ID used to identify a request as it crosses process boundaries, and in
			 *     turn link span events
			 * @return The SpanBuilder object with its traceId variable set to the given Trace Id
			 */
			public SpanBuilder TraceId(String traceId)
			{
				this._traceId = traceId;
				return this;
			}

			/**
			 * @param timestamp The start time of the span event in epoch milliseconds
			 * @return The SpanBuilder object with its timestamp variable set to the given timestamp
			 */
			public SpanBuilder TimeStamp(DateTimeOffset timestamp)
			{
				this._timestamp = timestamp;
				return this;
			}

			/**
			 * @param serviceName The name of the service this Span event occurred in
			 * @return The SpanBuilder object with its serviceName variable set to the given Service Name
			 */
			public SpanBuilder ServiceName(String serviceName)
			{
				this._serviceName = serviceName;
				return this;
			}

			/**
			 * @param durationMs The duration of the Span event, in milliseconds
			 * @return The SpanBuilder object with its durationMs variable set to the given duration
			 */
			public SpanBuilder DurationMs(TimeSpan durationMs)
			{
				this._durationMs = durationMs;
				return this;
			}

			/**
			 * @param name The name of the Span event
			 * @return The SpanBuilder object with its name variable set to the given name
			 */
			public SpanBuilder Name(String name)
			{
				this._name = name;
				return this;
			}

			/**
			 * @param parentId The Id of the parent span for this Span event. If it is a root span, this
			 *     variable should stay null, or not set
			 * @return The SpanBuilder object with its parentId variable set to the given Parent ID
			 */
			public SpanBuilder ParentId(String parentId)
			{
				this._parentId = parentId;
				return this;
			}

			/**
			 * Call this to indicate that the span contains an error condition.
			 *
			 * @return The SpanBuilder instance with the error field set to true
			 */
			public SpanBuilder WithError()
			{
				this._error = true;
				return this;
			}

			/** @return A Span object with the variables assigned to the builder class */
			public Span Build()
			{
				return new Span(
					_id, _attributes, _traceId, _timestamp, _serviceName, _durationMs, _name, _parentId, _error);
			}

			/** @return A string representing this SpanBuilder object and listing its variables */
			public override string ToString()
			{
				return "Span.SpanBuilder(id="
					+ this._id
					+ ", attributes="
					+ this._attributes
					+ ", traceId="
					+ this._traceId
					+ ", timestamp="
					+ this._timestamp
					+ ", serviceName="
					+ this._serviceName
					+ ", durationMs="
					+ this._durationMs
					+ ", name="
					+ this._name
					+ ", parentId="
					+ this._parentId
					+ ")";
			}
		}
	}
}
