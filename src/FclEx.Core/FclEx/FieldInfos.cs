namespace FclEx;

public static class FieldInfos
{
    private static readonly Lazy<FieldInfo> _Exception_Message = new(() => typeof(Exception).GetRequiredField("_message"));
    private static readonly Lazy<FieldInfo> _Exception_StackTrace = new(() => typeof(Exception).GetRequiredField("_stackTraceString"));

    public static FieldInfo Exception_Message => _Exception_Message.Value;
    public static FieldInfo Exception_StackTrace => _Exception_StackTrace.Value;

    private static readonly Lazy<FieldInfo> _PhysicalAddress_Address = new(() => typeof(PhysicalAddress).GetRequiredField(
#if NETFRAMEWORK
            "address"
#else
    "_address"
#endif
));

    public static FieldInfo PhysicalAddress_Address => _PhysicalAddress_Address.Value;

    private static readonly Lazy<FieldInfo> _IPAddress_Numbers = new(() => typeof(IPAddress).GetRequiredField(
#if NETFRAMEWORK
            "m_Numbers"
#else
        "_numbers"
#endif
    ));

    public static FieldInfo IPAddress_Numbers => _IPAddress_Numbers.Value;


    private static readonly Lazy<FieldInfo> _HttpMessageInvoker_Handler = new(() => typeof(HttpMessageInvoker).GetRequiredField(
#if NETFRAMEWORK
            "handler"
#else
        "_handler"
#endif
    ));

    public static FieldInfo HttpMessageInvoker_Handler => _HttpMessageInvoker_Handler.Value;

#if NET11_0_OR_GREATER
    private static readonly Lazy<FieldInfo> _HttpRequestMessage_Flags = new(() => typeof(HttpRequestMessage).GetRequiredField("_flags"));

    public static FieldInfo HttpRequestMessage_Flags => _HttpRequestMessage_Flags.Value;

#else
        private static readonly Lazy<FieldInfo> _HttpRequestMessage_SendStatus = new(() => typeof(HttpRequestMessage).GetRequiredField(
#if NETFRAMEWORK
            "sendStatus"
#else
            "_sendStatus"
#endif
        ));

        public static FieldInfo HttpRequestMessage_SendStatus => _HttpRequestMessage_SendStatus.Value;
#endif
}