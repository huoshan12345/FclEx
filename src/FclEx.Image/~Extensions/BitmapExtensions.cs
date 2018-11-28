using System.Drawing;
using System.Drawing.Imaging;
using System.IO;

namespace FclEx.Image
{
    public static class BitmapExtensions
    {
        public static Bitmap ToBitmap(this byte[] bytes)
        {
            using (var mem = new MemoryStream(bytes))
            {
                return new Bitmap(mem);
            }
        }

        public static byte[] ToBytes(this Bitmap bitmap)
        {
            using (var mem = new MemoryStream())
            {
                bitmap.Save(mem, ImageFormat.Png);
                return mem.ToArray();
            }
        }

        public static Bitmap Cut(this Bitmap bitmap, Rectangle rectangle)
        {
            // An empty bitmap which will hold the cropped image
            var bmp = new Bitmap(rectangle.Width, rectangle.Height);
            using (var g = Graphics.FromImage(bmp))
            {
                // Draw the given area (section) of the source image
                // at location 0,0 on the empty bitmap (bmp)
                g.DrawImage(bitmap, 0, 0, rectangle, GraphicsUnit.Pixel);
                return bmp;
            }
        }

        public static Bitmap Draw(this Bitmap bitmap, Rectangle rectangle, Brush color)
        {
            using (var g = Graphics.FromImage(bitmap))
            {
                g.FillRectangle(color, rectangle);
                return bitmap;
            }
        }
    }
}
