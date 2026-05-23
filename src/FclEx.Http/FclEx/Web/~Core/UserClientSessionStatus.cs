namespace FclEx.Web;

public enum UserClientSessionStatus
{
    Offline,
    Online,
    LoggingIn,
    LoggingOut,
    AwaitingCaptcha,
    AwaitingChallenge,
}