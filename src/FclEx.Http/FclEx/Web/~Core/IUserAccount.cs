namespace FclEx.Web;

/// <summary>
/// Represents credentials used by a user client.
/// </summary>
public interface IUserAccount
{
    /// <summary>
    /// The account user name.
    /// </summary>
    string UserName { get; }

    /// <summary>
    /// The account password.
    /// </summary>
    string Password { get; }
}
