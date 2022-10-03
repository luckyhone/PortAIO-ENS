using System;
using System.Collections.Generic;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using EnsoulSharp;
using EnsoulSharp.SDK;
using EnsoulSharp.SDK.MenuUI;
using hikiMarksmanRework.Core.Menus;
using hikiMarksmanRework.Core.Spells;
using hikiMarksmanRework.Core.Utilitys;
using Render = LeagueSharpCommon.Render;

namespace hikiMarksmanRework.Core.Drawings
{
    class CorkiDrawing
    {
        public static void Init()
        {
            if (ObjectManager.Player.IsDead)
            {
                return;
            }
            if (CorkiMenu.Config["Draw Settings"]["Skill Draws"]["corki.q.draw"].GetValue<MenuBool>().Enabled && CorkiSpells.Q.IsReady())
            {
                Render.Circle.DrawCircle(ObjectManager.Player.Position, CorkiSpells.Q.Range, Color.Gold);
            }
            if (CorkiMenu.Config["Draw Settings"]["Skill Draws"]["corki.w.draw"].GetValue<MenuBool>().Enabled && CorkiSpells.W.IsReady())
            {
                Render.Circle.DrawCircle(ObjectManager.Player.Position, CorkiSpells.W.Range, Color.Gold);
            }
            if (CorkiMenu.Config["Draw Settings"]["Skill Draws"]["corki.e.draw"].GetValue<MenuBool>().Enabled && CorkiSpells.E.IsReady())
            {
                Render.Circle.DrawCircle(ObjectManager.Player.Position, CorkiSpells.E.Range, Color.Gold);
            }
            if (CorkiMenu.Config["Draw Settings"]["Skill Draws"]["corki.r.draw"].GetValue<MenuBool>().Enabled && CorkiSpells.R.IsReady())
            {
                Render.Circle.DrawCircle(ObjectManager.Player.Position, CorkiSpells.R.Range, Color.Gold);
            }
            if (!CorkiMenu.Config["Draw Settings"]["Catch Draws"]["corki.disable.catch"].GetValue<MenuBool>().Enabled)
            {
                var selectedtarget = TargetSelector.SelectedTarget;
                if (selectedtarget != null)
                {
                    var playerposition = Drawing.WorldToScreen(ObjectManager.Player.Position);
                    var enemyposition = Drawing.WorldToScreen(selectedtarget.Position);
                    if (CorkiMenu.Config["Draw Settings"]["Catch Draws"]["corki.catch.line"].GetValue<MenuBool>().Enabled)
                    {
                        if (Catcher.GapcloseCalculte(selectedtarget, EzrealSpells.E) < 0 && Catcher.Calculate(selectedtarget) < 0)
                        {
                            Drawing.DrawLine(playerposition, enemyposition, 2, Color.LawnGreen);
                        }
                        if (Catcher.GapcloseCalculte(selectedtarget, EzrealSpells.E) > 0 && Catcher.Calculate(selectedtarget) > 0)
                        {
                            Drawing.DrawLine(playerposition, enemyposition, 2, Color.Red);
                        }
                        if (Catcher.GapcloseCalculte(selectedtarget, EzrealSpells.E) < 0 && Catcher.Calculate(selectedtarget) > 0)
                        {
                            Drawing.DrawLine(playerposition, enemyposition, 2, Color.Orange);
                        }
                    }
                    if (CorkiMenu.Config["Draw Settings"]["Catch Draws"]["corki.catch.circle"].GetValue<MenuBool>().Enabled)
                    {
                        if (Catcher.GapcloseCalculte(selectedtarget, EzrealSpells.E) < 0 && Catcher.Calculate(selectedtarget) < 0)
                        {
                            Render.Circle.DrawCircle(selectedtarget.Position, 100, Color.LawnGreen);
                        }
                        if (Catcher.GapcloseCalculte(selectedtarget, EzrealSpells.E) > 0 && Catcher.Calculate(selectedtarget) > 0)
                        {
                            Render.Circle.DrawCircle(selectedtarget.Position, 100, Color.Red);
                        }
                        if (Catcher.GapcloseCalculte(selectedtarget, EzrealSpells.E) < 0 && Catcher.Calculate(selectedtarget) > 0)
                        {
                            Render.Circle.DrawCircle(selectedtarget.Position, 100, Color.Orange);
                        }
                    }
                    if (CorkiMenu.Config["Draw Settings"]["Catch Draws"]["corki.catch.text"].GetValue<MenuBool>().Enabled)
                    {
                        if (Catcher.GapcloseCalculte(selectedtarget, EzrealSpells.E) < 0 && Catcher.Calculate(selectedtarget) < 0)
                        {
                            Drawing.DrawText(playerposition.X, playerposition.Y, Color.LawnGreen, "Catch (Time): " + (int)Catcher.Calculate(selectedtarget));
                            Drawing.DrawText(playerposition.X - 20, playerposition.Y - 20, Color.LawnGreen, "Catch With Gapclose (Time): " + (int)Catcher.GapcloseCalculte(selectedtarget, EzrealSpells.E));
                        }
                        if (Catcher.GapcloseCalculte(selectedtarget, EzrealSpells.E) > 0 && Catcher.Calculate(selectedtarget) > 0)
                        {
                            Drawing.DrawText(playerposition.X, playerposition.Y, Color.Red, "Catch (Time): " + (int)Catcher.Calculate(selectedtarget));
                            Drawing.DrawText(playerposition.X - 20, playerposition.Y - 20, Color.Red, "Catch With Gapclose (Time): " + (int)Catcher.GapcloseCalculte(selectedtarget, EzrealSpells.E));
                        }
                        if (Catcher.GapcloseCalculte(selectedtarget, EzrealSpells.E) < 0 && Catcher.Calculate(selectedtarget) > 0)
                        {
                            Drawing.DrawText(playerposition.X, playerposition.Y, Color.Orange, "Catch (Time): " + (int)Catcher.Calculate(selectedtarget));
                            Drawing.DrawText(playerposition.X - 20, playerposition.Y - 20, Color.Orange, "Catch With Gapclose (Time): " + (int)Catcher.GapcloseCalculte(selectedtarget, EzrealSpells.E));
                        }
                    }
                }
            }
        }
    }
}
