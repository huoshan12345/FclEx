using static FclEx.Utils.NameValueOmitOption;

namespace FclEx.Utils;

public interface INameValuesBuilder
{
    NameValuesBuilderOptions Options
#if NET6_0_OR_GREATER
        => NameValuesBuilderOptions.Default;
#else
    { get; }
#endif

    List<KeyValuePair<string, string>> Build()
#if NET6_0_OR_GREATER
        => DefaultNameValuesBuilder.Build(this);
#else
    ;
#endif

    string? ToString<T>(T? value, string? format)
#if NET6_0_OR_GREATER
        => DefaultNameValuesBuilder.ToString(value, format);
#else
    ;
#endif
}

public static class DefaultNameValuesBuilder
{
    public static string? ToString<T>(T? value, string? format)
    {
        return value is IFormattable formattable
            ? formattable.ToString(format, CultureInfo.InvariantCulture)
            : value?.ToString();
    }

    public static List<KeyValuePair<string, string>> Build(INameValuesBuilder builder)
    {
        var list = new List<KeyValuePair<string, string>>();

        var members = builder.GetType().GetDataMembers()
            .Where(m => m is { IsStatic: false, HasPublicGetter: true });

        foreach (var member in members)
        {
            if (member.TryGetAttribute<NameValueAttribute>(false, out var nameValueAttribute) == false)
                continue;

            var omit = nameValueAttribute.OmitOption;
            if (omit == Unset)
                omit = builder.Options.OmitOption;

            var name = nameValueAttribute.Name ?? member.Name;
            var value = member.GetValue(builder);
            var type = member.DataMemberType.UnwrapNullable();

            if (value is not null)
            {
                if (type.IsEnum)
                {
                    var enums = type.GetMember(value.ToString()!);
                    if (enums is [var enumMember, ..]
                        && enumMember.TryGetAttribute<EnumMemberAttribute>(false, out var memberAttr))
                    {
                        value = memberAttr.Value;
                    }
                }
                else if (type == typeof(bool))
                {
                    var convention = nameValueAttribute.BoolValueConvention;
                    if (convention == BoolValueConvention.Unset)
                        convention = builder.Options.BoolValueConvention;

                    var flag = (bool)value;
                    value = convention switch
                    {
                        BoolValueConvention.AsLowercase => flag ? "true" : "false",
                        BoolValueConvention.AsNumber => flag ? 1 : 0,
                        _ => value,
                    };
                }
            }

            if (omit.HasFlag(WhenNull) && value is null
                || omit.HasFlag(WhenEmpty) && value is IEnumerable e && e.IsNullOrEmpty()
                || omit.HasFlag(WhenDefault) && value == type.DefaultValue())
            {
                continue;
            }

            list.Add(new(name, builder.ToString(value, nameValueAttribute.Format) ?? ""));
        }

        return list;
    }
}

public class NameValuesBuilder : INameValuesBuilder
{
    public virtual NameValuesBuilderOptions Options { get; }
        = NameValuesBuilderOptions.Default;
    public virtual List<KeyValuePair<string, string>> Build()
        => DefaultNameValuesBuilder.Build(this);
    public string? ToString<T>(T? value, string? format)
        => DefaultNameValuesBuilder.ToString(value, format);
}