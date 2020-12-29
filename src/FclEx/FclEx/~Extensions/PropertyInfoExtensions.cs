using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Threading.Tasks;

namespace FclEx
{
    public static class PropertyInfoExtensions
    {
        public static T? GetValue<T>(this PropertyInfo info, object? obj)
        {
            var value = info.GetValue(obj);
            return value == null ? default : (T)value;
        }
    }
}
