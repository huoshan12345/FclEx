using System;
using System.Collections.Generic;
using System.Data;
using System.Linq.Expressions;
using System.Text;
using System.Threading;
using static ServiceStack.OrmLite.CommonMethods;

namespace ServiceStack.OrmLite
{
    public static partial class DbConnectionExtensions
    {
        public static int DeleteById(this IDbConnection dbConn, Type tableType, object id)
        {
            return (int)CommonMethods.DeleteById.MakeGenericMethod(tableType).Invoke(null, new[] { dbConn, id, null });
        }
    }
}
