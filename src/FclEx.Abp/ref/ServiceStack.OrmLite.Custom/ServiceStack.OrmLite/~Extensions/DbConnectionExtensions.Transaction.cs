using System;
using System.Collections.Generic;
using System.Data;
using System.Linq;
using System.Text;
using FclEx;
using FclEx.Extensions;
using FclEx.Utils;
using MoreLinq;

namespace ServiceStack.OrmLite
{
    public static partial class DbConnectionExtensions
    {
        internal static void RollbackWithCheck(this IDbTransaction tran)
        {
            if (tran.Connection != null && tran.Connection.State == ConnectionState.Open)
                tran.Rollback();
        }

        internal static void RollbackWithCheck(this IEnumerable<IDbTransaction> trans, Exception commitException)
        {
            // Ensure that every transaction will be rollbacked.
            var results = trans.Select(m => Operate.Excute(m.RollbackWithCheck));
            var errors = results.Where(m => m.Error).ToArray();
            if (errors.Any())
            {
                throw new AggregateException(Enumerable.Append(errors.Select(m => m.Exception), commitException));
            }
            else commitException.ReThrow();
        }

        public static void DoTran(this IDbConnection con, Action<IDbConnection> action,
            IsolationLevel level = IsolationLevel.ReadUncommitted)
        {
            var tran = con.OpenTransaction(level);
            try
            {
                action(con);
                tran.Commit();
            }
            catch
            {
                tran.RollbackWithCheck();
            }
            finally
            {
                tran.Dispose();
            }
        }

        public static T DoTran<T>(this IDbConnection con, Func<IDbConnection, T> action,
            IsolationLevel level = IsolationLevel.ReadUncommitted)
        {
            var tran = con.OpenTransaction(level);
            try
            {
                var result = action(con);
                tran.Commit();
                return result;
            }
            catch
            {
                tran.RollbackWithCheck();
                return default;
            }
            finally
            {
                tran.Dispose();
            }
        }

        public static void DoTran(this IList<IDbConnection> cons, Action<IList<IDbConnection>> action,
            IsolationLevel level = IsolationLevel.ReadUncommitted)
        {
            var trans = cons.Select(m => m.OpenTransaction(level)).ToArray();
            try
            {
                action(cons);
                foreach (var tran in trans)
                    tran.Commit();
            }
            catch (Exception ex)
            {
                trans.RollbackWithCheck(ex);
            }
            finally
            {
                foreach (var tran in trans)
                    tran.Dispose();
            }
        }

        public static void DoTran<TKey>(this IDictionary<TKey, IDbConnection> cons,
            Action<IDictionary<TKey, IDbConnection>> action, IsolationLevel level = IsolationLevel.ReadUncommitted)
        {
            var trans = cons.Select(m => m.Value.OpenTransaction(level)).ToArray();
            try
            {
                action(cons);
                foreach (var tran in trans)
                    tran.Commit();
            }
            catch (Exception ex)
            {
                trans.RollbackWithCheck(ex);
            }
            finally
            {
                foreach (var tran in trans)
                    tran.Dispose();
            }
        }
    }
}
