namespace System.Text.Json;

internal class ArrayIndexFilter : PathFilter
{
    public int? Index { get; set; }

    public override IEnumerable<JsonElement?> ExecuteFilter(JsonElement root, IEnumerable<JsonElement?> current, bool errorWhenNoMatch)
    {
        foreach (var t in current.NotNull())
        {
            if (Index != null)
            {
                var v = GetTokenIndex(t, errorWhenNoMatch, Index.Get());

                if (v != null)
                {
                    yield return v;
                }
            }
            else
            {
                if (t.ValueKind == JsonValueKind.Array)
                {
                    foreach (var v in t.EnumerateArray())
                    {
                        yield return v;
                    }
                }
                else
                {
                    if (errorWhenNoMatch)
                    {
                        throw new JsonException($"Index * not valid on {t.GetType().Name}.");
                    }
                }
            }
        }
    }
}