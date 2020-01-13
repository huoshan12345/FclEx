using System;
using System.Collections.Generic;
using System.Text;

namespace FclEx.Utils
{
    public class ColValue
    {
        public static ICollection<T> EmptyCol<T>() => Array.Empty<T>();
        public static IReadOnlyCollection<T> EmptyReadOnlyCol<T>() => Array.Empty<T>();
        public static IList<T> EmptyList<T>() => Array.Empty<T>();
        public static IReadOnlyList<T> EmptyReadOnlyList<T>() => Array.Empty<T>();
    }
}
