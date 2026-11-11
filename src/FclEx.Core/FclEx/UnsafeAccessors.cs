namespace FclEx;

#if NET8_0_OR_GREATER
/// <summary>Exposes writable references to selected non-public runtime fields on .NET 8 and later.</summary>
/// <remarks>
/// These methods bypass validation and synchronization. Do not mutate shared objects concurrently or invalidate
/// cached state. The methods require the exact non-public field names and types used by the running runtime;
/// incompatible runtimes can throw MissingFieldException. Availability on .NET 8 does not guarantee that future
/// runtimes retain these fields. These accessors bind directly to the named runtime fields.
/// </remarks>
public static class UnsafeAccessors
{
    /// <summary>Returns a writable reference to the raw message stored by Exception.</summary>
    /// <param name="ex">The exception whose raw message storage is accessed.</param>
    /// <returns>A writable reference to the nullable raw message, independent of overrides of Exception.Message.</returns>
    [UnsafeAccessor(UnsafeAccessorKind.Field, Name = "_message")]
    public static extern ref string? Exception_Message(Exception ex);

    /// <summary>Returns a writable reference to the cached stack-trace string stored by Exception.</summary>
    /// <param name="ex">The exception whose cached stack-trace string is accessed.</param>
    /// <returns>A writable reference to the nullable cached string, excluding other captured runtime stack data.</returns>
    [UnsafeAccessor(UnsafeAccessorKind.Field, Name = "_stackTraceString")]
    public static extern ref string? Exception_StackTrace(Exception ex);

    /// <summary>Returns a writable reference to a physical address's internal byte array.</summary>
    /// <param name="obj">The physical address whose internal array is accessed.</param>
    /// <returns>A writable reference to the stored byte array. Changes bypass cached hash-code maintenance.</returns>
    [UnsafeAccessor(UnsafeAccessorKind.Field, Name = "_address")]
    public static extern ref byte[] PhysicalAddress_Address(PhysicalAddress obj);

    /// <summary>Returns a writable reference to an IP address's internal IPv6 segment array.</summary>
    /// <param name="obj">The address whose IPv6 segment storage is accessed.</param>
    /// <returns>A writable reference to the eight network-order segments; the referenced value is null for IPv4 despite the signature.</returns>
    [UnsafeAccessor(UnsafeAccessorKind.Field, Name = "_numbers")]
    public static extern ref ushort[] IPAddress_Numbers(IPAddress obj);

    /// <summary>Returns a writable reference to an invoker's root HTTP handler.</summary>
    /// <param name="obj">The invoker or HttpClient whose root handler is accessed.</param>
    /// <returns>A writable reference to the root handler. Assignment does not dispose the replaced handler or change disposal ownership.</returns>
    [UnsafeAccessor(UnsafeAccessorKind.Field, Name = "_handler")]
    public static extern ref HttpMessageHandler HttpMessageInvoker_Handler(HttpMessageInvoker obj);

    // NOTE: _flags in HttpRequestMessage is a private enum type that cannot be accessed using UnsafeAccessor and UnsafeAccessorType.

#if !NET11_0_OR_GREATER
    /// <summary>Returns a writable reference to a request's pre-.NET 11 send-state integer.</summary>
    /// <param name="request">The request whose send state is accessed on .NET 8 through .NET 10.</param>
    /// <returns>A writable reference to the send-state integer; zero means not yet sent.</returns>
    [UnsafeAccessor(UnsafeAccessorKind.Field, Name = "_sendStatus")]
    public static extern ref int HttpRequestMessage_SendStatus(HttpRequestMessage request);
#endif
}
#endif
