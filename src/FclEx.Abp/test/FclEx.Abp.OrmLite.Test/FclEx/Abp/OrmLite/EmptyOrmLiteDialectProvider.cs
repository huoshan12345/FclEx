using System;
using System.Collections.Generic;
using System.Data;
using System.Linq.Expressions;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using ServiceStack.OrmLite;
using ServiceStack.Text;

namespace FclEx.Abp.OrmLite
{
    internal class EmptyOrmLiteDialectProvider : IOrmLiteDialectProvider
    {
        public void RegisterConverter<T>(IOrmLiteConverter converter)
        {
            throw new NotImplementedException();
        }

        public Action<IDbConnection> OnOpenConnection { get; set; }
        public IOrmLiteExecFilter ExecFilter { get; set; }
        public IOrmLiteConverter GetConverter(Type type)
        {
            throw new NotImplementedException();
        }

        public IOrmLiteConverter GetConverterBestMatch(Type type)
        {
            throw new NotImplementedException();
        }

        public IOrmLiteConverter GetConverterBestMatch(FieldDefinition fieldDef)
        {
            throw new NotImplementedException();
        }

        public string ParamString { get; set; }
        public string EscapeWildcards(string value)
        {
            throw new NotImplementedException();
        }

        public INamingStrategy NamingStrategy { get; set; }
        public IStringSerializer StringSerializer { get; set; }
        public Func<string, string> ParamNameFilter { get; set; }
        public Dictionary<string, string> Variables { get; }
        public string GetQuotedValue(string paramValue)
        {
            throw new NotImplementedException();
        }

        public string GetQuotedValue(object value, Type fieldType)
        {
            throw new NotImplementedException();
        }

        public string GetDefaultValue(Type tableType, string fieldName)
        {
            throw new NotImplementedException();
        }

        public string GetDefaultValue(FieldDefinition fieldDef)
        {
            throw new NotImplementedException();
        }

        public bool HasInsertReturnValues(ModelDefinition modelDef)
        {
            throw new NotImplementedException();
        }

        public object GetParamValue(object value, Type fieldType)
        {
            throw new NotImplementedException();
        }

        public void InitQueryParam(IDbDataParameter param)
        {
            throw new NotImplementedException();
        }

        public void InitUpdateParam(IDbDataParameter param)
        {
            throw new NotImplementedException();
        }

        public object ToDbValue(object value, Type type)
        {
            throw new NotImplementedException();
        }

        public object FromDbValue(object value, Type type)
        {
            throw new NotImplementedException();
        }

        public object GetValue(IDataReader reader, int columnIndex, Type type)
        {
            throw new NotImplementedException();
        }

        public int GetValues(IDataReader reader, object[] values)
        {
            throw new NotImplementedException();
        }

        public IDbConnection CreateConnection(string filePath, Dictionary<string, string> options)
        {
            throw new NotImplementedException();
        }

        public string GetTableName(ModelDefinition modelDef)
        {
            throw new NotImplementedException();
        }

        public string GetTableName(ModelDefinition modelDef, bool useStrategy)
        {
            throw new NotImplementedException();
        }

        public string GetTableName(string table, string schema = null)
        {
            throw new NotImplementedException();
        }

        public string GetTableName(string table, string schema, bool useStrategy)
        {
            throw new NotImplementedException();
        }

        public string GetQuotedTableName(ModelDefinition modelDef)
        {
            throw new NotImplementedException();
        }

        public string GetQuotedTableName(string tableName, string schema = null)
        {
            throw new NotImplementedException();
        }

        public string GetQuotedTableName(string tableName, string schema, bool useStrategy)
        {
            throw new NotImplementedException();
        }

        public string GetQuotedColumnName(string columnName)
        {
            throw new NotImplementedException();
        }

        public string GetQuotedName(string name)
        {
            throw new NotImplementedException();
        }

        public string GetQuotedName(string name, string schema)
        {
            throw new NotImplementedException();
        }

        public string SanitizeFieldNameForParamName(string fieldName)
        {
            throw new NotImplementedException();
        }

        public string GetColumnDefinition(FieldDefinition fieldDef)
        {
            throw new NotImplementedException();
        }

        public long GetLastInsertId(IDbCommand command)
        {
            throw new NotImplementedException();
        }

        public string GetLastInsertIdSqlSuffix<T>()
        {
            throw new NotImplementedException();
        }

        public string ToSelectStatement(Type tableType, string sqlFilter, params object[] filterParams)
        {
            throw new NotImplementedException();
        }

        public string ToSelectStatement(QueryType queryType, ModelDefinition modelDef, string selectExpression, string bodyExpression,
            string orderByExpression = null, int? offset = null, int? rows = null, ISet<string> tags = null)
        {
            throw new NotImplementedException();
        }

        public string ToInsertRowStatement(IDbCommand cmd, object objWithProperties, ICollection<string> insertFields = null)
        {
            throw new NotImplementedException();
        }

        public void PrepareParameterizedInsertStatement<T>(IDbCommand cmd, ICollection<string> insertFields = null, Func<FieldDefinition, bool> shouldInclude = null)
        {
            throw new NotImplementedException();
        }

        public bool PrepareParameterizedUpdateStatement<T>(IDbCommand cmd, ICollection<string> updateFields = null)
        {
            throw new NotImplementedException();
        }

        public bool PrepareParameterizedDeleteStatement<T>(IDbCommand cmd, IDictionary<string, object> deleteFieldValues)
        {
            throw new NotImplementedException();
        }

        public void PrepareStoredProcedureStatement<T>(IDbCommand cmd, T obj)
        {
            throw new NotImplementedException();
        }

        public void SetParameterValues<T>(IDbCommand dbCmd, object obj)
        {
            throw new NotImplementedException();
        }

        public void SetParameter(FieldDefinition fieldDef, IDbDataParameter p)
        {
            throw new NotImplementedException();
        }

        public void EnableIdentityInsert<T>(IDbCommand cmd)
        {
            throw new NotImplementedException();
        }

        public Task EnableIdentityInsertAsync<T>(IDbCommand cmd, CancellationToken token = default)
        {
            throw new NotImplementedException();
        }

        public void DisableIdentityInsert<T>(IDbCommand cmd)
        {
            throw new NotImplementedException();
        }

        public Task DisableIdentityInsertAsync<T>(IDbCommand cmd, CancellationToken token = default)
        {
            throw new NotImplementedException();
        }

        public void EnableForeignKeysCheck(IDbCommand cmd)
        {
            throw new NotImplementedException();
        }

        public Task EnableForeignKeysCheckAsync(IDbCommand cmd, CancellationToken token = default)
        {
            throw new NotImplementedException();
        }

        public void DisableForeignKeysCheck(IDbCommand cmd)
        {
            throw new NotImplementedException();
        }

        public Task DisableForeignKeysCheckAsync(IDbCommand cmd, CancellationToken token = default)
        {
            throw new NotImplementedException();
        }

        public Dictionary<string, FieldDefinition> GetFieldDefinitionMap(ModelDefinition modelDef)
        {
            throw new NotImplementedException();
        }

        public object GetFieldValue(FieldDefinition fieldDef, object value)
        {
            throw new NotImplementedException();
        }

        public object GetFieldValue(Type fieldType, object value)
        {
            throw new NotImplementedException();
        }

        public void PrepareUpdateRowStatement(IDbCommand dbCmd, object objWithProperties, ICollection<string> updateFields = null)
        {
            throw new NotImplementedException();
        }

        public void PrepareUpdateRowStatement<T>(IDbCommand dbCmd, Dictionary<string, object> args, string sqlFilter)
        {
            throw new NotImplementedException();
        }

        public void PrepareUpdateRowAddStatement<T>(IDbCommand dbCmd, Dictionary<string, object> args, string sqlFilter)
        {
            throw new NotImplementedException();
        }

        public void PrepareInsertRowStatement<T>(IDbCommand dbCmd, Dictionary<string, object> args)
        {
            throw new NotImplementedException();
        }

        public string ToDeleteStatement(Type tableType, string sqlFilter, params object[] filterParams)
        {
            throw new NotImplementedException();
        }

        public IDbCommand CreateParameterizedDeleteStatement(IDbConnection connection, object objWithProperties)
        {
            throw new NotImplementedException();
        }

        public string ToExistStatement(Type fromTableType, object objWithProperties, string sqlFilter, params object[] filterParams)
        {
            throw new NotImplementedException();
        }

        public string ToSelectFromProcedureStatement(object fromObjWithProperties, Type outputModelType, string sqlFilter,
            params object[] filterParams)
        {
            throw new NotImplementedException();
        }

        public string ToExecuteProcedureStatement(object objWithProperties)
        {
            throw new NotImplementedException();
        }

        public string ToCreateSchemaStatement(string schema)
        {
            throw new NotImplementedException();
        }

        public string ToCreateTableStatement(Type tableType)
        {
            throw new NotImplementedException();
        }

        public string ToPostCreateTableStatement(ModelDefinition modelDef)
        {
            throw new NotImplementedException();
        }

        public string ToPostDropTableStatement(ModelDefinition modelDef)
        {
            throw new NotImplementedException();
        }

        public List<string> ToCreateIndexStatements(Type tableType)
        {
            throw new NotImplementedException();
        }

        public List<string> ToCreateSequenceStatements(Type tableType)
        {
            throw new NotImplementedException();
        }

        public string ToCreateSequenceStatement(Type tableType, string sequenceName)
        {
            throw new NotImplementedException();
        }

        public List<string> SequenceList(Type tableType)
        {
            throw new NotImplementedException();
        }

        public Task<List<string>> SequenceListAsync(Type tableType, CancellationToken token = default)
        {
            throw new NotImplementedException();
        }

        public bool DoesSchemaExist(IDbCommand dbCmd, string schema)
        {
            throw new NotImplementedException();
        }

        public Task<bool> DoesSchemaExistAsync(IDbCommand dbCmd, string schema, CancellationToken token = default)
        {
            throw new NotImplementedException();
        }

        public bool DoesTableExist(IDbConnection db, string tableName, string schema = null)
        {
            throw new NotImplementedException();
        }

        public Task<bool> DoesTableExistAsync(IDbConnection db, string tableName, string schema = null, CancellationToken token = default)
        {
            throw new NotImplementedException();
        }

        public bool DoesTableExist(IDbCommand dbCmd, string tableName, string schema = null)
        {
            throw new NotImplementedException();
        }

        public Task<bool> DoesTableExistAsync(IDbCommand dbCmd, string tableName, string schema = null, CancellationToken token = default)
        {
            throw new NotImplementedException();
        }

        public bool DoesColumnExist(IDbConnection db, string columnName, string tableName, string schema = null)
        {
            throw new NotImplementedException();
        }

        public Task<bool> DoesColumnExistAsync(IDbConnection db, string columnName, string tableName, string schema = null,
            CancellationToken token = default)
        {
            throw new NotImplementedException();
        }

        public bool DoesSequenceExist(IDbCommand dbCmd, string sequenceName)
        {
            throw new NotImplementedException();
        }

        public Task<bool> DoesSequenceExistAsync(IDbCommand dbCmd, string sequenceName, CancellationToken token = default)
        {
            throw new NotImplementedException();
        }

        public void DropColumn(IDbConnection db, Type modelType, string columnName)
        {
            throw new NotImplementedException();
        }

        public object FromDbRowVersion(Type fieldType, object value)
        {
            throw new NotImplementedException();
        }

        public SelectItem GetRowVersionSelectColumn(FieldDefinition field, string tablePrefix = null)
        {
            throw new NotImplementedException();
        }

        public string GetRowVersionColumn(FieldDefinition field, string tablePrefix = null)
        {
            throw new NotImplementedException();
        }

        public string GetColumnNames(ModelDefinition modelDef)
        {
            throw new NotImplementedException();
        }

        public SelectItem[] GetColumnNames(ModelDefinition modelDef, string tablePrefix)
        {
            throw new NotImplementedException();
        }

        public SqlExpression<T> SqlExpression<T>()
        {
            throw new NotImplementedException();
        }

        public IDbDataParameter CreateParam()
        {
            throw new NotImplementedException();
        }

        public string GetDropForeignKeyConstraints(ModelDefinition modelDef)
        {
            throw new NotImplementedException();
        }

        public string ToAddColumnStatement(Type modelType, FieldDefinition fieldDef)
        {
            throw new NotImplementedException();
        }

        public string ToAlterColumnStatement(Type modelType, FieldDefinition fieldDef)
        {
            throw new NotImplementedException();
        }

        public string ToChangeColumnNameStatement(Type modelType, FieldDefinition fieldDef, string oldColumnName)
        {
            throw new NotImplementedException();
        }

        public string ToAddForeignKeyStatement<T, TForeign>(Expression<Func<T, object>> field, Expression<Func<TForeign, object>> foreignField, OnFkOption onUpdate,
            OnFkOption onDelete, string foreignKeyName = null)
        {
            throw new NotImplementedException();
        }

        public string ToCreateIndexStatement<T>(Expression<Func<T, object>> field, string indexName = null, bool unique = false)
        {
            throw new NotImplementedException();
        }

        public Task OpenAsync(IDbConnection db, CancellationToken token = default)
        {
            throw new NotImplementedException();
        }

        public Task<IDataReader> ExecuteReaderAsync(IDbCommand cmd, CancellationToken token = default)
        {
            throw new NotImplementedException();
        }

        public Task<int> ExecuteNonQueryAsync(IDbCommand cmd, CancellationToken token = default)
        {
            throw new NotImplementedException();
        }

        public Task<object> ExecuteScalarAsync(IDbCommand cmd, CancellationToken token = default)
        {
            throw new NotImplementedException();
        }

        public Task<bool> ReadAsync(IDataReader reader, CancellationToken token = default)
        {
            throw new NotImplementedException();
        }

        public Task<List<T>> ReaderEach<T>(IDataReader reader, Func<T> fn, CancellationToken token = default)
        {
            throw new NotImplementedException();
        }

        public Task<Return> ReaderEach<Return>(IDataReader reader, Action fn, Return source, CancellationToken token = default)
        {
            throw new NotImplementedException();
        }

        public Task<T> ReaderRead<T>(IDataReader reader, Func<T> fn, CancellationToken token = default)
        {
            throw new NotImplementedException();
        }

        public Task<long> InsertAndGetLastInsertIdAsync<T>(IDbCommand dbCmd, CancellationToken token)
        {
            throw new NotImplementedException();
        }

        public string GetLoadChildrenSubSelect<From>(SqlExpression<From> expr)
        {
            throw new NotImplementedException();
        }

        public string ToRowCountStatement(string innerSql)
        {
            throw new NotImplementedException();
        }

        public string ToUpdateStatement<T>(IDbCommand dbCmd, T item, ICollection<string> updateFields = null)
        {
            throw new NotImplementedException();
        }

        public string ToInsertStatement<T>(IDbCommand dbCmd, T item, ICollection<string> insertFields = null)
        {
            throw new NotImplementedException();
        }

        public string MergeParamsIntoSql(string sql, IEnumerable<IDbDataParameter> dbParams)
        {
            throw new NotImplementedException();
        }

        public string ToTableNamesStatement(string schema)
        {
            throw new NotImplementedException();
        }

        public string ToTableNamesWithRowCountsStatement(bool live, string schema)
        {
            throw new NotImplementedException();
        }

        public string SqlConflict(string sql, string conflictResolution)
        {
            throw new NotImplementedException();
        }

        public string SqlConcat(IEnumerable<object> args)
        {
            throw new NotImplementedException();
        }

        public string SqlCurrency(string fieldOrValue)
        {
            throw new NotImplementedException();
        }

        public string SqlCurrency(string fieldOrValue, string currencySymbol)
        {
            throw new NotImplementedException();
        }

        public string SqlBool(bool value)
        {
            throw new NotImplementedException();
        }

        public string SqlLimit(int? offset = null, int? rows = null)
        {
            throw new NotImplementedException();
        }

        public string SqlCast(object fieldOrValue, string castAs)
        {
            throw new NotImplementedException();
        }

        public string SqlRandom { get; }
        public string GenerateComment(in string text)
        {
            throw new NotImplementedException();
        }

        public bool IfDatabaseExists(string connectionString)
        {
            throw new NotImplementedException();
        }

        public void CreateDatabase(string connectionString)
        {
            throw new NotImplementedException();
        }

        public Task<bool> IfDatabaseExistsAsync(string connectionString)
        {
            throw new NotImplementedException();
        }

        public Task CreateDatabaseAsync(string connectionString)
        {
            throw new NotImplementedException();
        }
    }
}
