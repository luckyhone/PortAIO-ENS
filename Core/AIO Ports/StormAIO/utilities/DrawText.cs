using EnsoulSharp;
using EnsoulSharp.SDK.MenuUI;
using SharpDX;
using SharpDX.Direct3D9;
using Color = System.Drawing.Color;
using Font = SharpDX.Direct3D9.Font;

namespace StormAIO.utilities
{
    public class DrawText
    {
        private static Font Font;

        public DrawText(string text, string keytype, MenuKeyBind Toggle, Color coloron, Color coloroff,
            int Textpos = 77, int keypos = 84)
        {
            Drawing.OnDraw += delegate
            {
                Drawing.DrawLine(1819, keypos, 1907, keypos, 22,
                    Color.FromArgb(230, Toggle.Active ? coloron : coloroff));
                DrawTextWithFont(Font, text, 1672,
                    Textpos, SharpDX.Color.White);
                DrawTextWithFont(Font, keytype, 1855,
                    Textpos, !Toggle.Active ? SharpDX.Color.White : SharpDX.Color.Black);
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