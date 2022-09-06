using System;
using System.Drawing;
using EnsoulSharp;
using EnsoulSharp.SDK.MenuUI;
using LeagueSharpCommon;

namespace ElSinged
{
    internal class Drawings
    {
        public static void Drawing_OnDraw(EventArgs args)
        {
            var drawOff = ElSingedMenu.Menu["Drawings"]["ElSinged.Draw.off"].GetValue<MenuBool>().Enabled;
            var drawQ = ElSingedMenu.Menu["Drawings"]["ElSinged.Draw.Q"].GetValue<MenuBool>().Enabled;
            var drawW = ElSingedMenu.Menu["Drawings"]["ElSinged.Draw.W"].GetValue<MenuBool>().Enabled;
            var drawE = ElSingedMenu.Menu["Drawings"]["ElSinged.Draw.E"].GetValue<MenuBool>().Enabled;

            if (drawOff)
                return;

            if (drawQ)
                if (Singed.spells[Spells.Q].Level > 0)
                    Render.Circle.DrawCircle(ObjectManager.Player.Position, Singed.spells[Spells.Q].Range, Singed.spells[Spells.Q].IsReady() ? Color.Green : Color.Red);

            if (drawW)
                if (Singed.spells[Spells.W].Level > 0)
                    Render.Circle.DrawCircle(ObjectManager.Player.Position, Singed.spells[Spells.W].Range, Singed.spells[Spells.W].IsReady() ? Color.Green : Color.Red);

            if (drawE)
                if (Singed.spells[Spells.E].Level > 0)
                    Render.Circle.DrawCircle(ObjectManager.Player.Position, Singed.spells[Spells.E].Range, Singed.spells[Spells.E].IsReady() ? Color.Green : Color.Red);
        }
    }
}