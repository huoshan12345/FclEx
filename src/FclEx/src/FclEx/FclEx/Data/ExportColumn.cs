using System;

namespace FclEx.Data;

public static class ExportColumn
{
    public static IExportColumn<T> Create<T>(string title, Func<T, int, object> funcGetValue)
    {
        return new ExportColumn<T>(title, funcGetValue);
    }

    public static IExportColumn<T> Create<T>(string title, Func<T, object> funcGetValue)
    {
        return new ExportColumn<T>(title, (m, i) => funcGetValue(m));
    }
}

/// <summary>
/// 导出列
/// </summary>
/// <typeparam name="T"></typeparam>
public class ExportColumn<T> : IExportColumn<T>
{
    public ExportColumn(string title, Func<T, int, object> funcGetValue)
    {
        if (string.IsNullOrEmpty(title)) throw new ArgumentNullException(nameof(title));
        Title = title;
        _funcGetValue = funcGetValue ?? throw new ArgumentNullException(nameof(funcGetValue));
    }

    public ExportColumn(string title, Func<T, object> funcGetValue)
        : this(title, (m, i) => funcGetValue(m))
    {
    }

    private readonly Func<T, int, object> _funcGetValue;
    public string Title { get; private set; }

    public object GetValue(T row, int index)
    {
        return _funcGetValue(row, index);
    }
}