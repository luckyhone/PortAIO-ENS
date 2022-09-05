using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

using EnsoulSharp;
using EnsoulSharp.SDK;
using EnsoulSharp.SDK.MenuUI;
using EnsoulSharp.SDK.Utility;

using SharpDX;

using OlympusAIO.Helpers;
using MenuManager = OlympusAIO.Helpers.MenuManager;

namespace OlympusAIO.General
{
    class Methods
    {
        public static bool SpellHarass = false;

        public static bool SpellFarm = false;

        public static bool DisableAntiDC = false;

        public static void OnLoad()
        {
            Game.OnWndProc += delegate(GameWndEventArgs args)
            {
                if (!OlympusAIO.SupportedChampions.All(x => !string.Equals(x, OlympusAIO.objPlayer.CharacterName, StringComparison.CurrentCultureIgnoreCase)))
                {
                    if (args.Msg == 0x20a)
                    {
                        OlympusAIO.MainMenu["SpellFarm"].GetValue<MenuKeyBind>().Active = !OlympusAIO.MainMenu["SpellFarm"].GetValue<MenuKeyBind>().Active;
                    }

                    if (OlympusAIO.MainMenu["SpellHarass"].GetValue<MenuKeyBind>().Active)
                    {
                        SpellHarass = true;
                    }
                    else
                    {
                        SpellHarass = false;
                    }

                    if (OlympusAIO.MainMenu["SpellFarm"].GetValue<MenuKeyBind>().Active)
                    {
                        SpellFarm = true;
                    }
                    else
                    {
                        SpellFarm = false;
                    }
                }

                if (OlympusAIO.MainMenu["DisableAntiDC"].GetValue<MenuBool>().Enabled && !DisableAntiDC)
                {
                }
                else if (!OlympusAIO.MainMenu["DisableAntiDC"].GetValue<MenuBool>().Enabled && DisableAntiDC)
                {
                }
            };
            Drawing.OnDraw += delegate (EventArgs args)
            {
                if (OlympusAIO.SupportedChampions.All(x => !string.Equals(x, OlympusAIO.objPlayer.CharacterName, StringComparison.CurrentCultureIgnoreCase)))
                    return;

                if (OlympusAIO.objPlayer.IsDead || MenuGUI.IsShopOpen)
                    return;

                if (MenuManager.DrawingsMenu["SpellFarm"].GetValue<MenuBool>().Enabled)
                {
                    var playerPos = Drawing.WorldToScreen(OlympusAIO.objPlayer.Position);

                    if (playerPos != null)
                        Drawing.DrawText(playerPos.X - 53, playerPos.Y + 13, OlympusAIO.MainMenu["SpellFarm"].GetValue<MenuKeyBind>().Active ? System.Drawing.Color.Lime : System.Drawing.Color.Gray, OlympusAIO.MainMenu["SpellFarm"].GetValue<MenuKeyBind>().Active ? "Spell Farm: ON" : "Spell Farm: OFF");
                }
            };
            Orbwalker.OnBeforeAttack += delegate (object sender, BeforeAttackEventArgs args)
            {
                switch (Orbwalker.ActiveMode)
                {
                    case OrbwalkerMode.Combo:
                        if (OlympusAIO.MainMenu["DisableAAInCombo"].GetValue<MenuSliderButton>().Enabled)
                        {
                            if (args.Target.Type != GameObjectType.AIHeroClient)
                                return;

                            if (OlympusAIO.objPlayer.Level >= OlympusAIO.MainMenu["DisableAAInCombo"].GetValue<MenuSliderButton>().Value)
                            {
                                args.Process = false;
                            }
                        }
                        break;
                    case OrbwalkerMode.LaneClear:
                        if (OlympusAIO.MainMenu["SupportMode"].GetValue<MenuBool>().Enabled)
                        {
                            var enemyMinions = GameObjects.EnemyMinions.Where(x => x.IsValidTarget(OlympusAIO.objPlayer.AttackRange));

                            if (enemyMinions.Contains(args.Target) && args.Target.Type == GameObjectType.AIMinionClient)
                            {
                                args.Process = !GameObjects.AllyHeroes.Any(x => !x.IsMe && x.DistanceToPlayer() > 2000);
                            }
                        }
                        break;
                }
            };
            Spellbook.OnCastSpell += delegate (Spellbook sender, SpellbookCastSpellEventArgs args)
            {
                if (!DisableAntiDC)
                    return;

                if (sender.Owner.IsMe && (args.Slot == SpellSlot.Q || args.Slot == SpellSlot.W || args.Slot == SpellSlot.E || args.Slot == SpellSlot.R))
                {
                    if (Variables.TickCount - SpellManager.LastCastTime[args.Slot.To<int>()] < MenuManager.HumanizerMenu[args.Slot.ToString()].GetValue<MenuSlider>().Value)
                    {
                        args.Process = false;
                        return;
                    }

                    SpellManager.LastCastTime[args.Slot.To<int>()] = Variables.TickCount;
                }
            };
            AIBaseClient.OnIssueOrder += delegate (AIBaseClient sender, AIBaseClientIssueOrderEventArgs args)
            {
                if (!DisableAntiDC)
                    return;

                if (sender != null && sender.IsValid && sender.IsMe && args.Order == GameObjectOrder.MoveTo)
                {
                    if (Variables.TickCount - SpellManager.LastMove < MenuManager.HumanizerMenu["IssueOrder"].GetValue<MenuSlider>().Value)
                    {
                        args.Process = false;
                        return;
                    }

                    SpellManager.LastMove = Variables.TickCount;
                }
            };
        }
    }
}
