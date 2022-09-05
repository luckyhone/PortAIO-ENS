using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

using EnsoulSharp;
using EnsoulSharp.SDK;
using EnsoulSharp.SDK.MenuUI;
using EnsoulSharp.SDK.Utility;
using Entropy.AIO.Bases;
using SharpDX;

using OlympusAIO.General;
using OlympusAIO.Helpers;
using MenuManager = OlympusAIO.Helpers.MenuManager;

namespace OlympusAIO.Champions
{
    class Poppy
    {
        public static AIHeroClient objPlayer = ObjectManager.Player;

        public static void OnLoad()
        {
            MenuManager.Execute.Poppy();

            SpellManager.Q = new Spell(SpellSlot.Q, 400f);
            SpellManager.W = new Spell(SpellSlot.W, 400f);
            SpellManager.E = new Spell(SpellSlot.E, 500f);
            SpellManager.R = new Spell(SpellSlot.R, 500f);

            SpellManager.Q.SetSkillshot(0.55f, 90f, float.MaxValue, false, SpellType.Line);
            SpellManager.R.SetSkillshot(0.50f, 90f, 1400f, true, SpellType.Line);
            SpellManager.R.SetCharged("PoppyR", "PoppyR", 425, 1400, 1.0f);

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

                if (MenuManager.ComboMenu["EFlashForced"].GetValue<MenuKeyBind>().Active)
                {
                    Misc.ForceFlashE();
                }

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
                            Modes.LastHit();
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
                if (MenuManager.SpellRangesMenu["WRange"].GetValue<MenuBool>().Enabled && SpellManager.W.IsReady())
                {
                    Render.Circle.DrawCircle(objPlayer.Position, SpellManager.W.Range, System.Drawing.Color.DodgerBlue);
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
            public static void OnInterrupterSpell(AIHeroClient sender, Interrupter.InterruptSpellArgs args)
            {
                if (sender.IsMe || sender.IsAlly || objPlayer.IsDead)
                    return;

                if (MenuManager.MiscInterrupterMenu["E"].GetValue<MenuBool>().Enabled && SpellManager.E.IsReady() && sender.IsValidTarget(SpellManager.E.Range))
                {
                    if (args.DangerLevel >= Interrupter.DangerLevel.Medium)
                    {
                        SpellManager.E.CastOnUnit(objPlayer);
                    }
                }
            }
            public static void OnGapcloser(AIHeroClient sender, AntiGapcloser.GapcloserArgs args)
            {
                if (sender.IsMe || sender.IsAlly || objPlayer.IsDead)
                    return;

                if (MenuManager.MiscGapcloserMenu["W"].GetValue<MenuBool>().Enabled && SpellManager.E.IsReady() && sender.IsFacing(objPlayer) && sender.IsValidTarget(SpellManager.E.Range))
                {
                    SpellManager.E.CastOnUnit(sender);
                }
            }
        }
        public class Modes
        {
            public static void Combo()
            {
                var target = TargetSelector.GetTarget(1000,DamageType.Physical);

                if (target != null && target.IsValid)
                {
                    var comboDamage = DamageManager.GetDamageByChampion(target);

                    bool hasFlash = objPlayer.Spellbook.CanUseSpell(objPlayer.GetSpellSlot("SummonerFlash")) == SpellState.Ready;
                    bool hasIgnite = objPlayer.Spellbook.CanUseSpell(objPlayer.GetSpellSlot("SummonerDot")) == SpellState.Ready;

                    if (MenuManager.ComboMenu["E"].GetValue<MenuBool>().Enabled && SpellManager.E.IsReady() && target.IsValidTarget(SpellManager.E.Range))
                    {
                        if (MenuManager.ComboMenu["ETower"].GetValue<MenuBool>().Enabled && target.IsUnderEnemyTurret())
                            return;

                        if (MenuManager.ComboMenu["EWall"].GetValue<MenuBool>().Enabled)
                        {
                            var bestPos = Misc.BestVectorToFlash(target);

                            if (MenuManager.ComboMenu["EFlash"].GetValue<MenuBool>().Enabled && hasFlash && bestPos != null && Misc.WallCollision(objPlayer, target) && comboDamage > target.Health)
                            {
                                objPlayer.Spellbook.CastSpell(objPlayer.GetSpellSlot("SummonerFlash"), bestPos);

                                DelayAction.Add(50, () => SpellManager.E.CastOnUnit(target));
                            }

                            if (SpellManager.E.CanCast(target) && (Misc.WallCollision(objPlayer, target) || target.Health < SpellManager.E.GetDamage(target) + objPlayer.GetAutoAttackDamage(target)))
                            {
                                SpellManager.E.CastOnUnit(target);
                            }
                            if (SpellManager.E.CanCast(target) && SpellManager.Q.IsReady() && target.Health < SpellManager.E.GetDamage(target) + SpellManager.Q.GetDamage(target) + objPlayer.GetAutoAttackDamage(target))
                            {
                                SpellManager.E.CastOnUnit(target);
                            }
                        }
                        else
                        {
                            SpellManager.E.CastOnUnit(target);
                        }
                    }
                    if (MenuManager.ComboMenu["Q"].GetValue<MenuBool>().Enabled && SpellManager.Q.IsReady() && target.IsValidTarget(SpellManager.Q.Range))
                    {
                        var getPrediction = SpellManager.Q.GetPrediction(target, true);

                        if (getPrediction.Hitchance >= HitChance.High)
                        {
                            SpellManager.Q.Cast(target);
                        }
                    }
                    if (MenuManager.ComboMenu["W"].GetValue<MenuBool>().Enabled && SpellManager.W.IsReady() && target.IsValidTarget(SpellManager.Q.Range) && objPlayer.HealthPercent <= MenuManager.ComboMenu["WHealth"].GetValue<MenuSlider>().Value)
                    {
                        SpellManager.W.CastOnUnit(objPlayer);
                    }
                    if (MenuManager.ComboMenu["R"].GetValue<MenuBool>().Enabled && SpellManager.R.IsReady())
                    {
                        if (MenuManager.ComboMenu["RDefense"].GetValue<MenuBool>().Enabled && ((objPlayer.CountEnemyHeroesInRange(800) >= 2 && objPlayer.CountEnemyHeroesInRange(800) > objPlayer.CountAllyHeroesInRange(1500) + 1 && objPlayer.HealthPercent < 60 || (objPlayer.Health < target.Health && objPlayer.HealthPercent < 40 && objPlayer.CountAllyHeroesInRange(1000) + 1 < objPlayer.CountEnemyHeroesInRange(1000)))))
                        {
                            var newTarget = GameObjects.EnemyHeroes.Where(x => x.IsValidTarget() && SpellManager.R.CanCast(x) && (objPlayer.HealthPercent < 60 || x.CountEnemyHeroesInRange(300) > 2) && GameObjects.EnemyHeroes.Count(i => i.Distance(x) < 400 && x.HealthPercent < 35) == 0 && SpellManager.R.GetPrediction(x).CastPosition.DistanceToPlayer() < SpellManager.R.ChargedMaxRange).OrderByDescending(e => SpellManager.R.GetPrediction(e).CastPosition.CountEnemyHeroesInRange(400)).ThenByDescending(e => e.Distance(target)).FirstOrDefault();

                            if (SpellManager.R.Range > 1300 && newTarget == null)
                            {
                                newTarget = GameObjects.EnemyHeroes.Where(x => x.IsValidTarget() && SpellManager.R.CanCast(target) && SpellManager.R.GetPrediction(x).CastPosition.DistanceToPlayer() < SpellManager.R.ChargedMaxRange).OrderByDescending(x => SpellManager.R.GetPrediction(x).CastPosition.CountEnemyHeroesInRange(400)).ThenByDescending(x => x.Distance(target)).FirstOrDefault();
                            }

                            if (newTarget != null)
                            {
                                if (!SpellManager.R.IsCharging)
                                {
                                    SpellManager.R.StartCharging();
                                }
                                if (SpellManager.R.IsCharging && SpellManager.R.Range > 1000 && SpellManager.R.Range > newTarget.DistanceToPlayer())
                                {
                                    SpellManager.R.CastIfHitchanceEquals(newTarget, HitChance.Medium);
                                }
                                if (SpellManager.R.IsCharging && SpellManager.R.Range < 1000)
                                {
                                    return;
                                }
                            }
                        }

                        if (objPlayer.Distance(target) < 1400 && !target.IsUnderEnemyTurret())
                        {
                            var igniteDamage = hasIgnite ? (float)objPlayer.GetSummonerSpellDamage(target, SummonerSpell.Ignite) : 0f;
                            var ultDamage = SpellManager.R.GetDamage(target);

                            var canCast = ((ultDamage < target.Health && igniteDamage + ultDamage > target.Health && objPlayer.Distance(target) < 600 || (target.DistanceToPlayer() > SpellManager.E.Range && ultDamage > target.Health && target.DistanceToPlayer() < 1100)));

                            if (canCast)
                            {
                                if (!SpellManager.R.IsCharging && !SpellManager.Q.IsReady() && objPlayer.Health < 40)
                                {
                                    SpellManager.R.StartCharging();

                                    if (hasIgnite && comboDamage > target.Health && comboDamage - ultDamage < target.Health)
                                    {
                                        if (target.HasBuff("summonerdot"))
                                            return;

                                        objPlayer.Spellbook.CastSpell(objPlayer.GetSpellSlot("SummonerDot"), target);
                                    }
                                }
                                if (SpellManager.R.IsCharging)
                                {
                                    SpellManager.R.CastIfHitchanceEquals(target, HitChance.High);
                                }
                            }
                        }
                    }
                }
            }
            public static void Harass()
            {
                if (ManaManager.HaveNoEnoughMana(MenuManager.HarassMenu["MinMana"].GetValue<MenuSlider>()))
                    return;

                if (MenuManager.HarassMenu["Q"].GetValue<MenuBool>().Enabled && SpellManager.Q.IsReady())
                {
                    var target = TargetSelector.GetTarget(SpellManager.Q.Range,DamageType.Physical);

                    if (target != null && target.IsValidTarget(SpellManager.Q.Range))
                    {
                        var getPrediction = SpellManager.Q.GetPrediction(target, true);

                        if (getPrediction.Hitchance >= HitChance.High)
                        {
                            SpellManager.Q.Cast(getPrediction.CastPosition);
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
                    var minions = GameObjects.EnemyMinions.Where(x => x.IsValidTarget(SpellManager.Q.Range) && x.IsMinion() && !x.IsDead).OrderBy(x => x.Health).Cast<AIBaseClient>().ToList();

                    if (minions.Count() == 0)
                        return;

                    var getLineFarmLocation = SpellManager.Q.GetLineFarmLocation(minions);

                    if (getLineFarmLocation.MinionsHit >= MenuManager.LaneClearMenu["QMinHits"].GetValue<MenuSlider>().Value)
                    {
                        SpellManager.Q.Cast(getLineFarmLocation.Position);
                    }
                }
                if (MenuManager.LaneClearMenu["E"].GetValue<MenuBool>().Enabled && SpellManager.E.IsReady())
                {
                    var minions = GameObjects.EnemyMinions.Where(x => x.IsValidTarget(SpellManager.E.Range) && x.IsMinion() && !x.IsDead).OrderBy(x => x.MaxHealth).Cast<AIBaseClient>();

                    if (minions.Count() == 0)
                        return;

                    foreach (var minion in minions)
                    {
                        if (minion != null && minion.IsValidTarget(SpellManager.E.Range) && !minion.IsUnderEnemyTurret())
                        {
                            if (Misc.WallCollision(objPlayer, minion))
                            {
                                SpellManager.E.CastOnUnit(minion);
                            }
                        }
                    }
                }
            }
            public static void JungleClear()
            {
                if (ManaManager.HaveNoEnoughMana(MenuManager.JungleClearMenu["MinMana"].GetValue<MenuSlider>()))
                    return;

                if (MenuManager.JungleClearMenu["Q"].GetValue<MenuBool>().Enabled && SpellManager.Q.IsReady())
                {
                    var mobs = GameObjects.Jungle.Where(x => x.IsValidTarget(SpellManager.Q.Range) && x.IsJungle() && !x.IsDead).OrderBy(x => x.MaxHealth).Cast<AIBaseClient>();

                    if (mobs.Count() == 0)
                        return;

                    var firstMob = mobs.FirstOrDefault();

                    if (firstMob != null && firstMob.IsValidTarget(SpellManager.Q.Range))
                    {
                        var getPrediction = SpellManager.Q.GetPrediction(firstMob, true);

                        if (getPrediction.Hitchance >= HitChance.Medium)
                        {
                            SpellManager.Q.Cast(getPrediction.CastPosition);
                        }
                    }
                }
                if (MenuManager.JungleClearMenu["E"].GetValue<MenuBool>().Enabled && SpellManager.E.IsReady())
                {
                    var mobs = GameObjects.Jungle.Where(x => x.IsValidTarget(SpellManager.E.Range) && x.IsJungle() && !x.IsDead).OrderBy(x => x.MaxHealth).Cast<AIBaseClient>();

                    if (mobs.Count() == 0)
                        return;

                    var firstMob = mobs.FirstOrDefault();

                    if (firstMob != null && firstMob.IsValidTarget(SpellManager.E.Range) && Misc.WallCollision(objPlayer, firstMob))
                    {
                        SpellManager.E.CastOnUnit(firstMob);
                    }
                }
            }
            public static void LastHit()
            {
                if (ManaManager.HaveNoEnoughMana(MenuManager.LastHitMenu["MinMana"].GetValue<MenuSlider>()))
                    return;

               
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

                        if (getPrediction.Hitchance >= HitChance.High)
                        {
                            SpellManager.Q.Cast(getPrediction.CastPosition);
                        }
                    }
                }
                if (MenuManager.MiscKillSteal["E"].GetValue<MenuBool>().Enabled && SpellManager.E.IsReady())
                {
                    foreach (var target in GameObjects.EnemyHeroes.Where(x => x.IsValidTarget(SpellManager.E.Range) && x.Health + x.AllShield <= SpellManager.E.GetDamage(x)))
                    {
                        SpellManager.E.CastOnUnit(target);
                    }
                }
            }
            public static void ForceFlashE()
            {
                objPlayer.IssueOrder(GameObjectOrder.MoveTo, Game.CursorPos);

                var target = TargetSelector.GetTarget(1000,DamageType.Physical);

                if (target != null && target.IsValidTarget(SpellManager.E.Range + 500))
                {
                    var bestPos = BestVectorToFlash(target);

                    bool hasFlash = objPlayer.Spellbook.CanUseSpell(objPlayer.GetSpellSlot("SummonerFlash")) == SpellState.Ready;

                    if (SpellManager.E.IsReady() && hasFlash && !WallCollision(objPlayer, target) && bestPos.IsValid())
                    {
                        objPlayer.Spellbook.CastSpell(objPlayer.GetSpellSlot("SummonerFlash"), bestPos);

                        DelayAction.Add(50, () => SpellManager.E.CastOnUnit(target));
                    }
                    else if (!hasFlash)
                    {
                        Modes.Combo();
                    }
                }
            }
            public static bool WallCollision(AIBaseClient objPlayer, AIBaseClient target)
            {
                var distance = objPlayer.Position.Distance(target.Position);

                for (int i = 1; i < 6; i++)
                {
                    if (objPlayer.Position.Extend(target.Position, distance + 70 * i).IsWall())
                    {
                        return true;
                    }
                }
                return false;
            }
            public static Vector3 BestVectorToFlash(AIBaseClient target)
            {
                return UtilityManager.PointsAroundTarget(target.Position, 500).Where(x => x.IsValid() && target.Distance(x) > 80 && target.Distance(x) < 485 && objPlayer.Distance(x) < 400 && objPlayer.Distance(x) > 200 && !x.IsWall() && EnvironmentManager.Map.WallCollision(x, target.Position)).OrderByDescending(x => x.DistanceToPlayer()).FirstOrDefault();
            }
        }
    }
}
