using System;
using System.Collections.Generic;
using System.Text;
using System.Threading.Tasks;

namespace ServiceStack.OrmLite.Oracle
{
    partial class OracleOrmLiteDialectProvider
    {
        public override bool IfDatabaseExists(string connectionString)
        {
            throw new NotImplementedException();
        }

        public override void CreateDatabase(string connectionString)
        {
            throw new NotImplementedException();
        }
    }
}
