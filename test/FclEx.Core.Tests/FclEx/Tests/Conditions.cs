namespace FclEx.Tests;

public static class Conditions
{
    public static readonly IntCondition NET60_OR_GREATER = new(Environment.Version.Major, ComparisonOperator.GreaterThan, 6);
}