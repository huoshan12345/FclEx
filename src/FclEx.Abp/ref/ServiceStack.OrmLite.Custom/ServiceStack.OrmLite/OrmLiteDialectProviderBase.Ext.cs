using System;
using System.Collections.Generic;
using System.Text;
using System.Threading.Tasks;
using FclEx;
using FclEx.Extensions;

namespace ServiceStack.OrmLite
{
    public partial class OrmLiteDialectProviderBase<TDialect>
    {
        public abstract bool IfDatabaseExists(string connectionString);
        public abstract void CreateDatabase(string connectionString);

        public virtual Task<bool> IfDatabaseExistsAsync(string connectionString)
        {
            return IfDatabaseExists(connectionString).ToTask();
        }

        public virtual Task CreateDatabaseAsync(string connectionString)
        {
            CreateDatabase(connectionString);
            return Task.CompletedTask;
        }
    }
}
