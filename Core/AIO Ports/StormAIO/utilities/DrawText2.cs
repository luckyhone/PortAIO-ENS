using EnsoulSharp;
using EnsoulSharp.SDK.MenuUI;
using SharpDX;
using SharpDX.Direct3D9;

namespace StormAIO.utilities
{
    public class DrawText2
    {
        private static Font Font;

        public DrawText2(string text, MenuSliderButton Number, int Textpos = 77)
        {
            Drawing.OnDraw += delegate
            {
                DrawTextWithFont(Font, text, 1672,
                    Textpos, Color.White);
                DrawTextWithFont(Font,
                    Number.ActiveValue == -1 ? (Number.ActiveValue == 0).ToString() : Number.ActiveValue.ToString(),
                    1855,
                    Textpos, Color.White);
            };
            Font = new Font(
                Drawing.Direct3DDevice9,
                new FontDescription
                {
                    FaceName = "Comic Sans MS, Verdana",
                    Height = 16,
                    Weight = FontWeight.ExtraBold,
                    OutputPrecision = FontPrecision.Default,
                    Quality = FontQuality.ClearTypeNatural
                });
        }

        private static void DrawTextWithFont(Font aFont, string aText, int aPosX, int aPosY, ColorBGRA aColor)
        {
            aFont.DrawText(null, aText, aPosX, aPosY, aColor);
        }
    }
}