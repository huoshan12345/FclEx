namespace FclEx.Web.Accounts
{
    public interface IAccountGenerator
    {
        string GenerateUserName();

        string GeneratePassword();
    }
}
