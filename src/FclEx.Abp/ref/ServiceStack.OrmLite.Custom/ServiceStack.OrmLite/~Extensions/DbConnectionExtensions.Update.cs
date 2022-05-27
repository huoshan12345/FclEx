using System;
using System.Collections.Generic;
using System.Data;
using System.Linq.Expressions;
using System.Text;

namespace ServiceStack.OrmLite
{
    public static partial class DbConnectionExtensions
    {
        public static int Update<T>(this IDbConnection connection, T item,
            Expression<Func<T, bool>> where, params Expression<Func<T, object>>[] fields)
        {
            var sql = connection.From<T>().Where(where).AddUpdate(fields);
            return connection.UpdateOnlyFields(item, sql);
        }

        public static int UpdateById<T>(this IDbConnection connection,
            T item, object id, params Expression<Func<T, object>>[] fields)
        {
            var sql = connection.From<T>().WhereById(id).AddUpdate(fields);
            return connection.UpdateOnlyFields(item, sql);
        }

        public static int UpdateById<T>(this IDbConnection connection,
            T item, params Expression<Func<T, object>>[] fields)
        {
            var sql = connection.From<T>().WhereById(item).AddUpdate(fields);
            return connection.UpdateOnlyFields(item, sql);
        }

        public static int UpdateById<T>(this IDbConnection connection,
            T item, IList<string> updateFields)
        {
            var sql = connection.From<T>().WhereById(item).Update(updateFields);
            return connection.UpdateOnlyFields(item, sql);
        }
    }
}
