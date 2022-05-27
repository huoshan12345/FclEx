using System.Collections.Generic;
using System.Data;
using System.Diagnostics.CodeAnalysis;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using FclEx;
using FclEx.Extensions;

namespace ServiceStack.OrmLite
{
    public static partial class DbConnectionExtensions
    {
        public static async Task<int> InsertAndGetIdAsync<T>(this IDbConnection con, T item)
        {
            var id = await con.InsertAndGetLongIdAsync(item).DonotCapture();
            return (int)id;
        }

        public static Task<long> InsertAndGetLongIdAsync<T>(this IDbConnection con, T item)
        {
            return con.InsertAsync(item, true);
        }

        [SuppressMessage("ReSharper", "PossibleMultipleEnumeration")]
        public static Task<int> InsertBulkAsync<TEntity>(this IDbConnection con, IEnumerable<TEntity> entities)
        {
            Check.NotNull(entities);

            if (!entities.Any())
                return Task.FromResult(0);
            /*
             For mysql:
                Inserting NULL into a column that has been declared NOT NULL.
                For multiple-row INSERT statements or INSERT INTO ... SELECT statements,
                the column is set to the implicit default value for the column data type. This is 0 for numeric types,
                the empty string ('') for string types, and the "zero" value for date and time types.
                INSERT INTO ... SELECT statements are handled the same way as multiple-row inserts
                because the server does not examine the result set from the SELECT to see whether it returns a single row.
                (For a single-row INSERT, no warning occurs when NULL is inserted into a NOT NULL column. Instead, the statement fails with an error.)
             */
            return con.Exec(dbCmd =>
            {
                dbCmd.SetBulkInsertCmd(entities);
                return dbCmd.ExecNonQueryAsync();
            });
        }

        public static Task<long> InsertObjectAsync(this IDbConnection dbConn, object obj, bool selectIdentity = false, CancellationToken token = default)
        {
            var type = obj.GetType();
            return (Task<long>)CommonMethods.InsertAsync.MakeGenericMethod(type).Invoke(null, new[] { dbConn, obj, selectIdentity, false, token });
        }
    }
}
