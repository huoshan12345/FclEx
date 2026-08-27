namespace FclEx.Dapper;

public class SqlAdapterTests
{
    [Theory]
    [MemberData(nameof(QuotedIdentifierCases))]
    public void GetQuotedNames_EscapesOnlyTerminatingDelimiter(
        ISqlAdapter adapter,
        string name,
        string expected)
    {
        Assert.Equal(expected, adapter.GetQuotedTableName(name));
        Assert.Equal(expected, adapter.GetQuotedColumnName(name));
    }

    [Theory]
    [InlineData(1, 1000)]
    [InlineData(2, 1000)]
    [InlineData(3, 700)]
    [InlineData(2100, 1)]
    public void SqlServer_GetMaxInsertBatchSize_ObservesProviderLimits(
        int parameterCountPerRow,
        int expected)
    {
        Assert.Equal(expected, new SqlServerAdapter().GetMaxInsertBatchSize(parameterCountPerRow));
    }

    [Fact]
    public void SqlServer_BuildInsertCommandText_PlacesOutputBeforeValues()
    {
        var sql = new SqlServerAdapter().BuildInsertCommandText(
            "[dbo].[Users]",
            "[Name], [Age]",
            "(@Name_0, @Age_0)",
            "[Id]");

        Assert.Equal(
            $"INSERT INTO [dbo].[Users] ([Name], [Age]){Environment.NewLine}" +
            $"OUTPUT INSERTED.[Id]{Environment.NewLine}" +
            $"VALUES{Environment.NewLine}" +
            "(@Name_0, @Age_0)",
            sql);
    }

    [Fact]
    public void Npgsql_BuildInsertCommandText_ReturnsMappedKey()
    {
        var sql = new NpgsqlAdapter().BuildInsertCommandText(
            "\"Users\"",
            "\"Name\"",
            "(@Name_0)",
            "\"Id\"");

        Assert.Equal(
            $"INSERT INTO \"Users\" (\"Name\"){Environment.NewLine}" +
            $"VALUES{Environment.NewLine}" +
            $"(@Name_0){Environment.NewLine}" +
            "RETURNING \"Id\"",
            sql);
    }

    [Fact]
    public void Sqlite_BuildInsertCommandText_AppendsGeneratedKeyQuery()
    {
        var sql = new SqliteAdapter().BuildInsertCommandText(
            "\"Users\"",
            "\"Name\"",
            "(@Name_0)",
            "\"Id\"");

        Assert.Equal(
            $"INSERT INTO \"Users\" (\"Name\"){Environment.NewLine}" +
            $"VALUES{Environment.NewLine}" +
            $"(@Name_0);{Environment.NewLine}" +
            "SELECT last_insert_rowid()",
            sql);
    }

    [Theory]
    [MemberData(nameof(MySqlAdapters))]
    public void MySql_BuildInsertCommandText_DefaultValuesUsesEmptyRow(ISqlAdapter adapter)
    {
        var sql = adapter.BuildInsertCommandText("`Users`", null, null, "`Id`");

        Assert.Equal(
            $"INSERT INTO `Users` () VALUES ();{Environment.NewLine}" +
            "SELECT LAST_INSERT_ID()",
            sql);
    }

    public static TheoryData<ISqlAdapter> MySqlAdapters => new()
    {
        new MySqlAdapter(),
        new MySqlConnectorAdapter(),
    };

    public static TheoryData<ISqlAdapter, string, string> QuotedIdentifierCases => new()
    {
        { new SqlServerAdapter(), "name[part", "[name[part]" },
        { new SqlServerAdapter(), "name]part", "[name]]part]" },
        { new NpgsqlAdapter(), "name\"part", "\"name\"\"part\"" },
        { new SqliteAdapter(), "name\"part", "\"name\"\"part\"" },
        { new MySqlAdapter(), "name`part", "`name``part`" },
        { new MySqlConnectorAdapter(), "name`part", "`name``part`" },
    };
}
