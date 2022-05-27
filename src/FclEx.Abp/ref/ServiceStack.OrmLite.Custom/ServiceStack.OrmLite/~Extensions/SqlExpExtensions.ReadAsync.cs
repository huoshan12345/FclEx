using System;
using System.Collections.Generic;
using System.Data;
using System.Linq;
using System.Linq.Expressions;
using System.Threading.Tasks;
using FclEx;
using FclEx.Extensions;
using FclEx.Utils;
using MoreLinq.Extensions;
using static ServiceStack.OrmLite.CommonMethods;

namespace ServiceStack.OrmLite
{
    public static partial class SqlExpExtensions
    {
        public static Task<IPagedList<T>> ToPagedListAsync<T>(this SqlExpression<T> exp, IDbConnection db,
            int? pageNumber, int? pageSize)
        {
            return db.GetPagedListAsync(exp, pageNumber, pageSize);
        }

        public static Task<IPagedList<T>> ToPagedListAsync<T>(this SqlExpression<T> exp, IDbConnection db, PagedSearchDto dto)
        {
            return exp.ToPagedListAsync(db, dto?.PageNumber, dto?.PageSize);
        }

        public static Task<IPagedList<TTarget>> ToPagedListAsync<T, TTarget>(this SqlExpression<T> exp, IDbConnection db,
            int? pageNumber, int? pageSize)
        {
            return db.GetPagedListAsync<T, TTarget>(exp, pageNumber, pageSize);
        }

        public static Task<IPagedList<TTarget>> ToPagedListAsync<T, TTarget>(this SqlExpression<T> exp, IDbConnection db, PagedSearchDto dto)
        {
            return exp.ToPagedListAsync<T, TTarget>(db, dto?.PageNumber, dto?.PageSize);
        }

        public static Task<List<T>> ToListAsync<T>(this SqlExpression<T> exp, IDbConnection db)
        {
            return db.SelectAsync(exp);
        }

        public static Task<List<T>> ToListAsync<T>(this ISqlExpression exp, IDbConnection db)
        {
            return db.SelectAsync<T>(exp);
        }

        public static Task<T> SingleAsync<T>(this SqlExpression<T> exp, IDbConnection db)
        {
            exp.Take(1);
            return db.SingleAsync(exp);
        }

        public static Task<T> SingleAsync<T>(this ISqlExpression exp, IDbConnection db)
        {
            exp = BuildSingle(exp);
            return db.SingleAsync<T>(exp);
        }

        public static Task<List<T>> ToColumnAsync<T>(this ISqlExpression exp, IDbConnection db)
        {
            return db.ColumnAsync<T>(exp);
        }

        public static Task<Dictionary<TKey, TValue>> ToDicAsync<TKey, TValue>(this ISqlExpression exp, IDbConnection db)
        {
            return db.DictionaryAsync<TKey, TValue>(exp);
        }

        public static async Task<IList<T>> GetListByIdsAsync<T, TProp>(this SqlExpression<T> sql, Expression<Func<T, TProp>> idSelector,
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
                var templist = await p.ToListAsync(db).DonotCapture();
                list.AddRange(templist);
            }
            return list;
        }

        public static Task<bool> ExistsAsync<T>(this SqlExpression<T> exp, IDbConnection db)
        {
            return db.ExistsAsync(exp);
        }
    }
}
