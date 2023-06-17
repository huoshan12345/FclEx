using System.Linq.Expressions;

namespace FclEx.Data;

[AttributeUsage(AttributeTargets.Property, AllowMultiple = false, Inherited = true)]
public class ExportAttribute : Attribute
{
    public string Name { get; set; }
    public int Order { get; set; }

    public ExportAttribute(string? name = null)
    {
        Name = name ?? string.Empty;
    }
}

public abstract class ExportModel<TSelf> where TSelf : ExportModel<TSelf>, new()
{
    private static IList<IExportColumn<TSelf>>? _columns;
    public static IList<IExportColumn<TSelf>> Columns => _columns ??= new TSelf().GetColumnsRaw();

    protected static ExportColumn<TSelf> CreateColumn(string name, Func<TSelf, object> selector) => new(name, selector);

    protected static ExportColumn<TSelf> CreateColumn(string name, Func<TSelf, int, object> selector) => new(name, selector);

    protected virtual IList<IExportColumn<TSelf>> GetColumnsRaw()
    {
        var props = typeof(TSelf).GetProperties()
            .Where(m => m.CanRead)
            .Select((m, i) => (Index: i, Prop: m, ExportAttr: m.GetCustomAttribute<ExportAttribute>()))
            .Where(m => m.ExportAttr != null)
            .Select(m => (m.Index, m.Prop, m.ExportAttr, Title: (m.ExportAttr!.Name, m.Prop.Name).FirstValid()))
            .OrderBy(m => m.ExportAttr!.Order)
            .ThenBy(m => m.Index)
            .ThenBy(m => m.Title)
            .ToArray();

        return props.Select(m =>
        {
            var paraExp = Expression.Parameter(typeof(TSelf));
            var propExp = Expression.Property(paraExp, m.Prop);
            var e = Expression.Convert(propExp, typeof(object));
            var func = Expression.Lambda<Func<TSelf, object>>(e, paraExp);
            return CreateColumn(m.Title ?? string.Empty, func.Compile());
        }).Cast<IExportColumn<TSelf>>().ToArray();
    }
}