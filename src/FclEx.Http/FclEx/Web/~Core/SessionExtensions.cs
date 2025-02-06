namespace FclEx.Web;

public static class SessionExtensions
{
    public static bool IsCaptchaRequired(this IClientSession session)
    {
        return session.SessionState == ClientSessionState.CaptchaRequired;
    }

    public static bool IsLoggingIn(this IClientSession session)
    {
        return session.SessionState == ClientSessionState.LoggingIn;
    }

    public static void Offline(this IClientSession session)
    {
        session.SessionState = ClientSessionState.Offline;
    }

    public static void Online(this IClientSession session)
    {
        session.SessionState = ClientSessionState.Online;
    }

    public static void LoggingIn(this IClientSession session)
    {
        session.SessionState = ClientSessionState.LoggingIn;
    }
}