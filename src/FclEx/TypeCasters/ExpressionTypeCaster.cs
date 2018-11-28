using System;
using System.Collections.Generic;
using System.Linq.Expressions;
using System.Text;

namespace FclEx.TypeCasters
{
    public class ExpressionTypeCaster : AbstractTypeCaster<ExpressionTypeCaster>
    {
        public override TOutput CastTo<TInput, TOutput>(TInput obj)
        {
            return ConvertHelper<TInput, TOutput>.CastTo(obj);
        }

        private static class ConvertHelper<TInput, TOutput>
        {
            private static readonly Converter<TInput, TOutput> _converter;
            static ConvertHelper()
            {
                var p = Expression.Parameter(typeof(TInput));
                var e = Expression.Convert(p, typeof(TOutput));
                _converter = Expression.Lambda<Converter<TInput, TOutput>>(e, p).Compile();
            }
            public static TOutput CastTo(TInput obj) => _converter.Invoke(obj);
        }
    }
}
