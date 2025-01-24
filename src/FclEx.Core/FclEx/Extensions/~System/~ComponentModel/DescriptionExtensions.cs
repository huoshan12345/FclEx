using System.ComponentModel;

namespace FclEx.Extensions;

[SuppressMessage("ReSharper", "MergeConditionalExpressionWhenPossible")]
public static class DescriptionExtensions
{
    /// <summary>
    ///  Gets the description supplied by a System.ComponentModel.DescriptionAttribute if one is set.
    /// </summary>
    /// <param name="enum"></param>
    /// <returns></returns>
    public static string GetDescription<T>(this T @enum) where T : struct, Enum
    {
        var str = @enum.ToString();
        var field = typeof(T).GetField(str);
        return field is null ? str : GetDescription(field);
    }

    public static string GetDescription(this MemberInfo member)
    {
        var att = member.GetCustomAttribute<DescriptionAttribute>(false);
        return att is null ? member.Name : att.Description;
    }

    public static string GetDescription(this Type type)
    {
        var att = type.GetCustomAttribute<DescriptionAttribute>(false);
        return att is null ? type.Name : att.Description;
    }
}