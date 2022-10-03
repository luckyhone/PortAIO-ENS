using EnsoulSharp;
using EnsoulSharp.SDK.MenuUI;
using hikiMarksmanRework.Core.Menus;
using hikiMarksmanRework.Core.Spells;
using Color = System.Drawing.Color;
using Render = LeagueSharpCommon.Render;
namespace hikiMarksmanRework.Core.Drawings
{
    class LucianDrawing
    {
        public static void Init()
        {
            if (ObjectManager.Player.IsDead)
            {
                return;
            }
            if (LucianMenu.Config["Draw Settings"]["Skill Draws"]["lucian.q.draw"].GetValue<MenuBool>().Enabled && LucianSpells.Q.IsReady())
            {
                Render.Circle.DrawCircle(ObjectManager.Player.Position, LucianSpells.Q.Range, Color.Gold);
            }
            if (LucianMenu.Config["Draw Settings"]["Skill Draws"]["lucian.q2.draw"].GetValue<MenuBool>().Enabled && LucianSpells.Q2.IsReady())
            {
                Render.Circle.DrawCircle(ObjectManager.Player.Position, LucianSpells.Q2.Range, Color.Gold);
            }
            if (LucianMenu.Config["Draw Settings"]["Skill Draws"]["lucian.w.draw"].GetValue<MenuBool>().Enabled && LucianSpells.W.IsReady())
            {
                Render.Circle.DrawCircle(ObjectManager.Player.Position, LucianSpells.W.Range, Color.Gold);
            }
            if (LucianMenu.Config["Draw Settings"]["Skill Draws"]["lucian.e.draw"].GetValue<MenuBool>().Enabled && LucianSpells.E.IsReady())
            {
                Render.Circle.DrawCircle(ObjectManager.Player.Position, LucianSpells.E.Range, Color.Gold);
            }
            if (LucianMenu.Config["Draw Settings"]["Skill Draws"]["lucian.r.draw"].GetValue<MenuBool>().Enabled && LucianSpells.R.IsReady())
            {
                Render.Circle.DrawCircle(ObjectManager.Player.Position, LucianSpells.R.Range, Color.Gold);
            }
        }
    }
}