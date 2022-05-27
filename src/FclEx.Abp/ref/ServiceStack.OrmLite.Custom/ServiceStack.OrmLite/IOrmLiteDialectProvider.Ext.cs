using System;
using System.Collections.Generic;
using System.Text;
using System.Threading.Tasks;

namespace ServiceStack.OrmLite
{
    public partial interface IOrmLiteDialectProvider
    {
        bool IfDatabaseExists(string connectionString);
        void CreateDatabase(string connectionString);
        Task<bool> IfDatabaseExistsAsync(string connectionString);
        Task CreateDatabaseAsync(string connectionString);
    }
}
