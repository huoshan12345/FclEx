namespace FclEx.Xunit;

public class SerializableTheoryData<T> : TheoryData<XunitSerializable<T>>
{
    public SerializableTheoryData(params IEnumerable<T> values)
    {
        AddRange([.. values]);
    }

    public void Add(T p)
    {
        AddRange(XunitSerializable.Create(p));
    }

    public void AddRange(params T[] values)
    {
        foreach (var value in values)
        {
            Add(value);
        }
    }
}