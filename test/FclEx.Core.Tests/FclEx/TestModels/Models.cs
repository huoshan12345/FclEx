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

public class CommonClass
{
    public int Int { get; set; }
    public DateTime DateTime { get; set; }
    public string? String { get; set; }
}

public struct CommonStruct
{
    public int Int { get; set; }
    public DateTime DateTime { get; set; }
    public string? String { get; set; }
}

public record CommonRecord(int Int, DateTime DateTime, string String);

public readonly record struct CommonRecordStruct(int Int, DateTime DateTime, string String);

public class EmptyClass;

public struct EmptyStruct;

public record EmptyRecord;

public readonly record struct EmptyRecordStruct;