namespace FclEx.Xunit;

public class SerializableTheoryData<T> : TheoryData<XunitSerializable<T>>
{
    public SerializableTheoryData(params IEnumerable<T> values)
    {
        AddRange([.. values]);
    }

    public void Add(T p)
    {
        AddRow(XunitSerializable.Create(p));
    }

    public void AddRange(params T[] values)
    {
        AddRows(values.Select(x => new object[] { XunitSerializable.Create(x) }));
    }
}
