namespace FclEx.Extensions;

public static class ExpressionExtensions
{
    public static Expression<Func<T, bool>> Or<T>(this Expression<Func<T, bool>>? left, Expression<Func<T, bool>>? right)
    {
        if (Check.TryGetSingleNonNull(left, right, out var result))
            return result;

        var parameter = left.Parameters[0];
        var r = ExpressionReplacer.Replace(right.Body, right.Parameters[0], parameter);
        var body = Expression.OrElse(left.Body, r);
        var lambda = Expression.Lambda<Func<T, bool>>(body, parameter);
        return lambda;
    }

    public static Expression<Func<T, bool>> And<T>(this Expression<Func<T, bool>>? left, Expression<Func<T, bool>>? right)
    {
        if (Check.TryGetSingleNonNull(left, right, out var result))
            return result;

        var parameter = left.Parameters[0];
        var r = ExpressionReplacer.Replace(right.Body, right.Parameters[0], parameter);
        var body = Expression.AndAlso(left.Body, r);
        var lambda = Expression.Lambda<Func<T, bool>>(body, parameter);
        return lambda;
    }

    public static LambdaExpression Lambda(this Expression expression, params ParameterExpression[] parameters)
        => Expression.Lambda(expression, parameters);

    public static Expression<TDelegate> Lambda<TDelegate>(this Expression expression, params ParameterExpression[] parameters)
        where TDelegate : Delegate
        => Expression.Lambda<TDelegate>(expression, parameters);

    public static Expression Convert(this Expression expression, Type type) => Expression.Convert(expression, type);

    public static IEnumerable<Expression> Enumerate(this BlockExpression block)
    {
        foreach (var exp in block.Expressions)
        {
            if (exp is BlockExpression b)
            {
                foreach (var m in b.Enumerate())
                {
                    yield return m;
                }
            }
            else
            {
                yield return exp;
            }
        }
    }

    /// <summary>
    /// Evaluates each argument expression and returns its resulting value.
    /// </summary>
    /// <param name="arguments">The argument expressions to evaluate.</param>
    /// <returns>A deferred sequence containing the value produced by each expression.</returns>
    /// <remarks>
    /// Non-constant expressions are compiled and invoked. Enumerating the result can therefore execute arbitrary
    /// user code, produce side effects, or throw exceptions. Expressions containing unbound parameters cannot be evaluated.
    /// </remarks>
    public static IEnumerable<object?> EvaluateArguments(this IEnumerable<Expression> arguments)
    {
        return arguments.Select(e => e switch
        {
            ConstantExpression constant => constant.Value,
            _ => e.Convert(typeof(object)).Lambda<Func<object>>().Compile().Invoke()
        });
    }

    /// <summary>
    /// Converts a strongly typed selector expression (<typeparamref name="TMember"/>) 
    /// into an expression returning <see cref="object"/>.<br/>
    /// This is useful when you need a generic property accessor without knowing the member type at compile time.
    /// </summary>
    /// <typeparam name="T">The source type.</typeparam>
    /// <typeparam name="TMember">The member type of the selector.</typeparam>
    /// <param name="selector">An expression selecting a member of <typeparamref name="T"/>.</param>
    /// <returns>
    /// An equivalent expression returning <see cref="object"/> instead of <typeparamref name="TMember"/>.
    /// </returns>
    public static Expression<Func<T, object?>> ToObjectSelector<T, TMember>(this Expression<Func<T, TMember>> selector)
    {
        return ExpressionHelper.ToObjectSelector(selector);
    }
}
