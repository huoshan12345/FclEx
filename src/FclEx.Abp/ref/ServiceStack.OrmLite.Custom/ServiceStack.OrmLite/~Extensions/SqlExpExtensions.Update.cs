using System;
using System.Linq;
using System.Linq.Expressions;

namespace ServiceStack.OrmLite
{
    public static partial class SqlExpExtensions
    {
        public static SqlExpression<T> AddUpdate<T>(this SqlExpression<T> exp, params Expression<Func<T, object>>[] fields)
        {
            foreach (var field in fields.SelectMany(f => f.GetFieldNames()))
            {
                exp.UpdateFields.AddIfNotExists(field);
            }
            return exp;
        }

    }
}
