namespace System.Text.Json;

internal class ScanFilter : PathFilter
{
    internal string? Name { get; }

    public ScanFilter(string? name)
    {
        Name = name;
    }

    public override IEnumerable<JsonElement?> ExecuteFilter(JsonElement root, IEnumerable<JsonElement?> current, bool errorWhenNoMatch)
    {
        foreach (var c in current.NotNull())
        {
            foreach (var e in GetNextScanValue(c))
            {
                if (e.Name == Name)
                {
                    yield return e.Value;
                }
            }
        }
    }
}