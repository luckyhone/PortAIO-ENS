using System;
using System.Drawing;
using System.Drawing.Drawing2D;
using System.Drawing.Imaging;
using System.IO;
using EnsoulSharp.SDK.Core;
using EnsoulSharp.SDK.Utility;

namespace Entropy.Awareness.Helpers
{
    public static class BitmapHelper
    {
        private static readonly string ImagePath = Config.ImageFolder.FullName;
        private static readonly string ChampionImagePath = ImagePath + "\\Champions";
        
        public static string GetResourcePath(string championName)
            => Path.Combine(ChampionImagePath, championName + ".png");

        public static Bitmap ResizeImage(Bitmap imgToResize, Size size)
        {
            try
            {
                var b = new Bitmap(size.Width, size.Height);

                using (var g = Graphics.FromImage(b))
                {
                    g.InterpolationMode = InterpolationMode.HighQualityBicubic;
                    g.DrawImage(imgToResize, 0, 0, size.Width, size.Height);
                }

                return b;
            }
            catch (Exception e)
            {
                Console.WriteLine(@"Bitmap could not be resized " + e);
                return imgToResize;
            }
        }

        /* https://stackoverflow.com/questions/2265910/convert-an-image-to-grayscale */
        public static Bitmap MakeGrayscale(Bitmap original)
        {
            //create a blank bitmap the same size as original
            var newBitmap = new Bitmap(original.Width, original.Height);

            //get a graphics object from the new image
            var g = Graphics.FromImage(newBitmap);

            //create the grayscale ColorMatrix
            var colorMatrix = new ColorMatrix(
                new[]
                {
                    new[]
                    {
                        .3f,
                        .3f,
                        .3f,
                        0f,
                        0f
                    },
                    new[]
                    {
                        .59f,
                        .59f,
                        .59f,
                        0f,
                        0f
                    },
                    new[]
                    {
                        .11f,
                        .11f,
                        .11f,
                        0f,
                        0f
                    },
                    new[]
                    {
                        0f,
                        0f,
                        0f,
                        1f,
                        0f
                    },
                    new[]
                    {
                        0f,
                        0f,
                        0f,
                        0f,
                        1f
                    }
                });

            //create some image attributes
            var attributes = new ImageAttributes();

            //set the color matrix attribute
            attributes.SetColorMatrix(colorMatrix);

            //draw the original image on the new image
            //using the grayscale color matrix
            g.DrawImage(original,
                        new Rectangle(0, 0, original.Width, original.Height),
                        0,
                        0,
                        original.Width,
                        original.Height,
                        GraphicsUnit.Pixel,
                        attributes);

            //dispose the Graphics object
            g.Dispose();

            return newBitmap;
        }

        /* Converted to C# from VB https://stackoverflow.com/questions/5734710/c-sharp-crop-circle-in-a-image-or-bitmap */
        public static Bitmap CropImageToCircle(Bitmap bmp, int circleUpperLeftX, int circleUpperLeftY, int circleDiameter)
        {
            //'//Create a rectangle that crops a square of our image
            var cropRect = new Rectangle(circleUpperLeftX, circleUpperLeftY, circleDiameter, circleDiameter);

            //'//Crop the image to that square
            var croppedImage = bmp.Clone(cropRect, bmp.PixelFormat);

            //'//Create a texturebrush to draw our circle with
            var tb = new TextureBrush(croppedImage);

            //'//Create our output image
            var finalImage = new Bitmap(circleDiameter, circleDiameter);

            //'//Create a graphics object to draw with
            var g = Graphics.FromImage(finalImage);

            //'//Draw our cropped image onto the output image as an ellipse with the same width/height (circle)
            g.FillEllipse(tb, 0, 0, circleDiameter, circleDiameter);

            return finalImage;
        }

        public static Bitmap AddCircleOutline(Bitmap source, Color color, int width)
        {
            using (var g = Graphics.FromImage(source))
            {
                var p = new Pen(color, width) {Alignment = PenAlignment.Outset};

                g.DrawEllipse(p, 0, 0, source.Width, source.Width);

                return source;
            }
        }

        public static Bitmap ClipToCircle(Bitmap original, PointF center, float radius)
        {
            var copy = new Bitmap(original);

            using (var g = Graphics.FromImage(copy))
            {
                var r    = new RectangleF(center.X - radius, center.Y - radius, radius * 2, radius * 2);
                var path = new GraphicsPath();

                path.AddEllipse(r);
                g.Clip = new Region(path);
                g.DrawImage(original, 0, 0);

                return copy;
            }
        }
    }
}