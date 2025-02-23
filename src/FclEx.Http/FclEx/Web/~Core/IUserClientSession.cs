namespace FclEx.Web;

public interface IUserClientSession
{
    UserClientSessionState SessionState { get; set; }
    event Action<UserClientSessionState> OnSessionStateChanged;
}