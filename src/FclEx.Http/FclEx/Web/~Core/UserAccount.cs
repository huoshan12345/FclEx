namespace FclEx.Web;

/// <summary>
/// Basic user account model containing a user name and password.
/// </summary>
public record UserAccount(string UserName, string Password) : IUserAccount
{
    /// <summary>
    /// Returns the user name so logs and diagnostics do not include the password.
    /// </summary>
    public override string ToString() => UserName;

    /// <summary>
    /// Empty account instance used when a client needs a placeholder account.
    /// </summary>
    public static readonly UserAccount Empty = new("", "");
}
