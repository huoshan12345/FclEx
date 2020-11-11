namespace FclEx.Web.Models
{
    public class Session : ISession
    {
        public SessionState State { get; set; }
        public string? LoginCaptcha { get; set; }
        public byte[]? LoginCaptchaBytes { get; set; }
    }
}
