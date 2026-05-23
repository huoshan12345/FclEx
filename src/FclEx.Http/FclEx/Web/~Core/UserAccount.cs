namespace FclEx.Web;

public record UserAccount(string UserName, string Password) : IUserAccount
{
    public override string ToString() => UserName;

    public static readonly UserAccount Empty = new("", "");
}