namespace FclEx;

/// <summary>Provides cached descriptors for non-public fields used by runtime inspection and mutation helpers.</summary>
/// <remarks>
/// Each field is resolved independently on first access and the descriptor is then reused across threads.
/// Names and types depend on runtime internals; resolution can fail when those internals change or are trimmed.
/// Writing through a descriptor bypasses the declaring type's validation and synchronization.
/// </remarks>
public static class FieldInfos
{
    private static readonly Lazy<FieldInfo> _Exception_Message = new(() => typeof(Exception).GetRequiredField("_message"));
    private static readonly Lazy<FieldInfo> _Exception_StackTrace = new(() => typeof(Exception).GetRequiredField("_stackTraceString"));

    /// <summary>The nullable raw message field on Exception, before any virtual Message getter or fallback text is applied.</summary>
    public static FieldInfo Exception_Message => _Exception_Message.Value;
    /// <summary>The nullable cached stack-trace string on Exception; this does not include all captured runtime stack data.</summary>
    public static FieldInfo Exception_StackTrace => _Exception_StackTrace.Value;

    private static readonly Lazy<FieldInfo> _PhysicalAddress_Address = new(() => typeof(PhysicalAddress).GetRequiredField(
#if NETFRAMEWORK
        "address"
#else
        "_address"
#endif
    ));

    /// <summary>The byte-array field storing a physical address; mutations change the address without updating cached state.</summary>
    public static FieldInfo PhysicalAddress_Address => _PhysicalAddress_Address.Value;

    private static readonly Lazy<FieldInfo> _IPAddress_Numbers = new(() => typeof(IPAddress).GetRequiredField(
#if NETFRAMEWORK
        "m_Numbers"
#else
        "_numbers"
#endif
    ));

    /// <summary>The eight network-order IPv6 segments; IPv4 storage is null on modern .NET and eight zero segments on .NET Framework.</summary>
    public static FieldInfo IPAddress_Numbers => _IPAddress_Numbers.Value;


    private static readonly Lazy<FieldInfo> _HttpMessageInvoker_Handler = new(() => typeof(HttpMessageInvoker).GetRequiredField(
#if NETFRAMEWORK
        "handler"
#else
        "_handler"
#endif
    ));

    /// <summary>The root handler field shared by HttpMessageInvoker and HttpClient; replacing it does not dispose the previous handler.</summary>
    public static FieldInfo HttpMessageInvoker_Handler => _HttpMessageInvoker_Handler.Value;

#if NET11_0_OR_GREATER
    private static readonly Lazy<FieldInfo> _HttpRequestMessage_Flags = new(() => typeof(HttpRequestMessage).GetRequiredField("_flags"));

    /// <summary>The private enum field containing request state flags in .NET 11 and later, including sent and disposed state.</summary>
    public static FieldInfo HttpRequestMessage_Flags => _HttpRequestMessage_Flags.Value;

#else
    private static readonly Lazy<FieldInfo> _HttpRequestMessage_SendStatus = new(() => typeof(HttpRequestMessage).GetRequiredField(
#if NETFRAMEWORK
        "sendStatus"
#else
        "_sendStatus"
#endif
    ));

    /// <summary>The integer send-state field on runtimes before .NET 11; zero denotes a request that has not been sent.</summary>
    public static FieldInfo HttpRequestMessage_SendStatus => _HttpRequestMessage_SendStatus.Value;
#endif
}