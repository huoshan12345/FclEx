namespace FclEx.Web;

/// <summary>
/// Default mutable user session data.
/// </summary>
public class UserClientSession : IUserClientSession
{
    /// <summary>
    /// The authenticated user identifier, or an empty string when unknown.
    /// </summary>
    public virtual string UserId { get; set; } = "";

    /// <summary>
    /// The authenticated display/user name, or an empty string when unknown.
    /// </summary>
    public virtual string UserName { get; set; } = "";
}
