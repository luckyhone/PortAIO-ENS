using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

using EnsoulSharp;
using EnsoulSharp.SDK;
using EnsoulSharp.SDK.MenuUI;
using EnsoulSharp.SDK.Rendering;
using EnsoulSharp.SDK.Utility;
using Entropy.AIO.Bases;
using SharpDX;

using OlympusAIO.General;
using OlympusAIO.Helpers;
using MenuManager = OlympusAIO.Helpers.MenuManager;

namespace OlympusAIO.Champions
{
    class Heimerdinger
    {
        public static AIHeroClient objPlayer = OlympusAIO.objPlayer;

        public static Spell QR, WR, ER;

        public static void OnLoad()
        {
            MenuManager.Execute.Heimerdinger();

            SpellManager.Q = new Spell(SpellSlot.Q, 350f);
            SpellManager.Q.SetSkillshot(05f, 400f, 1450f, false, SpellType.Circle);

            SpellManager.W = new Spell(SpellSlot.W, 1325f);
            SpellManager.W.SetSkillshot(0.5f, 100f, 750f, true, SpellType.Line);

            SpellManager.E = new Spell(SpellSlot.E, 970f);
            SpellManager.E.SetSkillshot(0.5f, 100f, 1200f, false, SpellType.Circle);

            QR = new Spell(SpellSlot.Q, 350f);
            QR.SetSkillshot(0.5f, 400f, 1450f, false, SpellType.Circle);

            WR = new Spell(SpellSlot.W, 1325f);
            WR.SetSkillshot(0.5f, 100f, 750f, true, SpellType.Line);

            ER = new Spell(SpellSlot.E, 1500f);
            ER.SetSkillshot(0.5f, 120f, 1400f, false, SpellType.Circle);

            SpellManager.R = new Spell(SpellSlot.R, 200f);

            /* Main */
            Game.OnUpdate += Events.OnUpdate;

            /* Drawings */
            Drawing.OnDraw += Events.OnDraw;
            Drawing.OnEndScene += DamageIndicator.OnEndScene;

            /* Gapcloser */
            AntiGapcloser.OnGapcloser += Events.OnGapcloser;

            /* Interrupter */
            Interrupter.OnInterrupterSpell += Events.OnInterrupterSpell;
        }

        public class Events
        {
            public static void OnUpdate(EventArgs args)
            {
                if (objPlayer.IsDead || objPlayer.IsRecalling())
                    return;

                Misc.KillSteal();

                switch (Orbwalker.ActiveMode)
                {
                    case OrbwalkerMode.Combo:
                        Modes.Combo();
                        break;
                    case OrbwalkerMode.Harass:
                        if (Methods.SpellHarass)
                        {
                            Modes.Harass();
                        }
                        break;
                    case OrbwalkerMode.LaneClear:
                        if (Methods.SpellFarm)
                        {
                            Modes.LaneClear();
                            Modes.JungleClear();
                        }
                        break;
                    case OrbwalkerMode.LastHit:
                        if (Methods.SpellFarm)
                        {

                        }
                        break;
                }
            }
            public static void OnDraw(EventArgs args)
            {
                if (MenuManager.DrawingsMenu["Disable"].GetValue<MenuBool>().Enabled)
                    return;

                if (MenuManager.SpellRangesMenu["QRange"].GetValue<MenuBool>().Enabled && SpellManager.Q.IsReady())
                {
                    CircleRender.Draw(objPlayer.Position, SpellManager.Q.Range , Color.White);
                }
                if (MenuManager.SpellRangesMenu["WRange"].GetValue<MenuBool>().Enabled)
                {
                    CircleRender.Draw(objPlayer.Position, SpellManager.W.Range, Color.DodgerBlue);
                }
                if (MenuManager.SpellRangesMenu["ERange"].GetValue<MenuBool>().Enabled && SpellManager.E.IsReady())
                {
                    CircleRender.Draw(objPlayer.Position, SpellManager.E.Range, Color.Azure);
                }
                if (MenuManager.SpellRangesMenu["RRange"].GetValue<MenuBool>().Enabled && SpellManager.R.IsReady())
                {
                    CircleRender.Draw(objPlayer.Position, SpellManager.R.Range, Color.Cyan);
                }
            }
            public static void OnInterrupterSpell(AIHeroClient sender, Interrupter.InterruptSpellArgs args)
            {
                if (sender.IsMe || sender.IsAlly || objPlayer.IsDead)
                    return;

                if (MenuManager.MiscInterrupterMenu["E"].GetValue<MenuBool>().Enabled && SpellManager.E.IsReady() && sender.IsValidTarget(SpellManager.E.Range))
                {
                    if (args.DangerLevel >= Interrupter.DangerLevel.Medium)
                    {
                        var getPrediction = SpellManager.E.GetPrediction(sender, true);

                        if (getPrediction.Hitchance >= HitChance.Medium)
                        {
                            SpellManager.E.Cast(getPrediction.CastPosition);
                        }
                    }
                }
            }
            public static void OnGapcloser(AIHeroClient sender, AntiGapcloser.GapcloserArgs args)
            {
                if (sender.IsMe || sender.IsAlly || objPlayer.IsDead)
                    return;

                if (MenuManager.MiscGapcloserMenu["E"].GetValue<MenuBool>().Enabled && SpellManager.E.IsReady() && sender.IsFacing(objPlayer) && sender.IsValidTarget(SpellManager.E.Range))
                {
                    var getPrediction = SpellManager.E.GetPrediction(sender, true);

                    if (getPrediction.Hitchance >= HitChance.Medium)
                    {
                        SpellManager.E.Cast(getPrediction.CastPosition);
                    }
                }
            }
        }
        public class Modes
        {
            public static void Combo()
            {
                if (MenuManager.ComboMenu["R"].GetValue<MenuBool>().Enabled && SpellManager.R.IsReady())
                {
                    if (MenuManager.ComboMenu["QUpgradeHits"].GetValue<MenuSliderButton>().Enabled && QR.IsReady())
                    {
                        var target = TargetSelector.GetTarget(500,DamageType.Magical);

                        if (target != null)
                        {
                            if (target.IsValidTarget(QR.Range) && objPlayer.Position.CountEnemyHeroesInRange(550) >= MenuManager.ComboMenu["QUpgradeHits"].GetValue<MenuSliderButton>().Value && objPlayer.HasBuff("HeimerdingerR"))
                            {
                                SpellManager.Q.Cast(objPlayer.Position.Extend(target.Position, +280));
                            }
                            else if (!objPlayer.HasBuff("HeimerdingerR") && objPlayer.Position.CountEnemyHeroesInRange(550) <= MenuManager.ComboMenu["QUpgradeHits"].GetValue<MenuSliderButton>().Value)
                            {
                                SpellManager.R.Cast();
                            }
                        }
                    }

                    switch (MenuManager.ComboMenu["RMode"].GetValue<MenuList>().SelectedValue)
                    {
                        case "E-R-W":
                            var target = TargetSelector.GetTarget(WR.Range,DamageType.Magical);
                            if (target != null)
                            {
                                if (target.Health < DamageManager.GetDamageByChampion(target))
                                {
                                    if (target.IsValidTarget(SpellManager.E.Range) &&
                                        MenuManager.ComboMenu["E"].GetValue<MenuBool>().Enabled)
                                    {
                                        var getPrediction = SpellManager.E.GetPrediction(target);

                                        if (getPrediction.Hitchance >= HitChance.High)
                                        {
                                            SpellManager.E.Cast(getPrediction.CastPosition);
                                        }
                                    }

                                    if (target.IsValidTarget(WR.Range) && SpellManager.W.IsReady() &&
                                        objPlayer.HasBuff("HeimerdingerR"))
                                    {
                                        var getPrediction = WR.GetPrediction(target);

                                        if (getPrediction.Hitchance >= HitChance.Immobile &&
                                            Variables.TickCount - SpellManager.E.LastCastAttemptTime > 500)
                                        {
                                            WR.Cast(getPrediction.CastPosition);
                                        }
                                    }
                                    else if (!objPlayer.HasBuff("HeimerdingerR") && !SpellManager.E.IsReady())
                                    {
                                        if (SpellManager.E.IsReady() && !SpellManager.W.IsReady())
                                            return;

                                        SpellManager.R.Cast();
                                    }
                                }
                            }

                            break;
                        case "W-R-E":
                            target = TargetSelector.GetTarget(ER.Range,DamageType.Magical);
                            if (target != null)
                            {
                                if (target.IsValidTarget(SpellManager.W.Range) &&
                                    MenuManager.ComboMenu["W"].GetValue<MenuBool>().Enabled)
                                {
                                    var getPrediction = SpellManager.W.GetPrediction(target);

                                    if (getPrediction.Hitchance >= HitChance.High)
                                    {
                                        SpellManager.W.Cast(getPrediction.CastPosition);
                                    }
                                }

                                if (target.IsValidTarget(ER.Range) && SpellManager.E.IsReady() &&
                                    objPlayer.HasBuff("HeimerdingerR"))
                                {
                                    var getPrediction = ER.GetPrediction(target);

                                    if (getPrediction.Hitchance >= HitChance.Immobile)
                                    {
                                        ER.Cast(getPrediction.CastPosition);
                                    }
                                }
                                else if (!objPlayer.HasBuff("HeimerdingerR") && !SpellManager.W.IsReady())
                                {
                                    SpellManager.R.Cast();
                                }
                            }

                            break;
                    }
                }
                if (MenuManager.ComboMenu["E"].GetValue<MenuBool>().Enabled && SpellManager.E.IsReady() && !objPlayer.HasBuff("HeimerdingerR"))
                {
                    var target = TargetSelector.GetTarget(SpellManager.E.Range,DamageType.Magical);

                    if (target.IsValidTarget(SpellManager.E.Range) && target != null)
                    {
                        var getPrediction = SpellManager.E.GetPrediction(target);

                        SpellManager.E.Cast(getPrediction.CastPosition);
                    }
                }
                if (MenuManager.ComboMenu["Q"].GetValue<MenuBool>().Enabled && SpellManager.Q.IsReady() && !objPlayer.HasBuff("HeimerdingerR"))
                {
                    var target = TargetSelector.GetTarget(SpellManager.Q.Range + 300,DamageType.Magical);

                    if (target.IsValidTarget(SpellManager.Q.Range + 300) && target != null)
                    {
                        SpellManager.Q.Cast(objPlayer.Position.Extend(target.Position, +300));
                    }
                }
                if (MenuManager.ComboMenu["W"].GetValue<MenuBool>().Enabled && SpellManager.W.IsReady() && !objPlayer.HasBuff("HeimerdingerR") && !SpellManager.E.IsReady())
                {
                    var target = TargetSelector.GetTarget(SpellManager.W.Range,DamageType.Magical);

                    if (target.IsValidTarget(SpellManager.W.Range) && target != null)
                    {
                        var getPrediction = SpellManager.W.GetPrediction(target);

                        SpellManager.W.Cast(getPrediction.CastPosition);
                    }
                }
            }
            public static void Harass()
            {
                if (ManaManager.HaveNoEnoughMana(MenuManager.HarassMenu["MinMana"].GetValue<MenuSlider>()))
                    return;

                if (MenuManager.HarassMenu["E"].GetValue<MenuBool>().Enabled && SpellManager.E.IsReady() && !objPlayer.HasBuff("HeimerdingerR"))
                {
                    var target = TargetSelector.GetTarget(SpellManager.E.Range,DamageType.Magical);

                    if (!MenuManager.HarassMenu[target.CharacterName.ToLower()].GetValue<MenuBool>().Enabled)
                        return;

                    if (target.IsValidTarget(SpellManager.E.Range) && target != null)
                    {
                        var getPrediction = SpellManager.E.GetPrediction(target);

                        SpellManager.E.Cast(getPrediction.CastPosition);
                    }
                }
                if (MenuManager.HarassMenu["Q"].GetValue<MenuBool>().Enabled && SpellManager.Q.IsReady() && !objPlayer.HasBuff("HeimerdingerR"))
                {
                    var target = TargetSelector.GetTarget(SpellManager.Q.Range + 300,DamageType.Magical);

                    if (!MenuManager.HarassMenu[target.CharacterName.ToLower()].GetValue<MenuBool>().Enabled)
                        return;

                    if (target.IsValidTarget(SpellManager.Q.Range + 300) && target != null)
                    {
                        SpellManager.Q.Cast(objPlayer.Position.Extend(target.Position, +300));
                    }
                }
                if (MenuManager.HarassMenu["W"].GetValue<MenuBool>().Enabled && SpellManager.W.IsReady() && !objPlayer.HasBuff("HeimerdingerR") && !SpellManager.E.IsReady())
                {
                    var target = TargetSelector.GetTarget(SpellManager.W.Range,DamageType.Magical);

                    if (!MenuManager.HarassMenu[target.CharacterName.ToLower()].GetValue<MenuBool>().Enabled)
                        return;

                    if (target.IsValidTarget(SpellManager.W.Range) && target != null)
                    {
                        var getPrediction = SpellManager.W.GetPrediction(target);

                        SpellManager.W.Cast(getPrediction.CastPosition);
                    }
                }
            }
            public static void LaneClear()
            {
                if (ManaManager.HaveNoEnoughMana(MenuManager.LaneClearMenu["MinMana"].GetValue<MenuSlider>()))
                    return;

                if (MenuManager.LaneClearMenu["Q"].GetValue<MenuBool>().Enabled && SpellManager.Q.IsReady() && !objPlayer.HasBuff("HeimerdingerR"))
                {
                    var mobs = GameObjects.EnemyMinions.Where(x => x.IsValidTarget(SpellManager.Q.Range + 250) && x.IsMinion());

                    if (mobs.Count() < 2)
                        return;

                    if (objPlayer.GetBuffCount("heimerdingerturretready") > 1)
                    {
                        SpellManager.Q.Cast(objPlayer.Position.Extend(mobs.FirstOrDefault().Position + 150, +300));
                    }
                }
                if (MenuManager.LaneClearMenu["E"].GetValue<MenuBool>().Enabled && SpellManager.E.IsReady() && !objPlayer.HasBuff("HeimerdingerR"))
                {
                    var mobs = GameObjects.EnemyMinions.Where(x => x.IsValidTarget(SpellManager.E.Range) && x.IsMinion()).OrderBy(x => x.IsRanged);

                    if (mobs.Count() == 0)
                        return;

                    var getCircularFarmLocation = SpellManager.E.GetCircularFarmLocation(mobs.ToList());

                    if (getCircularFarmLocation.MinionsHit >= MenuManager.LaneClearMenu["EMinHits"].GetValue<MenuSlider>().Value)
                    {
                        SpellManager.E.Cast(getCircularFarmLocation.Position);
                    }
                }
                if (MenuManager.LaneClearMenu["W"].GetValue<MenuBool>().Enabled && SpellManager.W.IsReady() && !objPlayer.HasBuff("HeimerdingerR"))
                {
                    var mobs = GameObjects.EnemyMinions.Where(x => x.IsValidTarget(SpellManager.W.Range) && x.IsMinion()).OrderBy(x => x.Health);

                    if (mobs.Count() == 0)
                        return;

                    var killable = mobs.ThenBy(x => x.Health <= SpellManager.W.GetDamage(x)).ThenBy(x => x.DistanceToPlayer());

                    if (killable.Count() == 0)
                        killable = mobs;

                    var getLineFarmLocation = SpellManager.W.GetLineFarmLocation(killable.ToList());

                    if (getLineFarmLocation.MinionsHit >= 1)
                    {
                        SpellManager.W.Cast(getLineFarmLocation.Position);
                    }
                }
            }
            public static void JungleClear()
            {
                if (ManaManager.HaveNoEnoughMana(MenuManager.JungleClearMenu["MinMana"].GetValue<MenuSlider>()))
                    return;

                if (MenuManager.JungleClearMenu["E"].GetValue<MenuBool>().Enabled && SpellManager.E.IsReady() && !objPlayer.HasBuff("HeimerdingerR"))
                {
                    var mobs = GameObjects.Jungle.Where(x => x.IsValidTarget(SpellManager.E.Range) && x.IsJungle()).OrderBy(x => x.MaxHealth).Cast<AIBaseClient>();

                    if (mobs.Count() == 0)
                        return;

                    var getPrediction = SpellManager.E.GetPrediction(mobs.FirstOrDefault());

                    if (getPrediction.Hitchance >= HitChance.Medium)
                    {
                        SpellManager.E.Cast(getPrediction.CastPosition);
                    }
                }
                if (MenuManager.JungleClearMenu["W"].GetValue<MenuBool>().Enabled && SpellManager.W.IsReady() && !objPlayer.HasBuff("HeimerdingerR"))
                {
                    var mobs = GameObjects.Jungle.Where(x => x.IsValidTarget(SpellManager.W.Range) && x.IsJungle()).OrderBy(x => x.DistanceToPlayer()).Cast<AIBaseClient>();

                    if (mobs.Count() == 0)
                        return;

                    var getPrediction = SpellManager.W.GetPrediction(mobs.FirstOrDefault());

                    if (getPrediction.Hitchance >= HitChance.Medium)
                    {
                        SpellManager.W.Cast(getPrediction.CastPosition);
                    }
                }
                if (MenuManager.JungleClearMenu["Q"].GetValue<MenuBool>().Enabled && SpellManager.Q.IsReady() && !objPlayer.HasBuff("HeimerdingerR"))
                {
                    var mobs = GameObjects.Jungle.Where(x => x.IsValidTarget(SpellManager.Q.Range + 250) && x.IsJungle());

                    if (mobs.Count() == 0)
                        return;

                    var castPosition = objPlayer.Position.Extend(mobs.FirstOrDefault().Position + 150, +300);

                    if (objPlayer.GetBuffCount("heimerdingerturretready") != 0)
                    {
                        SpellManager.Q.Cast(castPosition);
                    }
                }
            }
            public static void LastHit()
            {

            }
        }
        public class Misc
        {
            public static void KillSteal()
            {
                if (MenuManager.MiscKillSteal["Disable"].GetValue<MenuBool>().Enabled)
                    return;

                if (MenuManager.MiscKillSteal["W"].GetValue<MenuBool>().Enabled && SpellManager.W.IsReady())
                {
                    foreach (var target in GameObjects.EnemyHeroes.Where(x => x.IsValidTarget(SpellManager.W.Range) && x.Health + x.AllShield <= SpellManager.W.GetDamage(x)))
                    {
                        var getPrediction = SpellManager.W.GetPrediction(target);

                        if (getPrediction.Hitchance >= HitChance.High)
                        {
                            SpellManager.W.Cast(getPrediction.CastPosition);
                        }
                    }
                }
                if (MenuManager.MiscKillSteal["E"].GetValue<MenuBool>().Enabled && SpellManager.E.IsReady())
                {
                    foreach (var target in GameObjects.EnemyHeroes.Where(x => x.IsValidTarget(SpellManager.E.Range) && x.Health + x.AllShield <= SpellManager.E.GetDamage(x)))
                    {
                        var getPrediction = SpellManager.E.GetPrediction(target);

                        if (getPrediction.Hitchance >= HitChance.High)
                        {
                            SpellManager.E.Cast(getPrediction.CastPosition);
                        }
                    }
                }
            }
        }
    }
}
