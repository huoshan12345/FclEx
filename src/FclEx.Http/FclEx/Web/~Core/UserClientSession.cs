namespace FclEx.Web;

public class UserClientSession : IUserClientSession
{
    private UserClientSessionState _state;

    public UserClientSessionState SessionState
    {
        get => _state;
        set
        {
            if (_state != value)
                OnSessionStateChanged.Invoke(value);
            _state = value;
        }
    }
    public event Action<UserClientSessionState> OnSessionStateChanged = m => { };
}