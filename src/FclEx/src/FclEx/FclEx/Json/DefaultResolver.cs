using System;
using System.Collections.Generic;
using System.Text;
using FclEx.Json.Converters;
using Newtonsoft.Json;
using Newtonsoft.Json.Serialization;

namespace FclEx.Json
{
    public class DefaultResolver<T> : DefaultContractResolver
        where T : JsonConverter
    {
        protected override JsonObjectContract CreateObjectContract(Type objectType)
        {
            var contract = base.CreateObjectContract(objectType);
            if (contract.Converter != null && contract.Converter.GetType() == typeof(T))
                contract.Converter = null;
            return contract;
        }
    }
}
