namespace FclEx.Dapper;

/// <summary>
/// Defines how explicit FclEx Dapper configuration handles an existing custom type map.
/// </summary>
public enum DapperRegistrationConflictBehavior
{
    /// <summary>
    /// Throws an <see cref="InvalidOperationException"/> without applying any selected mappings.
    /// </summary>
    Throw,

    /// <summary>
    /// Preserves the existing custom type map and skips that entity type.
    /// </summary>
    KeepExisting,

    /// <summary>
    /// Temporarily replaces the existing custom type map until the returned registration is disposed.
    /// </summary>
    Replace,
}
