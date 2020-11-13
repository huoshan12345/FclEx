using System;
using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;
using System.Linq;
using System.Threading.Tasks;
using FclEx.Serilog.Formatting;
using FclEx.Serilog.Sinks.Logstash.Inputs;
using Serilog.Debugging;
using Serilog.Events;
using Serilog.Formatting;
using Serilog.Sinks.PeriodicBatching;

namespace FclEx.Serilog.Sinks.Logstash
{
    public class LogstashSink : PeriodicBatchingSink
    {
        private readonly LogstashSinkOptions _options;
        private readonly ITextFormatter _formatter;
        private readonly ILogstashInput _input;

        public LogstashSink(LogstashSinkOptions options)
            : base(options.BatchSizeLimit, options.Period, options.QueueLimit)
        {
            _options = options;
            var uri = new Uri(options.Uri);
            var type = uri.Scheme.ToEnum(LogstashInputType.Tcp);
            _input = LogstashInputFactory.Create(type, uri);
            _formatter = options.Formatter ?? new LogstashJsonFormatter();
        }

        [SuppressMessage("ReSharper", "PossibleMultipleEnumeration")]
        protected override async Task EmitBatchAsync(IEnumerable<LogEvent> events)
        {
            try
            {
                var strs = events.Select(m => m.ToString(_formatter))
                    .ToList();
                await _input.SendAsync(strs).DonotCapture();
            }
            catch (Exception ex)
            {
                HandleException(ex, events);
            }
        }

        [SuppressMessage("ReSharper", "PossibleMultipleEnumeration")]
        protected virtual void HandleException(Exception ex, IEnumerable<LogEvent> events)
        {
            var flags = _options.EmitEventFailure;
            if (flags.HasFlag(EmitEventFailureHandling.WriteToSelfLog))
            {
                // ES reports an error, output the error to the selflog
                SelfLog.WriteLine("Caught exception while preforming bulk operation to Elasticsearch: {0}", ex);
            }
            if (flags.HasFlag(EmitEventFailureHandling.WriteToFailureSink)
                && _options.FailureSink != null)
            {
                // Send to a failure sink
                try
                {
                    foreach (var e in events)
                    {
                        _options.FailureSink.Emit(e);
                    }
                }
                catch (Exception exSink)
                {
                    // We do not let this fail too
                    SelfLog.WriteLine("Caught exception while emitting to sink {1}: {0}", exSink, _options.FailureSink);
                }
            }
            if (flags.HasFlag(EmitEventFailureHandling.RaiseCallback)
                && _options.FailureCallback != null)
            {
                // Send to a failure callback
                try
                {
                    foreach (var e in events)
                    {
                        _options.FailureCallback(e);
                    }
                }
                catch (Exception exCallback)
                {
                    // We do not let this fail too
                    SelfLog.WriteLine("Caught exception while emitting to callback {1}: {0}", exCallback, _options.FailureCallback);
                }
            }
            if (_options.EmitEventFailure.HasFlag(EmitEventFailureHandling.ThrowException))
                throw ex;
        }
    }
}
