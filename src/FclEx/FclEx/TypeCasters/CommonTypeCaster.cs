using System;
using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;
using System.Text;

namespace FclEx.TypeCasters
{
    public class CommonTypeCaster : AbstractTypeCaster<CommonTypeCaster>
    {
        [return: MaybeNull]
        public override TOutput CastTo<TInput, TOutput>([AllowNull] TInput obj)
        {
            if (obj == null)
                return default;

            var type = typeof(TOutput);
            return type.IsValueType
                ? ChangeType<TInput, TOutput>(obj!, type)
                : (TOutput)(object)obj;
        }

        [return: MaybeNull]
        private static TOutput ChangeType<TInput, TOutput>(TInput value, Type t)
        {
            if (t.IsGenericType && t.GetGenericTypeDefinition() == typeof(Nullable<>))
            {
                if (value == null) return default;
                t = Nullable.GetUnderlyingType(t)!;
            }
            else if (t.IsEnum)
            {
                t = t.GetEnumUnderlyingType();
            }
            return (TOutput)Convert.ChangeType(value, t);
        }
    }
}
