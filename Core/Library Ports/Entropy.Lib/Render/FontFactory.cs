using System.Collections.Generic;
using EnsoulSharp;
using SharpDX.Direct3D9;

namespace Entropy.Lib.Render
{
    public static class FontFactory
    {
        private static readonly HashSet<Font> CreatedFonts = new HashSet<Font>();

        static FontFactory()
        {
            Drawing.OnPreReset += args =>
            {
                foreach (var createdFont in CreatedFonts)
                {
                    createdFont.OnLostDevice();
                }
            };
            Drawing.OnPostReset += args =>
            {
                foreach (var createdFont in CreatedFonts)
                {
                    createdFont.OnResetDevice();
                }
            };
        }

        private static Font CreateNewFont(FontDescription description)
        {
            var font = new Font(Drawing.Direct3DDevice9, description);

            CreatedFonts.Add(font);

            return font;
        }

        public static Font CreateNewFont(int height, FontWeight weight, string faceName = "SegoeUI")
        {
            var desc = new FontDescription
            {
                Quality         = FontQuality.ClearType,
                Height          = height,
                FaceName        = faceName,
                Weight          = weight,
                OutputPrecision = FontPrecision.TrueType,
                PitchAndFamily  = FontPitchAndFamily.Default | FontPitchAndFamily.DontCare
            };

            return CreateNewFont(desc);
        }

        public static Font CreateNewFont(int height, string faceName = "SegoeUI", FontWeight weight = FontWeight.Normal)
        {
            var desc = new FontDescription
            {
                Quality         = FontQuality.ClearType,
                Height          = height,
                FaceName        = faceName,
                Weight          = weight,
                OutputPrecision = FontPrecision.TrueType,
                PitchAndFamily  = FontPitchAndFamily.Default | FontPitchAndFamily.DontCare
            };

            return CreateNewFont(desc);
        }
    }
}