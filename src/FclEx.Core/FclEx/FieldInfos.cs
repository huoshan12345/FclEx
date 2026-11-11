namespace FclEx;

public static class FieldInfos
{
    public static class Exception
    {
        private static readonly Lazy<FieldInfo> _message = new(() => typeof(Exception).GetRequiredField("_message"));
        private static readonly Lazy<FieldInfo> _stackTrace = new(() => typeof(Exception).GetRequiredField("_stackTraceString"));

        public static FieldInfo Message => _message.Value;
        public static FieldInfo StackTrace => _stackTrace.Value;

#if NET8_0_OR_GREATER
        [UnsafeAccessor(UnsafeAccessorKind.Field, Name = "_message")]
        public static extern ref string? GetMessage(System.Exception ex);

        [UnsafeAccessor(UnsafeAccessorKind.Field, Name = "_stackTraceString")]
        public static extern ref string? GetStackTrace(System.Exception ex);
#endif
    }

    public static class PhysicalAddress
    {
        private static readonly Lazy<FieldInfo> _address = new(() => typeof(PhysicalAddress).GetRequiredField(
#if NETFRAMEWORK
            "address"
#else
            "_address"
#endif
        ));

        public static FieldInfo Address => _address.Value;

#if NET8_0_OR_GREATER
        [UnsafeAccessor(UnsafeAccessorKind.Field, Name = "_address")]
        public static extern ref byte[] GetAddress(System.Net.NetworkInformation.PhysicalAddress obj);
#endif
    }

    public static class IPAddress
    {
        private static readonly Lazy<FieldInfo> _numbers = new(() => typeof(PhysicalAddress).GetRequiredField(
#if NETFRAMEWORK
            "m_Numbers"
#else
            "_numbers"
#endif
        ));

        public static FieldInfo Numbers => _numbers.Value;

#if NET8_0_OR_GREATER
        [UnsafeAccessor(UnsafeAccessorKind.Field, Name = "_numbers")]
        public static extern ref ushort[] GetNumbers(System.Net.IPAddress obj);
#endif
    }

    public static class HttpMessageInvoker
    {
        private static readonly Lazy<FieldInfo> _handler = new(() => typeof(HttpMessageInvoker).GetRequiredField(
#if NETFRAMEWORK
            "handler"
#else
            "_handler"
#endif
        ));

        public static FieldInfo Handler => _handler.Value;

#if NET8_0_OR_GREATER
        [UnsafeAccessor(UnsafeAccessorKind.Field, Name = "_handler")]
        public static extern ref HttpMessageHandler GetHandler(System.Net.Http.HttpMessageInvoker obj);
#endif
    }

    public static class HttpRequestMessage
    {
#if NET11_0_OR_GREATER
        private static readonly Lazy<FieldInfo> _flags = new(() => typeof(HttpRequestMessage).GetRequiredField("_flags"));

        public static FieldInfo Flags => _flags.Value;

        [UnsafeAccessor(UnsafeAccessorKind.Field, Name = "_flags")]
        public static extern ref int GetFlags(System.Net.Http.HttpRequestMessage request);
#else
        private static readonly Lazy<FieldInfo> _sendStatus = new(() => typeof(HttpRequestMessage).GetRequiredField(
#if NETFRAMEWORK
            "sendStatus"
#else
            "_sendStatus"
#endif
        ));

        public static FieldInfo SendStatus => _sendStatus.Value;
#endif

#if NET8_0_OR_GREATER
        [UnsafeAccessor(UnsafeAccessorKind.Field, Name = "_sendStatus")]
        public static extern ref int GetSendStatus(System.Net.Http.HttpRequestMessage request);
#endif
    }
}