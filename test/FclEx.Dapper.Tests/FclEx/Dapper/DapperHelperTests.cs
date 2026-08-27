using System.Diagnostics.CodeAnalysis;

namespace FclEx.Dapper;

public class DapperHelperTests
{
    [Fact]
    public async Task CreateTransactionScope_RequiresTimeoutAndEnablesAsyncFlow()
    {
        var method = typeof(DapperHelper).GetMethod(
            nameof(DapperHelper.CreateTransactionScope),
            [typeof(TimeSpan), typeof(System.Transactions.IsolationLevel)]);
        Assert.NotNull(method);
        var timeoutParameter = Assert.Single(method.GetParameters(), parameter => parameter.Name == "timeout");
        Assert.False(timeoutParameter.HasDefaultValue);
        Assert.Equal(typeof(TimeSpan), timeoutParameter.ParameterType);

        using var scope = DapperHelper.CreateTransactionScope(TimeSpan.FromSeconds(30));
        var transactionId = System.Transactions.Transaction.Current?.TransactionInformation.LocalIdentifier;
        Assert.NotNull(transactionId);

        await Task.Yield();

        Assert.Equal(
            transactionId,
            System.Transactions.Transaction.Current?.TransactionInformation.LocalIdentifier);
        scope.Complete();
    }

    [Fact]
    public void PublicExtensionMethods_UseCompleteParameterNames()
    {
        var abbreviatedNames = new HashSet<string>(["con", "tran", "cons", "paras"], StringComparer.Ordinal);
        var parameterNames = new[] { typeof(DbConnectionExtensions), typeof(DbTransactionExtensions) }
            .SelectMany(type => type.GetMethods())
            .SelectMany(method => method.GetParameters())
            .Select(parameter => parameter.Name);

        Assert.DoesNotContain(parameterNames, name => name is not null && abbreviatedNames.Contains(name));
    }

    [Fact]
    public void GetSqlAdapter_DerivedConnection_UsesMostSpecificRegistration()
    {
        var baseAdapter = new SqliteAdapter();
        var middleAdapter = new NpgsqlAdapter();
        DapperHelper.RegisterSqlAdapter<RegisteredBaseConnection>(baseAdapter);
        DapperHelper.RegisterSqlAdapter(typeof(RegisteredMiddleConnection), middleAdapter);
        using var connection = new RegisteredDerivedConnection();

        var adapter = DapperHelper.GetSqlAdapter(connection);

        Assert.Same(middleAdapter, adapter);
    }

    [Fact]
    public void GetSqlAdapter_ReplacedRegistration_IsImmediatelyVisible()
    {
        var firstAdapter = new SqliteAdapter();
        var replacementAdapter = new SqlServerAdapter();
        DapperHelper.RegisterSqlAdapter<ReplaceableBaseConnection>(firstAdapter);
        using var connection = new ReplaceableDerivedConnection();
        Assert.Same(firstAdapter, DapperHelper.GetSqlAdapter(connection));

        DapperHelper.RegisterSqlAdapter<ReplaceableBaseConnection>(replacementAdapter);

        Assert.Same(replacementAdapter, DapperHelper.GetSqlAdapter(connection));
    }

    [Fact]
    public void RegisterSqlAdapter_Replacement_RemovesOldAdapterSql()
    {
        var oldAdapter = new SqliteAdapter();
        var mapping = DapperHelper.GetEntityMapping(typeof(CacheEntity));
        var key = new InsertSqlKey(oldAdapter, mapping, false, false, 1);
        DapperHelper.RegisterSqlAdapter<CacheReplacementConnection>(oldAdapter);
        DbConnectionExtensions.GetInsertCommandText(
            oldAdapter,
            null,
            mapping,
            false,
            false,
            1,
            true);
        Assert.True(DbConnectionExtensions.InsertSqls.ContainsKey(key));

        DapperHelper.RegisterSqlAdapter<CacheReplacementConnection>(new SqlServerAdapter());

        Assert.False(DbConnectionExtensions.InsertSqls.ContainsKey(key));
    }

    [Fact]
    public void GetSqlAdapter_IncomparableRegistrations_Throws()
    {
        DapperHelper.RegisterSqlAdapter<IFirstConnection>(new SqliteAdapter());
        DapperHelper.RegisterSqlAdapter<ISecondConnection>(new NpgsqlAdapter());
        using var connection = new AmbiguousConnection();

        var exception = Assert.Throws<InvalidOperationException>(() =>
            DapperHelper.GetSqlAdapter(connection));

        Assert.Contains(typeof(IFirstConnection).FullName!, exception.Message);
        Assert.Contains(typeof(ISecondConnection).FullName!, exception.Message);
    }

    [Fact]
    public void GetSqlAdapter_BuiltInConnection_UsesAssemblyAndTypeIdentity()
    {
        using var connection = new SqliteConnection();

        var adapter = DapperHelper.GetSqlAdapter(connection);

        Assert.IsType<SqliteAdapter>(adapter);
    }

    [Fact]
    public void RegisterSqlAdapter_NonConnectionType_Throws()
    {
        Assert.Throws<ArgumentException>(() =>
            DapperHelper.RegisterSqlAdapter(typeof(string), new SqliteAdapter()));
    }

    [Fact]
    public void TryRegisterSqlAdapter_ExistingExactRegistration_ReturnsFalseWithoutReplacing()
    {
        var registeredAdapter = new SqliteAdapter();

        Assert.True(DapperHelper.TryRegisterSqlAdapter<TryRegisterConnection>(registeredAdapter));
        Assert.False(DapperHelper.TryRegisterSqlAdapter(typeof(TryRegisterConnection), new SqlServerAdapter()));
        using var connection = new TryRegisterConnection();
        Assert.Same(registeredAdapter, DapperHelper.GetSqlAdapter(connection));
    }

    [Fact]
    public void TryRegisterSqlAdapter_NonConnectionType_Throws()
    {
        Assert.Throws<ArgumentException>(() =>
            DapperHelper.TryRegisterSqlAdapter(typeof(string), new SqliteAdapter()));
    }

    private class RegisteredBaseConnection : TestConnection { }
    private class RegisteredMiddleConnection : RegisteredBaseConnection { }
    private sealed class RegisteredDerivedConnection : RegisteredMiddleConnection { }

    private class ReplaceableBaseConnection : TestConnection { }
    private sealed class ReplaceableDerivedConnection : ReplaceableBaseConnection { }

    private sealed class CacheReplacementConnection : TestConnection { }
    private sealed class TryRegisterConnection : TestConnection { }

    private sealed class CacheEntity
    {
        public int Id { get; set; }
    }

    private interface IFirstConnection : IDbConnection { }
    private interface ISecondConnection : IDbConnection { }
    private sealed class AmbiguousConnection : TestConnection, IFirstConnection, ISecondConnection { }

    private abstract class TestConnection : IDbConnection
    {
        [AllowNull]
        public string ConnectionString { get; set; } = "";
        public int ConnectionTimeout => 0;
        public string Database => "";
        public ConnectionState State => ConnectionState.Closed;

        public IDbTransaction BeginTransaction()
        {
            throw new NotSupportedException();
        }

        public IDbTransaction BeginTransaction(IsolationLevel il)
        {
            throw new NotSupportedException();
        }

        public void ChangeDatabase(string databaseName)
        {
            throw new NotSupportedException();
        }

        public void Close() { }

        public IDbCommand CreateCommand()
        {
            throw new NotSupportedException();
        }

        public void Open()
        {
            throw new NotSupportedException();
        }

        public void Dispose() { }
    }
}
