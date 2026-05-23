namespace System.Text.Json;

internal class ScanMultipleFilter : PathFilter
{
    public List<string> Names { get; }

    public ScanMultipleFilter(List<string> names)
    {
        Names = names;
    }

    public override IEnumerable<JsonElement?> ExecuteFilter(JsonElement root, IEnumerable<JsonElement?> current, bool errorWhenNoMatch)
    {
        foreach (var c in current.NotNull())
        {
            foreach (var e in GetNextScanValue(c))
            {
                if (e.Name == null)
                    continue;

                foreach (var name in Names)
                {
                    if (e.Name == name)
                    {
                        yield return e.Value;
                    }
                }
            }
        }
    }
}