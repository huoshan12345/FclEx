namespace FclEx.Web;

/// <summary>
/// Represents whether a user account can be used for login.
/// </summary>
public enum UserAccountStatus
{
    /// <summary>The account can be used normally.</summary>
    Normal,
    /// <summary>The supplied credentials are not accepted.</summary>
    InvalidCredentials,
    /// <summary>The account is locked by the remote service or application policy.</summary>
    Locked,
}
