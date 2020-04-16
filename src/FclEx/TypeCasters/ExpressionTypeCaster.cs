using System;
using System.Collections.Generic;
using System.Linq.Expressions;
using System.Text;

namespace FclEx.TypeCasters
{
    public class Caster<TInput>
    {
        public static TOutput CastTo<TOutput>(TInput s)
        {
            return Cache<TOutput>.Caster(s);
        }

        private static class Cache<TOutput>
        {
            public static readonly Func<TInput, TOutput> Caster = Get();

            private static Func<TInput, TOutput> Get()
            {
                var p = Expression.Parameter(typeof(TInput));
                var c = Expression.ConvertChecked(p, typeof(TOutput));
                return Expression.Lambda<Func<TInput, TOutput>>(c, p).Compile();
            }
        }
    }

    public class ExpressionTypeCaster : AbstractTypeCaster<ExpressionTypeCaster>
    {
        public override TOutput CastTo<TInput, TOutput>(TInput obj)
        {
            return Caster<TInput>.CastTo<TOutput>(obj);
        }
    }
}
