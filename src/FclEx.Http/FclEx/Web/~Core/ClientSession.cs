namespace FclEx.Web;

public interface IClientSession
{
    ClientSessionState SessionState { get; set; }
    event Action<ClientSessionState> OnSessionStateChanged;
}

public class ClientSession : IClientSession
{
    private ClientSessionState _state;

    public ClientSessionState SessionState
    {
        get => _state;
        set
        {
            if (_state != value)
                OnSessionStateChanged.Invoke(value);
            _state = value;
        }
    }
    public event Action<ClientSessionState> OnSessionStateChanged = m => { };
}