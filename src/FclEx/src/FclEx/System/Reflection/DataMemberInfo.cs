using System;
using System.Diagnostics;
using System.Linq;
using System.Runtime.CompilerServices;
using FclEx;

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
        GetValueFunc = field.GetValue;
        SetValueFunc = field.SetValue;
        IsStatic = field.IsStatic;
        IsField = true;
        IsProperty = false;
        DataMemberType = field.FieldType;
        HasPublicSetter = true;
        HasPublicGetter = true;
    }

    public DataMemberInfo(PropertyInfo property)
    {
        MemberInfo = Check.NotNull(property);
        IsCompilerGenerated = MemberInfo.IsDefined(typeof(CompilerGeneratedAttribute), false);
        CanRead = property.CanRead;
        CanWrite = property.CanWrite;
        GetValueFunc = property.GetValue;
        SetValueFunc = property.SetValue;
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
    public override MemberTypes MemberType => MemberInfo.MemberType;
    public override string Name => MemberInfo.Name;
    public override Type? ReflectedType => MemberInfo.ReflectedType;

    public Type DataMemberType { get; }
    public object? GetValue(object? obj) => GetValueFunc(obj);
    public void SetValue(object? obj, object? value) => SetValueFunc(obj, value);
    public bool CanRead { get; }
    public bool CanWrite { get; }
    public bool IsStatic { get; }
    internal Func<object?, object?> GetValueFunc { get; }
    internal Action<object?, object?> SetValueFunc { get; }
    public MemberInfo MemberInfo { get; }
    public bool IsField { get; }
    public bool IsProperty { get; }
    public bool IsCompilerGenerated { get; }
    public bool HasPublicSetter { get; }
    public bool HasPublicGetter { get; }

    public bool Equals(DataMemberInfo? other)
    {
        if (ReferenceEquals(null, other)) return false;
        if (ReferenceEquals(this, other)) return true;
        return base.Equals(other) && MemberInfo.Equals(other.MemberInfo);
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
        return HashCode.Combine(base.GetHashCode(), MemberInfo);
    }
}