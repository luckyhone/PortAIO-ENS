using EnsoulSharp;

namespace ElUtilitySuite.Vendor.SFX
{
    using System;
    using System.Drawing;
    using System.Drawing.Drawing2D;
    using System.Drawing.Imaging;


    using SharpDX.Direct3D9;

    public static class BitmapExtensions
    {
        /// <exception cref="Exception">The operation failed.</exception>
        public static Bitmap Grayscale(this Bitmap source)
        {
            var newBitmap = new Bitmap(source.Width, source.Height);
            using (var g = Graphics.FromImage(newBitmap))
            {
                var colorMatrix =
                    new ColorMatrix(
                        new[]
                        {
                            new[] { .3f, .3f, .3f, 0, 0 }, new[] { .59f, .59f, .59f, 0, 0 },
                            new[] { .11f, .11f, .11f, 0, 0 }, new float[] { 0, 0, 0, 1, 0 },
                            new float[] { 0, 0, 0, 0, 1 }
                        });
                using (var attributes = new ImageAttributes())
                {
                    attributes.SetColorMatrix(colorMatrix);
                    g.DrawImage(
                        source, new Rectangle(0, 0, source.Width, source.Height), 0, 0, source.Width, source.Height,
                        GraphicsUnit.Pixel, attributes);
                }
            }
            return newBitmap;
        }

        public static Texture ToTexture(this Bitmap bitmap)
        {
            return Texture.FromMemory(
                Drawing.Direct3DDevice9, (byte[])new ImageConverter().ConvertTo(bitmap, typeof(byte[])), bitmap.Width,
                bitmap.Height, 0, Usage.None, Format.A1, Pool.Managed, Filter.Default, Filter.Default, 0);
        }


        /// <exception cref="Exception">The operation failed.</exception>
        public static Bitmap Resize(this Bitmap source, int width, int height)
        {
            var destRect = new Rectangle(0, 0, width, height);
            var destImage = new Bitmap(width, height);
            destImage.SetResolution(source.HorizontalResolution, source.VerticalResolution);
            using (var graphics = Graphics.FromImage(destImage))
            {
                graphics.CompositingMode = CompositingMode.SourceCopy;
                graphics.CompositingQuality = CompositingQuality.HighQuality;
                graphics.InterpolationMode = InterpolationMode.HighQualityBicubic;
                graphics.SmoothingMode = SmoothingMode.HighQuality;
                graphics.PixelOffsetMode = PixelOffsetMode.HighQuality;

                using (var wrapMode = new ImageAttributes())
                {
                    wrapMode.SetWrapMode(WrapMode.TileFlipXY);
                    graphics.DrawImage(
                        source, destRect, 0, 0, source.Width, source.Height, GraphicsUnit.Pixel, wrapMode);
                }
            }
            return destImage;
        }
    }
}