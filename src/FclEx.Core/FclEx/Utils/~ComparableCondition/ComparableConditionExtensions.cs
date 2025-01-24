namespace FclEx.Utils;

public static class ComparableConditionExtensions
{
    public static bool IsMatch<T>(this T left, RightComparableCondition<T> condition) where T : IComparable<T>
    {
        return condition.IsMatch(left);
    }

    public static bool IsNotMatch<T>(this ComparableCondition<T> condition) where T : IComparable<T>
    {
        return condition.IsMatch() == false;
    }
}