using System;
using System.Text;

namespace FclEx.Utils
{
    public class ObjectException
    {
        public static ObjectException<T> Create<T>(T obj, string? msg = null, Exception? inner = null)
            => new ObjectException<T>(obj, msg, inner);
    }

    public class ObjectException<T> : SimpleException
    {
        public T Target { get; set; }

        public ObjectException(T obj, string? msg = null, Exception? inner = null)
            : base(msg, inner)
        {
            Target = obj;
        }
    }
}
