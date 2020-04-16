using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Linq.Expressions;
using System.Runtime.CompilerServices;
using Microsoft.Extensions.ObjectPool;

namespace FclEx.Utils
{
    public class ExpressionReplacer : ExpressionVisitor, IDisposable
    {
        private static readonly ObjectPool<ExpressionReplacer> _pool
            = new DefaultObjectPool<ExpressionReplacer>(new DefaultPooledObjectPolicy<ExpressionReplacer>());

        private Expression _oldValue;
        private Expression _newValue;

        private void Init(Expression oldExp, Expression newExp)
        {
            _oldValue = oldExp;
            _newValue = newExp;
        }


        public static Expression Replace(Expression exp, Expression oldExp, Expression newExp)
        {
            using var p = _pool.GetAsDisposable();
            p.Value.Init(oldExp, newExp);
            return p.Value.Visit(exp); // no thread switching
        }

        public override Expression Visit(Expression node)
        {
            return node == _oldValue
                ? _newValue
                : base.Visit(node);
        }

        public void Dispose()
        {
            _oldValue = null;
            _newValue = null;
        }
    }
}
