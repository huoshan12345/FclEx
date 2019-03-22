using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq.Expressions;
using FclEx.Helpers;

namespace FclEx.Utils
{
    public class PropComparer<T>
    {
        private readonly struct OrderProperty
        {
            public OrderProperty(Func<T, object> selector, bool desc, IComparer comparer)
            {
                Selector = selector;
                Desc = desc;
                Comparer = comparer;
            }

            public readonly Func<T, object> Selector;
            public readonly bool Desc;
            public readonly IComparer Comparer;
        }

        private readonly IList<OrderProperty> _orderProperties = new List<OrderProperty>();

        public static PropComparer<T> Create()
        {
            return new PropComparer<T>();
        }

        public static PropComparer<T> Create<TProp>(Expression<Func<T, TProp>> selector, bool desc = false)
        {
            var cmp = new PropComparer<T>();
            return cmp.OrderBy(selector, desc);
        }

        public PropComparer<T> OrderBy<TProp>(Expression<Func<T, TProp>> selector, bool desc = false)
        {
            var unTypedExp = ExpressionUtil.ErasureType(selector);
            var prop = new OrderProperty(unTypedExp.Compile(), desc, Comparer<TProp>.Default);
            _orderProperties.Add(prop);
            return this;
        }

        private static int Compare(T x, T y, OrderProperty property)
        {
            var left = property.Selector(x);
            var right = property.Selector(y);
            return property.Desc
                ? property.Comparer.Compare(right, left)
                : property.Comparer.Compare(left, right);
        }

        public Comparison<T> ToComparison()
        {
            return (x, y) =>
            {
                foreach (var property in _orderProperties)
                {
                    var cmp = Compare(x, y, property);
                    if (cmp != 0) return cmp;
                }
                return 0;
            };
        }

        public IComparer<T> ToComparer() => ComparerHelper.Create(ToComparison());
    }
}