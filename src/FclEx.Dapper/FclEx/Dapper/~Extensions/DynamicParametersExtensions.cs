namespace FclEx.Dapper;

/// <summary>
/// Provides helpers for constructing and inspecting Dapper <see cref="DynamicParameters"/> instances.
/// </summary>
public static class DynamicParametersExtensions
{
    /// <summary>
    /// Creates parameters by enumerating values and invoking index-aware selectors for each value.
    /// </summary>
    /// <typeparam name="T">The source value type.</typeparam>
    /// <param name="enumerable">The values to convert to parameters.</param>
    /// <param name="nameSelector">Selects each parameter name from a value and its zero-based index.</param>
    /// <param name="valueSelector">Selects each parameter value from a value and its zero-based index.</param>
    /// <param name="dbTypeSelector">Optionally selects an explicit database type from a value and its zero-based index.</param>
    /// <returns>A new parameter collection populated in source enumeration order.</returns>
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

    /// <summary>
    /// Creates parameters by enumerating values and invoking selectors for each value.
    /// </summary>
    /// <typeparam name="T">The source value type.</typeparam>
    /// <param name="enumerable">The values to convert to parameters.</param>
    /// <param name="nameSelector">Selects each parameter name.</param>
    /// <param name="valueSelector">Selects each parameter value.</param>
    /// <param name="dbTypeSelector">Optionally selects an explicit database type.</param>
    /// <returns>A new parameter collection populated in source enumeration order.</returns>
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

    /// <summary>
    /// Enumerates the parameter names reported by Dapper with an <c>@</c> prefix.
    /// </summary>
    /// <param name="parameters">The parameter collection to inspect.</param>
    /// <returns>A deferred sequence containing each reported name prefixed with <c>@</c>.</returns>
    public static IEnumerable<string> PrefixedNames(this DynamicParameters parameters)
    {
        return parameters.ParameterNames.Select(m => $"@{m}");
    }
}
