using System.Collections.Generic;
using System.Data;
using System.Diagnostics.CodeAnalysis;
using System.Linq;
using FclEx;

namespace ServiceStack.OrmLite
{
    public static partial class DbConnectionExtensions
    {
        public static int InsertAndGetId<T>(this IDbConnection con, T item)
        {
            return (int)con.InsertAndGetLongId(item);
        }

        public static long InsertAndGetLongId<T>(this IDbConnection con, T item)
        {
            return con.Insert(item, true);
        }

        [SuppressMessage("ReSharper", "PossibleMultipleEnumeration")]
        public static int InsertBulk<TEntity>(this IDbConnection con, IEnumerable<TEntity> entities)
        {
            Check.NotNull(entities);

            if (!entities.Any())
                return 0;
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
                return dbCmd.ExecuteNonQuery();
            });
        }

        public static long InsertObject(this IDbConnection dbConn, object obj, bool selectIdentity = false)
        {
            var type = obj.GetType();
            return (long)CommonMethods.Insert.MakeGenericMethod(type).Invoke(null, new[] { dbConn, obj, selectIdentity, false });
        }
    }
}
