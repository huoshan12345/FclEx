namespace FclEx;

#if NET8_0_OR_GREATER
public static class UnsafeAccessors
{
    [UnsafeAccessor(UnsafeAccessorKind.Field, Name = "_message")]
    public static extern ref string? Exception_Message(Exception ex);

    [UnsafeAccessor(UnsafeAccessorKind.Field, Name = "_stackTraceString")]
    public static extern ref string? Exception_StackTrace(Exception ex);

    [UnsafeAccessor(UnsafeAccessorKind.Field, Name = "_address")]
    public static extern ref byte[] PhysicalAddress_Address(PhysicalAddress obj);

    [UnsafeAccessor(UnsafeAccessorKind.Field, Name = "_numbers")]
    public static extern ref ushort[] IPAddress_Numbers(IPAddress obj);

    [UnsafeAccessor(UnsafeAccessorKind.Field, Name = "_handler")]
    public static extern ref HttpMessageHandler HttpMessageInvoker_Handler(HttpMessageInvoker obj);

    // NOTE: _flags in HttpRequestMessage is a private enum type that cannot be accessed using UnsafeAccessor and UnsafeAccessorType.

#if !NET11_0_OR_GREATER
    [UnsafeAccessor(UnsafeAccessorKind.Field, Name = "_sendStatus")]
    public static extern ref int HttpRequestMessage_SendStatus(HttpRequestMessage request);
#endif
}
#endif
