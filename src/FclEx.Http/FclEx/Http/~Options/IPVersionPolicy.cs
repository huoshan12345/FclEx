namespace FclEx.Http;

/// <summary>
/// Address-family policy used by the custom socket connect callback.
/// </summary>
public enum IPVersionPolicy
{
    /// <summary>Use only IPv4 addresses.</summary>
    OnlyIPv4,
    /// <summary>Use only IPv6 addresses.</summary>
    OnlyIPv6,
    /// <summary>Try IPv4 addresses before IPv6 addresses.</summary>
    PreferIPv4,
    /// <summary>Try IPv6 addresses before IPv4 addresses.</summary>
    PreferIPv6,
}
