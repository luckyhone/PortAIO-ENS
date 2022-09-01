using System;
using System.Drawing;
using EnsoulSharp;
using EnsoulSharp.SDK.MenuUI;
using LeagueSharpCommon;

namespace ElVarus
{
    internal class Drawings
    {
        #region Public Methods and Operators

        public static void Drawing_OnDraw(EventArgs args)
        {
            var drawOff = ElVarusMenu.Menu["ElVarus.Draw.off"].GetValue<MenuBool>().Enabled;
            var drawQ = ElVarusMenu.Menu["ElVarus.Draw.Q"].GetValue<MenuBool>().Enabled;
            var drawW = ElVarusMenu.Menu["ElVarus.Draw.W"].GetValue<MenuBool>().Enabled;
            var drawE = ElVarusMenu.Menu["ElVarus.Draw.E"].GetValue<MenuBool>().Enabled;
            var drawR = ElVarusMenu.Menu["ElVarus.Draw.E"].GetValue<MenuBool>().Enabled;

            if (drawOff)
            {
                return;
            }

            if (drawQ)
            {
                if (Varus.spells[Spells.Q].Level > 0)
                {
                    Render.Circle.DrawCircle(
                        ObjectManager.Player.Position,
                        Varus.spells[Spells.Q].Range,
                        Varus.spells[Spells.Q].IsReady() ? Color.Green : Color.Red);
                }
            }

            if (drawW)
            {
                if (Varus.spells[Spells.W].Level > 0)
                {
                    Render.Circle.DrawCircle(
                        ObjectManager.Player.Position,
                        Varus.spells[Spells.W].Range,
                        Varus.spells[Spells.W].IsReady() ? Color.Green : Color.Red);
                }
            }

            if (drawE)
            {
                if (Varus.spells[Spells.E].Level > 0)
                {
                    Render.Circle.DrawCircle(
                        ObjectManager.Player.Position,
                        Varus.spells[Spells.E].Range,
                        Varus.spells[Spells.E].IsReady() ? Color.Green : Color.Red);
                }
            }

            if (drawR)
            {
                if (Varus.spells[Spells.R].Level > 0)
                {
                    Render.Circle.DrawCircle(
                        ObjectManager.Player.Position,
                        Varus.spells[Spells.R].Range,
                        Varus.spells[Spells.R].IsReady() ? Color.Green : Color.Red);
                }
            }
        }

        #endregion
    }
}