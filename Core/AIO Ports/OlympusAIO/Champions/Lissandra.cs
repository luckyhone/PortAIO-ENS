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
    class Lissandra
    {
        public static AIHeroClient objPlayer = ObjectManager.Player;

        public static MissileClient missileClient;

        public static Vector2 missilePositon;

        public static bool IsJumping = false;

        public static void OnLoad()
        {
            MenuManager.Execute.Lissandra();

            SpellManager.Q = new Spell(SpellSlot.Q, 825f);
            SpellManager.W = new Spell(SpellSlot.W, 450f);
            SpellManager.E = new Spell(SpellSlot.E, 1050f);
            SpellManager.E2 = new Spell(SpellSlot.E, 1200f);
            SpellManager.R = new Spell(SpellSlot.R, 550f);

            SpellManager.Q.SetSkillshot(0.5f, 75f, 2200f, false, SpellType.Line);
            SpellManager.E.SetSkillshot(05f, 125f, 850f, false, SpellType.Line);

            /* Main */
            Game.OnUpdate += Events.OnUpdate;

            /* GameObject */
            GameObject.OnCreate += Events.OnMissileCreate;
            GameObject.OnDelete += Events.OnDelete;

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

                if (MenuManager.MiscMenu["Flee"].GetValue<MenuKeyBind>().Active)
                {
                    Misc.Flee();
                }

                Misc.KillSteal();

                Misc.SecondE();

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
            public static void OnMissileCreate(GameObject gameObject, EventArgs args)
            {
                var missile = gameObject as MissileClient;

                if (missile != null && missile.IsValid)
                {
                    if (missile.SpellCaster.IsEnemy)
                        return;

                    if (missile.SData.Name == "LissandraEMissile")
                    {
                        missileClient = missile;
                    }
                }
            }
            public static void OnDelete(GameObject gameObject, EventArgs args)
            {
                var missile = gameObject as MissileClient;

                if (missile != null && missile.IsValid)
                {
                    if (missile.SpellCaster.IsEnemy)
                        return;

                    if (missile.SData.Name == "LissandraEMissile")
                    {
                        missileClient = null;
                        missilePositon = new Vector2(0, 0);
                    }
                }
            }
            public static void OnDraw(EventArgs args)
            {
                if (MenuManager.DrawingsMenu["Disable"].GetValue<MenuBool>().Enabled)
                    return;

                if (MenuManager.SpellRangesMenu["QRange"].GetValue<MenuBool>().Enabled && SpellManager.Q.IsReady())
                {
                    CircleRender.Draw(objPlayer.Position, SpellManager.Q.Range,Color.White);
                }
                if (MenuManager.SpellRangesMenu["WRange"].GetValue<MenuBool>().Enabled && SpellManager.W.IsReady())
                {
                    CircleRender.Draw(objPlayer.Position, SpellManager.W.Range,Color.DodgerBlue);
                }
                if (MenuManager.SpellRangesMenu["ERange"].GetValue<MenuBool>().Enabled && SpellManager.E.IsReady())
                {
                    CircleRender.Draw(objPlayer.Position, SpellManager.E.Range,Color.Azure);
                }
                if (MenuManager.SpellRangesMenu["RRange"].GetValue<MenuBool>().Enabled && SpellManager.R.IsReady())
                {
                    CircleRender.Draw(objPlayer.Position, SpellManager.R.Range,Color.Cyan);
                }
            }
            public static void OnInterrupterSpell(AIHeroClient sender, Interrupter.InterruptSpellArgs args)
            {
                if (sender.IsMe || sender.IsAlly || objPlayer.IsDead)
                    return;

                if (MenuManager.MiscInterrupterMenu["W"].GetValue<MenuBool>().Enabled && SpellManager.W.IsReady() && sender.IsValidTarget(SpellManager.W.Range))
                {
                    if (args.DangerLevel >= Interrupter.DangerLevel.Low)
                    {
                        SpellManager.W.CastOnUnit(objPlayer);
                    }
                }
                if (MenuManager.MiscInterrupterMenu["R"].GetValue<MenuBool>().Enabled && SpellManager.R.IsReady() && sender.IsValidTarget(SpellManager.R.Range))
                {
                    if (args.DangerLevel >= Interrupter.DangerLevel.High)
                    {
                        SpellManager.R.CastOnUnit(sender);
                    }
                }
            }
            public static void OnGapcloser(AIHeroClient sender, AntiGapcloser.GapcloserArgs args)
            {
                if (sender.IsMe || sender.IsAlly || objPlayer.IsDead)
                    return;

                if (MenuManager.MiscGapcloserMenu["W"].GetValue<MenuBool>().Enabled && SpellManager.W.IsReady() && sender.IsFacing(objPlayer) && sender.IsValidTarget(SpellManager.W.Range))
                {
                    SpellManager.W.CastOnUnit(objPlayer);
                }
                if (MenuManager.MiscGapcloserMenu["R"].GetValue<MenuBool>().Enabled && SpellManager.R.IsReady() && sender.IsFacing(objPlayer) && sender.IsValidTarget(SpellManager.R.Range))
                {
                    if (args.Type == AntiGapcloser.GapcloserType.Targeted && objPlayer.HealthPercent <= 20)
                    {
                        SpellManager.R.CastOnUnit(objPlayer);
                    }
                    else if (args.Type == AntiGapcloser.GapcloserType.SkillShot && args.EndPosition.DistanceToPlayer() <= 200)
                    {
                        SpellManager.R.CastOnUnit(objPlayer);
                    }
                }
            }
        }
        public class Modes
        {
            public static void Combo()
            {
                if (MenuManager.ComboMenu["E"].GetValue<MenuBool>().Enabled && SpellManager.E.IsReady())
                {
                    var target = TargetSelector.GetTarget(SpellManager.E.Range,DamageType.Magical);

                    if (target != null)
                    {
                        if (target.CountEnemyHeroesInRange(SpellManager.E.Range) == 1)
                        {
                            var getPrediction = SpellManager.E.GetPrediction(target);

                            if (SpellManager.W.IsReady() && getPrediction != null)
                            {
                                if (SpellManager.E.ToggleState == (SpellToggleState)1)
                                    SpellManager.E.Cast(getPrediction.CastPosition);

                                if (missileClient == null || missileClient.EndPosition.IsUnderEnemyTurret())
                                    return;

                                IsJumping = true;

                                var wAoE = SpellManager.W.GetPrediction(target, true);

                                if (missileClient.Position.CountEnemyHeroesInRange(SpellManager.W.Range) >= 1)
                                {
                                    SpellManager.E.Cast();
                                    IsJumping = false;
                                }
                                if (wAoE.AoeTargetsHitCount >= 1)
                                {
                                    SpellManager.W.Cast();
                                }
                            }
                        }
                        else
                        {
                            var AoE = SpellManager.E.GetPrediction(target, true, SpellManager.E.Range + (SpellManager.W.Range / 2));

                            if (AoE.AoeTargetsHitCount < MenuManager.ComboMenu["EAoE"].GetValue<MenuSlider>().Value || !SpellManager.W.IsReady())
                                return;

                            if (SpellManager.E.ToggleState == (SpellToggleState)1)
                                SpellManager.E.Cast(AoE.CastPosition);

                            if (missileClient == null || missileClient.EndPosition.IsUnderEnemyTurret())
                                return;

                            IsJumping = true;

                            var wAoE = SpellManager.W.GetPrediction(target, true);

                            if (missileClient.Position.CountEnemyHeroesInRange(SpellManager.W.Range) >= MenuManager.ComboMenu["EAoE"].GetValue<MenuSlider>().Value)
                            {
                                SpellManager.E.Cast();
                                IsJumping = false;
                            }
                            if (wAoE.AoeTargetsHitCount > MenuManager.ComboMenu["EAoE"].GetValue<MenuSlider>().Value)
                            {
                                SpellManager.W.CastOnUnit(objPlayer);
                            }
                        }
                    }
                }
                if (MenuManager.ComboMenu["Q"].GetValue<MenuBool>().Enabled && SpellManager.Q.IsReady())
                {
                    var target = TargetSelector.GetTarget(SpellManager.Q.Range + 150,DamageType.Magical);

                    if (target != null)
                    {
                        var hitchance = UtilityManager.GetHitChance(MenuManager.ComboMenu["QHitchance"].GetValue<MenuList>());

                        if (target.IsValidTarget(SpellManager.Q.Range))
                        {
                            var getPrediction = SpellManager.Q.GetPrediction(target);

                            if (getPrediction.Hitchance >= hitchance)
                            {
                                SpellManager.Q.Cast(getPrediction.CastPosition);
                            }
                        }
                        else if (target.DistanceToPlayer() >= SpellManager.Q.Range)
                        {
                            var nearestMinion = GameObjects.EnemyMinions.Where(x => x.DistanceToPlayer() <= target.DistanceToPlayer() && target.Distance(x) < 150).OrderBy(x => x.DistanceToPlayer()).FirstOrDefault();

                            if (nearestMinion != null && nearestMinion.IsValidTarget(SpellManager.Q.Range))
                            {
                                SpellManager.Q.Cast(nearestMinion.Position);
                            }
                        }
                    }
                }
                if (MenuManager.ComboMenu["W"].GetValue<MenuBool>().Enabled && SpellManager.W.IsReady())
                {
                    var target = TargetSelector.GetTarget(SpellManager.W.Range,DamageType.Magical);

                    if (target != null && !SpellManager.E.IsReady())
                    {
                        SpellManager.W.CastOnUnit(objPlayer);
                    }
                }
                if (MenuManager.ComboMenu["R"].GetValue<MenuBool>().Enabled && SpellManager.R.IsReady())
                {
                    if (objPlayer.CountEnemyHeroesInRange(250) >= MenuManager.ComboMenu["RDefense"].GetValue<MenuSlider>().Value)
                    {
                        SpellManager.R.CastOnUnit(objPlayer);
                    }
                    if (objPlayer.HealthPercent < MenuManager.ComboMenu["RHP"].GetValue<MenuSlider>().Value)
                    {
                        if (objPlayer.CountEnemyHeroesInRange(SpellManager.R.Range - 100) < 0)
                            return;

                        SpellManager.R.CastOnUnit(objPlayer);
                    }

                    var target = TargetSelector.GetTarget(SpellManager.R.Range,DamageType.Magical);

                    if (target != null && target.IsValidTarget(SpellManager.R.Range) && !target.HasBuffOfType(BuffType.SpellImmunity))
                    {

                        if (SpellManager.R.GetDamage(target) * 1.2 > target.Health + 10)
                        {
                            SpellManager.R.CastOnUnit(target);
                        }
                        else if (DamageManager.GetDamageByChampion(target) > target.Health + 10)
                        {
                            SpellManager.R.CastOnUnit(target);
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
                    var target = TargetSelector.GetTarget(SpellManager.Q.Range + 150,DamageType.Magical);

                    if (target != null && MenuManager.HarassMenu[target.CharacterName.ToLower()].GetValue<MenuBool>().Enabled)
                    {
                        if (target.IsValidTarget(SpellManager.Q.Range))
                        {
                            var getPrediction = SpellManager.Q.GetPrediction(target);

                            if (getPrediction.Hitchance >= HitChance.Medium)
                            {
                                SpellManager.Q.Cast(getPrediction.CastPosition);
                            }
                        }
                        else if (target.DistanceToPlayer() >= SpellManager.Q.Range)
                        {
                            var nearestMinion = GameObjects.EnemyMinions.Where(x => x.DistanceToPlayer() <= target.DistanceToPlayer() && target.Distance(x) < 150).OrderBy(x => x.DistanceToPlayer()).FirstOrDefault();

                            if (nearestMinion != null && nearestMinion.IsValidTarget(SpellManager.Q.Range))
                            {
                                SpellManager.Q.Cast(nearestMinion.Position);
                            }
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
                    var minions = GameObjects.EnemyMinions.Where(x => x.IsValidTarget(SpellManager.Q.Range) && x.IsMinion()).OrderBy(x => x.Health).Cast<AIBaseClient>();

                    if (minions.Count() == 0)
                        return;

                    var firstMinion = minions.FirstOrDefault();

                    if (firstMinion != null)
                    {
                        var getPrediction = SpellManager.Q.GetPrediction(firstMinion, true);

                        if (getPrediction.Hitchance >= HitChance.Medium)
                        {
                            SpellManager.Q.Cast(getPrediction.CastPosition);
                        }
                    }
                }
                if (MenuManager.LaneClearMenu["W"].GetValue<MenuBool>().Enabled && SpellManager.W.IsReady())
                {
                    var minions = GameObjects.EnemyMinions.Where(x => x.IsValidTarget(SpellManager.W.Range) && x.IsMinion()).OrderBy(x => x.Name.Contains("Super")).ToList();

                    if (minions.Count() == 0)
                        return;

                    if (minions.Count() >= MenuManager.LaneClearMenu["WMinHits"].GetValue<MenuSlider>().Value)
                    {
                        SpellManager.W.CastOnUnit(objPlayer);
                    }
                    else if (minions.FirstOrDefault().Name.Contains("Super"))
                    {
                        SpellManager.W.CastOnUnit(objPlayer);
                    }
                }
            }
            public static void JungleClear()
            {
                if (ManaManager.HaveNoEnoughMana(MenuManager.JungleClearMenu["MinMana"].GetValue<MenuSlider>()))
                    return;

                if (MenuManager.JungleClearMenu["Q"].GetValue<MenuBool>().Enabled && SpellManager.Q.IsReady())
                {
                    var mobs = GameObjects.Jungle.Where(x => x.IsValidTarget(SpellManager.Q.Range) && x.IsJungle()).OrderBy(x => x.DistanceToPlayer()).Cast<AIBaseClient>();

                    if (mobs.Count() == 0)
                        return;

                    var firstMob = mobs.FirstOrDefault();

                    if (firstMob != null)
                    {
                        var getPrediction = SpellManager.Q.GetPrediction(firstMob, false);

                        if (getPrediction.Hitchance >= HitChance.Medium)
                        {
                            SpellManager.Q.Cast(getPrediction.CastPosition);
                        }
                    }
                }
                if (MenuManager.JungleClearMenu["W"].GetValue<MenuBool>().Enabled && SpellManager.W.IsReady())
                {
                    var mobs = GameObjects.Jungle.Where(x => x.IsValidTarget(SpellManager.W.Range + 150) && x.IsJungle()).OrderBy(x => x.MaxHealth).Cast<AIBaseClient>();

                    if (mobs.Count() == 0)
                        return;

                    if (mobs.All(x => x.IsValidTarget(SpellManager.W.Range)))
                    {
                        SpellManager.W.CastIfWillHit(objPlayer, mobs.Count());
                    }
                }
                if (MenuManager.JungleClearMenu["E"].GetValue<MenuBool>().Enabled && SpellManager.E.IsReady() )
                {
                    var mobs = GameObjects.Jungle.Where(x => x.IsValidTarget(SpellManager.E.Range) && x.IsJungle()).OrderBy(x => x.Health).Cast<AIBaseClient>();

                    if (mobs.Count() == 0)
                        return;

                    var firstMob = mobs.FirstOrDefault();

                    if (firstMob != null && !SpellManager.Q.IsReady() && !SpellManager.W.IsReady())
                    {
                        if (SpellManager.E.ToggleState == (SpellToggleState)1)
                        {
                            var getPrediction = SpellManager.E.GetPrediction(firstMob, true);

                            if (getPrediction.Hitchance >= HitChance.Medium)
                            {
                                SpellManager.E.Cast(getPrediction.CastPosition);
                            }
                        }
                        else if (SpellManager.E.ToggleState == (SpellToggleState)2 && MenuManager.JungleClearMenu["E2"].GetValue<MenuBool>().Enabled && Misc.IsSafe(missileClient.EndPosition, 150))
                        {
                            if (firstMob.Distance(missileClient.EndPosition) <= 600)
                            {
                                IsJumping = true;
                            }
                        }
                    }
                }
            }
            public static void LastHit()
            {
                if (ManaManager.HaveNoEnoughMana(MenuManager.LastHitMenu["MinMana"].GetValue<MenuSlider>()))
                    return;

                if (MenuManager.LastHitMenu["Q"].GetValue<MenuBool>().Enabled && SpellManager.Q.IsReady())
                {
                    var minions = GameObjects.EnemyMinions.Where(x => x.IsValidTarget(SpellManager.Q.Range) && x.IsMinion() && x.Health < SpellManager.Q.GetDamage(x)).OrderBy(x => x.DistanceToPlayer()).Cast<AIBaseClient>();

                    if (minions.Count() == 0)
                        return;

                    var firstMinion = minions.FirstOrDefault();

                    if (firstMinion != null)
                    {
                        var getPrediction = SpellManager.Q.GetPrediction(firstMinion, false);

                        if (getPrediction.Hitchance >= HitChance.Medium)
                        {
                            SpellManager.Q.Cast(getPrediction.CastPosition);
                        }
                    }
                }
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
                        var getPrediction = SpellManager.Q.GetPrediction(target);

                        if (getPrediction.Hitchance >= HitChance.High)
                        {
                            SpellManager.Q.Cast(getPrediction.CastPosition);
                        }
                    }
                }
                if (MenuManager.MiscKillSteal["W"].GetValue<MenuBool>().Enabled && SpellManager.W.IsReady())
                {
                    foreach (var target in GameObjects.EnemyHeroes.Where(x => x.IsValidTarget(SpellManager.W.Range) && x.Health + x.AllShield <= SpellManager.W.GetDamage(x)))
                    {
                        SpellManager.W.CastOnUnit(objPlayer);
                    }
                }
            }
            public static void Flee()
            {
                objPlayer.IssueOrder(GameObjectOrder.MoveTo, Game.CursorPos);

                if (missileClient == null && SpellManager.E.IsReady())
                {
                    SpellManager.E.Cast(Game.CursorPos);
                    IsJumping = true;
                }
            }
            public static void SecondE()
            {
                if (missileClient == null)
                    return;

                missilePositon = missileClient.Position.ToVector2();

                if (IsJumping)
                {
                    if (missilePositon.Distance(missileClient.EndPosition.ToVector2()) < 40)
                    {
                        SpellManager.E.Cast();
                        IsJumping = false;
                    }

                    DelayAction.Add(2000, () => IsJumping = false);
                }
            }
            public static bool IsSafe(Vector3 endPos, float extraRange)
            {
                if (endPos.IsZero)
                    return false;

                if (endPos.CountAllyHeroesInRange(250 + extraRange) >= 1)
                    return false;

                if (endPos.IsUnderEnemyTurret())
                    return false;

                return true;
            }
        }
    }
}
