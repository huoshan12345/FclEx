using System.Security.Cryptography;

namespace FclEx.Extensions;

public static class HashExtensions
{
    private static readonly Regex _regMd5 = new(@"^([a-fA-F0-9]{32})$", RegexOptions.Compiled);

    public static string ToMd5String(this byte[] input, bool upperCase = false)
    {
        return input.Md5().ToHex(upperCase);
    }

    public static byte[] Hash(this HashAlgorithm algorithm, byte[] input)
    {
        if (input.IsNullOrEmpty()) return Array.Empty<byte>();
        return algorithm.ComputeHash(input);
    }

    public static byte[] Md5(this byte[] input)
    {
        using var md5 = MD5.Create();
        return md5.Hash(input);
    }

    public static byte[] Sha1(this byte[] input)
    {
        using var md5 = SHA1.Create();
        return md5.Hash(input);
    }

    public static byte[] Sha256(this byte[] input)
    {
        using var md5 = SHA256.Create();
        return md5.Hash(input);
    }

    public static byte[] Sha512(this byte[] input)
    {
        using var md5 = SHA512.Create();
        return md5.Hash(input);
    }

    public static bool IsMd5String(this string input)
    {
        return _regMd5.IsMatch(input);
    }

    public static byte[] Hash(this HashAlgorithm algorithm, ArraySegment<byte> input)
    {
        return input.Array.IsNullOrEmpty()
            ? Array.Empty<byte>()
            : algorithm.ComputeHash(input.Array, input.Offset, input.Count);
    }

    public static byte[] Md5(this ArraySegment<byte> input)
    {
        using var md5 = MD5.Create();
        return md5.Hash(input);
    }

    public static string ToMd5String(this ArraySegment<byte> input, bool upperCase = false)
    {
        return input.Md5().ToHex(upperCase);
    }
}