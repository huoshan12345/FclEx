using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Data;
using System.Linq;
using System.Linq.Expressions;
using System.Reflection;
using MoreLinq.Extensions;
using static ServiceStack.OrmLite.CommonMethods;

namespace ServiceStack.OrmLite
{
    using FclEx;
    using FclEx.Utils;

    public static partial class SqlExpExtensions
    {
        public static List<Tuple<T1, T2>> ToMulti<T1, T2>(this SqlExpression<T1> exp, IDbConnection db)
            => db.SelectMulti<T1, T2>(exp);

        public static Tuple<T1, T2> ToTuple<T1, T2>(this SqlExpression<T1> exp, IDbConnection db)
            => exp.Take(1).ToMulti<T1, T2>(db).FirstOrDefault();

        public static List<Tuple<T1, T2, T3>> ToMulti<T1, T2, T3>(this SqlExpression<T1> exp, IDbConnection db)
            => db.SelectMulti<T1, T2, T3>(exp);

        public static Tuple<T1, T2, T3> ToTuple<T1, T2, T3>(this SqlExpression<T1> exp, IDbConnection db)
            => exp.Take(1).ToMulti<T1, T2, T3>(db).FirstOrDefault();

        public static List<Tuple<T1, T2, T3, T4>> ToMulti<T1, T2, T3, T4>(this SqlExpression<T1> exp, IDbConnection db)
            => db.SelectMulti<T1, T2, T3, T4>(exp);

        public static Tuple<T1, T2, T3, T4> ToTuple<T1, T2, T3, T4>(this SqlExpression<T1> exp, IDbConnection db)
            => exp.Take(1).ToMulti<T1, T2, T3, T4>(db).FirstOrDefault();

        public static List<Tuple<T1, T2, T3, T4, T5>> ToMulti<T1, T2, T3, T4, T5>(this SqlExpression<T1> exp, IDbConnection db)
            => db.SelectMulti<T1, T2, T3, T4, T5>(exp);

        public static Tuple<T1, T2, T3, T4, T5> ToTuple<T1, T2, T3, T4, T5>(this SqlExpression<T1> exp, IDbConnection db)
            => exp.Take(1).ToMulti<T1, T2, T3, T4, T5>(db).FirstOrDefault();

        public static List<Tuple<T1, T2, T3, T4, T5, T6>> ToMulti<T1, T2, T3, T4, T5, T6>(this SqlExpression<T1> exp, IDbConnection db)
            => db.SelectMulti<T1, T2, T3, T4, T5, T6>(exp);

        public static Tuple<T1, T2, T3, T4, T5, T6> ToTuple<T1, T2, T3, T4, T5, T6>(this SqlExpression<T1> exp, IDbConnection db)
            => exp.Take(1).ToMulti<T1, T2, T3, T4, T5, T6>(db).FirstOrDefault();
    }
}
