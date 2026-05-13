namespace FclEx.Web;

public class UserClientState : IUserClientState
{
    public virtual UserClientSessionStatus SessionStatus
    {
        get;
        set
        {
            if (field == value)
                return;

            var oldValue = field;
            field = value;
            SessionStatusChanged(oldValue, value);
        }
    }

    public virtual UserAccountStatus AccountStatus
    {
        get;
        set
        {
            if (field == value)
                return;

            var oldValue = field;
            field = value;
            AccountStatusChanged(oldValue, value);
        }
    }

    public event ValueChangedHandler<UserClientSessionStatus> SessionStatusChanged = (oldValue, newValue) => { };
    public event ValueChangedHandler<UserAccountStatus> AccountStatusChanged = (oldValue, newValue) => { };
}