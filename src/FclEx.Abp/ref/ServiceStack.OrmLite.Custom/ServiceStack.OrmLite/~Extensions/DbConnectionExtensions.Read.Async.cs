using System;
using System.Collections.Generic;
using System.Data;
using System.Linq;
using System.Linq.Expressions;
using System.Reflection;
using System.Threading;
using System.Threading.Tasks;
using FclEx;
using FclEx.Extensions;
using FclEx.Helpers;
using FclEx.Utils;
using ServiceStack.Data;

namespace ServiceStack.OrmLite
{
    public static partial class DbConnectionExtensions
    {
        internal static Task<long> RowCountAsync(this IDbCommand dbCmd, string sql, IEnumerable<IDbDataParameter> sqlParams, CancellationToken token)
        {
            return dbCmd.SetParameters(sqlParams).ScalarAsync<long>(dbCmd.GetDialectProvider().ToRowCountStatement(sql), token);
        }

        public static Task<long> RowCountAsync(this IDbConnection dbConn, string sql, IEnumerable<IDbDataParameter> sqlParams, CancellationToken token = default)
        {
            return dbConn.Exec(dbCmd => dbCmd.RowCountAsync(sql, sqlParams, token));
        }

        public static async Task<IPagedList<T>> GetPagedListAsync<T>(this IDbConnection connection,
            SqlExpression<T> expression, int? pageNumber, int? pageSize)
        {
            var totalCount = await connection.GetRowCountAsync(expression);
            expression.Paging(ref pageNumber, ref pageSize);
            var items = await connection.SelectAsync(expression);
            if (pageSize == -1) pageSize = (int)totalCount;
            return new PagedList<T>(items, pageNumber.Get() - 1,
                pageSize.Get(), (int)totalCount);
        }

        public static async Task<IPagedList<TTarget>> GetPagedListAsync<T, TTarget>(this IDbConnection connection,
            SqlExpression<T> expression, int? pageNumber, int? pageSize)
        {
            var totalCount = await connection.GetRowCountAsync(expression);
            expression.Paging(ref pageNumber, ref pageSize);
            var items = await connection.SelectAsync<TTarget>(expression);
            if (pageSize == -1) pageSize = (int)totalCount;
            return new PagedList<TTarget>(items, pageNumber.Get() - 1,
                pageSize.Get(), (int)totalCount);
        }

        public static Task<TModel> SingleByIdAsync<T, TModel>(this IDbConnection con, object id)
        {
            return con.From<T>().WhereById(id).SingleAsync<TModel>(con);
        }

        public static Task<long> GetRowCountAsync<T>(this IDbConnection connection, SqlExpression<T> expression)
        {
            return expression.GroupByExpression.IsNullOrEmpty()
                ? connection.CountAsync(expression)
                : connection.RowCountAsync(expression.SelectInto<T>(), expression.Params);
        }

        public static async Task DoAsync(this IDbConnectionFactory fac, Func<IDbConnection, Task> action)
        {
            using var con = await fac.OpenAsync().DonotCapture();
            await action(con).DonotCapture();
        }

        public static async Task<T> DoAsync<T>(this IDbConnectionFactory fac, Func<IDbConnection, Task<T>> action)
        {
            using var con = await fac.OpenAsync().DonotCapture();
            return await action(con).DonotCapture();
        }

        public static Task<bool> ExistsByIdAsync<T>(this IDbConnection con, object id)
        {
            return con.ExistsAsync<T>(con.From<T>().WhereById(id));
        }

        public static async Task ReadRefAsync<T, TInclude>(this IDbConnection con, List<T> objs,
            Expression<Func<T, IEnumerable<TInclude>>> selector, Expression<Func<TInclude, object>> refIdSelector)
        {
            var idField = OrmLiteHelper.GetIdField<T>();
            var ids = objs.Select(m => idField.GetValueFn(m)).ToList();
            var refs = await con.From<TInclude>()
                .InArray(refIdSelector, ids)
                .ToListAsync<TInclude>(con)
                .DonotCapture();

            var func = refIdSelector.Compile();
            var dic = refs.GroupBy(m => func(m)).ToDictionary(m => m.Key, m => m.ToList());
            var prop = ExpressionHelper.GetProperty(selector);
            foreach (var obj in objs)
            {
                var id = idField.GetValueFn(obj);
                var list = dic.Get(id);
                prop.SetValue(obj, list);
            }
        }

        public static Task<int> InsertRefAsync<T, TInclude>(this IDbConnection con, T obj,
            Expression<Func<T, List<TInclude>>> selector, Expression<Func<TInclude, object>> refIdSelector)
        {
            var idField = OrmLiteHelper.GetIdField<T>();
            var id = idField.GetValueFn(obj);
            var list = selector.Compile()(obj);
            var prop = GetProp(refIdSelector);
            foreach (var item in list)
            {
                prop.SetValue(item, id);
            }
            return con.InsertBulkAsync(list);
        }

        private static PropertyInfo GetProp(Expression expression)
        {
            if (expression == null)
                throw new ArgumentNullException(nameof(expression));

            if (expression is LambdaExpression lambda)
            {
                return GetProp(lambda.Body);
            }
            if (expression is UnaryExpression e)
            {
                return GetProp(e.Operand);
            }

            if (!(expression is MemberExpression member))
                throw new ArgumentException($"Expression '{expression}' refers to a method, not a property.");

            if (!(member.Member is PropertyInfo propInfo))
                throw new ArgumentException($"Expression '{expression}' refers to a field, not a property.");

            //If the MemberInfo object is a global member (that is, if it was obtained from the Module.GetMethods method,
            //which returns global methods on a module), the returned DeclaringType will be null.
            if (propInfo.ReflectedType == null)
                throw new ArgumentException($"Expression '{expression}' does not refer to a property of a class.");

            return propInfo;
        }
    }
}
