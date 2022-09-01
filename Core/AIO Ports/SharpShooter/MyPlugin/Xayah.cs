using System;
using System.Collections.Generic;
using System.Linq;
using EnsoulSharp;
using EnsoulSharp.SDK;
using EnsoulSharp.SDK.MenuUI;
using SharpDX;
using SharpShooter.MyBase;
using SharpShooter.MyCommon;
using SharpShooter.MyLibrary;

namespace SharpShooter.MyPlugin
{
    public class Xayah : MyLogic
    {
        private static List<MyFeather> FeatherList = new List<MyFeather>();

        private static int GetPassiveCount =>
            Me.HasBuff("XayahPassiveActive") ? Me.GetBuffCount("XayahPassiveActive") : 0;

        private static bool isWActive => Me.HasBuff("XayahW");

        public Xayah()
        {
            Initializer();
        }

        private static void Initializer()
        {
            Q = new Spell(SpellSlot.Q, 1100f);
            Q.SetSkillshot(0.25f, 60f, 4000f, false, SpellType.Line);

            W = new Spell(SpellSlot.W);

            E = new Spell(SpellSlot.E);

            R = new Spell(SpellSlot.R, 1100f);
            R.SetSkillshot(1.0f, 60f, float.MaxValue, false, SpellType.Cone);

            MyMenuExtensions.ComboOption.AddMenu();
            MyMenuExtensions.ComboOption.AddQ();
            MyMenuExtensions.ComboOption.AddW();
            MyMenuExtensions.ComboOption.AddE();
            MyMenuExtensions.ComboOption.AddBool("ComboERoot", "Use E| If Target Can imprison", false);
            MyMenuExtensions.ComboOption.AddBool("ComboELogic", "Use E| Logic Cast(1AA + 1Q + 1E DMG)");

            MyMenuExtensions.HarassOption.AddMenu();
            MyMenuExtensions.HarassOption.AddQ();
            MyMenuExtensions.HarassOption.AddE();
            MyMenuExtensions.HarassOption.AddSlider("HarassECount", "Use E|Min Passive Hit Count >= x", 3, 1, 10);
            MyMenuExtensions.HarassOption.AddMana();
            MyMenuExtensions.HarassOption.AddTargetList();

            MyMenuExtensions.LaneClearOption.AddMenu();
            MyMenuExtensions.LaneClearOption.AddQ();
            MyMenuExtensions.LaneClearOption.AddSlider("LaneClearQCount", "Use Q|Min Hit Count >= x", 3, 1, 5);
            MyMenuExtensions.LaneClearOption.AddW();
            MyMenuExtensions.LaneClearOption.AddMana();

            MyMenuExtensions.JungleClearOption.AddMenu();
            MyMenuExtensions.JungleClearOption.AddQ();
            MyMenuExtensions.JungleClearOption.AddW();
            MyMenuExtensions.JungleClearOption.AddE();
            MyMenuExtensions.JungleClearOption.AddMana();

            MyMenuExtensions.KillStealOption.AddMenu();
            MyMenuExtensions.KillStealOption.AddQ();
            MyMenuExtensions.KillStealOption.AddE();

            MyMenuExtensions.MiscOption.AddMenu();
            MyMenuExtensions.MiscOption.AddBasic();
            MyMenuExtensions.MiscOption.AddSubMenu("Block", "Block Spell Settings");
            //MyEvadeManager.Attach(MiscMenu["SharpShooter.MiscSettings.Block"] as Menu);

            MyMenuExtensions.DrawOption.AddMenu();
            MyMenuExtensions.DrawOption.AddQ(Q);
            MyMenuExtensions.DrawOption.AddR(R);
            MyMenuExtensions.DrawOption.AddDamageIndicatorToHero(false, false, true, false, false);

            CPrediction.BoundingRadiusMultiplicator = 1.15f;

            Game.OnUpdate += OnUpdate;
            GameObject.OnCreate += (sender, args) => OnCreate(sender);
            GameObject.OnDelete += (sender, args) => OnDestroy(sender);
            AIBaseClient.OnProcessSpellCast += OnProcessSpellCast;
            Orbwalker.OnBeforeAttack += OnAction;
        }

        private static void OnUpdate(EventArgs args)
        {
            if (FeatherList.Any())
            {
                FeatherList.RemoveAll(f => f.EndTime - Variables.GameTimeTickCount <= 0);
            }

            if (Me.IsDead || Me.IsRecalling())
            {
                return;
            }

            if (Me.HasBuff("XayahR"))
            {
                Me.IssueOrder(GameObjectOrder.MoveTo, Game.CursorPos);
                return;
            }

            if (Me.IsWindingUp)
            {
                return;
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

        private static void KillSteal()
        {
            if (MyMenuExtensions.KillStealOption.UseE && E.IsReady())
            {
                if (
                    GameObjects.EnemyHeroes.Where(x => x.IsValidTarget() && FeatherList.Count > 0)
                    .Any(
                        target =>
                            target != null && !target.IsUnKillable() && !target.IsDashing() &&
                            target.Health <= GetEDamage(target)))
                {
                    E.Cast();
                    return;
                }
            }

            if (MyMenuExtensions.KillStealOption.UseQ && Q.IsReady())
            {
                foreach (var target in GameObjects.EnemyHeroes.Where(x => x.IsValidTarget(Q.Range)))
                {
                    if (!target.IsValidTarget(Q.Range) || !(target.Health < Me.GetSpellDamage(target, SpellSlot.Q)))
                    {
                        continue;
                    }

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
            var target = MyTargetSelector.GetTarget(1500);

            if (target != null && target.IsValidTarget(1500))
            {
                if (MyMenuExtensions.ComboOption.UseQ && Q.IsReady() && target.IsValidTarget(Q.Range))
                {
                    if (target.DistanceToPlayer() > Me.GetRealAutoAttackRange(target) + 150 || !Orbwalker.CanAttack())
                    {
                        var qPred = Q.GetPrediction(target);

                        if (qPred.Hitchance >= HitChance.High)
                        {
                            Q.Cast(qPred.CastPosition);
                        }
                    }
                }

                if (!E.IsReady() || target.IsDashing())
                {
                    return;
                }

                if (MyMenuExtensions.ComboOption.UseE && target.Health < GetEDamage(target))
                {
                    E.Cast();
                }

                if (MyMenuExtensions.ComboOption.GetBool("ComboERoot").Enabled && HitECount(target) >= 3 &&
                    !target.HasBuffOfType(BuffType.SpellShield))
                {
                    E.Cast();
                }

                if (MyMenuExtensions.ComboOption.GetBool("ComboELogic").Enabled && Me.Level >= 5 &&
                    target.Health + target.HPRegenRate * 2 <
                    GetEDamage(target) + Me.GetSpellDamage(target, SpellSlot.Q) + Me.GetAutoAttackDamage(target))
                {
                    E.Cast();
                }
            }
        }

        private static void Harass()
        {
            if (MyMenuExtensions.HarassOption.HasEnouguMana())
            {
                var target = MyMenuExtensions.HarassOption.GetTarget(1500);

                if (target.IsValidTarget(1500))
                {
                    if (MyMenuExtensions.HarassOption.UseQ && Q.IsReady() && target.IsValidTarget(Q.Range))
                    {
                        var qPred = Q.GetPrediction(target);

                        if (qPred.Hitchance >= HitChance.High)
                        {
                            Q.Cast(qPred.CastPosition);
                        }
                    }

                    if (MyMenuExtensions.HarassOption.UseE && E.IsReady() && target.IsValidTarget() &&
                        !target.HasBuffOfType(BuffType.SpellShield))
                    {
                        if (HitECount(target) >= MyMenuExtensions.HarassOption.GetSlider("HarassECount").Value)
                        {
                            E.Cast();
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
            if (MyMenuExtensions.LaneClearOption.HasEnouguMana() && MyMenuExtensions.LaneClearOption.UseQ &&
                Q.IsReady())
            {
                var minions = GameObjects.EnemyMinions.Where(x => x.IsValidTarget(Q.Range) && x.IsMinion()).ToList();

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

        private static void JungleClear()
        {
            if (MyMenuExtensions.JungleClearOption.HasEnouguMana())
            {
                var mobs =
                    GameObjects.Jungle.Where(x => x.IsValidTarget(Q.Range) && x.GetJungleType() != JungleType.Unknown)
                        .OrderByDescending(x => x.MaxHealth)
                        .ToList();

                if (mobs.Any())
                {
                    var bigMob = mobs.First(x => !x.Name.Contains("mini") && !x.Name.Contains("Crap"));

                    if (bigMob != null && bigMob.IsValidTarget())
                    {
                        if (MyMenuExtensions.JungleClearOption.UseQ && Q.IsReady() && bigMob.IsValidTarget(Q.Range) &&
                            (bigMob.DistanceToPlayer() > Me.GetRealAutoAttackRange(bigMob) ||
                             !Orbwalker.CanAttack()))
                        {
                            Q.CastIfHitchanceEquals(bigMob, HitChance.Medium);
                        }

                        if (MyMenuExtensions.JungleClearOption.UseE && E.IsReady() && bigMob.IsValidTarget())
                        {
                            if (GetEDamageForMinion(bigMob) > bigMob.Health)
                            {
                                E.Cast();
                            }
                        }
                    }
                    else
                    {
                        if (MyMenuExtensions.JungleClearOption.UseQ && Q.IsReady())
                        {
                            var qFarm = Q.GetLineFarmLocation(mobs);

                            if (qFarm.MinionsHit >= 2)
                            {
                                Q.Cast(qFarm.Position);
                            }
                        }
                    }
                }
            }
        }

        private static void OnCreate(GameObject sender)
        {
            if (sender == null || sender.Type != GameObjectType.EffectEmitter)
            {
                return;
            }

            if (sender.Name.ToLower() == "xayah_base_passive_dagger_indicator8s")
            {
                FeatherList.Add(new MyFeather(sender.NetworkId, sender.PreviousPosition,
                    Variables.GameTimeTickCount + 8000 - Game.Ping));
            }
        }

        private static void OnDestroy(GameObject sender)
        {
            if (sender == null || sender.Type != GameObjectType.EffectEmitter)
            {
                return;
            }

            FeatherList.RemoveAll(f => f.NetWorkId == sender.NetworkId);
        }

        private static void OnProcessSpellCast(AIBaseClient sender, AIBaseClientProcessSpellCastEventArgs Args)
        {
            if (sender.IsMe)
            {
                if (Args.Slot == SpellSlot.E)
                {
                    FeatherList.Clear();
                }

                if (Orbwalker.ActiveMode == OrbwalkerMode.Combo)
                {
                    if (Args.Slot == SpellSlot.Q && Me.CountEnemyHeroesInRange(600) > 0 &&
                        MyMenuExtensions.ComboOption.UseW && W.IsReady())
                    {
                        W.Cast();
                    }
                }
            }
        }

        private static void OnAction(object sender, BeforeAttackEventArgs Args)
        {
            if (Args.Target == null || Args.Target.IsDead || !Args.Target.IsValidTarget() ||
                Args.Target.Health <= 0)
            {
                return;
            }

            switch (Args.Target.Type)
            {
                case GameObjectType.AIHeroClient:
                {
                    var target = (AIHeroClient) Args.Target;
                    if (target != null && target.IsValidTarget())
                    {
                        if (Orbwalker.ActiveMode == OrbwalkerMode.Combo)
                        {
                            if (MyMenuExtensions.ComboOption.UseW && W.IsReady())
                            {
                                W.Cast();
                            }
                        }
                    }
                }
                    break;
                case GameObjectType.AIMinionClient:
                {
                    if (Orbwalker.ActiveMode == OrbwalkerMode.LaneClear)
                    {
                        var mob = (AIMinionClient) Args.Target;
                        if (mob != null && mob.IsValidTarget() && mob.GetJungleType() != JungleType.Unknown &&
                            mob.GetJungleType() != JungleType.Small)
                        {
                            if (MyMenuExtensions.JungleClearOption.HasEnouguMana() && GetPassiveCount < 3)
                            {
                                if (MyMenuExtensions.JungleClearOption.UseW && W.IsReady())
                                {
                                    W.Cast();
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
                        if (Me.CountEnemyHeroesInRange(800) == 0)
                        {
                            if (MyMenuExtensions.LaneClearOption.UseW && W.IsReady())
                            {
                                W.Cast();
                            }
                        }
                    }
                }
                    break;
            }
        }

        private static void AfterAttackEventArgs(object sender, AfterAttackEventArgs Args)
        {
            if (Args.Target == null || Args.Target.IsDead || !Args.Target.IsValidTarget() || Args.Target.Health <= 0)
            {
                return;
            }

            switch (Args.Target.Type)
            {
                case GameObjectType.AIHeroClient:
                {
                    var target = (AIHeroClient) Args.Target;
                    if (target != null && target.IsValidTarget())
                    {
                        if (Orbwalker.ActiveMode == OrbwalkerMode.Combo)
                        {
                            if (MyMenuExtensions.ComboOption.UseQ && Q.IsReady())
                            {
                                if (!isWActive)
                                {
                                    var qPred = Q.GetPrediction(target);

                                    if (qPred.Hitchance >= HitChance.High)
                                    {
                                        Q.Cast(qPred.CastPosition);
                                    }
                                }
                            }

                            if (MyMenuExtensions.ComboOption.UseW && W.IsReady())
                            {
                                W.Cast();
                            }
                        }
                        else if (Orbwalker.ActiveMode == OrbwalkerMode.Harass ||
                                 Orbwalker.ActiveMode == OrbwalkerMode.LaneClear && MyManaManager.SpellHarass)
                        {
                            if (MyMenuExtensions.HarassOption.HasEnouguMana() &&
                                MyMenuExtensions.HarassOption.GetHarassTargetEnabled(target.CharacterName))
                            {
                                if (MyMenuExtensions.HarassOption.UseQ && Q.IsReady())
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
                }
                    break;
            }
        }

        public static double GetEDamageForMinion(AIBaseClient target)
        {
            if (HitECount(target) == 0)
            {
                return 0;
            }
            return GetEDMG(target, HitECount(target)) * 0.5;
        }

        public static double GetEDMG(AIBaseClient target, int eCount)
        {
            if (eCount == 0)
            {
                return 0;
            }

            double damage = 0;
            double multiplier = 1;
            var basicDMG = new double[] { 50, 60, 70, 80, 90 }[E.Level - 1] +
                           0.6 * Me.FlatPhysicalDamageMod;
            var realBasicDMG = basicDMG + basicDMG * 0.5 * Me.Crit;

            for (var cycle = 0; cycle <= eCount; cycle++)
            {
                multiplier -= 0.1 * cycle;
                damage += Me.CalculateDamage(target, DamageType.Physical, realBasicDMG) * Math.Max(0.1, multiplier);
            }

            return (float)damage;
        }

        private static double GetEDamage(AIBaseClient target)
        {
            if (HitECount(target) == 0)
            {
                return 0;
            }
            var eDMG = GetEDMG(target, HitECount(target));
            return MyExtraManager.GetRealDamage(eDMG, target);
        }

        public static int HitECount(AIBaseClient target)
        {
            return
                FeatherList.Select(
                    f =>
                        CPrediction.GetLineAoeCanHit(Me.PreviousPosition.Distance(f.PreviousPosition), 55, target, 
                            HitChance.High, f.PreviousPosition)).Count(pred => pred);
        }

        public class MyFeather
        {
            public uint NetWorkId { get; set; }
            public Vector3 PreviousPosition { get; set; }
            public int EndTime { get; set; }

            public MyFeather(int NetWorkId, Vector3 PreviousPosition, int EndTime)
            {
                this.NetWorkId = (uint) NetWorkId;
                this.PreviousPosition = PreviousPosition;
                this.EndTime = EndTime;
            }
        }
    }
}