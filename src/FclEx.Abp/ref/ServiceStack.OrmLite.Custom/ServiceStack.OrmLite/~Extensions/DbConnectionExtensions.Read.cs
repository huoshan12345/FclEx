using System;
using System.Collections.Generic;
using System.Data;
using System.Linq;
using System.Linq.Expressions;
using System.Reflection;
using FclEx.Helpers;
using MoreLinq.Extensions;
using ServiceStack.Data;
using static ServiceStack.OrmLite.CommonMethods;

namespace ServiceStack.OrmLite
{
    using FclEx;
    using FclEx.Utils;

    public static partial class DbConnectionExtensions
    {
        public static IPagedList<T> GetPagedList<T>(this IDbConnection connection,
            SqlExpression<T> expression, int? pageNumber, int? pageSize)
        {
            var totalCount = connection.GetRowCount(expression);
            expression.Paging(ref pageNumber, ref pageSize);
            var items = connection.Select(expression);
            if (pageSize == -1) pageSize = (int)totalCount;
            return new PagedList<T>(items, pageNumber.GetValueOrDefault() - 1,
                pageSize.GetValueOrDefault(), (int)totalCount);
        }

        public static IPagedList<TTarget> GetPagedList<T, TTarget>(this IDbConnection connection,
            SqlExpression<T> expression, int? pageNumber, int? pageSize)
        {
            var totalCount = connection.GetRowCount(expression);
            expression.Paging(ref pageNumber, ref pageSize);
            var items = connection.Select<TTarget>(expression);
            if (pageSize == -1) pageSize = (int)totalCount;
            return new PagedList<TTarget>(items, pageNumber.GetValueOrDefault() - 1,
                pageSize.GetValueOrDefault(), (int)totalCount);
        }

        public static IPagedList<TTarget> GetPagedList<T, TTarget>(this IDbConnection connection,
            int? pageNumber, int? pageSize)
        {
            return GetPagedList<T, TTarget>(connection, connection.From<T>(), pageNumber, pageSize);
        }

        public static long GetRowCount<T>(this IDbConnection connection, SqlExpression<T> expression)
        {
            return expression.GroupByExpression.IsNullOrEmpty()
                ? connection.Count(expression)
                : connection.RowCount(expression.SelectInto<T>(), expression.Params);
        }

        public static TProp Max<T, TProp>(this IDbConnection con, Expression<Func<T, TProp>> selector)
        {
            var untypedSelector = ExpressionHelper.ErasureType(selector);
            var exp = Expression.Call(MaxOfSqlOfObj, untypedSelector.Body);
            var lambda = Expression.Lambda<Func<T, object>>(exp, untypedSelector.Parameters);
            return con.From<T>().Select(ExpressionHelper.ErasureType(lambda)).Single<TProp>(con);
        }

        public static void Do(this IDbConnectionFactory fac, Action<IDbConnection> action)
        {
            using var con = fac.OpenDbConnection();
            action(con);
        }

        public static T Do<T>(this IDbConnectionFactory fac, Func<IDbConnection, T> action)
        {
            using var con = fac.OpenDbConnection();
            return action(con);
        }

        public static bool ExistsById<T>(this IDbConnection con, object id)
        {
            return con.Exists<T>(con.From<T>().WhereById(id));
        }
    }
}
