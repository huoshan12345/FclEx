namespace FclEx.Dapper;

public static class DynamicParametersExtensions
{
    public static DynamicParameters ToDynamicParameters<T>(
        this IEnumerable<T> enumerable,
        Func<T, int, string> nameSelector,
        Func<T, int, object?> valueSelector,
        Func<T, int, DbType>? dbTypeSelector = null)
    {
        var param = new DynamicParameters();
        foreach (var (i, item) in enumerable.Index())
        {
            param.Add(nameSelector(item, i), valueSelector(item, i), dbTypeSelector?.Invoke(item, i));
        }
        return param;
    }

    public static DynamicParameters ToDynamicParameters<T>(
        this IEnumerable<T> enumerable,
        Func<T, string> nameSelector,
        Func<T, object?> valueSelector,
        Func<T, DbType>? dbTypeSelector = null)
    {
        var param = new DynamicParameters();
        foreach (var item in enumerable)
        {
            param.Add(nameSelector(item), valueSelector(item), dbTypeSelector?.Invoke(item));
        }
        return param;
    }

    public static IEnumerable<string> PrefixedNames(this DynamicParameters parameters)
    {
        return parameters.ParameterNames.Select(m => $"@{m}");
    }
}
