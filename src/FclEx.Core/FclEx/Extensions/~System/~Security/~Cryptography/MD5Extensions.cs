namespace FclEx.Extensions;

public static class MD5Extensions
{
    extension(MD5)
    {
#if !NET5_0_OR_GREATER
        public static byte[] HashData(byte[] source)
        {
            using var md5 = MD5.Create();
            return md5.ComputeHash(source);
        }
#endif
    }
}
