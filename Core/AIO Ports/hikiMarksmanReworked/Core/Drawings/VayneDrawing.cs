using System.Drawing;
using EnsoulSharp;
using EnsoulSharp.SDK.MenuUI;
using hikiMarksmanRework.Core.Menus;
using hikiMarksmanRework.Core.Spells;
using hikiMarksmanRework.Core.Utilitys;
using LeagueSharpCommon;

namespace hikiMarksmanRework.Core.Drawings
{
    class VayneDrawing
    {
        public static void Init()
        {
            if (ObjectManager.Player.IsDead)
            {
                return;
            }
            if (VayneMenu.Config[":: Draw Settings"]["Skill Draws"]["vayne.q.draw"].GetValue<MenuBool>().Enabled && VayneSpells.Q.IsReady())
            {
                Render.Circle.DrawCircle(ObjectManager.Player.Position, VayneSpells.Q.Range,Color.Gold);
            }
            if (VayneMenu.Config[":: Draw Settings"]["Skill Draws"]["vayne.e.draw"].GetValue<MenuBool>().Enabled && VayneSpells.Q.IsReady())
            {
                Render.Circle.DrawCircle(ObjectManager.Player.Position, VayneSpells.E.Range,Color.Gold);
            }

        }
    }
}