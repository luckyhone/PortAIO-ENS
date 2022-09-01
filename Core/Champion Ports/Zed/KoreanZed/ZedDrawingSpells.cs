using System.Drawing;
using EnsoulSharp;
using EnsoulSharp.SDK;
using EnsoulSharp.SDK.Rendering;

namespace KoreanZed
{
    class ZedDrawingSpells
    {
        private readonly ZedMenu zedMenu;

        private readonly ZedSpells zedSpells;

        public ZedDrawingSpells(ZedMenu zedMenu, ZedSpells zedSpells)
        {
            this.zedMenu = zedMenu;
            this.zedSpells = zedSpells;

            Drawing.OnDraw += Drawing_OnDraw;
        }

        private void Drawing_OnDraw(System.EventArgs args)
        {
            if (!zedMenu.GetParamBool("koreanzed.drawing.skillranges"))
            {
                return;
            }

            float range = 0F;

            if (zedSpells.R.UseOnCombo && zedSpells.R.IsReady() && zedSpells.R.Instance.ToggleState == 0)
            {
                range = zedSpells.R.Range;
            }
            else if (zedSpells.Q.UseOnCombo && zedSpells.Q.IsReady())
            {
                if (zedSpells.W.UseOnCombo && zedSpells.W.Instance.ToggleState == 0 && zedSpells.W.IsReady())
                {
                    range = zedSpells.E.Range + zedSpells.W.Range;
                }
                else
                {
                    range = zedSpells.Q.Range;
                }
            }
            else if (zedSpells.E.UseOnCombo && zedSpells.E.IsReady())
            {
                range = zedSpells.E.Range + (zedSpells.W.UseOnCombo ? zedSpells.W.IsReady() ? zedSpells.W.Range : 0F : 0F);
            }

            if (range > 0F)
            {
                CircleRender.Draw(ObjectManager.Player.Position, range, Color.FromArgb(150, Color.DarkGreen).ToSharpDxColor(), 5);
            }
        }
    }
}