using System;
using System.Collections.Generic;
using System.Text;
using Microsoft.Extensions.ObjectPool;

namespace FclEx.Helpers
{
    public static class ObjectPoolHelper
    {
        public static ObjectPool<StringBuilder> StringBuilderPool { get; } = new DefaultObjectPool<StringBuilder>(new StringBuilderPooledObjectPolicy());
    }
}
