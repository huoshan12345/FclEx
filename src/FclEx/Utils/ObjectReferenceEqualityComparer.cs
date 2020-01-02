using System;
using System.Collections.Generic;
using System.Runtime.CompilerServices;

namespace FclEx.Utils
{
    /// <summary>
    /// A generic object comparerer that would only use object's reference, 
    /// ignoring any <see cref="IEquatable{T}"/> or <see cref="object.Equals(object)"/>  overrides.
    /// see details via https://stackoverflow.com/questions/1890058/iequalitycomparert-that-uses-referenceequals
    /// </summary>
    public sealed class ObjectReferenceEqualityComparer<T> : IEqualityComparer<T> where T : class
    {
        private static readonly Lazy<IEqualityComparer<T>> _defaultComparer
            = new Lazy<IEqualityComparer<T>>(() => new ObjectReferenceEqualityComparer<T>(), true);
        public static IEqualityComparer<T> Default => _defaultComparer.Value;

        public bool Equals(T x, T y)
        {
            return ReferenceEquals(x, y);
        }

        public int GetHashCode(T obj)
        {
            /*
             Rationale for using RuntimeHelpers.GetHashCode(object)
             This method has two effects that make it the correct call for this implementation:
                1. It returns 0 when the object is null. Since ReferenceEquals works for null parameters, 
                    so should the comparer's implementation of GetHashCode().
                2. It calls Object.GetHashCode() non-virtually. ReferenceEquals specifically ignores any overrides of Equals, 
                    so the implementation of GetHashCode() should use a special method that matches the effect of ReferenceEquals, 
                    which is exactly what RuntimeHelpers.GetHashCode is for.
            */
            return RuntimeHelpers.GetHashCode(obj);
        }
    }
}
