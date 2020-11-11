using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;

namespace FclEx.Helpers
{
    public static class EnumHelper
    {
        public static T[] GetValues<T>() where T : Enum
        {
            return (T[])Enum.GetValues(typeof(T));
        }

        public static T ParseFromStrNum<T>(string number, T defaultValue) where T : Enum
        {
            return ParseFromStrNum(number, input => defaultValue);
        }

        public static T ParseFromStrNum<T>(string number) where T : Enum
        {
            return ParseFromStrNum<T>(number, input => throw new ArgumentOutOfRangeException(nameof(number)));
        }

        public static T ParseFromStrNum<T>(string number, Func<string, T> defaultValueFunc) where T : Enum
        {
            if (int.TryParse(number, out var val))
            {
                if (typeof(T).IsEnumDefined(val))
                    return val.CastTo<T>()!;
            }
            return defaultValueFunc(number);
        }
    }
}
