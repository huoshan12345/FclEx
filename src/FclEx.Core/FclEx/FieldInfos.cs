namespace FclEx;

public static class FieldInfos
{
    public static readonly FieldInfo Exception_Message = typeof(Exception).GetRequiredField("_message");
    public static readonly FieldInfo Exception_StackTrace = typeof(Exception).GetRequiredField("_stackTraceString");

    public static readonly FieldInfo PhysicalAddress_Address = typeof(PhysicalAddress).GetRequiredField(
#if NETFRAMEWORK
            "address"
#else
            "_address"
#endif
        );

    public static readonly FieldInfo IPAddress_Numbers = typeof(IPAddress).GetRequiredField(
#if NETFRAMEWORK
            "m_Numbers"
#else
            "_numbers"
#endif
        );

    public static readonly FieldInfo HttpMessageInvoker_Handler = typeof(HttpMessageInvoker).GetRequiredField(
#if NETFRAMEWORK
            "handler"
#else
            "_handler"
#endif
        );

    public static readonly FieldInfo HttpRequestMessage_SendStatus = typeof(HttpRequestMessage).GetRequiredField(
#if NETFRAMEWORK
            "sendStatus"
#else
            "_sendStatus"
#endif
        );
}