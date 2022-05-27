using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Data;
using System.Linq;
using System.Linq.Expressions;
using System.Reflection;
using FclEx.Helpers;
using MoreLinq.Extensions;
using static ServiceStack.OrmLite.CommonMethods;

namespace ServiceStack.OrmLite
{
    using FclEx;
    using FclEx.Utils;

    public static partial class SqlExpExtensions
    {
        public static IPagedList<T> ToPagedList<T>(this SqlExpression<T> exp, IDbConnection db,
            int? pageNumber, int? pageSize)
        {
            return db.GetPagedList(exp, pageNumber, pageSize);
        }

        public static IPagedList<T> ToPagedList<T>(this SqlExpression<T> exp, IDbConnection db, PagedSearchDto dto)
        {
            return exp.ToPagedList(db, dto?.PageNumber, dto?.PageSize);
        }

        public static IPagedList<TTarget> ToPagedList<T, TTarget>(this SqlExpression<T> exp, IDbConnection db,
            int? pageNumber, int? pageSize)
        {
            return db.GetPagedList<T, TTarget>(exp, pageNumber, pageSize);
        }

        public static IPagedList<TTarget> ToPagedList<T, TTarget>(this SqlExpression<T> exp, IDbConnection db, PagedSearchDto dto)
        {
            return exp.ToPagedList<T, TTarget>(db, dto?.PageNumber, dto?.PageSize);
        }

        /// <typeparam name="T"></typeparam>
        /// <param name="expression"></param>
        /// <param name="pageNumber"></param>
        /// <param name="pageSize">the count of items in one page, (-1) means no paging</param>
        /// <returns></returns>
        public static SqlExpression<T> Paging<T>(this SqlExpression<T> expression, ref int? pageNumber, ref int? pageSize)
        {
            if (pageNumber == null || pageNumber < 1) pageNumber = 1;
            if (pageSize != -1)
            {
                if (pageSize == null || pageSize < 1) pageSize = 10;
                expression.Limit((pageNumber - 1) * pageSize, pageSize);
            }
            return expression;
        }

        public static List<T> ToList<T>(this SqlExpression<T> exp, IDbConnection db)
        {
            return db.Select(exp);
        }

        public static List<T> ToList<T>(this ISqlExpression exp, IDbConnection db)
        {
            return db.Select<T>(exp);
        }

        public static TModel SingleById<T, TModel>(this IDbConnection con, object id)
        {
            return con.From<T>().WhereById(id).Single<TModel>(con);
        }

        public static T Single<T>(this SqlExpression<T> exp, IDbConnection db)
        {
            exp.Take(1);
            return db.Single(exp);
        }

        public static T Single<T>(this ISqlExpression exp, IDbConnection db)
        {
            exp = BuildSingle(exp);
            return db.Single<T>(exp);
        }

        private static ISqlExpression BuildSingle(this ISqlExpression exp)
        {
            return InvokeTakeOne(exp);
        }

        public static TModel Single<T, TModel>(this SqlExpression<T> exp, IDbConnection db)
        {
            exp.Take(1);
            return db.Single<TModel>(exp);
        }

        public static List<T> ToColumn<T>(this ISqlExpression exp, IDbConnection db)
        {
            return db.Column<T>(exp);
        }

        public static Dictionary<TKey, TValue> ToDic<TKey, TValue>(this ISqlExpression exp, IDbConnection db)
        {
            return db.Dictionary<TKey, TValue>(exp);
        }

        public static string Column<T>(this SqlExpression<T> exp,
            Expression<Func<T, object>> selector, bool prefixTable = false)
        {
            return ((ISqlExpression)exp).Column(selector, prefixTable);
        }

        public static string Table<T>(this SqlExpression<T> exp)
        {
            return ((ISqlExpression)exp).Table<T>();
        }

        public static TProp Max<T, TProp>(this SqlExpression<T> exp, Expression<Func<T, TProp>> selector, IDbConnection db)
        {
            var untypedSelector = ExpressionHelper.ErasureType(selector);
            var callExp = Expression.Call(MaxOfSqlOfObj, untypedSelector.Body);
            var lambda = Expression.Lambda<Func<T, object>>(callExp, untypedSelector.Parameters);
            return exp.Select(lambda).Single<TProp>(db);
        }

        public static bool Exists<T>(this SqlExpression<T> exp, IDbConnection db)
        {
            return db.Exists(exp);
        }

        public static IList<T> GetListByIds<T, TProp>(this SqlExpression<T> sql, Expression<Func<T, TProp>> idSelector,
            IEnumerable<TProp> ids, IDbConnection db, int batchSize = 10000)
        {
            batchSize = Math.Max(1000, batchSize);
            var list = new List<T>();
            var method = ContainsOfEnumerable.MakeGenericMethod(typeof(TProp));
            foreach (var temp in ids.Batch(batchSize))
            {
                var argOfArr = Expression.Constant(temp, typeof(IEnumerable<TProp>));
                var callExp = Expression.Call(null, method, argOfArr, idSelector.Body);
                var where = Expression.Lambda<Func<T, bool>>(callExp, idSelector.Parameters);
                var p = sql.Clone().Where(where);
                var templist = p.ToList(db);
                list.AddRange(templist);
            }
            return list;
        }
    }
}
