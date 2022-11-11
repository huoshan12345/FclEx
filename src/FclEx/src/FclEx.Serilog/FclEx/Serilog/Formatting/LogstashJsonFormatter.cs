using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using FclEx.Extensions;
using FclEx.Serilog.Models;
using Serilog.Core;
using Serilog.Events;
using Serilog.Extensions.Logging;
using Serilog.Formatting.Elasticsearch;
using Serilog.Parsing;

namespace FclEx.Serilog.Formatting
{
    public class LogstashJsonFormatter : ElasticsearchJsonFormatter
    {
        private static readonly ConcurrentDictionary<string, string> Map = new();

        private readonly bool _useCamelCase;

        public LogstashJsonFormatter(bool useCamelCase = true)
            : base(omitEnclosingObject: false,
                closingDelimiter: null,
                renderMessage: true,
                formatProvider: null,
                serializer: null,
                inlineFields: true,
                renderMessageTemplate: false,
                formatStackTraceAsArray: false)
        {
            _useCamelCase = useCamelCase;
        }

        protected override void WriteJsonProperty(string name, object value, ref string precedingDelimiter, TextWriter output)
        {
            if (_useCamelCase && name.IsValid())
            {
                var ch = name[0];
                if ('A' <= ch && ch <= 'Z')
                {
                    name = Map.GetOrAdd(name, m => (char)(ch + 32) + name.Substring(1));
                }
            }
            base.WriteJsonProperty(name, value, ref precedingDelimiter, output);
        }

        protected override void WriteLevel(LogEventLevel level, ref string delim, TextWriter output)
        {
            var l = LevelConvert.ToExtensionsLevel(level);
            WriteJsonProperty("Level", l, ref delim, output);
        }

        protected override void WriteException(Exception exception, ref string delim, TextWriter output)
        {
            output.Write(delim);
            output.Write("\"exception\":");
            var jsonEx = JsonEx.Create(exception);
            output.Write(jsonEx.ToJson(useCamelCase: _useCamelCase));
        }

        protected override void WriteRenderingsValues(IGrouping<string, PropertyToken>[] tokensWithFormat,
            IReadOnlyDictionary<string, LogEventPropertyValue> properties, TextWriter output)
        {
            var rdelim = "";
            foreach (var ptoken in tokensWithFormat)
            {
                output.Write(rdelim);
                rdelim = ",";
                output.Write("\"");
                output.Write(ptoken.Key);
                output.Write("\":[");

                var fdelim = "";
                foreach (var format in ptoken)
                {
                    output.Write(fdelim);
                    fdelim = ",";

                    output.Write("{");
                    var eldelim = "";

                    WriteJsonProperty("format", format.Format, ref eldelim, output);

                    var str = format.Render(properties);
                    WriteJsonProperty("rendering", str, ref eldelim, output);

                    output.Write("}");
                }

                output.Write("]");
            }
        }

        protected override void WritePropertiesValues(IReadOnlyDictionary<string, LogEventPropertyValue> properties, TextWriter output)
        {
            base.WritePropertiesValues(properties, output);
            var precedingDelimiter = ",";
            if (properties.TryGetValue(Constants.SourceContextPropertyName, out var sourceContextProperty) &&
                sourceContextProperty is ScalarValue sourceContextValue &&
                sourceContextValue.Value is string sourceContext)
            {
                WriteJsonProperty("loggerName", sourceContext, ref precedingDelimiter, output);
            }
        }
    }
}
