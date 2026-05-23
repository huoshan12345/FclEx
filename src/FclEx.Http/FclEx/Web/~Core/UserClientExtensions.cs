namespace FclEx.Web;

public static class UserClientExtensions
{
    extension<TAccount>(IUserClient<TAccount> client) where TAccount : IUserAccount
    {
        public bool IsOnline => client.State.SessionStatus == UserClientSessionStatus.Online;
    }
}
