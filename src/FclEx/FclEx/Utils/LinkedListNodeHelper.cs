using System;
using System.Collections.Generic;
using System.Text;

namespace FclEx.Utils
{
    public static class LinkedListNodeHelper
    {
        public static LinkedListNode<T> Create<T>(T item)
        {
            return new LinkedListNode<T>(item);
        }
    }
}
