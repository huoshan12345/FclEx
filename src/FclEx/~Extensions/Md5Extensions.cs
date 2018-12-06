using System.Security.Cryptography;
using System.Text.RegularExpressions;

namespace FclEx
{
    public static class Md5Extensions
    {
        private static readonly Regex _regMd5 = new Regex(@"^([a-fA-F0-9]{32})$");

        public static string ToMd5String(this byte[] input, bool upperCase = false)
        {
            return input.ToMd5().ToHexString(upperCase);
        }

        public static byte[] ToMd5(this byte[] input)
        {
            return MD5.Create().ComputeHash(input);
        }
        
        public static bool IsMd5String(this string input)
        {
            return _regMd5.IsMatch(input);
        }
    }
}
