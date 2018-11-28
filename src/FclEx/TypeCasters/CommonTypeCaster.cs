using System;
using System.Collections.Generic;
using System.Text;

namespace FclEx.TypeCasters
{
    public class CommonTypeCaster : AbstractTypeCaster<CommonTypeCaster>
    {
        public override TOutput CastTo<TInput, TOutput>(TInput obj)
        {
            var type = typeof(TOutput);
            return type.IsValueType
                ? ChangeType<TInput, TOutput>(obj, type)
                : (TOutput)(object)obj;
        }

        private static TOutput ChangeType<TInput, TOutput>(TInput value, Type t)
        {
            if (t.IsGenericType && t.GetGenericTypeDefinition() == typeof(Nullable<>))
            {
                if (value == null) return default;
                t = Nullable.GetUnderlyingType(t);
            }
            else if (t.IsEnum)
            {
                t = t.GetEnumUnderlyingType();
            }
            return (TOutput)Convert.ChangeType(value, t);
        }
    }
}
