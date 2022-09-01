using System;
using System.Collections.Generic;
using System.Linq;
using EnsoulSharp;
using EnsoulSharp.SDK;
using EnsoulSharp.SDK.MenuUI;
using SharpDX;
using SharpShooter.MyBase;
using SharpShooter.MyCommon;

namespace SharpShooter.MyPlugin
{
    public class Caitlyn : MyLogic
    {
        private static int lastQTime, lastWTime;

        private static float rRange => 500f * Me.Spellbook.GetSpell(SpellSlot.R).Level + 1500f;

        public Caitlyn()
        {
            Initializer();
        }

        private static void Initializer()
        {
            Q = new Spell(SpellSlot.Q, 1250f);
            Q.SetSkillshot(0.70f, 50f, 2000f, false, SpellType.Line);

            W = new Spell(SpellSlot.W, 800f);
            W.SetSkillshot(0.80f, 80f, 2000f, false, SpellType.Circle);

            E = new Spell(SpellSlot.E, 750f);
            E.SetSkillshot(0.25f, 60f, 1600f, true, SpellType.Line);

            R = new Spell(SpellSlot.R, rRange) {Delay = 1.5f};

            MyMenuExtensions.ComboOption.AddMenu();
            MyMenuExtensions.ComboOption.AddQ();
            MyMenuExtensions.ComboOption.AddSlider("ComboQCount", "Use Q |Min Hit Count >= x(0 = Off)", 3, 0, 5);
            MyMenuExtensions.ComboOption.AddSlider("ComboQRange", "UseQ |Min Cast Range >= x", 800, 500, 1100);
            MyMenuExtensions.ComboOption.AddW();
            MyMenuExtensions.ComboOption.AddSlider("ComboWCount", "Use W|Min Stack >= x", 1, 1, 3);
            MyMenuExtensions.ComboOption.AddE();
            MyMenuExtensions.ComboOption.AddR();
            MyMenuExtensions.ComboOption.AddBool("ComboRSafe", "Use R|Safe Check");
            MyMenuExtensions.ComboOption.AddSlider("ComboRRange", "Use R|Min Cast Range >= x", 900, 500, 1500);

            MyMenuExtensions.HarassOption.AddMenu();
            MyMenuExtensions.HarassOption.AddQ();
            MyMenuExtensions.HarassOption.AddMana();
            MyMenuExtensions.HarassOption.AddTargetList();

            MyMenuExtensions.LaneClearOption.AddMenu();
            MyMenuExtensions.LaneClearOption.AddQ();
            MyMenuExtensions.LaneClearOption.AddSlider("LaneClearQCount", "Use Q|Min Hit Count >= x", 3, 1, 5);
            MyMenuExtensions.LaneClearOption.AddMana();

            MyMenuExtensions.JungleClearOption.AddMenu();
            MyMenuExtensions.JungleClearOption.AddQ();
            MyMenuExtensions.JungleClearOption.AddMana();

            MyMenuExtensions.KillStealOption.AddMenu();
            MyMenuExtensions.KillStealOption.AddQ();

            //GapcloserOption.AddMenu();

            MyMenuExtensions.MiscOption.AddMenu();
            MyMenuExtensions.MiscOption.AddBasic();
            MyMenuExtensions.MiscOption.AddQ();
            MyMenuExtensions.MiscOption.AddBool("Q", "AutoQ", "Use Q| CC");
            MyMenuExtensions.MiscOption.AddW();
            MyMenuExtensions.MiscOption.AddBool("W", "AutoWCC", "Use W| CC");
            MyMenuExtensions.MiscOption.AddBool("W", "AutoWTP", "Use W| TP");
            MyMenuExtensions.MiscOption.AddE();
            MyMenuExtensions.MiscOption.AddBool("E", "AutoE", "Use E| Anti Gapcloser");
            MyMenuExtensions.MiscOption.AddR();
            MyMenuExtensions.MiscOption.AddKey("R", "SemiR", "Semi-manual R Key", Keys.T, KeyBindType.Press);
            //MiscOption.AddSetting("EQ");
            //MiscOption.AddKey("EQKey", "Semi-manual EQ Key", KeyCode.G, KeyBindType.Press);

            MyMenuExtensions.DrawOption.AddMenu();
            MyMenuExtensions.DrawOption.AddQ(Q);
            MyMenuExtensions.DrawOption.AddW(W);
            MyMenuExtensions.DrawOption.AddE(E);
            MyMenuExtensions.DrawOption.AddR(R);
            MyMenuExtensions.DrawOption.AddDamageIndicatorToHero(true, false, true, true, true);

            Game.OnUpdate += OnUpdate;
            //Gapcloser.OnGapcloser += OnGapcloser;
            AIBaseClient.OnProcessSpellCast += OnProcessSpellCast;
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

            R.Range = rRange;

            //if (MiscOption.GetKey("EQKey").Enabled)
            //{
            //    OneKeyEQ();
            //}

            if (MyMenuExtensions.MiscOption.GetKey("R", "SemiR").Active && R.IsReady())
            {
                OneKeyCastR();
            }

            Auto();
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


        private static void OneKeyCastR()
        {
            var target = MyTargetSelector.GetTarget(R.Range);

            if (target != null && target.IsValidTarget(R.Range))
            {
                R.CastOnUnit(target);
            }
        }

        private static void Auto()
        {
            if (MyMenuExtensions.MiscOption.GetBool("Q", "AutoQ").Enabled && Q.IsReady() &&
                Orbwalker.ActiveMode != OrbwalkerMode.Combo && Orbwalker.ActiveMode != OrbwalkerMode.Harass)
            {
                var target = MyTargetSelector.GetTarget(Q.Range - 50);

                if (target.IsValidTarget(Q.Range) && !target.CanMoveMent())
                {
                    Q.Cast(target.PreviousPosition);
                }
            }

            if (W.IsReady())
            {
                if (MyMenuExtensions.MiscOption.GetBool("W", "AutoWCC").Enabled)
                {
                    foreach (
                        var target in
                        GameObjects.EnemyHeroes.Where(
                            x => x.IsValidTarget(W.Range) && !x.CanMoveMent() && !x.HasBuff("caitlynyordletrappublic")))
                    {
                        if (Variables.GameTimeTickCount - lastWTime > 1500)
                        {
                            W.Cast(target.PreviousPosition);
                        }
                    }
                }

                if (MyMenuExtensions.MiscOption.GetBool("W", "AutoWTP").Enabled)
                {
                    var obj =
                        ObjectManager
                            .Get<AIBaseClient>()
                            .FirstOrDefault(x => !x.IsAlly && !x.IsMe && x.DistanceToPlayer() <= W.Range &&
                                                 x.Buffs.Any(
                                                     a =>
                                                         a.Name.ToLower().Contains("teleport") ||
                                                         a.Name.ToLower().Contains("gate")) &&
                                                 !ObjectManager.Get<AIBaseClient>()
                                                     .Any(b => b.Name.ToLower().Contains("trap") && b.Distance(x) <= 150));

                    if (obj != null)
                    {
                        if (Variables.GameTimeTickCount - lastWTime > 1500)
                        {
                            W.Cast(obj.PreviousPosition);
                        }
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
                    if (target.InAutoAttackRange() && target.Health <= Me.GetAutoAttackDamage(target) * 2)
                    {
                        continue;
                    }

                    if (!target.IsUnKillable())
                    {
                        Q.Cast(target.PreviousPosition);
                    }
                }
            }
        }

        private static void Combo()
        {
            var target = MyTargetSelector.GetTarget(R.Range);

            if (target.IsValidTarget(R.Range) && !target.IsUnKillable())
            {
                if (MyMenuExtensions.ComboOption.UseE && E.IsReady() && target.IsValidTarget(700))
                {
                    var ePred = E.GetPrediction(target);

                    if (!ePred.CollisionObjects.Any() || ePred.Hitchance >= HitChance.High)
                    {
                        if (MyMenuExtensions.ComboOption.UseQ && Q.IsReady())
                        {
                            if (E.Cast(ePred.CastPosition))
                            {

                            }
                        }
                        else
                        {
                            E.Cast(ePred.CastPosition);
                        }
                    }
                    else
                    {
                        if (MyMenuExtensions.ComboOption.UseQ && Q.IsReady() && target.IsValidTarget(Q.Range) && !Me.IsDashing())
                        {
                            if (Me.CountEnemyHeroesInRange(MyMenuExtensions.ComboOption.GetSlider("ComboQRange").Value) < 0)
                            {
                                var qPred = Q.GetPrediction(target);

                                if (qPred.Hitchance >= HitChance.High)
                                {
                                    Q.Cast(qPred.CastPosition);
                                }

                                if (MyMenuExtensions.ComboOption.GetSlider("ComboQCount").Value != 0 &&
                                    Me.CountEnemyHeroesInRange(Q.Range) >= MyMenuExtensions.ComboOption.GetSlider("ComboQCount").Value)
                                {
                                    Q.CastIfWillHit(target, MyMenuExtensions.ComboOption.GetSlider("ComboQCount").Value);
                                }
                            }
                        }
                    }
                }

                if (MyMenuExtensions.ComboOption.UseQ && Q.IsReady() && !E.IsReady() && target.IsValidTarget(Q.Range) && !Me.IsDashing())
                {
                    if (Me.CountEnemyHeroesInRange(MyMenuExtensions.ComboOption.GetSlider("ComboQRange").Value) < 0)
                    {
                        var qPred = Q.GetPrediction(target);

                        if (qPred.Hitchance >= HitChance.High)
                        {
                            Q.Cast(qPred.CastPosition);
                        }

                        if (MyMenuExtensions.ComboOption.GetSlider("ComboQCount").Value != 0 &&
                            Me.CountEnemyHeroesInRange(Q.Range) >= MyMenuExtensions.ComboOption.GetSlider("ComboQCount").Value)
                        {
                            Q.CastIfWillHit(target, MyMenuExtensions.ComboOption.GetSlider("ComboQCount").Value);
                        }
                    }
                }

                if (MyMenuExtensions.ComboOption.UseW && W.IsReady() && target.IsValidTarget(W.Range) &&
                    W.Ammo >= MyMenuExtensions.ComboOption.GetSlider("ComboWCount").Value)
                {
                    if (Variables.GameTimeTickCount - lastWTime > 1800 + Game.Ping * 2)
                    {
                        if (target.CanMoveMent())
                        {
                            if (target.IsFacing(Me))
                            {
                                if (target.IsMelee && target.DistanceToPlayer() < target.AttackRange + 100)
                                {
                                    CastW(Me.PreviousPosition);
                                }
                                else
                                {
                                    var wPred = W.GetPrediction(target);

                                    if (wPred.Hitchance >= HitChance.High && target.IsValidTarget(W.Range))
                                    {
                                        CastW(wPred.CastPosition);
                                    }
                                }
                            }
                            else
                            {
                                var wPred = W.GetPrediction(target);

                                if (wPred.Hitchance >= HitChance.High && target.IsValidTarget(W.Range))
                                {
                                    CastW(wPred.CastPosition +
                                          Vector3.Normalize(target.PreviousPosition - Me.PreviousPosition) * 100);
                                }
                            }
                        }
                        else
                        {
                            if (target.IsValidTarget(W.Range))
                            {
                                CastW(target.PreviousPosition);
                            }
                        }
                    }
                }

                if (MyMenuExtensions.ComboOption.UseR && R.IsReady() && Variables.GameTimeTickCount - lastQTime > 2500)
                {
                    if (MyMenuExtensions.ComboOption.GetBool("ComboRSafe").Enabled &&
                        (Me.IsUnderEnemyTurret() || Me.CountEnemyHeroesInRange(1000) > 2))
                    {
                        return;
                    }

                    if (!target.IsValidTarget(R.Range))
                    {
                        return;
                    }

                    if (target.DistanceToPlayer() < MyMenuExtensions.ComboOption.GetSlider("ComboRRange").Value)
                    {
                        return;
                    }

                    if (target.Health + target.HPRegenRate * 3 > Me.GetSpellDamage(target, SpellSlot.R))
                    {
                        return;
                    }

                    var RCollision =
                        Collisions.GetCollision(new List<Vector3> {target.PreviousPosition},
                                new PredictionInput
                                {
                                    Delay = R.Delay,
                                    Radius = 500,
                                    Speed = 1500,
                                    From = ObjectManager.Player.PreviousPosition,
                                    Unit = target,
                                    CollisionObjects = new[] {CollisionObjects.YasuoWall | CollisionObjects.Heroes}
                                })
                            .Any(x => x.NetworkId != target.NetworkId);

                    if (RCollision)
                    {
                        return;
                    }

                    R.CastOnUnit(target);
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

                    if (target.IsValidTarget(Q.Range))
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
                    var minions =
                        GameObjects.EnemyMinions.Where(x => x.IsValidTarget(Q.Range) && x.IsMinion()).ToList();

                    if (minions.Any())
                    {
                        var qFarm = Q.GetLineFarmLocation(minions);

                        if (qFarm.MinionsHit >= MyMenuExtensions.LaneClearOption.GetSlider("LaneClearQCount").Value)
                        {
                            Q.Cast(qFarm.Position);
                        }
                    }
                }
            }
        }

        private static void JungleClear()
        {
            if (MyMenuExtensions.JungleClearOption.HasEnouguMana())
            {
                if (MyMenuExtensions.JungleClearOption.UseQ && Q.IsReady())
                {
                    var mobs =
                        GameObjects.Jungle.Where(x => x.IsValidTarget(Q.Range) && x.GetJungleType() != JungleType.Unknown)
                            .OrderByDescending(x => x.MaxHealth)
                            .ToList();

                    if (mobs.Any())
                    {
                        Q.CastIfHitchanceEquals(mobs[0], HitChance.Medium);
                    }
                }
            }
        }

        private static void OneKeyEQ()
        {
            Me.IssueOrder(GameObjectOrder.MoveTo, Game.CursorPos);

            if (E.IsReady() && Q.IsReady())
            {
                var target = MyTargetSelector.GetTarget(E.Range);

                if (target.IsValidTarget(E.Range))
                {
                    var ePred = E.GetPrediction(target);
                    if (ePred.CollisionObjects.Count == 0)
                    {
                        E.Cast(target);
                        Q.Cast(target);
                    }
                }
            }
        }

        //private static void OnGapcloser(AIHeroClient target, GapcloserArgs Args)
        //{
        //    if (MiscOption.GetBool("E", "AutoE").Enabled && target != null && target.IsValidTarget() && E.IsReady())
        //    {
        //        if (E.IsReady() && target.IsValidTarget(E.Range))
        //        {
        //            switch (Args.Type)
        //            {
        //                case SpellType.Melee:
        //                    if (target.IsValidTarget(target.AttackRange + target.BoundingRadius + 100))
        //                    {
        //                        var ePred = E.GetPrediction(target);
        //                        E.Cast(ePred.UnitPosition);
        //                    }
        //                    break;
        //                case SpellType.Dash:
        //                case SpellType.SkillShot:
        //                case SpellType.Targeted:
        //                {
        //                    var ePred = E.GetPrediction(target);
        //                    E.Cast(ePred.UnitPosition);
        //                }
        //                    break;
        //            }
        //        }
        //    }
        //}

        private static void OnProcessSpellCast(AIBaseClient sender, AIBaseClientProcessSpellCastEventArgs Args)
        {
            if (!sender.IsMe)
            {
                return;
            }

            if (Args.SData.Name == Q.Name)
            {
                lastQTime = Variables.GameTimeTickCount;
            }

            if (Args.SData.Name == W.Name)
            {
                lastWTime = Variables.GameTimeTickCount;
            }
        }

        private static void CastW(Vector3 position)
        {
            if (
                ObjectManager.Get<GameObject>()
                    .Any(
                        x =>
                            x.IsValid && x.PreviousPosition.Distance(position) <= 120 &&
                            x.Name.Equals("cupcake trap",
                                System.StringComparison.CurrentCultureIgnoreCase)))
            {
                return;
            }

            W.Cast(position);
        }
    }
}