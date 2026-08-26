using FclEx.Dapper.SqlAdapters;
using Microsoft.Data.Sqlite;
using System.Diagnostics.CodeAnalysis;

namespace FclEx.Dapper;

public class DapperHelperTests
{
    [Fact]
    public void GetSqlAdapter_DerivedConnection_UsesMostSpecificRegistration()
    {
        DapperHelper.RegisterSqlAdapter<RegisteredBaseConnection>(SqliteAdapter.Instance);
        DapperHelper.RegisterSqlAdapter(typeof(RegisteredMiddleConnection), NpgsqlAdapter.Instance);
        using var connection = new RegisteredDerivedConnection();

        var adapter = DapperHelper.GetSqlAdapter(connection);

        Assert.Same(NpgsqlAdapter.Instance, adapter);
    }

    [Fact]
    public void GetSqlAdapter_ReplacedRegistration_IsImmediatelyVisible()
    {
        DapperHelper.RegisterSqlAdapter<ReplaceableBaseConnection>(SqliteAdapter.Instance);
        using var connection = new ReplaceableDerivedConnection();
        Assert.Same(SqliteAdapter.Instance, DapperHelper.GetSqlAdapter(connection));

        DapperHelper.RegisterSqlAdapter<ReplaceableBaseConnection>(SqlServerAdapter.Instance);

        Assert.Same(SqlServerAdapter.Instance, DapperHelper.GetSqlAdapter(connection));
    }

    [Fact]
    public void GetSqlAdapter_IncomparableRegistrations_Throws()
    {
        DapperHelper.RegisterSqlAdapter<IFirstConnection>(SqliteAdapter.Instance);
        DapperHelper.RegisterSqlAdapter<ISecondConnection>(NpgsqlAdapter.Instance);
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

        Assert.Same(SqliteAdapter.Instance, adapter);
    }

    [Fact]
    public void RegisterSqlAdapter_NonConnectionType_Throws()
    {
        Assert.Throws<ArgumentException>(() =>
            DapperHelper.RegisterSqlAdapter(typeof(string), SqliteAdapter.Instance));
    }

    private class RegisteredBaseConnection : TestConnection { }
    private class RegisteredMiddleConnection : RegisteredBaseConnection { }
    private sealed class RegisteredDerivedConnection : RegisteredMiddleConnection { }

    private class ReplaceableBaseConnection : TestConnection { }
    private sealed class ReplaceableDerivedConnection : ReplaceableBaseConnection { }

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
