using System;

namespace FclEx.Utils;

/// <summary>
/// Represents the possible outcomes of a comparison operation.
/// </summary>
public enum ComparisonOperator
{
    /// <summary>
    /// Indicates that two values are equal.
    /// </summary>
    Equal,

    /// <summary>
    /// Indicates that two values are not equal.
    /// </summary>
    NotEqual,

    /// <summary>
    /// Indicates that one value is greater than another.
    /// </summary>
    GreaterThan,

    /// <summary>
    /// Indicates that one value is greater than or equal to another.
    /// </summary>
    GreaterThanOrEqual,

    /// <summary>
    /// Indicates that one value is less than another.
    /// </summary>
    LessThan,

    /// <summary>
    /// Indicates that one value is less than or equal to another.
    /// </summary>
    LessThanOrEqual,
}

public static class ComparisonOperatorExtensions
{
    public static string ToOperatorString(this ComparisonOperator @operator)
    {
        return @operator switch
        {
            ComparisonOperator.Equal => "=",
            ComparisonOperator.NotEqual => "!=",
            ComparisonOperator.GreaterThan => ">",
            ComparisonOperator.GreaterThanOrEqual => ">=",
            ComparisonOperator.LessThan => "<",
            ComparisonOperator.LessThanOrEqual => "<=",
            _ => throw new ArgumentOutOfRangeException(nameof(@operator), @operator, null),
        };
    }
}