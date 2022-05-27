using System;
using System.Linq.Expressions;

namespace ServiceStack.OrmLite
{
    public static partial class SqlExpExtensions
    {
        public static SqlExpression<T> JoinSelf<T>(this SqlExpression<T> sql,
            Expression<Func<T, object>> tableFieldSelector,
            string tableAlias,
            Expression<Func<T, object>> joinTableFieldSelector,
            string joinTableAlias)
        {
            var p = sql.DialectProvider;
            var table = sql.DialectProvider.Table<T>();
            var tableField = p.Column(tableFieldSelector);
            var joinTableField = p.Column(joinTableFieldSelector);

            sql.CustomJoin(" \nJOIN {0} {1} on ({2}.{3} = {1}.{4})".Fmt(
                table, joinTableAlias, tableAlias, tableField, joinTableField));

            return sql;
        }

        public static SqlExpression<T> JoinSelf<T>(this SqlExpression<T> sql,
            Expression<Func<T, object>> tableFieldSelector,
            Expression<Func<T, object>> joinTableFieldSelector,
            string joinTableAlias)
        {
            var table = sql.DialectProvider.Table<T>();
            return sql.JoinSelf(tableFieldSelector, table, joinTableFieldSelector, joinTableAlias);
        }
    }
}
