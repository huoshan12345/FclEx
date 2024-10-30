namespace FclEx.TestModels;

[SuppressMessage("ReSharper", "PropertyCanBeMadeInitOnly.Global")]
public record Person
{
    public int Id { get; set; }
    public string Name { get; set; } = "";
    public int Age { get; set; }
    public int? CoinCount { get; set; }
    public int Height { get; set; }
    public object? Obj { get; set; }
}

public class TestClass
{
    public int Int { get; set; }
    public DateTime DateTime { get; set; }
    public string? String { get; set; }
}

public struct TestStruct
{
    public int Int { get; set; }
    public DateTime DateTime { get; set; }
    public string? String { get; set; }
}

public record TestRecord(int Int, DateTime DateTime, string String);

public readonly record struct TestRecordStruct(int Int, DateTime DateTime, string String);