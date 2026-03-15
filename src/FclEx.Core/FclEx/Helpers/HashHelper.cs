namespace FclEx.Helpers;

public static class HashHelper
{
    public static byte[] Md5(ArraySegment<byte> input)
    {
        using var md5 = MD5.Create();
        return md5.Hash(input);
    }

    public static byte[] Md5(byte[] input)
    {
        using var md5 = MD5.Create();
        return md5.Hash(input);
    }

    public static byte[] Sha1(byte[] input)
    {
        using var md5 = SHA1.Create();
        return md5.Hash(input);
    }

    public static byte[] Sha256(byte[] input)
    {
        using var md5 = SHA256.Create();
        return md5.Hash(input);
    }

    public static byte[] Sha512(byte[] input)
    {
        using var md5 = SHA512.Create();
        return md5.Hash(input);
    }

    public static bool IsMd5String(string input)
    {
        return Regexes.Md5.IsMatch(input);
    }
}
