using System;
using System.Collections.Generic;
using System.Text;

namespace FclEx
{
    public static class NullableExtensions
    {
        /// <summary>
        /// Exactly same as GetValueOrDefault but with shorter name.
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="t"></param>
        /// <param name="defaultValue"></param>
        /// <returns></returns>
        public static T Get<T>(this T? t, T defaultValue = default) where T : struct
        {
            return t.GetValueOrDefault(defaultValue);
        }

        public static bool IsValid<T>(this T? t) where T : struct
        {
            return !t.IsNullOrDefault();
        }

        public static bool IsNullOrDefault<T>(this T? t) where T : struct
        {
            return EqualityComparer<T>.Default.Equals(t.Get(), default);
        }
    }
}
