namespace FclEx.Web;

/// <summary>
/// Session identity exposed by a user client after login or session restoration.
/// </summary>
public interface IUserClientSession
{
    /// <summary>
    /// User display or login name associated with the current session.
    /// </summary>
    string UserName { get; }
}
