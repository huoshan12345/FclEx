using System.Globalization;
using System.Text.RegularExpressions;
using Microsoft.EntityFrameworkCore.Diagnostics;
using Microsoft.EntityFrameworkCore.Storage;

namespace FclEx.EfCore;

partial class DbContextExtensions
{
    // Execute through EF's relational command pipeline so transactions, timeouts, logging, and interceptors apply.
    private sealed class IdentitySequenceCommands(
        DbContext context, RelationalDialect dialect, string tableName, string? schema, string columnName)
    {
        private readonly ISqlGenerationHelper _sql = context.GetService<ISqlGenerationHelper>();
        private string Table => _sql.DelimitIdentifier(tableName, schema);
        private string Column => _sql.DelimitIdentifier(columnName);
        private string P(string name) => _sql.GenerateParameterName("identity_" + name);
        private static bool IsNull(object? value) => value is null or DBNull;
        private static string Number(decimal value) => value.ToString(CultureInfo.InvariantCulture);

        public async Task<bool> SynchronizeAsync(CancellationToken cancellationToken)
        {
            var exists = dialect switch
            {
                RelationalDialect.SqlServer => $"SELECT COUNT(*) FROM sys.tables WHERE object_id = OBJECT_ID({P("table")}, 'U')",
                RelationalDialect.PostgreSql => $"SELECT COUNT(*) FROM pg_class WHERE oid = to_regclass({P("table")}) AND relkind IN ('r', 'p')",
                RelationalDialect.MySql => $"SELECT COUNT(*) FROM information_schema.tables WHERE table_schema = COALESCE({P("schema")}, DATABASE()) AND table_name = {P("name")} AND table_type = 'BASE TABLE'",
                RelationalDialect.Oracle => $"SELECT COUNT(*) FROM all_tables WHERE owner = COALESCE(TO_CHAR({P("schema")}), SYS_CONTEXT('USERENV', 'CURRENT_SCHEMA')) AND table_name = TO_CHAR({P("name")})",
                RelationalDialect.Sqlite => $"SELECT COUNT(*) FROM (SELECT name, type FROM temp.sqlite_master UNION ALL SELECT name, type FROM main.sqlite_master) WHERE type = 'table' AND name = {P("name")} COLLATE NOCASE",
                _ => throw new NotSupportedException($"Unsupported relational dialect '{dialect}'."),
            };
            (string, object?)[] parameters = dialect switch
            {
                RelationalDialect.SqlServer or RelationalDialect.PostgreSql => [("table", Table)],
                RelationalDialect.Sqlite => [("name", tableName)],
                _ => [("schema", schema), ("name", tableName)],
            };
            if (Convert.ToInt64(await ReadAsync(exists, cancellationToken, parameters).ConfigureAwait(false)) == 0)
                return false;
            return dialect switch
            {
                RelationalDialect.SqlServer => await SynchronizeSqlServerAsync(cancellationToken).ConfigureAwait(false),
                RelationalDialect.PostgreSql => await SynchronizePostgreSqlAsync(cancellationToken).ConfigureAwait(false),
                RelationalDialect.MySql => await SynchronizeMySqlAsync(cancellationToken).ConfigureAwait(false),
                RelationalDialect.Oracle => await SynchronizeOracleAsync(cancellationToken).ConfigureAwait(false),
                RelationalDialect.Sqlite => await SynchronizeSqliteAsync(cancellationToken).ConfigureAwait(false),
                _ => throw new NotSupportedException($"Unsupported relational dialect '{dialect}'."),
            };
        }

        private async Task<bool> SynchronizePostgreSqlAsync(CancellationToken cancellationToken)
        {
            var sequence = await ReadAsync(
                $"SELECT pg_get_serial_sequence({P("table")}, {P("column")}) FROM pg_attribute " +
                $"WHERE attrelid = CAST({P("table")} AS regclass) AND attname = {P("column")} AND attnum > 0 AND NOT attisdropped",
                cancellationToken, ("table", Table), ("column", columnName)).ConfigureAwait(false);
            if (IsNull(sequence))
                return false;
            var increment = Convert.ToDecimal(await ReadAsync(
                $"SELECT seqincrement FROM pg_sequence WHERE seqrelid = CAST({P("sequence")} AS regclass)", cancellationToken,
                ("sequence", sequence)).ConfigureAwait(false));
            if (increment <= 0)
                throw new NotSupportedException("Identity synchronization requires an ascending PostgreSQL sequence.");
            var start = Convert.ToDecimal(await ReadAsync(
                $"SELECT seqstart FROM pg_sequence WHERE seqrelid = CAST({P("sequence")} AS regclass)", cancellationToken,
                ("sequence", sequence)).ConfigureAwait(false));
            var maximum = await ReadAsync($"SELECT MAX({Column}) FROM {Table}", cancellationToken).ConfigureAwait(false);
            var next = IsNull(maximum) ? start : Math.Max(start, checked(Convert.ToDecimal(maximum) + increment));
            await ReadAsync($"SELECT setval(CAST({P("sequence")} AS regclass), {P("next")}, false)", cancellationToken,
                ("sequence", sequence), ("next", checked((long)next))).ConfigureAwait(false);
            return true;
        }

        private async Task<bool> SynchronizeSqlServerAsync(CancellationToken cancellationToken)
        {
            var predicate = $"FROM sys.identity_columns WHERE object_id = OBJECT_ID({P("table")}, 'U') AND name = {P("column")}";
            var seed = await ReadAsync("SELECT seed_value " + predicate, cancellationToken,
                ("table", Table), ("column", columnName)).ConfigureAwait(false);
            if (IsNull(seed))
                return false;
            var increment = Convert.ToDecimal(await ReadAsync("SELECT increment_value " + predicate, cancellationToken,
                ("table", Table), ("column", columnName)).ConfigureAwait(false));
            if (increment <= 0)
                throw new NotSupportedException("Identity synchronization requires an ascending SQL Server identity.");
            var maximum = await ReadAsync($"SELECT MAX({Column}) FROM {Table}", cancellationToken).ConfigureAwait(false);
            if (IsNull(maximum))
            {
                var last = await ReadAsync("SELECT last_value " + predicate, cancellationToken,
                    ("table", Table), ("column", columnName)).ConfigureAwait(false);
                // Never-used and truncated identities already start at their seed.
                if (IsNull(last))
                    return true;
            }
            var reseed = IsNull(maximum) ? checked(Convert.ToDecimal(seed) - increment) : Convert.ToDecimal(maximum);
            await ExecuteAsync($"DBCC CHECKIDENT (N'{Table.Replace("'", "''")}', RESEED, {Number(reseed)}) WITH NO_INFOMSGS", cancellationToken).ConfigureAwait(false);
            return true;
        }

        private async Task<bool> SynchronizeMySqlAsync(CancellationToken cancellationToken)
        {
            var count = await ReadAsync(
                $"SELECT COUNT(*) FROM information_schema.columns WHERE table_schema = COALESCE({P("schema")}, DATABASE()) " +
                $"AND table_name = {P("name")} AND column_name = {P("column")} AND extra LIKE '%auto_increment%'", cancellationToken,
                ("schema", schema), ("name", tableName), ("column", columnName)).ConfigureAwait(false);
            if (Convert.ToInt64(count) == 0)
                return false;
            var maximum = await ReadAsync($"SELECT MAX({Column}) FROM {Table}", cancellationToken).ConfigureAwait(false);
            var next = IsNull(maximum) ? 1m : Math.Max(1m, checked(Convert.ToDecimal(maximum) + 1m));
            await ExecuteAsync($"ALTER TABLE {Table} AUTO_INCREMENT = {Number(next)}", cancellationToken).ConfigureAwait(false);
            return true;
        }

        private async Task<bool> SynchronizeOracleAsync(CancellationToken cancellationToken)
        {
            var generation = await ReadAsync(
                $"SELECT generation_type FROM all_tab_identity_cols WHERE table_name = TO_CHAR({P("name")}) " +
                $"AND column_name = TO_CHAR({P("column")}) AND owner = COALESCE(TO_CHAR({P("schema")}), SYS_CONTEXT('USERENV', 'CURRENT_SCHEMA'))", cancellationToken,
                ("name", tableName), ("column", columnName), ("schema", schema)).ConfigureAwait(false);
            if (IsNull(generation))
                return false;
            var mode = Convert.ToString(generation)?.Trim();
            if (mode is not ("ALWAYS" or "BY DEFAULT" or "BY DEFAULT ON NULL"))
                throw new NotSupportedException($"Unsupported Oracle identity generation mode '{mode}'.");
            await ExecuteAsync($"ALTER TABLE {Table} MODIFY {Column} GENERATED {mode} AS IDENTITY (START WITH LIMIT VALUE)", cancellationToken).ConfigureAwait(false);
            return true;
        }

        private async Task<bool> SynchronizeSqliteAsync(CancellationToken cancellationToken)
        {
            var temp = await ReadAsync($"SELECT COUNT(*) FROM temp.sqlite_master WHERE type = 'table' AND name = {P("name")} COLLATE NOCASE",
                cancellationToken, ("name", tableName)).ConfigureAwait(false);
            var database = Convert.ToInt64(temp) > 0 ? "temp" : "main";
            var definition = await ReadAsync($"SELECT sql FROM {database}.sqlite_master WHERE type = 'table' AND name = {P("name")} COLLATE NOCASE",
                cancellationToken, ("name", tableName)).ConfigureAwait(false);
            var tokens = Regex.Replace(Convert.ToString(definition) ?? "",
                "--[^\\r\\n]*|/\\*[\\s\\S]*?\\*/|'(?:''|[^'])*'|\"(?:\"\"|[^\"])*\"|`(?:``|[^`])*`|\\[[^\\]]*\\]", " ");
            if (!Regex.IsMatch(tokens, @"\bAUTOINCREMENT\b", RegexOptions.IgnoreCase))
                return false;
            var key = await ReadAsync($"SELECT COUNT(*) FROM pragma_table_info({P("name")}, '{database}') " +
                $"WHERE name = {P("column")} COLLATE NOCASE AND pk = 1 AND upper(type) = 'INTEGER'", cancellationToken,
                ("name", tableName), ("column", columnName)).ConfigureAwait(false);
            if (Convert.ToInt64(key) == 0)
                return false;
            await ExecuteAsync($"DELETE FROM {database}.sqlite_sequence WHERE name = {P("name")} COLLATE NOCASE", cancellationToken,
                ("name", tableName)).ConfigureAwait(false);
            await ExecuteAsync($"INSERT INTO {database}.sqlite_sequence(name, seq) SELECT name, " +
                $"MAX(0, COALESCE((SELECT MAX({Column}) FROM {database}.{_sql.DelimitIdentifier(tableName)}), 0)) " +
                $"FROM {database}.sqlite_master WHERE type = 'table' AND name = {P("name")} COLLATE NOCASE", cancellationToken,
                ("name", tableName)).ConfigureAwait(false);
            return true;
        }

        private RawSqlCommand Build(string sql, (string Name, object? Value)[] parameters)
        {
            using var factory = context.Database.GetDbConnection().CreateCommand();
            var values = parameters.Select(item =>
            {
                var parameter = factory.CreateParameter();
                parameter.ParameterName = "identity_" + item.Name;
                parameter.Value = item.Value ?? DBNull.Value;
                return (object)parameter;
            }).ToArray();
            // RawSqlCommandBuilder formats SQL even when parameters are explicitly named. Escape literal braces
            // in quoted model identifiers; parameter values are never interpolated into the SQL.
            return context.GetService<IRawSqlCommandBuilder>().Build(sql.Replace("{", "{{").Replace("}", "}}"), values);
        }

        private RelationalCommandParameterObject Settings(RawSqlCommand raw)
            => new(context.GetService<IRelationalConnection>(), raw.ParameterValues, null, context,
                context.GetService<IRelationalCommandDiagnosticsLogger>(), CommandSource.ExecuteSqlRaw);

        private Task<object?> ReadAsync(string sql, CancellationToken cancellationToken, params (string Name, object? Value)[] parameters)
        {
            var raw = Build(sql, parameters);
            return raw.RelationalCommand.ExecuteScalarAsync(Settings(raw), cancellationToken);
        }

        private Task<int> ExecuteAsync(string sql, CancellationToken cancellationToken, params (string Name, object? Value)[] parameters)
        {
            var raw = Build(sql, parameters);
            return raw.RelationalCommand.ExecuteNonQueryAsync(Settings(raw), cancellationToken);
        }
    }
}
