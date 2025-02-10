namespace FclEx.Web;

public enum ClientSessionState
{
    Offline,
    LoggingIn,
    CaptchaRequired,
    ChallengeRequired,
    Online,
}