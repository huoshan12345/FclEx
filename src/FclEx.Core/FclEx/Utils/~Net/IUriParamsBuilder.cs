using static FclEx.Utils.UriParamOmitOption;

namespace FclEx.Utils;

public interface IUriParamsBuilder
{
    UriParamsBuilderOptions Options { get; }

    IReadOnlyCollection<UriParam> Build()
#if NET6_0_OR_GREATER
    {
        var list = new List<UriParam>();

        var members = GetType().GetDataMembers()
            .Where(m => m is { IsStatic: false, HasPublicGetter: true });

        foreach (var member in members)
        {
            if (member.TryGetAttribute<UriParamAttribute>(false, out var queryAttr) == false)
                continue;

            var omit = queryAttr.OmitOption;
            if (omit == Unset)
                omit = Options.OmitOption;

            var name = queryAttr.Name ?? member.Name;
            var value = member.GetValue(this);
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
                    var convention = queryAttr.BoolValueConvention;
                    if (convention == BoolValueConvention.Unset)
                        convention = Options.BoolValueConvention;

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

            list.Add(new(name, value.ToStringOrEmpty()));
        }

        return list;
    }
#else
;
#endif
}