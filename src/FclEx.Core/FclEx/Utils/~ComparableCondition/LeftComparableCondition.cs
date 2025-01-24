namespace FclEx.Utils;

public record LeftComparableCondition<T>(T Left, ComparisonOperator Operator) where T : IComparable<T>
{
    public override string ToString()
    {
        return $"{Left} {Operator.ToOperatorString()}";
    }

    public bool IsMatch(T right)
    {
        return ComparableCondition.IsMatch(Left, Operator, right);
    }
}