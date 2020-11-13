using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using Newtonsoft.Json;

namespace FclEx.Serilog.Models
{
    [JsonObject(ItemNullValueHandling = NullValueHandling.Ignore)]
    public class JsonEx
    {
        private static readonly char[] _newlineChars = Environment.NewLine.ToCharArray();

        public string? Type { get; set; }
        public string? Message { get; set; }
        public string? Source { get; set; }
        public int HResult { get; set; }
        public List<string>? StackTrace { get; set; }
        public IDictionary? Data { get; set; }
        public JsonEx? InnerException { get; set; }

        public static JsonEx? Create(Exception ex)
        {
            if (ex == null)
                return null;

            var jsonEx = new JsonEx
            {
                Type = ex.GetType().LongName(),
                Message = ex.Message,
                Source = ex.Source,
                StackTrace = ex.StackTrace?.Split(_newlineChars).Select(m => m.Trim()).Where(m => m.IsValid()).ToList(),
                Data = ex.Data
            };

            if (ex.InnerException != null)
            {
                jsonEx.InnerException = Create(ex.InnerException);
            }
            return jsonEx;
        }
    }
}
