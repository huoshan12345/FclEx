namespace FclEx.Web;

public static class UserClientStateExtensions
{
    public static bool IsAwaitingCaptcha(this IUserClientState state)
    {
        return state.SessionStatus == UserClientSessionStatus.AwaitingCaptcha;
    }

    public static bool IsAwaitingChallenge(this IUserClientState state)
    {
        return state.SessionStatus == UserClientSessionStatus.AwaitingChallenge;
    }

    public static bool IsLoggingIn(this IUserClientState state)
    {
        return state.SessionStatus == UserClientSessionStatus.LoggingIn;
    }

    public static bool IsAccountNormal(this IUserClientState state)
    {
        return state.AccountStatus == UserAccountStatus.Normal;
    }

    public static void Offline(this IUserClientState state)
    {
        state.SessionStatus = UserClientSessionStatus.Offline;
    }

    public static void Online(this IUserClientState state)
    {
        state.SessionStatus = UserClientSessionStatus.Online;
    }

    public static void LoggingIn(this IUserClientState state)
    {
        state.SessionStatus = UserClientSessionStatus.LoggingIn;
    }
}