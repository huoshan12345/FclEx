using System;
using System.Collections.Generic;
using System.Text;

namespace FclEx
{
    public static class NullableExtensions
    {
        /// <summary>
        /// 功能同GetValueOrDefault(仅仅就是把方法名缩短而已)
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="t"></param>
        /// <param name="defaultValue"></param>
        /// <returns></returns>
        public static T Get<T>(this T? t, T defaultValue = default)
            where T : struct
        {
            return t.GetValueOrDefault(defaultValue);
        }

        public static bool IsValid<T>(this T? t)
            where T : struct
        {
            return !EqualityComparer<T>.Default.Equals(t.Get(), default);
        }
    }
}
