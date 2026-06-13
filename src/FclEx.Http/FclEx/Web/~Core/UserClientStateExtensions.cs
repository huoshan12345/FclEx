namespace FclEx.Web;

/// <summary>
/// Convenience methods for reading and changing user-client state.
/// </summary>
public static class UserClientStateExtensions
{
    /// <summary>
    /// Returns whether the client is waiting for captcha input.
    /// </summary>
    public static bool IsAwaitingCaptcha(this IUserClientState state)
    {
        return state.SessionStatus == UserClientSessionStatus.AwaitingCaptcha;
    }

    /// <summary>
    /// Returns whether the client is waiting for an additional login challenge.
    /// </summary>
    public static bool IsAwaitingChallenge(this IUserClientState state)
    {
        return state.SessionStatus == UserClientSessionStatus.AwaitingChallenge;
    }

    /// <summary>
    /// Returns whether the client is currently in the login flow.
    /// </summary>
    public static bool IsLoggingIn(this IUserClientState state)
    {
        return state.SessionStatus == UserClientSessionStatus.LoggingIn;
    }

    /// <summary>
    /// Returns whether the account status is normal.
    /// </summary>
    public static bool IsAccountNormal(this IUserClientState state)
    {
        return state.AccountStatus == UserAccountStatus.Normal;
    }

    /// <summary>
    /// Sets the session status to offline.
    /// </summary>
    public static void Offline(this IUserClientState state)
    {
        state.SessionStatus = UserClientSessionStatus.Offline;
    }

    /// <summary>
    /// Sets the session status to online.
    /// </summary>
    public static void Online(this IUserClientState state)
    {
        state.SessionStatus = UserClientSessionStatus.Online;
    }

    /// <summary>
    /// Sets the session status to logging in.
    /// </summary>
    public static void LoggingIn(this IUserClientState state)
    {
        state.SessionStatus = UserClientSessionStatus.LoggingIn;
    }
}
