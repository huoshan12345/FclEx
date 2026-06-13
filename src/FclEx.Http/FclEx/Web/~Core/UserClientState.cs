namespace FclEx.Web;

/// <summary>
/// Default mutable implementation of <see cref="IUserClientState"/>.
/// Change events are raised only when a status value actually changes.
/// </summary>
public class UserClientState : IUserClientState
{
    /// <inheritdoc />
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

    /// <inheritdoc />
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

    /// <inheritdoc />
    public event ValueChangedHandler<UserClientSessionStatus> SessionStatusChanged = (oldValue, newValue) => { };

    /// <inheritdoc />
    public event ValueChangedHandler<UserAccountStatus> AccountStatusChanged = (oldValue, newValue) => { };
}
