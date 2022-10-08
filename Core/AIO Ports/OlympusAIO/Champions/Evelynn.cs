using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

using EnsoulSharp;
using EnsoulSharp.SDK;
using EnsoulSharp.SDK.MenuUI;
using EnsoulSharp.SDK.Rendering;
using Entropy.AIO.Bases;
using SharpDX;

using OlympusAIO.General;
using OlympusAIO.Helpers;
using MenuManager = OlympusAIO.Helpers.MenuManager;

namespace OlympusAIO.Champions
{
    class Evelynn
    {
        public static AIHeroClient objPlayer = OlympusAIO.objPlayer;

        public static void OnLoad()
        {
            MenuManager.Execute.Evelynn();

            SpellManager.Q = new Spell(SpellSlot.Q, 800f);
            SpellManager.Q2 = new Spell(SpellSlot.Q, 550f);
            SpellManager.W = new Spell(SpellSlot.W, 1200f);
            SpellManager.E = new Spell(SpellSlot.E, 225f + objPlayer.BoundingRadius);
            SpellManager.R = new Spell(SpellSlot.R, 450f + objPlayer.BoundingRadius);

            SpellManager.Q.SetSkillshot(0.25f, 60f, 2400f, true, SpellType.Line);
            SpellManager.R.SetSkillshot(0.25f, UtilityManager.GetAngleByDegrees(180), float.MaxValue, false, SpellType.Cone);

            /* Main */
            Game.OnUpdate += Events.OnUpdate;

            /* OnAction */
            Orbwalker.OnBeforeAttack += Events.OnAction;

            /* Drawings */
            Drawing.OnDraw += Events.OnDraw;
            Drawing.OnEndScene += DamageIndicator.OnEndScene;

            /* Gapcloser */
            AntiGapcloser.OnGapcloser += Events.OnGapcloser;
        }

        public class Events
        {
            public static void OnUpdate(EventArgs args)
            {
                if (objPlayer.IsDead || objPlayer.IsRecalling())
                    return;

                if (!objPlayer.GetSpell(SpellSlot.W).State.HasFlag(SpellState.NotLearned))
                {
                    SpellManager.W.Range = 1100 + 100 * objPlayer.GetSpell(SpellSlot.W).Level;
                }

                if (MenuManager.ComboMenu["SemiRKey"].GetValue<MenuKeyBind>().Active && SpellManager.R.IsReady())
                {
                    Misc.SemiManualR();
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

                        }
                        break;
                }
            }
            public static void OnAction(object sender, BeforeAttackEventArgs args)
            {
                if (MenuManager.MiscMenu["AASemiAllured"].GetValue<MenuBool>().Enabled)
                {
                    var OrbwalkerTarget = Orbwalker.GetTarget();

                    if (OrbwalkerTarget == null)
                        return;

                    var BaseTarget = OrbwalkerTarget as AIBaseClient;   

                    if (BaseTarget == null)
                        return;

                    if (Misc.IsAllured(BaseTarget) && !Misc.IsFullyAllured(BaseTarget))
                    {
                        args.Process = false;
                    }
                }
            }
            public static void OnDraw(EventArgs args)
            {
                if (MenuManager.DrawingsMenu["Disable"].GetValue<MenuBool>().Enabled)
                    return;

                if (MenuManager.SpellRangesMenu["QRange"].GetValue<MenuBool>().Enabled && SpellManager.Q.IsReady())
                {
                    CircleRender.Draw(objPlayer.Position, Misc.IsQSkillshot() ? SpellManager.Q.Range : SpellManager.Q2.Range, Color.White);
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

                    if (objPlayer.Path.PathLength() > 1 && MenuManager.DrawingsMiscMenu["RPos"].GetValue<MenuBool>().Enabled && objPlayer.Position.CountEnemyHeroesInRange(1000) >= 1)
                    {
                        CircleRender.Draw(objPlayer.Position.Extend(objPlayer.Path[1], -700), objPlayer.BoundingRadius, Color.OrangeRed);
                        CircleRender.Draw(objPlayer.Position.Extend(objPlayer.Path[1], -700), MenuManager.ComboMenu["RSafetyRange"].GetValue<MenuSlider>().Value, Color.OrangeRed);
                    }
                }
            }
            public static void OnGapcloser(AIHeroClient sender, AntiGapcloser.GapcloserArgs args)
            {
                if (sender.IsMe || sender.IsAlly || objPlayer.IsDead || !sender.IsMelee)
                    return;

                if (MenuManager.MiscGapcloserMenu["R"].GetValue<MenuBool>().Enabled && SpellManager.R.IsReady() && args.Type == AntiGapcloser.GapcloserType.SkillShot)
                {
                    if (args.EndPosition.DistanceToPlayer() <= 250)
                    {
                        SpellManager.R.Cast(args.StartPosition);
                    }
                }
            }
        }
        public class Modes
        {
            public static void Combo()
            {
                if (MenuManager.ComboMenu["W"].GetValue<MenuBool>().Enabled && SpellManager.W.IsReady())
                {
                    var target = TargetSelector.GetTarget(SpellManager.W.Range,DamageType.Magical);

                    var enemies = TargetSelector.GetTargets(SpellManager.W.Range,DamageType.Magical);

                    switch (MenuManager.ComboMenu["WSelectBy"].GetValue<MenuList>().SelectedValue)
                    {
                        case "Most AD":
                            target = enemies.MinBy(x => x.TotalAttackDamage);
                            break;
                        case "Most AP":
                            target = enemies.MinBy(x => x.TotalMagicalDamage);
                            break;
                        case "Lowest Health":
                            target = enemies.MinBy(x => x.Health);
                            break;
                        case "Most Priority":
                            target = enemies.MinBy(x => TargetSelector.GetPriority(x) == (int)TargetPriority.Max);
                            break;
                    }

                    if (TargetSelector.SelectedTarget != null)
                    {
                        target = TargetSelector.SelectedTarget;
                    }

                    if (target != null && !Misc.IsAllured(target) && target.IsValidTarget(SpellManager.W.Range))
                    {
                        SpellManager.W.CastOnUnit(target);
                    }
                }
                if (MenuManager.ComboMenu["R"].GetValue<MenuBool>().Enabled && SpellManager.R.IsReady())
                {
                    var target = TargetSelector.GetTarget(SpellManager.R.Range,DamageType.Magical);

                    if (target != null && target.IsValidTarget(SpellManager.R.Range))
                    {
                        if (MenuManager.ComboRWhiteList[target.CharacterName.ToLower()].GetValue<MenuBool>().Enabled && SpellManager.R.GetDamage(target, target.HealthPercent < 30 ? 1 : 0) >= UtilityManager.GetTargetHealthWithShield(target))
                        {
                            var getPrediction = SpellManager.R.GetPrediction(target, MenuManager.ComboMenu["RAoE"].GetValue<MenuSliderButton>().Enabled);

                            if (MenuManager.ComboMenu["RAoE"].GetValue<MenuSliderButton>().Enabled && getPrediction.AoeTargetsHit.Count() >= MenuManager.ComboMenu["RAoE"].GetValue<MenuSliderButton>().Value && getPrediction.Hitchance >= HitChance.High)
                            {
                                SpellManager.R.Cast(getPrediction.CastPosition);
                            }
                            else if (getPrediction.Hitchance >= HitChance.High)
                            {
                                SpellManager.R.Cast(getPrediction.CastPosition);
                            }
                        }
                    }
                }
                if (MenuManager.ComboMenu["Q"].GetValue<MenuBool>().Enabled && SpellManager.Q.IsReady())
                {
                    var target = TargetSelector.GetTarget(Misc.RealQRange(),DamageType.Magical);

                    if (target != null && target.IsValidTarget(Misc.RealQRange()))
                    {
                        if (Misc.IsAllured(target))
                        {
                            if (!Misc.IsFullyAllured(target) || MenuManager.ComboMenu["QOnlyIfFullyAllured"].GetValue<MenuBool>().Enabled)
                                return;

                            if (Misc.IsQSkillshot())
                            {
                                var getPrediction = SpellManager.Q.GetPrediction(target);

                                if (getPrediction.Hitchance >= HitChance.High)
                                {
                                    SpellManager.Q.Cast(getPrediction.CastPosition);
                                }
                            }
                            else
                            {
                                SpellManager.Q.CastOnUnit(target);
                            }
                        }
                        else
                        {
                            if (Misc.IsQSkillshot())
                            {
                                var getPrediction = SpellManager.Q.GetPrediction(target);

                                if (getPrediction.Hitchance >= HitChance.High)
                                {
                                    SpellManager.Q.Cast(getPrediction.CastPosition);
                                }
                            }
                            else
                            {
                                SpellManager.Q.CastOnUnit(target);
                            }
                        }
                    }
                }
                if (MenuManager.ComboMenu["E"].GetValue<MenuBool>().Enabled && SpellManager.E.IsReady())
                {
                    var target = TargetSelector.GetTargets(SpellManager.E.Range,DamageType.Magical).OrderByDescending(x => Misc.IsFullyAllured(x));

                    if (target != null && target.FirstOrDefault().IsValidTarget(SpellManager.W.Range))
                    {
                        var firstTarget = target.FirstOrDefault();

                        if (Misc.IsAllured(firstTarget))
                        {
                            if (!Misc.IsFullyAllured(firstTarget) || MenuManager.ComboMenu["EOnlyIfFullyAllured"].GetValue<MenuBool>().Enabled)
                                return;

                            SpellManager.E.CastOnUnit(firstTarget);
                        }
                        else
                        {
                            SpellManager.E.CastOnUnit(firstTarget);
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
                    var target = TargetSelector.GetTarget(Misc.RealQRange(),DamageType.Magical);

                    if (target != null && MenuManager.HarassMenu[target.CharacterName.ToLower()].GetValue<MenuBool>().Enabled)
                    {
                        if (Misc.IsQSkillshot())
                        {
                            var getPrediction = SpellManager.Q.GetPrediction(target, false);

                            if (getPrediction.Hitchance >= HitChance.High && target.IsValidTarget(Misc.RealQRange()))
                            {
                                SpellManager.Q.Cast(getPrediction.CastPosition);
                            }
                        }
                        else
                        {
                            if (target.IsValidTarget(Misc.RealQRange()))
                            {
                                SpellManager.Q.CastOnUnit(target);
                            }
                        }
                    }
                }
                if (MenuManager.HarassMenu["E"].GetValue<MenuBool>().Enabled && SpellManager.E.IsReady())
                {
                    var target = TargetSelector.GetTarget(SpellManager.E.Range,DamageType.Magical);

                    if (target != null && MenuManager.HarassMenu[target.CharacterName.ToLower()].GetValue<MenuBool>().Enabled && target.IsValidTarget(SpellManager.E.Range))
                    {
                        SpellManager.E.CastOnUnit(target);
                    }
                }
            }
            public static void LaneClear()
            {
                if (ManaManager.HaveNoEnoughMana(MenuManager.LaneClearMenu["MinMana"].GetValue<MenuSlider>()))
                    return;

                if (MenuManager.LaneClearMenu["Q"].GetValue<MenuBool>().Enabled && SpellManager.Q.IsReady())
                {
                    var getMinions = GameObjects.EnemyMinions.Where(x => x.IsValidTarget(Misc.RealQRange()) && x.IsMinion()).Cast<AIBaseClient>().OrderByDescending(x => x.Health);

                    if (getMinions.Count() == 0)
                        return;

                    var firstMinion = getMinions.FirstOrDefault();

                    if (getMinions.Count() >= MenuManager.LaneClearMenu["QHits"].GetValue<MenuSlider>().Value)
                    {
                        if (Misc.IsQSkillshot())
                        {
                            var getPrediction = SpellManager.Q.GetPrediction(firstMinion, false);

                            if (getPrediction.Hitchance >= HitChance.High && getPrediction != null)
                            {
                                SpellManager.Q.Cast(getPrediction.CastPosition);
                            }
                        }
                        else
                        {
                            SpellManager.Q.CastOnUnit(firstMinion);
                        }
                    }
                    else if (!Misc.IsQSkillshot())
                    {
                        SpellManager.Q.CastOnUnit(firstMinion);
                    }
                }
                if (MenuManager.LaneClearMenu["E"].GetValue<MenuBool>().Enabled && SpellManager.E.IsReady())
                {
                    var getMinions = GameObjects.EnemyMinions.Where(x => x.IsValidTarget(SpellManager.E.Range) && x.IsMinion()).OrderByDescending(x => x.MaxHealth).Cast<AIBaseClient>();

                    if (getMinions.Count() == 0)
                        return;

                    SpellManager.E.CastOnUnit(getMinions.MinBy(x => x.MaxHealth));
                }
            }
            public static void JungleClear()
            {
                if (ManaManager.HaveNoEnoughMana(MenuManager.JungleClearMenu["MinMana"].GetValue<MenuSlider>()))
                    return;

                if (MenuManager.JungleClearMenu["W"].GetValue<MenuBool>().Enabled && SpellManager.W.IsReady())
                {
                    var getMinions = GameObjects.Jungle.Where(x => x.IsValidTarget(SpellManager.W.Range) && x.IsJungle() && !Misc.IsAllured(x)).OrderByDescending(x => x.Health).Cast<AIBaseClient>();

                    if (getMinions.Count() == 0)
                        return;

                    var firstMinion = getMinions.FirstOrDefault();

                    if (UtilityManager.JungleList.Contains(firstMinion.SkinName) && MenuManager.JungleWhiteList[firstMinion.SkinName].GetValue<MenuBool>().Enabled && firstMinion.Health > SpellManager.Q.GetDamage(firstMinion))
                    {
                        SpellManager.W.CastOnUnit(firstMinion);
                    }
                }
                if (MenuManager.JungleClearMenu["E"].GetValue<MenuBool>().Enabled && SpellManager.E.IsReady())
                {
                    var getMinions = GameObjects.Jungle.Where(x => x.IsValidTarget(SpellManager.E.Range) && x.IsJungle()).OrderBy(x => Misc.IsFullyAllured(x)).Cast<AIBaseClient>();

                    if (getMinions.Count() == 0)
                        return;

                    var firstMinion = getMinions.FirstOrDefault();

                    if (Misc.IsAllured(firstMinion) && !Misc.IsFullyAllured(firstMinion))
                        return;

                    SpellManager.E.CastOnUnit(firstMinion);
                }
                if (MenuManager.JungleClearMenu["Q"].GetValue<MenuBool>().Enabled && SpellManager.Q.IsReady())
                {
                    var getMinions = GameObjects.Jungle.Where(x => x.IsValidTarget(Misc.RealQRange()) && x.IsJungle()).OrderBy(x => x.Health).Cast<AIBaseClient>();

                    if (getMinions.Count() == 0)
                        return;

                    var firstMinion = getMinions.FirstOrDefault();

                    if (Misc.IsAllured(firstMinion) && !Misc.IsFullyAllured(firstMinion))
                        return;

                    if (Misc.IsQSkillshot())
                    {
                        var getPrediction = SpellManager.Q.GetPrediction(firstMinion, false);

                        if (getPrediction.Hitchance >= HitChance.Medium)
                        {
                            SpellManager.Q.Cast(getPrediction.CastPosition);
                        }
                    }
                    else
                    {
                        SpellManager.Q2.CastOnUnit(firstMinion);
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

                if (MenuManager.MiscKillSteal["E"].GetValue<MenuBool>().Enabled && SpellManager.E.IsReady())
                {
                    foreach (var target in  GameObjects.EnemyHeroes.Where(x => x.IsValidTarget(SpellManager.E.Range) && SpellManager.E.GetDamage(x, Misc.IsWEmpowered() ? 1:0) >= UtilityManager.GetTargetHealthWithShield(x)))
                    {
                        SpellManager.E.CastOnUnit(target);
                        break;
                    }
                }
                if (MenuManager.MiscKillSteal["R"].GetValue<MenuBool>().Enabled && SpellManager.R.IsReady())
                {
                    foreach (var target in GameObjects.EnemyHeroes.Where(x => x.IsValidTarget(SpellManager.R.Range) && SpellManager.R.GetDamage(x, x.HealthPercent < 30 ? 1:0) >= UtilityManager.GetTargetHealthWithShield(x)))
                    {
                        var getPrediction = SpellManager.R.GetPrediction(target, true);

                        if (getPrediction.Hitchance >= HitChance.High)
                        {
                            SpellManager.R.Cast(getPrediction.CastPosition);
                            break;
                        }
                    }
                }
            }
            public static void SemiManualR()
            {
                objPlayer.IssueOrder(GameObjectOrder.MoveTo, Game.CursorPos);

                foreach (var target in GameObjects.EnemyHeroes.Where(x => x.IsValidTarget(SpellManager.R.Range) && x.Health <= SpellManager.R.GetDamage(x, x.HealthPercent < 30 ? 1:0)))
                {
                    var getPrediction = SpellManager.R.GetPrediction(target);

                    if (getPrediction.Hitchance >= HitChance.High)
                    {
                        SpellManager.R.Cast(getPrediction.CastPosition);
                    }
                }
            }
            public static bool IsAllured(AIBaseClient target)
            {
                return target.HasBuff("EvelynnW");
            }
            public static bool IsFullyAllured(AIBaseClient target)
            {
                if (!target.HasBuff("EvelynnW"))
                    return false;

                var normalObjects = ObjectManager.Get<GameObject>().Where(x => x.IsValid && x.Name == "Evelynn_Base_W_Fizz_Mark_Decay");

                return normalObjects.Any(x => ObjectManager.Get<AIBaseClient>().Where(c => c.Team != x.Team).MinBy(c => c.Distance(x)) == target);
            }
            public static float RealQRange()
            {
                return IsQSkillshot() ? SpellManager.Q.Range : SpellManager.Q2.Range;
            }
            public static bool IsWEmpowered()
            {
                return objPlayer.GetSpell(SpellSlot.E).SData.Name == "EvelynnE2";
            }
            public static bool IsQSkillshot()
            {
                return objPlayer.GetSpell(SpellSlot.Q).SData.Name == "EvelynnQ";
            }
            public static Geometry.Sector GetSector()
            {
                var pos = objPlayer.Path[1];
                var dir = (pos - objPlayer.Position).Normalized();
                var spot = pos + dir * SpellManager.R.Range;

                return new Geometry.Sector((Vector2)pos, (Vector2)spot, SpellManager.R.Width, SpellManager.R.Range);
            }
            public static Geometry.Sector GetSectorByVector3()
            {
                var pos = objPlayer.Path[1];
                var dir = (pos - objPlayer.Position).Normalized();
                var spot = pos + dir * SpellManager.R.Range;

                return new Geometry.Sector(pos, spot, SpellManager.Q2.Width, SpellManager.R.Range);
            }
        }
    }
}
