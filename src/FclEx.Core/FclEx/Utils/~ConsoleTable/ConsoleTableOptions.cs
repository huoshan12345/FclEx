namespace FclEx.Utils;

public readonly record struct ConsoleTableOptions(
    IEnumerable<object>? Columns = null, 
    bool RenderColumns = false, 
    string? Title = null);