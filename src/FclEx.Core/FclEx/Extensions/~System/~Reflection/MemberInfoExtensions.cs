namespace FclEx.Extensions;

public static class MemberInfoExtensions
{
    /// <summary>Determines whether an attribute of type <typeparamref name="T"/> is applied to a member.</summary>
    /// <typeparam name="T">The attribute type to locate.</typeparam>
    /// <param name="memberInfo">The member metadata to inspect.</param>
    /// <param name="inherit">Whether to search the inheritance chain when the member kind supports it.</param>
    /// <returns><see langword="true"/> when the attribute is defined; otherwise, <see langword="false"/>.</returns>
    public static bool IsDefined<T>(this MemberInfo memberInfo, bool inherit = false) where T : Attribute
    {
        return memberInfo.IsDefined(typeof(T), inherit);
    }

    public static DataMemberInfo ToDataMemberInfo(this MemberInfo memberInfo)
    {
        return memberInfo switch
        {
            PropertyInfo propInfo => new DataMemberInfo(propInfo),
            FieldInfo fieldInfo => new DataMemberInfo(fieldInfo),
            _ => throw new ArgumentException($"MemberInfo '{memberInfo.Name}' refers to neither a field nor a property.")
        };
    }

    public static bool IsCompilerGenerated(this MemberInfo memberInfo, bool inherit = false)
    {
        return memberInfo.IsDefined<CompilerGeneratedAttribute>(inherit);
    }

    internal static Type GetDataMemberType(this MemberInfo member)
    {
        return member switch
        {
            PropertyInfo propInfo => propInfo.PropertyType,
            FieldInfo fieldInfo => fieldInfo.FieldType,
            DataMemberInfo dataMemberInfo => dataMemberInfo.DataMemberType,
            _ => throw new ArgumentException($"MemberInfo '{member.Name}' refers to neither a field nor a property.")
        };
    }

}
