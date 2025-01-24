namespace System.Reflection;

[DebuggerDisplay("{" + nameof(Name) + "}")]
public class DataMemberInfo : MemberInfo, IEquatable<DataMemberInfo>
{
    public DataMemberInfo(FieldInfo field)
    {
        MemberInfo = Check.NotNull(field);
        IsCompilerGenerated = MemberInfo.IsDefined(typeof(CompilerGeneratedAttribute), false);
        CanRead = true;
        CanWrite = true;
        Getter = field.GetValue;
        Setter = field.SetValue;
        IsStatic = field.IsStatic;
        IsField = true;
        IsProperty = false;
        DataMemberType = field.FieldType;
        HasPublicSetter = field.IsPublic;
        HasPublicGetter = field.IsPublic;
    }

    public DataMemberInfo(PropertyInfo property)
    {
        MemberInfo = Check.NotNull(property);
        IsCompilerGenerated = MemberInfo.IsDefined(typeof(CompilerGeneratedAttribute), false);
        CanRead = property.CanRead;
        CanWrite = property.CanWrite;
        Getter = property.GetValue;
        Setter = property.SetValue;
        var accessors = property.GetAccessors(true);
        IsStatic = accessors.Any(m => m.IsStatic);
        IsField = false;
        IsProperty = true;
        HasPublicSetter = property.GetSetMethod(true)?.IsPublic == true;
        HasPublicGetter = property.GetGetMethod(true)?.IsPublic == true;
        DataMemberType = property.PropertyType;
    }

    public override object[] GetCustomAttributes(bool inherit)
        => MemberInfo.GetCustomAttributes(inherit);

    public override object[] GetCustomAttributes(Type attributeType, bool inherit)
        => MemberInfo.GetCustomAttributes(attributeType, inherit);

    public override bool IsDefined(Type attributeType, bool inherit)
        => MemberInfo.IsDefined(attributeType, inherit);

    public override Type? DeclaringType => MemberInfo.DeclaringType;
    public override MemberTypes MemberType => MemberTypes.Custom;
    public override string Name => MemberInfo.Name;
    public override Type? ReflectedType => MemberInfo.ReflectedType;

    public MemberInfo MemberInfo { get; }
    public Type DataMemberType { get; }
    public bool CanRead { get; }
    public bool CanWrite { get; }
    public bool IsStatic { get; }
    public bool IsField { get; }
    public bool IsProperty { get; }
    public bool IsCompilerGenerated { get; }
    public bool HasPublicSetter { get; }
    public bool HasPublicGetter { get; }
    public Func<object?, object?> Getter { get; }
    public Action<object?, object?> Setter { get; }

    public object? GetValue(object? obj) => Getter(obj);
    public void SetValue(object? obj, object? value) => Setter(obj, value);

    public bool Equals(DataMemberInfo? other)
    {
        if (ReferenceEquals(null, other)) return false;
        if (ReferenceEquals(this, other)) return true;
        return MemberInfo.Equals(other.MemberInfo);
    }

    public override bool Equals(object? obj)
    {
        if (ReferenceEquals(null, obj)) return false;
        if (ReferenceEquals(this, obj)) return true;
        if (obj.GetType() != GetType()) return false;
        return Equals((DataMemberInfo)obj);
    }

    public override int GetHashCode()
    {
        return MemberInfo.GetHashCode();
    }
}