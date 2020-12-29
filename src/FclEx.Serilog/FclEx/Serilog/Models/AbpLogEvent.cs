using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Runtime.InteropServices;
using FclEx.Extensions.Json;
using FclEx.Helpers;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using Serilog.Core;
using Serilog.Events;
using Serilog.Extensions.Logging;
using Serilog.Parsing;

namespace FclEx.Serilog.Models
{
    public class AbpLogEvent
    {
        public DateTime TimeStamp { get; set; }
        public string? Message { get; set; }
        public JsonEx? Exception { get; set; }
        public string? Level { get; set; }
        public IList<string>? Renderings { get; set; }
        public string? LoggerName { get; set; }

        public static JObject Create(LogEvent logEvent, IList<string>? excludePaths)
        {
            var @event = new AbpLogEvent
            {
                TimeStamp = logEvent.Timestamp.UtcDateTime,
                Exception = JsonEx.Create(logEvent.Exception),
                Message = logEvent.MessageTemplate.Text,
                Level = LevelConvert.ToExtensionsLevel(logEvent.Level).ToString(),
            };

            var tokensWithFormat = logEvent.MessageTemplate.Tokens
                .OfType<PropertyToken>()
                .Where(pt => pt.Format != null);

            @event.Renderings = tokensWithFormat.Select(m => m.Render(logEvent.Properties)).ToList();

            if (logEvent.Properties.TryGetValue(Constants.SourceContextPropertyName, out var sourceContextProperty) &&
                sourceContextProperty is ScalarValue sourceContextValue &&
                sourceContextValue.Value is string sourceContext)
            {
                @event.LoggerName = sourceContext;
            }

            var jObj = JObject.FromObject(@event, JsonHelper.CamelSerializer);

            var propDic = logEvent.Properties
                .ToDictionary(m => m.Key, m => m.Value.Render());

            var propDicObj = JObject.FromObject(propDic, JsonHelper.CamelSerializer);

            jObj.Merge(propDicObj);

            if (@event.Message.TryToJObject(out var json))
            {
                jObj.Remove("message");
                jObj.Merge(json!);
            }

            if (excludePaths.IsValid())
            {
                var list = excludePaths.NotNull()
                    .SelectMany(m => jObj.SelectTokens(m))
                    .NotNull()
                    .Distinct(m => m!.Path)
                    .ToList();
                list.ForEach(m => m.Parent?.Remove());
            }
            return jObj;
        }
    }
}
