using System;
using System.Collections.Generic;
using System.Data;
using System.Linq.Expressions;
using System.Text;
using System.Threading.Tasks;

namespace ServiceStack.OrmLite
{
    public static partial class DbConnectionExtensions
    {
        public static Task<int> UpdateAsync<T>(this IDbConnection connection, T item,
            Expression<Func<T, bool>> where, params Expression<Func<T, object>>[] fields)
        {
            var sql = connection.From<T>().Where(where).AddUpdate(fields);
            return connection.UpdateOnlyFieldsAsync(item, sql);
        }

        public static Task<int> UpdateByIdAsync<T>(this IDbConnection connection,
            T item, object id, params Expression<Func<T, object>>[] fields)
        {
            var sql = connection.From<T>().WhereById(id).AddUpdate(fields);
            return connection.UpdateOnlyFieldsAsync(item, sql);
        }

        public static Task<int> UpdateByIdAsync<T>(this IDbConnection connection,
            T item, params Expression<Func<T, object>>[] fields)
        {
            var sql = connection.From<T>().WhereById(item).AddUpdate(fields);
            return connection.UpdateOnlyFieldsAsync(item, sql);
        }

        public static Task<int> UpdateByIdAsync<T>(this IDbConnection connection,
            T item, IList<string> updateFields)
        {
            var sql = connection.From<T>().WhereById(item).Update(updateFields);
            return connection.UpdateOnlyFieldsAsync(item, sql);
        }
    }
}
