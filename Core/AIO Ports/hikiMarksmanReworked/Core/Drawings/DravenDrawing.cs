using System;
using System.Drawing;
using System.Linq;
using EnsoulSharp;
using EnsoulSharp.SDK.MenuUI;
using Flowers_Viktor;
using hikiMarksmanRework.Core.Menus;
using hikiMarksmanRework.Core.Spells;
using hikiMarksmanRework.Core.Utilitys;
using LeagueSharpCommon;

namespace hikiMarksmanRework.Core.Drawings
{
    class DravenDrawing
    {
        private static readonly AIHeroClient Player = ObjectManager.Player;
        public static void Init()
        {
            if (DravenMenu.Config["Draw Settings"]["Axe Draws"]["DCR"].GetValue<MenuBool>().Enabled)
            {
                Render.Circle.DrawCircle(Game.CursorPos, 600, Color.Gold);
            }

            if (DravenMenu.Config["Draw Settings"]["Skill Draws"]["DE"].GetValue<MenuBool>().Enabled)
            {
                Render.Circle.DrawCircle(Player.Position, DravenSpells.E.Range, Color.Gold);
            }

            if (DravenMenu.Config["Draw Settings"]["Skill Draws"]["DR"].GetValue<MenuBool>().Enabled)
            {
                Render.Circle.DrawCircle(Player.Position, DravenSpells.R.Range, Color.Gold);
            }

            if (DravenMenu.Config["Draw Settings"]["Axe Draws"]["DAR"].GetValue<MenuBool>().Enabled)
            {
                foreach (var axe in DravenAxeHelper.AxeSpots.Where(x => /*x.AxeObj.IsVisibleOnScreen &&*/ x.AxeObj.Position.Distance(ObjectManager.Player.Position) < 1000))
                {
                    Drawing.DrawText(Drawing.WorldToScreen(axe.AxeObj.Position).X - 40, Drawing.WorldToScreen(axe.AxeObj.Position).Y, Color.Gold, (((float)(axe.EndTick - Environment.TickCount))) + " ms");
                    Render.Circle.DrawCircle(axe.AxeObj.Position, 120, DravenAxeHelper.InCatchRadius(axe) ? Color.White : Color.Gold);
                }
            }
        }
    }
}