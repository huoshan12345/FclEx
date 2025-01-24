namespace FclEx.Utils;

public readonly record struct TableData(
    string Title, 
    string? TableTitle, 
    IEnumerable<string> Columns, 
    IEnumerable<string?[]> Rows, 
    bool RenderColumns = true);