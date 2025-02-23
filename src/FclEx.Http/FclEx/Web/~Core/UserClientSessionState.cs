namespace FclEx.Web;

public enum UserClientSessionState
{
    Offline,
    LoggingIn,
    CaptchaRequired,
    ChallengeRequired,
    Online,
}