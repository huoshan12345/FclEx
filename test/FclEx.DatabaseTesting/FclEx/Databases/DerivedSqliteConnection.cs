namespace FclEx.Databases;

/// <summary>A real SQLite connection subclass used to verify provider resolution through base connection types.</summary>
public sealed class DerivedSqliteConnection(string connectionString) : SqliteConnection(connectionString);
