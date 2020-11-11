using System;
using System.Collections.Generic;
using System.Reflection;
using System.Text;

namespace FclEx
{
    public static class MemberInfoExtensions
    {
        public static bool IsDefined<T>(this MemberInfo type, bool inherit = true)
        {
            return Attribute.IsDefined(type, typeof(T), inherit);
        }
    }
}
