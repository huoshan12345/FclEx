namespace FclEx.Web;

/// <summary>
/// Extensions for user-client state checks.
/// </summary>
public static class UserClientExtensions
{
    extension<TAccount>(IUserClient<TAccount> client) where TAccount : IUserAccount
    {
        /// <summary>
        /// Indicates whether the client session status is online.
        /// </summary>
        public bool IsOnline => client.State.SessionStatus == UserClientSessionStatus.Online;
    }
}
