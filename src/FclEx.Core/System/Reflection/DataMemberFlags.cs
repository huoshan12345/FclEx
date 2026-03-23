namespace System.Reflection;

/// <summary>
/// Specifies filters for selecting data members (fields and properties)
/// from a type. Flags can be combined to control visibility, scope,
/// member kind, and access capabilities.
/// </summary>
[Flags]
public enum DataMemberFlags
{
    /// <summary>No flags.</summary>
    None = 0,

    /// <summary>Members declared on the specified type.</summary>
    Declared = 1 << 0,

    /// <summary>Members inherited from base types.</summary>
    Inherited = 1 << 1,

    /// <summary>Instance members.</summary>
    Instance = 1 << 2,

    /// <summary>Static members.</summary>
    Static = 1 << 3,

    /// <summary>Public members.</summary>
    Public = 1 << 4,

    /// <summary>Non-public members (private, protected, internal, etc.).</summary>
    NonPublic = 1 << 5,

    /// <summary>Fields.</summary>
    Field = 1 << 6,

    /// <summary>Compiler-generated backing fields of auto-properties.</summary>
    AutoPropertyBackingField = 1 << 7,

    /// <summary>
    /// Properties (excluding indexers unless <see cref="Indexer"/> is specified).
    /// </summary>
    Property = 1 << 8,

    /// <summary>Indexer properties (properties with parameters).</summary>
    Indexer = 1 << 9,

    /// <summary>Members that can be read.</summary>
    CanRead = 1 << 10,

    /// <summary>Members that can be written.</summary>
    CanWrite = 1 << 11,

    /// <summary>
    /// Members that are writable via reflection bypassing normal language constraints
    /// (e.g. init-only properties or readonly fields).
    /// </summary>
    UnsafeWrite = 1 << 12,
}