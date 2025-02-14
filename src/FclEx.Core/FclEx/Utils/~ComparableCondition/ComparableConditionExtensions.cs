namespace FclEx.Utils;

public static class ComparableConditionExtensions
{
    public static bool IsNotMatch<T>(this ComparableCondition<T> condition) where T : IComparable<T>
    {
        return condition.IsMatch() == false;
    }
}