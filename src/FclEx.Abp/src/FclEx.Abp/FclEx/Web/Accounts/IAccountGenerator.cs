namespace FclEx.Web.Accounts
{
    public interface IAccountGenerator
    {
        string GenerateUsername();

        string GeneratePassword();
    }
}
