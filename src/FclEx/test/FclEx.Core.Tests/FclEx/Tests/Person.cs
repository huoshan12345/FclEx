namespace FclEx.Tests;

public record Person
{
    public int Id { get; set; }
    public string Name { get; set; } = "";
    public int Age { get; set; }
    public int? CoinCount { get; set; }
    public int Height { get; set; }
    public object? Obj { get; set; }
}
