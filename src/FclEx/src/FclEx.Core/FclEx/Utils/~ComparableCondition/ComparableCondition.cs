namespace FclEx.Utils;

/// <summary>
/// Represents a condition for comparing two values of type T.
/// </summary>
/// <typeparam name="T">The type of the values to compare, which must implement <see cref="IComparable{T}"/>.</typeparam>
/// <param name="Left">The first value to compare.</param>
/// <param name="Operator">The comparison operator used to evaluate the relationship between the left and right values.</param>
/// <param name="Right">The second value to compare.</param>
public record ComparableCondition<T>(T Left, ComparisonOperator Operator, T Right) where T : IComparable<T>
{
    /// <summary>
    /// Returns a string representation of the comparison condition, including the left value, operator, and right value.
    /// </summary>
    /// <returns>A string that describes the comparison.</returns>
    public override string ToString()
    {
        return $"{Left} {Operator.ToOperatorString()} {Right}";
    }

    /// <summary>
    /// Checks whether the left value matches the comparison condition against the right value.
    /// </summary>
    /// <returns>true if the left value satisfies the condition; otherwise, false.</returns>
    public bool IsMatch()
    {
        return ComparableCondition.IsMatch(Left, Operator, Right);
    }
}

/// <summary>
/// Provides static methods for comparing values using various comparison operators.
/// </summary>
public static class ComparableCondition
{
    /// <summary>
    /// Checks whether the left value matches the specified comparison operator against the right value.
    /// </summary>
    /// <typeparam name="T">The type of the values to compare, which must implement IComparable&lt;T&gt;.</typeparam>
    /// <param name="left">The first value to compare.</param>
    /// <param name="operator">The comparison operator used to evaluate the relationship between the left and right values.</param>
    /// <param name="right">The second value to compare.</param>
    /// <returns>true if the left value satisfies the condition defined by the operator and the right value; otherwise, false.</returns>
    public static bool IsMatch<T>(T left, ComparisonOperator @operator, T right) where T : IComparable<T>
    {
        var result = left.CompareTo(right);
        return @operator switch
        {
            ComparisonOperator.Equal => result == 0,
            ComparisonOperator.NotEqual => result != 0,
            ComparisonOperator.GreaterThan => result > 0,
            ComparisonOperator.GreaterThanOrEqual => result >= 0,
            ComparisonOperator.LessThan => result < 0,
            ComparisonOperator.LessThanOrEqual => result <= 0,
            _ => throw new ArgumentOutOfRangeException(nameof(@operator), @operator, null),
        };
    }
}