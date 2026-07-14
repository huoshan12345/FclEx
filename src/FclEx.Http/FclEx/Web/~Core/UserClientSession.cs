namespace FclEx.Web;

/// <summary>
/// Default mutable user session data.
/// </summary>
public class UserClientSession : IUserClientSession
{
    /// <summary>
    /// The authenticated username, or an empty string when unknown.
    /// </summary>
    public virtual string UserName { get; set; } = "";
}
