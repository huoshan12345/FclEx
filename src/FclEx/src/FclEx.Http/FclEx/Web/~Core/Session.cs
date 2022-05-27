using System;

namespace FclEx.Web
{
    public interface ISession
    {
        SessionState State { get; set; }
        string? LoginCaptcha { get; set; }
        byte[]? LoginCaptchaBytes { get; set; }
        event Action<SessionState> OnSessionStateChanged;
    }

    public class Session : ISession
    {
        private SessionState _state;

        public SessionState State
        {
            get => _state;
            set
            {
                if (_state != value)
                    OnSessionStateChanged.Invoke(value);
                _state = value;
            }
        }
        
        public string? LoginCaptcha { get; set; }
        public byte[]? LoginCaptchaBytes { get; set; }
        public event Action<SessionState> OnSessionStateChanged = m => { };
    }
}
