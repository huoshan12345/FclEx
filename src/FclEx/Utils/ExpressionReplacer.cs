using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Linq.Expressions;
using System.Runtime.CompilerServices;

namespace FclEx.Utils
{
    public class ExpressionReplacer : ExpressionVisitor, IDisposable
    {
        private Expression _oldValue;
        private Expression _newValue;

        private ExpressionReplacer() { }

        private void Init(Expression oldExp, Expression newExp)
        {
            _oldValue = oldExp;
            _newValue = newExp;
        }

        [ThreadStatic]
        private static ExpressionReplacer _replacer;

        public static Expression Replace(Expression exp, Expression oldExp, Expression newExp)
        {
            using var p = (_replacer ??= new ExpressionReplacer());
            p.Init(oldExp, newExp);
            return p.Visit(exp); // no thread switching
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
