using System;
using System.Collections.Generic;
using System.Data;
using System.Linq;
using System.Threading.Tasks;
using FclEx;
using FclEx.Extensions;

namespace ServiceStack.OrmLite
{
    public static partial class DbConnectionExtensions
    {
        public static async Task DoTranAsync(this IDbConnection con, Func<IDbConnection, Task> action,
            IsolationLevel level = IsolationLevel.ReadUncommitted)
        {
            var tran = con.OpenTransaction(level);
            try
            {
                await action(con).DonotCapture();
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

        public static async Task<T> DoTranAsync<T>(this IDbConnection con, Func<IDbConnection, Task<T>> action,
            IsolationLevel level = IsolationLevel.ReadUncommitted)
        {
            var tran = con.OpenTransaction(level);
            try
            {
                var result = await action(con).DonotCapture();
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

        public static async Task DoTranAsync(this IList<IDbConnection> cons, Func<IList<IDbConnection>, Task> action,
            IsolationLevel level = IsolationLevel.ReadUncommitted)
        {
            var trans = cons.Select(m => m.OpenTransaction(level)).ToArray();
            try
            {
                await action(cons).DonotCapture();
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
