namespace System.Collections.Generic;

/// <summary>
/// A generic object comparer that would only use object's reference, 
/// ignoring any <see cref="IEquatable{T}"/> or <see cref="object.Equals(object)"/>  overrides. <br/>
/// see details via https://stackoverflow.com/questions/1890058/iequalitycomparert-that-uses-referenceequals
/// </summary>
public class ReferenceEqualityComparer<T> : IEqualityComparer<T>
{
    public static readonly ReferenceEqualityComparer<T> Instance = new();

    public bool Equals(T? x, T? y)
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

#if NETSTANDARD2_0
public class ReferenceEqualityComparer : ReferenceEqualityComparer<object>;
#endif