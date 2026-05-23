namespace FclEx.Helpers;

public static class HashHelper
{
    public static bool IsMd5String(string input)
    {
        return Regexes.Md5.IsMatch(input);
    }
}
