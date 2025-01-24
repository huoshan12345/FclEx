namespace System.Text.Json;

internal class ArrayMultipleIndexFilter : PathFilter
{
    internal List<int> Indexes { get; }

    public ArrayMultipleIndexFilter(List<int> indexes)
    {
        Indexes = indexes;
    }

    public override IEnumerable<JsonElement?> ExecuteFilter(JsonElement root, IEnumerable<JsonElement?> current, bool errorWhenNoMatch)
    {
        foreach (var t in current.NotNull())
        {
            foreach (var i in Indexes)
            {
                var v = GetTokenIndex(t, errorWhenNoMatch, i);

                if (v != null)
                {
                    yield return v;
                }
            }
        }
    }
}