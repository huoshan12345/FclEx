using System;
using System.Diagnostics;
using System.Reflection;
using Dawn;

namespace FclEx.Utils
{
    [DebuggerDisplay("Name = {" + nameof(Name) + "}")]
    public readonly struct DataMemberInfo
    {
        public string Name { get; }
        public Type MemberType { get; }
        public bool CanRead { get; }
        public bool CanWrite { get; }
        public MemberInfo MemberInfo { get; }
        internal Func<object, object> GetValueFunc { get; }
        internal Action<object, object> SetValueFunc { get; }

        public DataMemberInfo(FieldInfo fieldInfo)
        {
            Guard.Argument(fieldInfo, nameof(fieldInfo)).NotNull();
            MemberInfo = fieldInfo;
            Name = fieldInfo.Name;
            MemberType = fieldInfo.FieldType;
            CanRead = true;
            CanWrite = true;
            GetValueFunc = fieldInfo.GetValue;
            SetValueFunc = fieldInfo.SetValue;
        }

        public DataMemberInfo(PropertyInfo propertyInfo)
        {
            Guard.Argument(propertyInfo, nameof(propertyInfo)).NotNull();
            MemberInfo = propertyInfo;
            Name = propertyInfo.Name;
            MemberType = propertyInfo.PropertyType;
            CanRead = propertyInfo.CanRead;
            CanWrite = propertyInfo.CanWrite;
            GetValueFunc = propertyInfo.GetValue;
            SetValueFunc = propertyInfo.SetValue;
        }

        public object GetValue(object obj) => GetValueFunc(obj);
        public void SetValue(object obj, object value) => SetValueFunc(obj, value);
    }
}