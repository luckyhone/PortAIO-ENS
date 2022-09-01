using System;
using System.Linq;
using EnsoulSharp;
using EnsoulSharp.SDK;
using EnsoulSharp.SDK.MenuUI;
using SharpShooter.MyBase;
using SharpShooter.MyCommon;

namespace SharpShooter.MyPlugin
{
    public class Corki : MyLogic
    {
        private static float rRange => Me.HasBuff("CorkiMissileBarrageCounterBig") ? 1500f : 1300f;

        public Corki()
        {
            Initializer();
        }

        private static void Initializer()
        {
            Q = new Spell(SpellSlot.Q, 825f);
            Q.SetSkillshot(0.30f, 200f, 1000f, false, SpellType.Circle);

            W = new Spell(SpellSlot.W, 800f);

            E = new Spell(SpellSlot.E, 600f);

            R = new Spell(SpellSlot.R, rRange);
            R.SetSkillshot(0.20f, 50f, 2000f, true, SpellType.Line);

            MyMenuExtensions.ComboOption.AddMenu();
            MyMenuExtensions.ComboOption.AddQ();
            MyMenuExtensions.ComboOption.AddE();
            MyMenuExtensions.ComboOption.AddR();
            MyMenuExtensions.ComboOption.AddSlider("ComboRLimit", "Use R|Limit Stack >= x", 0, 0, 7);
            MyMenuExtensions.ComboOption.AddSlider("ComboRHP", "Use R|Target HealthPercent <= x%", 100, 1, 101);

            MyMenuExtensions.HarassOption.AddMenu();
            MyMenuExtensions.HarassOption.AddQ();
            MyMenuExtensions.HarassOption.AddE();
            MyMenuExtensions.HarassOption.AddR();
            MyMenuExtensions.HarassOption.AddSlider("HarassRLimit", "Use R|Limit Stack >= x", 4, 0, 7);
            MyMenuExtensions.HarassOption.AddMana();
            MyMenuExtensions.HarassOption.AddTargetList();

            MyMenuExtensions.LaneClearOption.AddMenu();
            MyMenuExtensions.LaneClearOption.AddQ();
            MyMenuExtensions.LaneClearOption.AddSlider("LaneClearQCount", "Use Q|Min Hit Count >= x", 3, 1, 5);
            MyMenuExtensions.LaneClearOption.AddE();
            MyMenuExtensions.LaneClearOption.AddSlider("LaneClearECount", "Use E|Min Hit Count >= x", 3, 1, 5);
            MyMenuExtensions.LaneClearOption.AddR();
            MyMenuExtensions.LaneClearOption.AddSlider("LaneClearRCount", "Use R|Min Hit Count >= x", 3, 1, 5);
            MyMenuExtensions.LaneClearOption.AddSlider("LaneClearRLimit", "Use R|Limit Stack >= x", 4, 0, 7);
            MyMenuExtensions.LaneClearOption.AddMana();

            MyMenuExtensions.JungleClearOption.AddMenu();
            MyMenuExtensions.JungleClearOption.AddQ();
            MyMenuExtensions.JungleClearOption.AddE();
            MyMenuExtensions.JungleClearOption.AddR();
            MyMenuExtensions.JungleClearOption.AddSlider("JungleClearRLimit", "Use R|Limit Stack >= x", 0, 0, 7);
            MyMenuExtensions.JungleClearOption.AddMana();

            MyMenuExtensions.KillStealOption.AddMenu();
            MyMenuExtensions.KillStealOption.AddQ();
            MyMenuExtensions.KillStealOption.AddR();

            MyMenuExtensions.MiscOption.AddMenu();
            MyMenuExtensions.MiscOption.AddBasic();
            MyMenuExtensions.MiscOption.AddR();
            MyMenuExtensions.MiscOption.AddKey("R", "SemiR", "Semi-manual R Key", Keys.T, KeyBindType.Press);

            MyMenuExtensions.DrawOption.AddMenu();
            MyMenuExtensions.DrawOption.AddQ(Q);
            MyMenuExtensions.DrawOption.AddW(W);
            MyMenuExtensions.DrawOption.AddE(E);
            MyMenuExtensions.DrawOption.AddR(R);
            MyMenuExtensions.DrawOption.AddDamageIndicatorToHero(true, false, true, true, true);

            Game.OnUpdate += OnUpdate;
            Orbwalker.OnAfterAttack += OnAction;
        }

        private static void OnUpdate(EventArgs args)
        {
            if (Me.IsDead || Me.IsRecalling())
            {
                return;
            }

            if (Me.IsWindingUp)
            {
                return;
            }

            if (R.Level > 0)
            {
                R.Range = rRange;
            }

            if (MyMenuExtensions.MiscOption.GetKey("R", "SemiR").Active)
            {
                SemiRLogic();
            }

            KillSteal();

            switch (Orbwalker.ActiveMode)
            {
                case OrbwalkerMode.Combo:
                    Combo();
                    break;
                case OrbwalkerMode.Harass:
                    Harass();
                    break;
                case OrbwalkerMode.LaneClear:
                    FarmHarass();
                    break;
            }
        }

        private static void SemiRLogic()
        {
            Me.IssueOrder(GameObjectOrder.MoveTo, Game.CursorPos);

            if (R.IsReady() && R.Ammo > 0)
            {
                var target = MyTargetSelector.GetTarget(R.Range);

                if (target.IsValidTarget(R.Range))
                {
                    var rPred = R.GetPrediction(target);

                    if (rPred.Hitchance >= HitChance.High)
                    {
                        R.Cast(rPred.CastPosition);
                    }
                }
            }
        }

        private static void KillSteal()
        {
            if (MyMenuExtensions.KillStealOption.UseQ && Q.IsReady())
            {
                foreach (
                    var target in
                    GameObjects.EnemyHeroes.Where(
                        x => x.IsValidTarget(Q.Range) && x.Health < Me.GetSpellDamage(x, SpellSlot.Q)))
                {
                    if (target.IsValidTarget(Q.Range) && !target.IsUnKillable())
                    {
                        var qPred = Q.GetPrediction(target);

                        if (qPred.Hitchance >= HitChance.High)
                        {
                            Q.Cast(qPred.CastPosition);
                        }
                    }
                }
            }

            if (MyMenuExtensions.KillStealOption.UseR && R.IsReady())
            {
                foreach (
                    var target in
                    GameObjects.EnemyHeroes.Where(
                        x => x.IsValidTarget(R.Range) && x.Health < Me.GetSpellDamage(x, SpellSlot.R)))
                {
                    if (target.IsValidTarget(R.Range) && !target.IsUnKillable())
                    {
                        var rPred = R.GetPrediction(target);

                        if (rPred.Hitchance >= HitChance.High)
                        {
                            R.Cast(rPred.UnitPosition);
                        }
                    }
                }
            }
        }

        private static void Combo()
        {
            var target = MyTargetSelector.GetTarget(R.Range);

            if (target.IsValidTarget(R.Range) && !target.IsUnKillable() && (!target.InAutoAttackRange() || !Orbwalker.CanAttack()))
            {
                if (MyMenuExtensions.ComboOption.UseR && R.IsReady() &&
                    R.Ammo >= MyMenuExtensions.ComboOption.GetSlider("ComboRLimit").Value &&
                    target.IsValidTarget(R.Range) && target.HealthPercent <= MyMenuExtensions.ComboOption.GetSlider("ComboRHP").Value)
                {
                    var rPred = R.GetPrediction(target);

                    if (rPred.Hitchance >= HitChance.High)
                    {
                        R.Cast(rPred.UnitPosition);
                    }
                    else if (rPred.Hitchance == HitChance.Collision)
                    {
                        foreach (var collsion in rPred.CollisionObjects.Where(x => x.IsValidTarget(R.Range)))
                        {
                            if (collsion.DistanceSquared(target) <= Math.Pow(85, 2))
                            {
                                R.Cast(collsion.PreviousPosition);
                            }
                        }
                    }
                }

                if (MyMenuExtensions.ComboOption.UseQ && Q.IsReady() && target.IsValidTarget(Q.Range))
                {
                    var qPred = Q.GetPrediction(target);

                    if (qPred.Hitchance >= HitChance.High)
                    {
                        Q.Cast(qPred.CastPosition);
                    }
                }

                if (MyMenuExtensions.ComboOption.UseE && E.IsReady() && target.IsValidTarget(E.Range))
                {
                    E.Cast(Me.PreviousPosition);
                }
            }
        }

        private static void Harass()
        {
            if (MyMenuExtensions.HarassOption.HasEnouguMana())
            {
                var target = MyMenuExtensions.HarassOption.GetTarget(R.Range);

                if (target.IsValidTarget(R.Range))
                {
                    if (MyMenuExtensions.HarassOption.UseR && R.IsReady() &&
                        R.Ammo >= MyMenuExtensions.HarassOption.GetSlider("HarassRLimit").Value &&
                        target.IsValidTarget(R.Range))
                    {
                        var rPred = R.GetPrediction(target);

                        if (rPred.Hitchance >= HitChance.High)
                        {
                            R.Cast(rPred.UnitPosition);
                        }
                    }

                    if (MyMenuExtensions.HarassOption.UseQ && Q.IsReady() && target.IsValidTarget(Q.Range))
                    {
                        var qPred = Q.GetPrediction(target);

                        if (qPred.Hitchance >= HitChance.High)
                        {
                            Q.Cast(qPred.CastPosition);
                        }
                    }

                    if (MyMenuExtensions.HarassOption.UseE && E.IsReady() && target.InAutoAttackRange())
                    {
                        E.Cast(Me.PreviousPosition);
                    }
                }
            }
        }

        private static void FarmHarass()
        {
            if (MyManaManager.SpellHarass)
            {
                Harass();
            }

            if (MyManaManager.SpellFarm)
            {
                LaneClear();
                JungleClear();
            }
        }

        private static void LaneClear()
        {
            if (MyMenuExtensions.LaneClearOption.HasEnouguMana())
            {
                if (MyMenuExtensions.LaneClearOption.UseQ && Q.IsReady())
                {
                    var minions = GameObjects.EnemyMinions.Where(x => x.IsValidTarget(Q.Range) && x.IsMinion()).ToList();
                    if (minions.Any())
                    {
                        var qFarm = Q.GetCircularFarmLocation(minions);

                        if (qFarm.MinionsHit >= MyMenuExtensions.LaneClearOption.GetSlider("LaneClearQCount").Value)
                        {
                            Q.Cast(qFarm.Position);
                        }
                    }
                }

                if (MyMenuExtensions.LaneClearOption.UseE && E.IsReady())
                {
                    var minions = GameObjects.EnemyMinions.Where(x => x.IsValidTarget(E.Range) && x.IsMinion()).ToList();

                    if (minions.Any() && minions.Count >= MyMenuExtensions.LaneClearOption.GetSlider("LaneClearECount").Value)
                    {
                        E.Cast(Me.PreviousPosition);
                    }
                }

                if (MyMenuExtensions.LaneClearOption.UseR && R.IsReady() &&
                    R.Ammo >= MyMenuExtensions.LaneClearOption.GetSlider("LaneClearRLimit").Value)
                {
                    var minions = GameObjects.EnemyMinions.Where(x => x.IsValidTarget(R.Range) && x.IsMinion()).ToList();

                    if (minions.Any())
                    {
                        var rFarm = R.GetLineFarmLocation(minions);

                        if (rFarm.MinionsHit >= MyMenuExtensions.LaneClearOption.GetSlider("LaneClearRCount").Value)
                        {
                            R.Cast(rFarm.Position);
                        }
                    }
                }
            }
        }

        private static void JungleClear()
        {
            if (MyMenuExtensions.JungleClearOption.HasEnouguMana())
            {
                var mobs =
                    GameObjects.Jungle.Where(x => x.IsValidTarget(R.Range) && x.GetJungleType() != JungleType.Unknown && !x.InAutoAttackRange())
                        .OrderByDescending(x => x.MaxHealth)
                        .ToList();

                if (mobs.Any())
                {
                    var mob = mobs.FirstOrDefault();

                    if (MyMenuExtensions.JungleClearOption.UseR && R.IsReady() &&
                        R.Ammo >= MyMenuExtensions.JungleClearOption.GetSlider("JungleClearRLimit").Value &&
                        mob.IsValidTarget(R.Range) && !mob.InAutoAttackRange())
                    {
                        R.CastIfHitchanceEquals(mob, HitChance.Medium);
                    }

                    if (MyMenuExtensions.JungleClearOption.UseQ && Q.IsReady() &&
                        mob.IsValidTarget(Q.Range) && !mob.InAutoAttackRange())
                    {
                        Q.CastIfHitchanceEquals(mob, HitChance.Medium);
                    }
                }
            }
        }

        private static void OnAction(object sender, AfterAttackEventArgs Args)
        {
            if (Args.Target == null || Args.Target.IsDead || !Args.Target.IsValidTarget() || Args.Target.Health <= 0)
            {
                return;
            }

            switch (Args.Target.Type)
            {
                case GameObjectType.AIHeroClient:
                    {
                        if (Orbwalker.ActiveMode == OrbwalkerMode.Combo)
                        {
                            var target = (AIHeroClient)Args.Target;
                            if (target != null && target.IsValidTarget() && !target.IsUnKillable())
                            {
                                if (MyMenuExtensions.ComboOption.UseR && R.IsReady() &&
                                    R.Ammo >= MyMenuExtensions.ComboOption.GetSlider("ComboRLimit").Value &&
                                    target.IsValidTarget(R.Range) &&
                                    target.HealthPercent <= MyMenuExtensions.ComboOption.GetSlider("ComboRHP").Value)
                                {
                                    var rPred = R.GetPrediction(target);

                                    if (rPred.Hitchance >= HitChance.High)
                                    {
                                        R.Cast(rPred.UnitPosition);
                                    }
                                }
                                else if (MyMenuExtensions.ComboOption.UseQ && Q.IsReady() && target.IsValidTarget(Q.Range))
                                {
                                    var qPred = Q.GetPrediction(target);

                                    if (qPred.Hitchance >= HitChance.High)
                                    {
                                        Q.Cast(qPred.CastPosition);
                                    }
                                }
                                else if (MyMenuExtensions.ComboOption.UseE && E.IsReady() && target.InAutoAttackRange())
                                {
                                    E.Cast(Me.PreviousPosition);
                                }
                            }
                        }
                    }
                    break;
                case GameObjectType.AIMinionClient:
                    {
                        if (Orbwalker.ActiveMode == OrbwalkerMode.LaneClear)
                        {
                            if (Args.Target is AIMinionClient)
                            {
                                var mob = (AIMinionClient)Args.Target;
                                if (mob != null && mob.IsValidTarget() && mob.GetJungleType() != JungleType.Unknown)
                                {
                                    if (MyMenuExtensions.JungleClearOption.HasEnouguMana())
                                    {
                                        if (MyMenuExtensions.JungleClearOption.UseR && R.IsReady() &&
                                            R.Ammo >=
                                            MyMenuExtensions.JungleClearOption.GetSlider("JungleClearRLimit").Value)
                                        {
                                            R.CastIfHitchanceEquals(mob,  HitChance.Medium);
                                        }
                                        else if (MyMenuExtensions.JungleClearOption.UseQ && Q.IsReady() && mob.IsValidTarget(Q.Range))
                                        {
                                            Q.CastIfHitchanceEquals(mob, HitChance.Medium);
                                        }
                                        else if (MyMenuExtensions.JungleClearOption.UseE && E.IsReady() && mob.InAutoAttackRange())
                                        {
                                            E.Cast(Me.PreviousPosition);
                                        }
                                    }
                                }
                            }
                        }
                    }
                    break;
            }
        }
    }
}