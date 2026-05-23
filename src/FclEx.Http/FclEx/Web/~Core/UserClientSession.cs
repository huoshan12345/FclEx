namespace FclEx.Web;

public class UserClientSession : IUserClientSession
{
    public virtual string UserId { get; set; } = "";
    public virtual string UserName { get; set; } = "";
}