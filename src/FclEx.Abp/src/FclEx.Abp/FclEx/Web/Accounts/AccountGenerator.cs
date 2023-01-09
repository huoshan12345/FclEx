using System;

namespace FclEx.Web.Accounts
{
    public class AccountGenerator : IAccountGenerator
    {
        private const string SmallLetters = "abcdefghijklmnopqrstuvwxyz";
        private const string BigLetters = "ABCDEFGHIJKLMNOPQRSTUVWXYZ";
        private const string Digits = "0123456789";
        private const string OtherChars = "`~!@#$%^&*()-_+={}[]";
        private const string Letters = SmallLetters + BigLetters;
        private const string UserNameChars = Letters + Digits;
        private const string PasswordChars = Letters + Digits + OtherChars;

        private readonly AccountGeneratorOption _option;

        public AccountGenerator(AccountGeneratorOption option)
        {
            _option = option;
        }

        public AccountGenerator() : this(new AccountGeneratorOption()) { }

        public string GenerateUserName()
        {
            var stringChars = new char[_option.UserNameOption.RequiredLength];
            var random = new Random();
            stringChars[0] = Letters[random.Next(Letters.Length)];
            for (var i = 1; i < stringChars.Length; i++)
            {
                stringChars[i] = UserNameChars[random.Next(0, UserNameChars.Length - 1)];
            }
            return new string(stringChars);
        }

        public string GeneratePassword()
        {
            var stringChars = new char[_option.PasswordOption.RequiredLength];
            var random = new Random();
            for (var i = 0; i < stringChars.Length; i++)
            {
                stringChars[i] = PasswordChars[random.Next(0, PasswordChars.Length - 1)];
            }
            return new string(stringChars);
        }
    }
}
