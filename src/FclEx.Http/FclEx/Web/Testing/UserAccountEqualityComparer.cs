namespace FclEx.Web.Testing;

public class UserAccountEqualityComparer : IEqualityComparer<UserAccount>
{
    public static UserAccountEqualityComparer Instance { get; } = new();

    public bool Equals(UserAccount? x, UserAccount? y)
    {
        if (ReferenceEquals(x, y)) return true;
        if (x is null) return false;
        if (y is null) return false;
        if (x.GetType() != y.GetType()) return false;
        return x.UserName == y.UserName
               && x.Password == y.Password;
    }

    public int GetHashCode(UserAccount obj)
    {
        unchecked
        {
            return (obj.UserName.GetHashCodeSafely() * 397) 
                   ^ obj.Password.GetHashCodeSafely();
        }
    }
}