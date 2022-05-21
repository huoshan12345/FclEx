using System;
using Serilog.Core;
using Serilog.Events;
using Serilog.Formatting;

namespace FclEx.Serilog.Sinks.Logstash
{
    public class LogstashSinkOptions
    {
        public LogstashSinkOptions(string uri)
        {
            Check.NotEmpty(uri);
            Uri = uri;
        }

        public string Uri { get; }

        /// <summary>
        /// The maximum number of events to include in a single batch.
        /// </summary>
        public int BatchSizeLimit { get; set; } = 10;

        /// <summary>
        /// The time to wait between checking for event batches.
        /// </summary>
        public TimeSpan Period { get; set; } = TimeSpan.FromSeconds(3);

        /// <summary>
        /// The maximum number of events that will be held in-memory while waiting to ship them to Logstash.
        /// Beyond this limit, events will be dropped. The default is 100,000.
        /// Has no effect on durable log shipping.
        /// </summary>
        public int QueueLimit { get; set; } = 100000;

        /// <summary>
        /// Customizes the formatter used when converting log events into ElasticSearch documents.
        /// Please note that the formatter output must be valid JSON :)
        /// </summary>
        public ITextFormatter? Formatter { get; set; }

        /// <summary>
        /// The minimum log event level required in order to write an event to the sink.
        /// Ignored when LoggingLevelSwitch is specified.
        /// </summary>
        public LogEventLevel? MinimumLogEventLevel { get; set; }

        /// <summary>
        /// A switch allowing the pass-through minimum level to be changed at runtime.
        /// </summary>
        public LoggingLevelSwitch? LevelSwitch { get; set; }
    }
}
