using System;

namespace FclEx.Web.Core
{
    public interface IUserAccount
    {
        string UserName { get; set; }
        string Password { get; set; }
    }

    public class UserAccount : IUserAccount, IEquatable<UserAccount>
    {
        public bool Equals(UserAccount? other)
        {
            return other != null 
                   && UserName == other.UserName 
                   && Password == other.Password;
        }

        public override bool Equals(object? obj)
        {
            if (ReferenceEquals(null, obj)) return false;
            if (ReferenceEquals(this, obj)) return true;
            if (obj.GetType() != this.GetType()) return false;
            return Equals((UserAccount)obj);
        }

        public override int GetHashCode()
        {
            return HashCode.Combine(UserName, Password);
        }

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
