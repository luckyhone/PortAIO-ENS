using System.Drawing;
using EnsoulSharp;
using EnsoulSharp.SDK.MenuUI;
using hikiMarksmanRework.Core.Menus;
using hikiMarksmanRework.Core.Spells;
using LeagueSharpCommon;

namespace hikiMarksmanRework.Core.Drawings
{
    class SivirDrawing
    {
        public static void Init()
        {
            if (ObjectManager.Player.IsDead)
            {
                return;
            }
            if (SivirMenu.Config["Draw Settings"]["Skill Draws"]["sivir.q.draw"].GetValue<MenuBool>().Enabled && SivirSpells.Q.IsReady())
            {
                Render.Circle.DrawCircle(ObjectManager.Player.Position, SivirSpells.Q.Range, Color.Gold);
            }
            if (SivirMenu.Config["Draw Settings"]["Skill Draws"]["sivir.w.draw"].GetValue<MenuBool>().Enabled && SivirSpells.W.IsReady())
            {
                Render.Circle.DrawCircle(ObjectManager.Player.Position, SivirSpells.W.Range, Color.Gold);
            }
            if (SivirMenu.Config["Draw Settings"]["Skill Draws"]["sivir.e.draw"].GetValue<MenuBool>().Enabled && SivirSpells.E.IsReady())
            {
                Render.Circle.DrawCircle(ObjectManager.Player.Position, SivirSpells.E.Range, Color.Gold);
            }
            if (SivirMenu.Config["Draw Settings"]["Skill Draws"]["sivir.r.draw"].GetValue<MenuBool>().Enabled && SivirSpells.R.IsReady())
            {
                Render.Circle.DrawCircle(ObjectManager.Player.Position, SivirSpells.R.Range, Color.Gold);
            }
        }
    }
}