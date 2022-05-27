using System.Security.Cryptography;

namespace FclEx.Extensions
{
    public static class CryptoTransformExtensions
    {
        public static byte[] Transform(this ICryptoTransform xfrm, byte[] plain) => xfrm.TransformFinalBlock(plain, 0, plain.Length);

        public static string TransformToBase64(this ICryptoTransform xfrm, byte[] plain) => xfrm.Transform(plain).ToBase64();

        public static string TransformToBase64(this ICryptoTransform xfrm, string plain) => xfrm.TransformToBase64(plain.ToUtf8Bytes());
    }
}
