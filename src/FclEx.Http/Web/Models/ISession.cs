namespace FclEx.Web.Models
{
    public interface ISession
    {
        SessionState State { get; set; }
        string? LoginCaptcha { get; set; }
        byte[]? LoginCaptchaBytes { get; set; }
    }
}
