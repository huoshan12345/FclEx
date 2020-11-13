namespace FclEx.Web.Core
{
    public interface ISession
    {
        SessionState State { get; set; }
        string? LoginCaptcha { get; set; }
        byte[]? LoginCaptchaBytes { get; set; }
    }

    public class Session : ISession
    {
        public SessionState State { get; set; }
        public string? LoginCaptcha { get; set; }
        public byte[]? LoginCaptchaBytes { get; set; }
    }
}
