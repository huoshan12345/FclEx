namespace FclEx.Web.Core
{
    public interface IUserAccount
    {
        string UserName { get; set; }
        string Password { get; set; }
    }

    public class UserAccount : IUserAccount
    {
        public UserAccount(string? username = null, string? password = null)
        {
            UserName = username ?? string.Empty;
            Password = password ?? string.Empty;
        }

        public string UserName { get; set; }
        public string Password { get; set; }

        public override string ToString()
        {
            return UserName;
        }
    }
}
