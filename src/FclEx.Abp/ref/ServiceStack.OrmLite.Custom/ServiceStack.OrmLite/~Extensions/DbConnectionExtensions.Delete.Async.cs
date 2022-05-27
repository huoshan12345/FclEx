using System;
using System.Collections.Generic;
using System.Data;
using System.Linq.Expressions;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace ServiceStack.OrmLite
{
    public static partial class DbConnectionExtensions
    {
        public static Task<int> DeleteByIdAsync(this IDbConnection dbConn, Type tableType, object id, CancellationToken token = default)
        {
            return (Task<int>)CommonMethods.DeleteByIdAsync.MakeGenericMethod(tableType).Invoke(null, new[] { dbConn, id, null, token });
        }
    }
}
