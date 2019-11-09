using System;
using System.Collections.Generic;
using System.Text;
using Newtonsoft.Json;

namespace FclEx.Json.Converters
{
    public abstract class ConverterWithDefault<TSelf> : JsonConverter where TSelf : JsonConverter
    {
        public static DefaultResolver<TSelf> DefaultResolver { get; } = new DefaultResolver<TSelf>();
        public static JsonSerializerSettings DefaultSettings { get; } = new JsonSerializerSettings() { ContractResolver = DefaultResolver };
    }
}
