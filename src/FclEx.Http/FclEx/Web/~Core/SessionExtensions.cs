namespace FclEx.Web;

public static class SessionExtensions
{
    public static bool IsCaptchaRequired(this IUserClientSession session)
    {
        return session.SessionState == UserClientSessionState.CaptchaRequired;
    }

    public static bool IsLoggingIn(this IUserClientSession session)
    {
        return session.SessionState == UserClientSessionState.LoggingIn;
    }

    public static void Offline(this IUserClientSession session)
    {
        session.SessionState = UserClientSessionState.Offline;
    }

    public static void Online(this IUserClientSession session)
    {
        session.SessionState = UserClientSessionState.Online;
    }

    public static void LoggingIn(this IUserClientSession session)
    {
        session.SessionState = UserClientSessionState.LoggingIn;
    }
}