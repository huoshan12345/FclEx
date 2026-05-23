namespace System.Text.Json;

internal class QueryFilter : PathFilter
{
    internal QueryExpression Expression { get; }

    public QueryFilter(QueryExpression expression)
    {
        Expression = expression;
    }

    public override IEnumerable<JsonElement?> ExecuteFilter(JsonElement root, IEnumerable<JsonElement?> current, bool errorWhenNoMatch)
    {
        foreach (var el in current.NotNull())
        {
            if (el.ValueKind == JsonValueKind.Array)
            {
                foreach (var v in el.EnumerateArray())
                {
                    if (Expression.IsMatch(root, v))
                    {
                        yield return v;
                    }
                }
            }
            else if (el.ValueKind == JsonValueKind.Object)
            {
                foreach (var v in el.EnumerateObject())
                {
                    if (Expression.IsMatch(root, v.Value))
                    {
                        yield return v.Value;
                    }
                }
            }
        }
    }
}