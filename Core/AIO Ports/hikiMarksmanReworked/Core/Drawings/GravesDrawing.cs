using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using EnsoulSharp;
using EnsoulSharp.SDK;
using EnsoulSharp.SDK.MenuUI;
using hikiMarksmanRework.Core.Menus;
using hikiMarksmanRework.Core.Spells;
using hikiMarksmanRework.Core.Utilitys;
using Color = System.Drawing.Color;
using Render = LeagueSharpCommon.Render;

namespace hikiMarksmanRework.Core.Drawings
{
    class GravesDrawing
    {
        public static void Init()
        {
            if (ObjectManager.Player.IsDead)
            {
                return;
            }
            if (GravesMenu.Config["Draw Settings"]["Skill Draws"]["graves.q.draw"].GetValue<MenuBool>().Enabled && GravesSpells.Q.IsReady())
            {
                Render.Circle.DrawCircle(ObjectManager.Player.Position, GravesSpells.Q.Range, Color.Gold);
            }
            if (GravesMenu.Config["Draw Settings"]["Skill Draws"]["graves.w.draw"].GetValue<MenuBool>().Enabled && GravesSpells.W.IsReady())
            {
                Render.Circle.DrawCircle(ObjectManager.Player.Position, GravesSpells.W.Range, Color.Gold);
            }
            if (GravesMenu.Config["Draw Settings"]["Skill Draws"]["graves.e.draw"].GetValue<MenuBool>().Enabled && GravesSpells.E.IsReady())
            {
                Render.Circle.DrawCircle(ObjectManager.Player.Position, GravesSpells.E.Range, Color.Gold);
            }
            if (GravesMenu.Config["Draw Settings"]["Skill Draws"]["graves.r.draw"].GetValue<MenuBool>().Enabled && GravesSpells.R.IsReady())
            {
                Render.Circle.DrawCircle(ObjectManager.Player.Position, GravesSpells.R.Range, Color.Gold);
            }
            if (!GravesMenu.Config["Draw Settings"]["Catch Draws"]["graves.disable.catch"].GetValue<MenuBool>().Enabled)
            {
                var selectedtarget = TargetSelector.SelectedTarget;
                if (selectedtarget != null)
                {
                    var playerposition = Drawing.WorldToScreen(ObjectManager.Player.Position);
                    var enemyposition = Drawing.WorldToScreen(selectedtarget.Position);
                    if (GravesMenu.Config["Draw Settings"]["Catch Draws"]["graves.catch.line"].GetValue<MenuBool>().Enabled)
                    {
                        if (Catcher.GapcloseCalculte(selectedtarget, GravesSpells.E) < 0 && Catcher.Calculate(selectedtarget) < 0)
                        {
                            Drawing.DrawLine(playerposition, enemyposition, 2, Color.LawnGreen);
                        }
                        if (Catcher.GapcloseCalculte(selectedtarget, GravesSpells.E) > 0 && Catcher.Calculate(selectedtarget) > 0)
                        {
                            Drawing.DrawLine(playerposition, enemyposition, 2, Color.Red);
                        }
                        if (Catcher.GapcloseCalculte(selectedtarget, GravesSpells.E) < 0 && Catcher.Calculate(selectedtarget) > 0)
                        {
                            Drawing.DrawLine(playerposition, enemyposition, 2, Color.Orange);
                        }
                    }
                    if (GravesMenu.Config["Draw Settings"]["Catch Draws"]["graves.catch.circle"].GetValue<MenuBool>().Enabled)
                    {
                        if (Catcher.GapcloseCalculte(selectedtarget, GravesSpells.E) < 0 && Catcher.Calculate(selectedtarget) < 0)
                        {
                            Render.Circle.DrawCircle(selectedtarget.Position, 100, Color.LawnGreen);
                        }
                        if (Catcher.GapcloseCalculte(selectedtarget, GravesSpells.E) > 0 && Catcher.Calculate(selectedtarget) > 0)
                        {
                            Render.Circle.DrawCircle(selectedtarget.Position, 100, Color.Red);
                        }
                        if (Catcher.GapcloseCalculte(selectedtarget, GravesSpells.E) < 0 && Catcher.Calculate(selectedtarget) > 0)
                        {
                            Render.Circle.DrawCircle(selectedtarget.Position, 100, Color.Orange);
                        }
                    }
                    if (GravesMenu.Config["Draw Settings"]["Catch Draws"]["graves.catch.text"].GetValue<MenuBool>().Enabled)
                    {
                        if (Catcher.GapcloseCalculte(selectedtarget, GravesSpells.E) < 0 && Catcher.Calculate(selectedtarget) < 0)
                        {
                            Drawing.DrawText(playerposition.X, playerposition.Y, Color.LawnGreen, "Catch (Time): " + (int)Catcher.Calculate(selectedtarget));
                            if (GravesSpells.E.IsReady())
                            {
                                Drawing.DrawText(playerposition.X - 20, playerposition.Y - 20, Color.LawnGreen, "Catch With Gapclose (Time): " + (int)Catcher.GapcloseCalculte(selectedtarget, GravesSpells.E));
                            }
                        }
                        if (Catcher.GapcloseCalculte(selectedtarget, GravesSpells.E) > 0 && Catcher.Calculate(selectedtarget) > 0)
                        {
                            Drawing.DrawText(playerposition.X, playerposition.Y, Color.Red, "Catch (Time): " + (int)Catcher.Calculate(selectedtarget));
                            if (GravesSpells.E.IsReady())
                            {
                                Drawing.DrawText(playerposition.X - 20, playerposition.Y - 20, Color.Red, "Catch With Gapclose (Time): " + (int)Catcher.GapcloseCalculte(selectedtarget, GravesSpells.E));
                            }
                        }
                        if (Catcher.GapcloseCalculte(selectedtarget, GravesSpells.E) < 0 && Catcher.Calculate(selectedtarget) > 0)
                        {
                            Drawing.DrawText(playerposition.X, playerposition.Y, Color.Orange, "Catch (Time): " + (int)Catcher.Calculate(selectedtarget));
                            if (GravesSpells.E.IsReady())
                            {
                                Drawing.DrawText(playerposition.X - 20, playerposition.Y - 20, Color.Orange, "Catch With Gapclose (Time): " + (int)Catcher.GapcloseCalculte(selectedtarget, GravesSpells.E));
                            }
                        }
                    }
                }
            }
        }
    }
}
