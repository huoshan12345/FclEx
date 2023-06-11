using System.Collections.Generic;
using System.Linq.Expressions;

namespace FclEx.TypeCasters;

public sealed class DelegateTypeCaster : AbstractTypeCaster<DelegateTypeCaster>
{
    [return: MaybeNull]
    public override TOutput CastTo<TInput, TOutput>([AllowNull]TInput obj)
    {
        return obj == null
            ? default
            : ConvertHelper<TOutput>.ConvertTo(obj);
    }

    private static class ConvertHelper<TOutput>
    {
        private delegate TOutput Converter(object obj);
        private static readonly ConcurrentDictionary<Type, Converter> _dic = new();

        public static TOutput ConvertTo(object obj)
        {
            var converter = _dic.GetOrAdd(obj.GetType(), t =>
            {
                var p = Expression.Parameter(typeof(object));
                var exp = (Expression)p;
                if (t != typeof(object))
                {
                    exp = Expression.Convert(exp, t);
                }
                var lambda = Expression.Convert(exp, typeof(TOutput));
                return Expression.Lambda<Converter>(lambda, p).Compile();
            });
            return converter.Invoke(obj);
        }
    }
}