namespace FclEx.Web;

public interface IUserClientState
{
    UserClientSessionStatus SessionStatus { get; set; }
    UserAccountStatus AccountStatus { get; set; }
    event ValueChangedHandler<UserClientSessionStatus> SessionStatusChanged;
    event ValueChangedHandler<UserAccountStatus> AccountStatusChanged;
}