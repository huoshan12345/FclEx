namespace FclEx.Xunit;

public class JsonSerializableTheoryData<T> : TheoryData<XunitJsonSerializable<T>>
{
    public JsonSerializableTheoryData(params IEnumerable<T> values)
    {
        AddRange([.. values]);
    }

    public void Add(T p)
    {
        AddRange(XunitJsonSerializable.Create(p));
    }

    public void AddRange(params T[] values)
    {
        foreach (var value in values)
        {
            Add(value);
        }
    }
}