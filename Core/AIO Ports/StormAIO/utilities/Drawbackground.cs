using System.Drawing;
using EnsoulSharp;

namespace StormAIO.utilities
{
    public class Drawbackground
    {
        public Drawbackground()
        {
            Drawing.OnDraw += delegate
            {
                Drawing.DrawLine(2000, 120, 1650, 120, 120, Color.FromArgb(45, Color.Black));
                Drawing.DrawLine(1913, 120, 1661, 120, (float) (120 * 0.85), Color.FromArgb(120, Color.Black));
            };
        }
    }
}