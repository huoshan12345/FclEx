namespace FclEx.TypeCasters;

public class Caster<TInput>
{
    public static TOutput? CastTo<TOutput>(TInput? obj)
    {
        return obj == null 
            ? default 
            : Cache<TOutput>.Caster(obj);
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

public sealed class ExpressionTypeCaster : AbstractTypeCaster<ExpressionTypeCaster>
{
    [return: MaybeNull]
    public override TOutput CastTo<TInput, TOutput>([AllowNull] TInput obj)
    {
        return Caster<TInput>.CastTo<TOutput>(obj);
    }
}