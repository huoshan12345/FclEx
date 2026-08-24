namespace FclEx.Dapper;

/// <summary>
/// Describes when a database generates the value of a mapped property.
/// </summary>
public enum DatabaseValueGeneration
{
    /// <summary>
    /// The application supplies the value for inserts and updates.
    /// </summary>
    None,

    /// <summary>
    /// The database generates the value when a row is inserted, such as an identity key.
    /// </summary>
    OnInsert,

    /// <summary>
    /// The database generates the value when a row is inserted or updated, such as a computed column.
    /// </summary>
    OnInsertOrUpdate,
}
