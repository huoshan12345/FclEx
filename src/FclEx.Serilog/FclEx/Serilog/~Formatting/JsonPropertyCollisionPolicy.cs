namespace FclEx.Serilog;

/// <summary>Resolves top-level event and linking-metadata property names that collide after JSON naming and escaping.</summary>
/// <remarks>Basic field names remain reserved and are never overridden under any policy.</remarks>
public enum JsonPropertyCollisionPolicy
{
    /// <summary>Write the first encountered value and omit later values with the same final name.</summary>
    KeepFirst = 0,

    /// <summary>Write the last encountered value, retaining the first occurrence's output position.</summary>
    KeepLast = 1,

    /// <summary>Throw InvalidOperationException without writing an event when a duplicate name is encountered.</summary>
    Throw = 2,
}
