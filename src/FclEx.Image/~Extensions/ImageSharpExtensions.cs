//using System.IO;
//using SixLabors.ImageSharp;
//using SixLabors.ImageSharp.Formats;
//using SixLabors.ImageSharp.Formats.Png;
//using SixLabors.ImageSharp.PixelFormats;

namespace FclEx.Image
{
    //public static class ImageSharpExtensions
    //{
    //    private static readonly IImageEncoder _encoder = new PngEncoder();

    //    public static Image<Rgba32> Base64StringToImage(this string base64String) => base64String.Base64StringToBytes().ToImage();

    //    public static Image<Rgba32> ToImage(this byte[] bytes) => SixLabors.ImageSharp.Image.Load(bytes);

    //    public static byte[] ToBytes(this Image<Rgba32> image, IImageEncoder encoder = null)
    //    {
    //        using (var m = new MemoryStream())
    //        {
    //            image.Save(m, _encoder);
    //            return m.ToArray();
    //        }
    //    }

    //    public static string ToRawBase64String(this Image<Rgba32> image, IImageEncoder encoder = null) => image.ToBytes(encoder).ToBase64String();
    //}
}
