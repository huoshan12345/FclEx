using System.Linq;
using Dawn;

namespace System.Reflection
{
    public class DataMemberInfo : MemberInfo
    {
        public DataMemberInfo(FieldInfo field)
        {
            MemberInfo = Guard.Argument(field, nameof(field)).NotNull();
            CanRead = true;
            CanWrite = true;
            GetValueFunc = field.GetValue;
            SetValueFunc = field.SetValue;
            IsStatic = field.IsStatic;
        }

        public DataMemberInfo(PropertyInfo property)
        {
            MemberInfo = Guard.Argument(property, nameof(property)).NotNull();
            CanRead = property.CanRead;
            CanWrite = property.CanWrite;
            GetValueFunc = property.GetValue;
            SetValueFunc = property.SetValue;
            var accessors = property.GetAccessors(true);
            IsStatic = accessors.Any(m => m.IsStatic);
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

        public object? GetValue(object? obj) => GetValueFunc(obj);
        public void SetValue(object? obj, object? value) => SetValueFunc(obj, value);
        public bool CanRead { get; }
        public bool CanWrite { get; }
        public bool IsStatic { get; }
        internal Func<object?, object?> GetValueFunc { get; }
        internal Action<object?, object?> SetValueFunc { get; }
        public MemberInfo MemberInfo { get; }
    }
}
