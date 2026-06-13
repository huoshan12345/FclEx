namespace FclEx.Web;

/// <summary>
/// Represents the current login/session lifecycle of a user client.
/// </summary>
public enum UserClientSessionStatus
{
    /// <summary>The client is not logged in.</summary>
    Offline,
    /// <summary>The client is logged in and usable.</summary>
    Online,
    /// <summary>The client is currently performing login.</summary>
    LoggingIn,
    /// <summary>The client is currently performing logout.</summary>
    LoggingOut,
    /// <summary>The login flow is waiting for captcha input.</summary>
    AwaitingCaptcha,
    /// <summary>The login flow is waiting for an additional challenge.</summary>
    AwaitingChallenge,
}
