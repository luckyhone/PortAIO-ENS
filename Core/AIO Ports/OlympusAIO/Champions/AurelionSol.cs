using System;
using System.Linq;

using EnsoulSharp;
using EnsoulSharp.SDK;
using EnsoulSharp.SDK.MenuUI;
using EnsoulSharp.SDK.Utility;
using OlympusAIO.General;
using OlympusAIO.Helpers;
using MenuManager = OlympusAIO.Helpers.MenuManager;

namespace OlympusAIO.Champions
{
    class AurelionSol
    {
        public static AIHeroClient objPlayer = OlympusAIO.objPlayer;

        public static MissileClient AurelionSolQMissile;

        public static bool InterrupterQ = true, GapcloserQ = true, KillStealQ = true;

        public static void OnLoad()
        {
            MenuManager.Execute.AurelionSol();

            SpellManager.Q  = new Spell(SpellSlot.Q, 1000f);
            SpellManager.W  = new Spell(SpellSlot.W, 325f);
            SpellManager.W2 = new Spell(SpellSlot.W, 650f);
            SpellManager.E  = new Spell(SpellSlot.E, 650f);
            SpellManager.R  = new Spell(SpellSlot.R, 1500f);

            SpellManager.Q.SetSkillshot(0.25f, 200f, 800f, false, SpellType.Circle);
            SpellManager.R.SetSkillshot(0.25f, 180f, 1750f, false, SpellType.Line);

            /* Main */
            Game.OnUpdate += Events.OnUpdate;

            /* Drawings */
            Drawing.OnDraw += Events.OnDraw;
            Drawing.OnEndScene += DamageIndicator.OnEndScene;

            /* GameObject */
            GameObject.OnCreate += Events.OnMissileCreate;
            GameObject.OnDelete += Events.OnDelete;

            /* Interrupter */
            Interrupter.OnInterrupterSpell += Events.OnInterrupterSpell;

            /* Gapcloser */
            AntiGapcloser.OnGapcloser += Events.OnGapcloser;
        }

        public class Events
        {
            public static void OnUpdate(EventArgs args)
            {
                if (objPlayer.IsDead || objPlayer.IsRecalling())
                    return;

                Misc.KillSteal();

                if (MenuManager.ComboMenu["SemiRKey"].GetValue<MenuKeyBind>().Active && SpellManager.R.IsReady())
                {
                    Misc.SemiManualR();
                }

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
                    Render.Circle.DrawCircle(objPlayer.Position, SpellManager.Q.Range, System.Drawing.Color.White);
                }
                if (MenuManager.SpellRangesMenu["WRange"].GetValue<MenuBool>().Enabled)
                {
                    if (objPlayer.HasBuff("AurelionSolWActive"))
                    {
                        Render.Circle.DrawCircle(objPlayer.Position, SpellManager.W2.Range, System.Drawing.Color.DodgerBlue);
                    }
                    else
                    {
                        Render.Circle.DrawCircle(objPlayer.Position, SpellManager.W.Range, System.Drawing.Color.DodgerBlue);
                    }
                }
                if (MenuManager.SpellRangesMenu["ERange"].GetValue<MenuBool>().Enabled && SpellManager.E.IsReady())
                {
                    Render.Circle.DrawCircle(objPlayer.Position, SpellManager.E.Range, System.Drawing.Color.Azure);
                }
                if (MenuManager.SpellRangesMenu["RRange"].GetValue<MenuBool>().Enabled && SpellManager.R.IsReady())
                {
                    Render.Circle.DrawCircle(objPlayer.Position, SpellManager.R.Range, System.Drawing.Color.Cyan);
                }
            }
            public static void OnMissileCreate(GameObject gameObject, EventArgs args)
            {
                var missile = gameObject as MissileClient;

                if (missile != null && missile.IsValid && missile.SpellCaster.IsMe)
                {
                    if (missile.SData.Name == "AurelionSolQMissile")
                    {
                        AurelionSolQMissile = missile;
                    }
                }

            }
            public static void OnDelete(GameObject gameObject, EventArgs args)
            {
                var missile = gameObject as MissileClient;

                if (missile != null && missile.IsValid && missile.SpellCaster.IsMe)
                {
                    if (missile.SData.Name == "AurelionSolQMissile")
                    {
                        AurelionSolQMissile = null;
                    }
                }
            }
            public static void OnInterrupterSpell(AIHeroClient sender, Interrupter.InterruptSpellArgs args)
            {
                if (sender.IsMe || sender.IsAlly)
                    return;

                if (SpellManager.Q.IsReady() && MenuManager.MiscInterrupterMenu["Q"].GetValue<MenuBool>().Enabled && args.DangerLevel >= Interrupter.DangerLevel.Medium)
                {
                    var getPrediction = SpellManager.Q.GetPrediction(sender, true);

                    if (SpellManager.Q.SData.Name == "AurelionSolQ")
                    {
                        if (sender.IsValidTarget(SpellManager.Q.Range) && getPrediction.Hitchance >= HitChance.High && InterrupterQ)
                        {
                            SpellManager.Q.Cast(getPrediction.CastPosition);
                            InterrupterQ = false; DelayAction.Add(2000, () => InterrupterQ = true);
                        }
                    }
                    else if (SpellManager.Q.SData.Name == "AurelionSolQCancelButton")
                    {
                        if (AurelionSolQMissile != null && sender.IsValidTarget(150f, false, AurelionSolQMissile.Position))
                        {
                            SpellManager.Q.Cast();
                        }
                    }
                }
            }
            public static void OnGapcloser(AIHeroClient sender, AntiGapcloser.GapcloserArgs args)
            {
                if (sender.IsMe || sender.IsAlly)
                    return;

                if (SpellManager.Q.IsReady() && MenuManager.MiscGapcloserMenu["Q"].GetValue<MenuBool>().Enabled)
                {
                    var getPrediction = SpellManager.Q.GetPrediction(sender, true);

                    if (SpellManager.Q.SData.Name == "AurelionSolQ")
                    {
                        if (sender.IsValidTarget(SpellManager.Q.Range) && getPrediction.Hitchance >= HitChance.High && GapcloserQ && sender.IsFacing(objPlayer))
                        {
                            SpellManager.Q.Cast(getPrediction.CastPosition);
                            GapcloserQ = false; DelayAction.Add(2000, () => GapcloserQ = true);
                        }
                    }
                    else if (SpellManager.Q.SData.Name == "AurelionSolQCancelButton")
                    {
                        if (AurelionSolQMissile != null && sender.IsValidTarget(150f, false, AurelionSolQMissile.Position))
                        {
                            SpellManager.Q.Cast();
                        }
                    }
                }
            }
        }
        public class Modes
        {
            public static bool ComboQCasted = true;

            public static bool HarassQCasted = true;

            public static bool LaneClearQCasted = true;

            public static bool JungleclearQCasted = true;

            public static void Combo()
            {
                if (MenuManager.ComboMenu["Q"].GetValue<MenuBool>().Enabled && SpellManager.Q.IsReady())
                {
                    var target = TargetSelector.GetTarget(SpellManager.Q.Range,DamageType.Magical);

                    if (target == null)
                        return;

                    var hitchance = UtilityManager.GetHitChance(MenuManager.ComboMenu["QHitChance"].GetValue<MenuList>());

                    if (target.IsValidTarget(SpellManager.Q.Range) && SpellManager.Q.IsInRange(target))
                    {
                        var getPrediction = SpellManager.Q.GetPrediction(target, true);

                        if (SpellManager.Q.SData.Name == "AurelionSolQ")
                        {
                            if (getPrediction.Hitchance >= hitchance && ComboQCasted)
                            {
                                if (MenuManager.ComboMenu["QHits"].GetValue<MenuSlider>().Value == 1)
                                {
                                    SpellManager.Q.Cast(getPrediction.CastPosition);
                                    ComboQCasted = false;
                                    DelayAction.Add(2000, () => ComboQCasted = true);
                                }
                                else if (MenuManager.ComboMenu["QHits"].GetValue<MenuSlider>().Value > 1 && getPrediction.AoeTargetsHitCount >= MenuManager.ComboMenu["QHits"].GetValue<MenuSlider>().Value)
                                {
                                    SpellManager.Q.Cast(getPrediction.CastPosition);
                                    ComboQCasted = false;
                                    DelayAction.Add(2000, () => ComboQCasted = true);
                                }
                            }
                        }
                        else if (SpellManager.Q.SData.Name == "AurelionSolQCancelButton")
                        {
                            if (AurelionSolQMissile != null && target.IsValidTarget(150f, false, AurelionSolQMissile.Position))
                            {
                                SpellManager.Q.Cast();
                            }
                        }
                    }
                }
                if (MenuManager.ComboMenu["W"].GetValue<MenuBool>().Enabled && SpellManager.W.IsReady())
                {
                    var target = TargetSelector.GetTarget(SpellManager.W2.Range + 100,DamageType.Magical);

                    if (target == null)
                        return;

                    if (Misc.IsWActive)
                    {
                        if (target.IsValidTarget(420) || !target.IsValidTarget(800))
                        {
                            SpellManager.W.Cast();
                        }
                    }
                    else if (!Misc.IsWActive)
                    {
                        if (target.IsValidTarget(SpellManager.W2.Range) && !target.IsValidTarget(420))
                        {
                            SpellManager.W.Cast();
                        }
                    }

                }
                if (MenuManager.ComboMenu["R"].GetValue<MenuBool>().Enabled && SpellManager.R.IsReady())
                {

                    foreach (var target in GameObjects.EnemyHeroes.Where(x => x.IsValidTarget(SpellManager.R.Range) && x.Health < SpellManager.R.GetDamage(x)))
                    {
                        var getPrediction = SpellManager.R.GetPrediction(target, true);

                        if (getPrediction.Hitchance >= HitChance.High)
                        {

                            if (MenuManager.ComboMenu["RHits"].GetValue<MenuSlider>().Value == 1)
                            {
                                SpellManager.R.Cast(getPrediction.CastPosition);
                            }
                            else if (MenuManager.ComboMenu["Rhits"].GetValue<MenuSlider>().Value > 1 && getPrediction.AoeTargetsHitCount >= MenuManager.ComboMenu["Rhits"].GetValue<MenuSlider>().Value)
                            {
                                SpellManager.R.Cast(getPrediction.CastPosition);
                            }
                        }
                    }
                }
            }
            public static void Harass()
            {
                if (ManaManager.HaveNoEnoughMana(MenuManager.HarassMenu["MinMana"].GetValue<MenuSlider>()))
                    return;

                var target = TargetSelector.GetTarget(SpellManager.Q.Range,DamageType.Magical);

                if (target == null || !MenuManager.HarassMenu[target.CharacterName.ToLower()].GetValue<MenuBool>().Enabled)
                    return;

                if (MenuManager.HarassMenu["Q"].GetValue<MenuBool>().Enabled && SpellManager.Q.IsReady())
                {
                    var hitchance = UtilityManager.GetHitChance(MenuManager.HarassMenu["QHitChance"].GetValue<MenuList>());

                    if (target.IsValidTarget(SpellManager.Q.Range))
                    {
                        var getPrediction = SpellManager.Q.GetPrediction(target, true);

                        if (SpellManager.Q.SData.Name == "AurelionSolQ")
                        {
                            if (getPrediction.Hitchance >= hitchance && ComboQCasted)
                            {
                                SpellManager.Q.Cast(getPrediction.CastPosition);
                                ComboQCasted = false;
                                DelayAction.Add(2000, () => ComboQCasted = true);
                            }
                        }
                        else if (SpellManager.Q.SData.Name == "AurelionSolQCancelButton")
                        {
                            if (AurelionSolQMissile != null && target.IsValidTarget(130f, false, AurelionSolQMissile.Position))
                            {
                                SpellManager.Q.Cast();
                            }
                        }
                    }
                }
                if (MenuManager.HarassMenu["W"].GetValue<MenuBool>().Enabled && SpellManager.W.IsReady())
                {
                    if (Misc.IsWActive)
                    {
                        if (target.IsValidTarget(420) || !target.IsValidTarget(800))
                        {
                            SpellManager.W.Cast();
                        }
                    }
                    else if (!Misc.IsWActive)
                    {
                        if (target.IsValidTarget(SpellManager.W2.Range) && !target.IsValidTarget(420))
                        {
                            SpellManager.W.Cast();
                        }
                    }
                }
            }
            public static void LaneClear()
            {
                if (ManaManager.HaveNoEnoughMana(MenuManager.LaneClearMenu["MinMana"].GetValue<MenuSlider>()))
                    return;

                if (MenuManager.LaneClearMenu["Q"].GetValue<MenuBool>().Enabled && SpellManager.Q.IsReady())
                {
                    var minions = GameObjects.EnemyMinions.Where(x => x.IsValidTarget(SpellManager.Q.Range - 200) && x.IsMinion()).ToList();

                    if (minions.Count() == 0)
                        return;

                    var circularFarmLocation = SpellManager.Q.GetCircularFarmLocation(minions, SpellManager.Q.Width);

                    if (SpellManager.Q.SData.Name == "AurelionSolQ")
                    {
                        if (circularFarmLocation.MinionsHit >= MenuManager.LaneClearMenu["QHits"].GetValue<MenuSlider>().Value && LaneClearQCasted)
                        {
                            SpellManager.Q.Cast(circularFarmLocation.Position);
                            LaneClearQCasted = false;
                            DelayAction.Add(2000, () => LaneClearQCasted = true);
                        }
                    }
                    else if (SpellManager.Q.SData.Name == "AurelionSolQCancelButton")
                    {
                        if (AurelionSolQMissile != null && AurelionSolQMissile.Position.Distance(circularFarmLocation.Position) < 100)
                        {
                            SpellManager.Q.Cast();
                        }
                    }
                }
                if (MenuManager.LaneClearMenu["W"].GetValue<MenuBool>().Enabled && SpellManager.W.IsReady())
                {
                    var minions = GameObjects.EnemyMinions.Where(x => x.IsValidTarget(SpellManager.W2.Range + 20) && x.IsMinion()).ToList();

                    if (minions.Count() <= 2 && Misc.IsWActive && (minions.FirstOrDefault().IsValidTarget(325) || !minions.FirstOrDefault().IsValidTarget(625)))
                    {
                        SpellManager.W2.Cast();
                    }
                    if (minions.Count() > 2 && !Misc.IsWActive && minions.FirstOrDefault().IsValidTarget(SpellManager.W2.Range) && !minions.FirstOrDefault().IsValidTarget(325))
                    {
                        var getCircularFarmLocation = SpellManager.W2.GetCircularFarmLocation(minions, SpellManager.W2.Range);

                        if (getCircularFarmLocation.MinionsHit >= MenuManager.LaneClearMenu["WHits"].GetValue<MenuSlider>().Value)
                            SpellManager.W.Cast();
                    }
                }
            }
            public static void JungleClear()
            {
                if (ManaManager.HaveNoEnoughMana(MenuManager.JungleClearMenu["MinMana"].GetValue<MenuSlider>()))
                    return;

                if (MenuManager.JungleClearMenu["Q"].GetValue<MenuBool>().Enabled && SpellManager.Q.IsReady())
                {
                    var mob = GameObjects.Jungle.Where(x => x.IsValidTarget(SpellManager.Q.Range) && x.IsJungle()).Cast<AIBaseClient>().MinBy(x => x.MaxHealth);

                    if (mob == null)
                        return;

                    var getPrediction = SpellManager.Q.GetPrediction(mob, true);

                    if (SpellManager.Q.SData.Name == "AurelionSolQ")
                    {
                        if (getPrediction.Hitchance >= HitChance.Medium && JungleclearQCasted)
                        {
                            SpellManager.Q.Cast(getPrediction.CastPosition);
                            JungleclearQCasted = false;
                            DelayAction.Add(2000, () => JungleclearQCasted = true);
                        }
                    }
                    else if (SpellManager.Q.SData.Name == "AurelionSolQCancelButton")
                    {
                        if (AurelionSolQMissile != null && AurelionSolQMissile.Position.Distance(getPrediction.CastPosition) < 100)
                        {
                            SpellManager.Q.Cast();
                        }
                    }
                }
                if (MenuManager.JungleClearMenu["W"].GetValue<MenuBool>().Enabled && SpellManager.W.IsReady())
                {
                    var mob = GameObjects.Jungle.Where(x => x.IsValidTarget(SpellManager.W2.Range) && x.IsJungle()).Cast<AIBaseClient>().MinBy(x => x.MaxHealth);

                    if (mob == null)
                        return;

                    if (mob.IsValidTarget(SpellManager.W2.Range) && !mob.IsValidTarget(420) && !Misc.IsWActive)
                    {
                        SpellManager.W.Cast();
                    }
                    else if (Misc.IsWActive && mob.IsValidTarget(420))
                    {
                        SpellManager.W.Cast();
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

                if (MenuManager.MiscKillSteal["Q"].GetValue<MenuBool>().Enabled && SpellManager.Q.IsReady())
                {
                    foreach (var target in GameObjects.EnemyHeroes.Where(x => x.IsValidTarget(SpellManager.Q.Range) && x.Health + x.AllShield <= SpellManager.Q.GetDamage(x)))
                    {
                        var getPrediction = SpellManager.Q.GetPrediction(target, true);

                        if (SpellManager.Q.SData.Name == "AurelionSolQ")
                        {
                            if (getPrediction.Hitchance >= HitChance.Medium && KillStealQ)
                            {
                                SpellManager.Q.Cast(getPrediction.CastPosition);
                                KillStealQ = false;
                                DelayAction.Add(2000, () => KillStealQ = true);
                            }
                        }
                        else if (SpellManager.Q.SData.Name == "AurelionSolQCancelButton")
                        {
                            if (AurelionSolQMissile != null && target.IsValidTarget(150f, false, AurelionSolQMissile.Position))
                            {
                                SpellManager.Q.Cast();
                            }
                        }
                    }
                }
                if (MenuManager.MiscKillSteal["R"].GetValue<MenuBool>().Enabled && SpellManager.R.IsReady())
                {
                    foreach (var target in GameObjects.EnemyHeroes.Where(x => x.IsValidTarget(SpellManager.R.Range) && x.Health <= SpellManager.R.GetDamage(x)))
                    {
                        var getPrediction = SpellManager.R.GetPrediction(target);

                        if (getPrediction.Hitchance >= HitChance.Medium)
                        {
                            SpellManager.R.Cast(getPrediction.CastPosition);
                        }
                    }
                }
            }
            public static void SemiManualR()
            {
                objPlayer.IssueOrder(GameObjectOrder.MoveTo, Game.CursorPos);

                foreach (var target in GameObjects.EnemyHeroes.Where(x => x.IsValidTarget(SpellManager.R.Range) && x.Health <= SpellManager.R.GetDamage(x)))
                {
                    var getPrediction = SpellManager.R.GetPrediction(target);

                    if (getPrediction.Hitchance >= HitChance.Medium)
                    {
                        SpellManager.R.Cast(getPrediction.CastPosition);
                    }
                }
            }
            public static bool IsWActive 
            {
                get
                {
                    return objPlayer.HasBuff("AurelionSolWActive");
                }
            }
        }
    }
}
