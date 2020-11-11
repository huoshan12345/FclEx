using System.Collections.Generic;

namespace FclEx.Helpers
{
    public static class ListHelper<T>
    {
        public static IReadOnlyList<T> Empty { get; } = new List<T>();
    }
}
