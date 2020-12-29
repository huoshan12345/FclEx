using System;
using System.Diagnostics.CodeAnalysis;

namespace FclEx.Utils
{
    public class ObjectException<T> : SimpleException
    {
        public T Target { get; }

        public ObjectException(T obj, string? msg = null, Exception? inner = null)
            : base(msg, inner)
        {
            Target = obj;
        }
    }
    
    public class ObjectException
    {
        public static ObjectException<T> Create<T>(T obj, string? msg = null, Exception? inner = null)
            => new(obj, msg, inner);
    }
}
