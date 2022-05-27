namespace FclEx.Web.Accounts
{
    public class UsernameOption
    {
        public UsernameOption(int requiredLength)
        {
            RequiredLength = requiredLength;
        }

        public int RequiredLength { get; }
    }

    public class PasswordOption
    {
        public int RequiredLength { get; }
        public bool RequireNonLetterOrDigit { get; }
        public bool RequireDigit { get; }
        public bool RequireLowercase { get; }
        public bool RequireUppercase { get; }

        public PasswordOption(int requiredLength, bool requireNonLetterOrDigit, bool requireDigit, bool requireLowercase, bool requireUppercase)
        {
            RequiredLength = requiredLength;
            RequireNonLetterOrDigit = requireNonLetterOrDigit;
            RequireDigit = requireDigit;
            RequireLowercase = requireLowercase;
            RequireUppercase = requireUppercase;
        }

        public PasswordOption(int requiredLength) :  this(requiredLength, false, true, true, false)
        {
        }
    }

    public class AccountGeneratorOption
    {
        public AccountGeneratorOption(UsernameOption usernameOption, PasswordOption passwordOption)
        {
            UsernameOption = usernameOption;
            PasswordOption = passwordOption;
        }

        public AccountGeneratorOption() : this(new UsernameOption(10), new PasswordOption(10, false, true, true, false)) { }

        public UsernameOption UsernameOption { get; }

        public PasswordOption PasswordOption { get; }

    }
}
