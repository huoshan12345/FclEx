namespace FclEx.Utils;

public record IntCondition(int Left, ComparisonOperator Operator, int Right)
    : ComparableCondition<int>(Left, Operator, Right);