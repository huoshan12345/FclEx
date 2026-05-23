namespace System.Text.Json;

internal class QueryScanFilter : PathFilter
{
    internal QueryExpression Expression { get; }

    public QueryScanFilter(QueryExpression expression)
    {
        Expression = expression;
    }

    public override IEnumerable<JsonElement?> ExecuteFilter(JsonElement root, IEnumerable<JsonElement?> current, bool errorWhenNoMatch)
    {
        foreach (var t in current.NotNull())
        {
            foreach (var (_, value) in GetNextScanValue(t))
            {
                if (Expression.IsMatch(root, value))
                {
                    yield return value;
                }
            }
        }
    }
}