namespace FclEx.Utils;

public record RightComparableCondition<T>(ComparisonOperator Operator, T Right) where T : IComparable<T>
{
    public override string ToString()
    {
        return $"{Operator.ToOperatorString()} {Right}";
    }

    public bool IsMatch(T left)
    {
        return ComparableCondition.IsMatch(left, Operator, Right);
    }
}