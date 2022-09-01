using System;
using System.Linq;
using EnsoulSharp;
using EnsoulSharp.SDK;
using EnsoulSharp.SDK.MenuUI;
using SharpShooter.MyBase;
using SharpShooter.MyCommon;

namespace SharpShooter.MyPlugin
{
    public class Ezreal : MyLogic
    {
        public Ezreal()
        {
            Initializer();
        }

        private static void Initializer()
        {
            Q = new Spell(SpellSlot.Q, 1150f);
            Q.SetSkillshot(0.25f, 60f, 2000f, true,SpellType.Line);

            W = new Spell(SpellSlot.W, 950f);
            W.SetSkillshot(0.25f, 60f, 1200f, false,SpellType.Line);

            E = new Spell(SpellSlot.E, 475f) { Delay = 0.65f };

            R = new Spell(SpellSlot.R, 5000f);
            R.SetSkillshot(1.05f, 160f, 2200f, false,SpellType.Line);

            EQ = new Spell(SpellSlot.Q, 1625f);
            EQ.SetSkillshot(0.90f, 60f, 1350f, true,SpellType.Line);

            MyMenuExtensions.ComboOption.AddMenu();
            MyMenuExtensions.ComboOption.AddQ();
            MyMenuExtensions.ComboOption.AddW();
            MyMenuExtensions.ComboOption.AddE();
            MyMenuExtensions.ComboOption.AddBool("ComboECheck", "Use E |Safe Check");
            MyMenuExtensions.ComboOption.AddBool("ComboEWall", "Use E |Wall Check");
            MyMenuExtensions.ComboOption.AddR();

            MyMenuExtensions.HarassOption.AddMenu();
            MyMenuExtensions.HarassOption.AddQ();
            MyMenuExtensions.HarassOption.AddW();
            MyMenuExtensions.HarassOption.AddMana();
            MyMenuExtensions.HarassOption.AddTargetList();

            MyMenuExtensions.LaneClearOption.AddMenu();
            MyMenuExtensions.LaneClearOption.AddQ();
            MyMenuExtensions.LaneClearOption.AddBool("LaneClearQLH", "Use Q| Only LastHit", false);
            MyMenuExtensions.LaneClearOption.AddW();
            MyMenuExtensions.LaneClearOption.AddMana();

            MyMenuExtensions.JungleClearOption.AddMenu();
            MyMenuExtensions.JungleClearOption.AddQ();
            MyMenuExtensions.JungleClearOption.AddW();
            MyMenuExtensions.JungleClearOption.AddMana();

            MyMenuExtensions.LastHitOption.AddMenu();
            MyMenuExtensions.LastHitOption.AddQ();
            MyMenuExtensions.LastHitOption.AddMana();

            MyMenuExtensions.KillStealOption.AddMenu();
            MyMenuExtensions.KillStealOption.AddQ();

            //GapcloserOption.AddMenu();

            MyMenuExtensions.MiscOption.AddMenu();
            MyMenuExtensions.MiscOption.AddBasic();
            MyMenuExtensions.MiscOption.AddR();
            MyMenuExtensions.MiscOption.AddBool("R", "AutoR", "Auto R");
            MyMenuExtensions.MiscOption.AddSlider("R", "RRange", "Auto R |Min Cast Range >= x", 800, 0, 1500);
            MyMenuExtensions.MiscOption.AddSlider("R", "RMaxRange", "Auto R |Max Cast Range >= x", 3000, 1500, 5000);
            MyMenuExtensions.MiscOption.AddKey("R", "SemiR", "Semi-manual R Key", Keys.T, KeyBindType.Press);

            MyMenuExtensions.DrawOption.AddMenu();
            MyMenuExtensions.DrawOption.AddQ(Q);
            MyMenuExtensions.DrawOption.AddW(W);
            MyMenuExtensions.DrawOption.AddE(E);
            MyMenuExtensions.DrawOption.AddDamageIndicatorToHero(true, true, true, true, true);

            Game.OnUpdate += OnUpdate;
            //Gapcloser.OnGapcloser += OnGapcloser;
            Orbwalker.OnBeforeAttack += OnAction;
            Orbwalker.OnAfterAttack += OnAfterAttack;
        }

        private static void OnAfterAttack(object sender, AfterAttackEventArgs Args)
        {
            if (Args.Target == null || Args.Target.IsDead || !Args.Target.IsValidTarget() || Args.Target.Health <= 0)
            {
                return;
            }

            switch (Args.Target.Type)
            {
                case GameObjectType.AIHeroClient:
                    {
                        var target = (AIHeroClient)Args.Target;
                        if (target != null && target.IsValidTarget())
                        {
                            if (Orbwalker.ActiveMode == OrbwalkerMode.Combo)
                            {
                                if (MyMenuExtensions.ComboOption.UseQ && Q.IsReady() && target.IsValidTarget(Q.Range))
                                {
                                    var qPred = Q.GetPrediction(target);
                                    if (qPred.Hitchance >= HitChance.High)
                                    {
                                        Q.Cast(qPred.CastPosition);
                                    }
                                }
                            }
                            else if (Orbwalker.ActiveMode == OrbwalkerMode.Harass ||
                                     Orbwalker.ActiveMode == OrbwalkerMode.LaneClear && MyManaManager.SpellHarass)
                            {
                                if (!MyMenuExtensions.HarassOption.HasEnouguMana() ||
                                    !MyMenuExtensions.HarassOption.GetHarassTargetEnabled(target.CharacterName))
                                {
                                    return;
                                }

                                if (MyMenuExtensions.HarassOption.UseQ && Q.IsReady() && target.IsValidTarget(Q.Range))
                                {
                                    var qPred = Q.GetPrediction(target);
                                    if (qPred.Hitchance >= HitChance.High)
                                    {
                                        Q.Cast(qPred.CastPosition);
                                    }
                                }
                            }
                        }
                    }
                    break;
            }
        }

        private static void OnUpdate(EventArgs args)
        {
            if (Me.IsDead || Me.IsRecalling())
            {
                return;
            }

            if (R.Level > 0)
            {
                R.Range = MyMenuExtensions.MiscOption.GetSlider("R", "RMaxRange").Value;
            }

            if (MyMenuExtensions.MiscOption.GetKey("R", "SemiR").Active)
            {
                OneKeyCastR();
            }

            if (MyMenuExtensions.MiscOption.GetBool("R", "AutoR").Enabled && R.IsReady() && Me.CountEnemyHeroesInRange(1000) == 0)
            {
                AutoRLogic();
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
                case OrbwalkerMode.LastHit:
                    LastHit();
                    break;
            }
        }

        private static void OneKeyCastR()
        {
            Me.IssueOrder(GameObjectOrder.MoveTo, Game.CursorPos);

            if (!R.IsReady())
            {
                return;
            }

            var target = MyTargetSelector.GetTarget(R.Range);
            if (target.IsValidTarget(R.Range) && !target.IsValidTarget(MyMenuExtensions.MiscOption.GetSlider("R", "RRange").Value))
            {
                var rPred = R.GetPrediction(target);

                if (rPred.Hitchance >= HitChance.High)
                {
                    R.Cast(rPred.CastPosition);
                }
            }
        }

        private static void AutoRLogic()
        {
            foreach (
                var target in
                GameObjects.EnemyHeroes.Where(
                    x =>
                        x.IsValidTarget(R.Range) && x.DistanceToPlayer() >= MyMenuExtensions.MiscOption.GetSlider("R", "RRange").Value))
            {
                if (!target.CanMoveMent() && target.IsValidTarget(EQ.Range) &&
                    Me.GetSpellDamage(target, SpellSlot.R) + Me.GetSpellDamage(target, SpellSlot.Q) * 3 >=
                    target.Health + target.HPRegenRate * 2)
                {
                    R.Cast(target);
                }

                if (Me.GetSpellDamage(target, SpellSlot.R) > target.Health + target.HPRegenRate * 2 &&
                    target.Path.Length < 2 &&
                    R.GetPrediction(target).Hitchance >= HitChance.High)
                {
                    R.Cast(target);
                }
            }
        }

        private static void KillSteal()
        {
            foreach (var target in GameObjects.EnemyHeroes.Where(x => x.IsValidTarget(Q.Range)))
            {
                if (MyMenuExtensions.KillStealOption.UseQ && Me.GetSpellDamage(target, SpellSlot.Q) > target.Health &&
                    target.IsValidTarget(Q.Range))
                {
                    if (!target.IsUnKillable())
                    {
                        var qPred = Q.GetPrediction(target);

                        if (qPred.Hitchance >= HitChance.High)
                        {
                            Q.Cast(qPred.CastPosition);
                        }
                    }
                }
            }
        }

        private static void Combo()
        {
            var target = MyTargetSelector.GetTarget(EQ.Range);

            if (target.IsValidTarget(EQ.Range))
            {
                if (MyMenuExtensions.ComboOption.UseE && E.IsReady() && target.IsValidTarget(EQ.Range))
                {
                    ComboELogic(target);
                }

                if (MyMenuExtensions.ComboOption.UseW && W.IsReady() && target.IsValidTarget(W.Range))
                {
                    var wPred = W.GetPrediction(target);

                    if (wPred.Hitchance >= HitChance.High)
                    {
                        if (Q.IsReady())
                        {
                            var qPred = Q.GetPrediction(target);

                            if (qPred.Hitchance >= HitChance.High)
                            {
                                W.Cast(qPred.CastPosition);
                            }
                        }

                        if (Orbwalker.CanAttack() && target.InAutoAttackRange())
                        {
                            W.Cast(wPred.CastPosition);
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

                if (MyMenuExtensions.ComboOption.UseR && R.IsReady())
                {
                    if (Me.IsUnderEnemyTurret() || Me.CountEnemyHeroesInRange(800) > 1)
                    {
                        return;
                    }

                    foreach (var rTarget in GameObjects.EnemyHeroes.Where(
                        x =>
                            x.IsValidTarget(R.Range) &&
                            x.DistanceToPlayer() >= MyMenuExtensions.MiscOption.GetSlider("R", "RRange").Value))
                    {
                        if (rTarget.Health < Me.GetSpellDamage(rTarget, SpellSlot.R) &&
                            R.GetPrediction(rTarget).Hitchance >= HitChance.High &&
                            rTarget.DistanceToPlayer() > Q.Range + E.Range / 2)
                        {
                            R.Cast(target);
                        }

                        if (rTarget.IsValidTarget(Q.Range + E.Range) &&
                            Me.GetSpellDamage(rTarget, SpellSlot.R) +
                            (Q.IsReady() ? Me.GetSpellDamage(rTarget, SpellSlot.Q) : 0) +
                            (W.IsReady() ? Me.GetSpellDamage(rTarget, SpellSlot.W) : 0) >
                            rTarget.Health + rTarget.HPRegenRate * 2)
                        {
                            R.Cast(rTarget);
                        }
                    }
                }
            }
        }

        private static void ComboELogic(AIHeroClient target)
        {
            if (target == null || !target.IsValidTarget())
            {
                return;
            }

            if (MyMenuExtensions.ComboOption.GetBool("ComboECheck").Enabled && !Me.IsUnderEnemyTurret() && Me.CountEnemyHeroesInRange(1200f) <= 2)
            {
                if (target.DistanceToPlayer() > Me.GetRealAutoAttackRange(target) && target.IsValidTarget())
                {
                    if (target.Health < Me.GetSpellDamage(target, SpellSlot.E) + Me.GetAutoAttackDamage(target) &&
                        target.PreviousPosition.Distance(Game.CursorPos) < Me.PreviousPosition.Distance(Game.CursorPos))
                    {
                        var CastEPos = Me.PreviousPosition.Extend(target.PreviousPosition, 475f);

                        if (MyMenuExtensions.ComboOption.GetBool("ComboEWall").Enabled)
                        {
                            if (!CastEPos.IsWall())
                            {
                                E.Cast(Me.PreviousPosition.Extend(target.PreviousPosition, 475f));
                            }
                        }
                        else
                        {
                            E.Cast(Me.PreviousPosition.Extend(target.PreviousPosition, 475f));
                        }
                        return;
                    }

                    if (target.Health <
                        Me.GetSpellDamage(target, SpellSlot.E) + Me.GetSpellDamage(target, SpellSlot.W) &&
                        W.IsReady() &&
                        target.PreviousPosition.Distance(Game.CursorPos) + 350 < Me.PreviousPosition.Distance(Game.CursorPos))
                    {
                        var CastEPos = Me.PreviousPosition.Extend(target.PreviousPosition, 475f);

                        if (MyMenuExtensions.ComboOption.GetBool("ComboEWall").Enabled)
                        {
                            if (!CastEPos.IsWall())
                            {
                                E.Cast(Me.PreviousPosition.Extend(target.PreviousPosition, 475f));
                            }
                        }
                        else
                        {
                            E.Cast(Me.PreviousPosition.Extend(target.PreviousPosition, 475f));
                        }
                        return;
                    }

                    if (target.Health <
                        Me.GetSpellDamage(target, SpellSlot.E) + Me.GetSpellDamage(target, SpellSlot.Q) &&
                        Q.IsReady() &&
                        target.PreviousPosition.Distance(Game.CursorPos) + 300 < Me.PreviousPosition.Distance(Game.CursorPos))
                    {
                        var CastEPos = Me.PreviousPosition.Extend(target.PreviousPosition, 475f);

                        if (MyMenuExtensions.ComboOption.GetBool("ComboEWall").Enabled)
                        {
                            if (!CastEPos.IsWall())
                            {
                                E.Cast(Me.PreviousPosition.Extend(target.PreviousPosition, 475f));
                            }
                        }
                        else
                        {
                            E.Cast(Me.PreviousPosition.Extend(target.PreviousPosition, 475f));
                        }
                    }
                }
            }

        }

        private static void Harass()
        {
            if (MyMenuExtensions.HarassOption.HasEnouguMana())
            {
                if (MyMenuExtensions.HarassOption.UseQ && Q.IsReady())
                {
                    var target = MyMenuExtensions.HarassOption.GetTarget(Q.Range);
                    if (target != null && target.IsValidTarget(Q.Range))
                    {
                        var qPred = Q.GetPrediction(target);
                        if (qPred.Hitchance >= HitChance.High)
                        {
                            Q.Cast(qPred.CastPosition);
                        }
                    }
                }
            }
        }

        private static void FarmHarass()
        {
            if (Me.IsWindingUp)
            {
                return;
            }

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
                        foreach (var minion in minions.Where(x => !x.IsDead && x.Health > 0))
                        {
                            if (MyMenuExtensions.LaneClearOption.GetBool("LaneClearQLH").Enabled)
                            {
                                if (minion.Health < Me.GetSpellDamage(minion, SpellSlot.Q))
                                {
                                    Q.Cast(minion);
                                }
                            }
                            else
                            {
                                Q.Cast(minion);
                            }
                        }
                    }
                }
            }
        }

        private static void JungleClear()
        {
            if (MyMenuExtensions.JungleClearOption.HasEnouguMana())
            {
                if (MyMenuExtensions.JungleClearOption.UseW && W.IsReady())
                {
                    var mobs = GameObjects.Jungle.Where(x => x.IsValidTarget(Q.Range) && x.GetJungleType() == JungleType.Legendary)
                        .OrderByDescending(x => x.MaxHealth)
                        .ToList();
                    foreach (var mob in mobs)
                    {
                        W.CastIfHitchanceEquals(mob, HitChance.Medium);
                    }
                }

                if (MyMenuExtensions.JungleClearOption.UseQ && Q.IsReady())
                {
                    var mobs = GameObjects.Jungle.Where(x => x.IsValidTarget(Q.Range) && x.GetJungleType() != JungleType.Unknown)
                        .OrderByDescending(x => x.MaxHealth)
                        .ToList();
                    foreach (var mob in mobs)
                    {
                        Q.CastIfHitchanceEquals(mob, HitChance.Medium);
                    }
                }
            }
        }

        private static void LastHit()
        {
            if (MyMenuExtensions.LastHitOption.HasEnouguMana)
            {
                if (MyMenuExtensions.LastHitOption.UseQ && Q.IsReady())
                {
                    var minions =
                        GameObjects.EnemyMinions.Where(x => x.IsValidTarget(Q.Range) && x.IsMinion())
                            .Where(
                                x =>
                                    x.DistanceToPlayer() <= Q.Range &&
                                    x.DistanceToPlayer() > Me.GetRealAutoAttackRange(x) &&
                                    x.Health < Me.GetSpellDamage(x, SpellSlot.Q)).ToList();

                    if (minions.Any())
                    {
                        Q.CastIfHitchanceEquals(minions[0], HitChance.Medium);
                    }
                }
            }
        }

        //private static void OnGapcloser(AIHeroClient target, GapcloserArgs Args)
        //{
        //    if (E.IsReady() && target != null && target.IsValidTarget(E.Range))
        //    {
        //        switch (Args.Type)
        //        {
        //            case SpellType.Melee:
        //                {
        //                    if (target.IsValidTarget(target.AttackRange + target.BoundingRadius + 100))
        //                    {
        //                        E.Cast(Me.PreviousPosition.Extend(target.PreviousPosition, -E.Range));
        //                    }
        //                }
        //                break;
        //            case SpellType.Dash:
        //                {
        //                    if (Args.EndPosition.DistanceToPlayer() <= 250 ||
        //                        target.PreviousPosition.DistanceToPlayer() <= 300)
        //                    {
        //                        E.Cast(Me.PreviousPosition.Extend(target.PreviousPosition, -E.Range));
        //                    }
        //                }
        //                break;
        //            case SpellType.SkillShot:
        //            case SpellType.Targeted:
        //                {
        //                    if (target.PreviousPosition.DistanceToPlayer() <= 300)
        //                    {
        //                        E.Cast(Me.PreviousPosition.Extend(target.PreviousPosition, -E.Range));
        //                    }
        //                }
        //                break;
        //        }
        //    }
        //}


        private static void OnAction(object sender, BeforeAttackEventArgs Args)
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
                        var target = (AIHeroClient) Args.Target;
                        if (target != null && target.IsValidTarget(W.Range))
                        {
                            if (MyMenuExtensions.ComboOption.UseW && W.IsReady())
                            {
                                var pred = W.GetPrediction(target);
                                if (pred.Hitchance >= HitChance.High)
                                {
                                    W.Cast(pred.CastPosition);
                                }
                            }
                        }
                    }
                }
                    break;
                case GameObjectType.AITurretClient:
                case GameObjectType.HQClient:
                case GameObjectType.Barracks:
                case GameObjectType.BarracksDampenerClient:
                case GameObjectType.BuildingClient:
                {
                    if (Orbwalker.ActiveMode == OrbwalkerMode.LaneClear && MyManaManager.SpellFarm &&
                        MyMenuExtensions.LaneClearOption.HasEnouguMana(true))
                    {
                        if (MyMenuExtensions.LaneClearOption.UseW && W.IsReady())
                        {
                            W.Cast(Args.Target.Position);
                        }
                    }
                }
                    break;
            }
        }
    }
}