using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace FclEx.Attributes
{
    [AttributeUsage(AttributeTargets.Field, AllowMultiple = false, Inherited = false)]
    public class EnumValueAttribute : Attribute
    {
        public EnumValueAttribute(string? value)
        {
            Value = value;
        }

        public string? Value { get; }
    }
}
