namespace FclEx.Web;

/// <summary>
/// Mutable session and account state for a user client.
/// </summary>
public interface IUserClientState
{
    /// <summary>
    /// Current session lifecycle status.
    /// </summary>
    UserClientSessionStatus SessionStatus { get; set; }

    /// <summary>
    /// Current account validity status.
    /// </summary>
    UserAccountStatus AccountStatus { get; set; }

    /// <summary>
    /// Raised after <see cref="SessionStatus"/> changes.
    /// </summary>
    event ValueChangedHandler<UserClientSessionStatus> SessionStatusChanged;

    /// <summary>
    /// Raised after <see cref="AccountStatus"/> changes.
    /// </summary>
    event ValueChangedHandler<UserAccountStatus> AccountStatusChanged;
}
