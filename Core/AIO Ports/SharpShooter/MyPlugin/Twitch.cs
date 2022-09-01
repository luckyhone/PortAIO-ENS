using System;
using System.Linq;
using EnsoulSharp;
using EnsoulSharp.SDK;
using SharpShooter.MyBase;
using SharpShooter.MyCommon;

namespace SharpShooter.MyPlugin
{
    public class Twitch : MyLogic
    {
        public Twitch()
        {
            Initializer();
        }

        private static void Initializer()
        {
            Q = new Spell(SpellSlot.Q);

            W = new Spell(SpellSlot.W, 950f);
            W.SetSkillshot(0.25f, 100f, 1400f, false, SpellType.Circle);

            E = new Spell(SpellSlot.E, 1200f);

            R = new Spell(SpellSlot.R, 975f);

            MyMenuExtensions.ComboOption.AddMenu();
            MyMenuExtensions.ComboOption.AddQ();
            MyMenuExtensions.ComboOption.AddSlider("ComboQCount", "Use Q| Enemies Count >= x", 3, 1, 5);
            MyMenuExtensions.ComboOption.AddSlider("ComboQRange", "Use Q| Search Enemies Range", 600, 0, 1800);
            MyMenuExtensions.ComboOption.AddW();
            MyMenuExtensions.ComboOption.AddE();
            MyMenuExtensions.ComboOption.AddBool("ComboEKill", "Use E| When Target Can KillAble");
            MyMenuExtensions.ComboOption.AddBool("ComboEFull", "Use E| When Target have Full Stack", false);
            MyMenuExtensions.ComboOption.AddR();
            MyMenuExtensions.ComboOption.AddBool("ComboRKillSteal", "Use R| When Target Can KillAble");
            MyMenuExtensions.ComboOption.AddSlider("ComboRCount", "Use R| Enemies Count >= x", 3, 1, 5);

            MyMenuExtensions.HarassOption.AddMenu();
            MyMenuExtensions.HarassOption.AddW();
            MyMenuExtensions.HarassOption.AddE();
            MyMenuExtensions.HarassOption.AddBool("HarassEStack", "Use E| When Target will Leave E Range");
            MyMenuExtensions.HarassOption.AddSlider("HarassEStackCount", "Use E(Leave)| Min Stack Count >= x", 3, 1, 6);
            MyMenuExtensions.HarassOption.AddBool("HarassEFull", "Use E| When Target have Full Stack");
            MyMenuExtensions.HarassOption.AddMana();
            MyMenuExtensions.HarassOption.AddTargetList();

            MyMenuExtensions.LaneClearOption.AddMenu();
            MyMenuExtensions.LaneClearOption.AddE();
            MyMenuExtensions.LaneClearOption.AddSlider("LaneClearECount", "Use E| Min KillAble Count >= x", 3, 1, 5);
            MyMenuExtensions.LaneClearOption.AddMana();

            MyMenuExtensions.JungleClearOption.AddMenu();
            MyMenuExtensions.JungleClearOption.AddE();
            MyMenuExtensions.JungleClearOption.AddMana();

            MyMenuExtensions.KillStealOption.AddMenu();
            MyMenuExtensions.KillStealOption.AddE();

            MyMenuExtensions.MiscOption.AddMenu();
            MyMenuExtensions.MiscOption.AddBasic();

            MyMenuExtensions.DrawOption.AddMenu();
            MyMenuExtensions.DrawOption.AddW(W);
            MyMenuExtensions.DrawOption.AddE(E);
            MyMenuExtensions.DrawOption.AddR(R);
            MyMenuExtensions.DrawOption.AddDamageIndicatorToHero(false, false, true, false, false);

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
                foreach (
                    var target in
                    GameObjects.EnemyHeroes.Where(
                        x => x.IsValidTarget(E.Range) && !x.IsUnKillable()))
                {
                    if (target.IsValidTarget(E.Range) && target.Health < GetRealEDamage(target) - target.HPRegenRate)
                    {
                        E.Cast();
                    }
                }
            }
        }

        private static void Combo()
        {
            var target = MyTargetSelector.GetTarget(E.Range);

            if (target.IsValidTarget(E.Range))
            {
                if (MyMenuExtensions.ComboOption.UseR && R.IsReady())
                {
                    if (MyMenuExtensions.ComboOption.GetBool("ComboRKillSteal").Enabled &&
                        GameObjects.EnemyHeroes.Count(x => x.DistanceToPlayer() <= R.Range) <= 2 &&
                        target.Health <= Me.GetAutoAttackDamage(target) * 4 + GetRealEDamage(target) * 2)
                    {
                        R.Cast();
                    }

                    if (GameObjects.EnemyHeroes.Count(x => x.DistanceToPlayer() <= R.Range) >= MyMenuExtensions.ComboOption.GetSlider("ComboRCount").Value)
                    {
                        R.Cast();
                    }
                }

                if (MyMenuExtensions.ComboOption.UseQ && Q.IsReady() &&
                    GameObjects.EnemyHeroes.Count(x => x.DistanceToPlayer() <= MyMenuExtensions.ComboOption.GetSlider("ComboQRange").Value) >=
                    MyMenuExtensions.ComboOption.GetSlider("ComboQCount").Value)
                {
                    Q.Cast();
                }

                if (MyMenuExtensions.ComboOption.UseW && W.IsReady() && target.IsValidTarget(W.Range) &&
                    target.Health > GetRealEDamage(target) && GetEStackCount(target) < 6 &&
                    Me.Mana > Q.Mana + W.Mana + E.Mana + R.Mana)
                {
                    var wPred = W.GetPrediction(target);

                    if (wPred.Hitchance >= HitChance.High)
                    {
                        W.Cast(wPred.CastPosition);
                    }
                }

                if (MyMenuExtensions.ComboOption.UseE && E.IsReady() && target.IsValidTarget(E.Range) &&
                    target.Buffs.Any(b => b.Name.ToLower() == "twitchdeadlyvenom"))
                {
                    if (MyMenuExtensions.ComboOption.GetBool("ComboEFull").Enabled && GetEStackCount(target) >= 6)
                    {
                        E.Cast();
                    }

                    if (MyMenuExtensions.ComboOption.GetBool("ComboEKill").Enabled && target.Health <= GetRealEDamage(target) &&
                        target.IsValidTarget(E.Range))
                    {
                        E.Cast();
                    }
                }
            }
        }

        private static void Harass()
        {
            if (MyMenuExtensions.HarassOption.HasEnouguMana())
            {
                if (MyMenuExtensions.HarassOption.UseW && W.IsReady())
                {
                    var target = MyMenuExtensions.HarassOption.GetTarget(W.Range);

                    if (target.IsValidTarget(W.Range))
                    {
                        var wPred = W.GetPrediction(target);

                        if (wPred.Hitchance >= HitChance.High)
                        {
                            W.Cast(wPred.CastPosition);
                        }
                    }
                }

                if (MyMenuExtensions.HarassOption.UseE && E.IsReady())
                {
                    var target = MyMenuExtensions.HarassOption.GetTarget(E.Range);

                    if (target.IsValidTarget(E.Range))
                    {
                        if (MyMenuExtensions.HarassOption.GetBool("HarassEStack").Enabled)
                        {
                            if (target.DistanceToPlayer() > E.Range * 0.8 && target.IsValidTarget(E.Range) &&
                                GetEStackCount(target) >= MyMenuExtensions.HarassOption.GetSlider("HarassEStackCount").Value)
                            {
                                E.Cast();
                            }
                        }

                        if (MyMenuExtensions.HarassOption.GetBool("HarassEFull").Enabled && GetEStackCount(target) >= 6)
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
            if (MyMenuExtensions.LaneClearOption.HasEnouguMana())
            {
                if (MyMenuExtensions.LaneClearOption.UseE && E.IsReady())
                {
                    var eKillMinionsCount =
                        GameObjects.EnemyMinions.Where(x => x.IsValidTarget(E.Range) && x.IsMinion())
                            .Count(
                                x =>
                                    x.DistanceToPlayer() <= E.Range && x.Buffs.Any(b => b.Name.ToLower() == "twitchdeadlyvenom") &&
                                    x.Health < GetRealEDamage(x));

                    if (eKillMinionsCount >= MyMenuExtensions.LaneClearOption.GetSlider("LaneClearECount").Value)
                    {
                        E.Cast();
                    }
                }
            }
        }

        private static void JungleClear()
        {
            if (MyMenuExtensions.JungleClearOption.HasEnouguMana())
            {
                if (MyMenuExtensions.JungleClearOption.UseE && E.IsReady())
                {
                    var mobs = GameObjects.Jungle.Where(x => x.IsValidTarget(E.Range) && x.GetJungleType() != JungleType.Unknown).ToList();

                    foreach (
                        var mob in
                        mobs.Where(
                            x =>
                                !x.Name.ToLower().Contains("mini") && x.DistanceToPlayer() <= E.Range &&
                               x.Buffs.Any(b => b.Name.ToLower() == "twitchdeadlyvenom")))
                    {
                        if (mob.Health < GetRealEDamage(mob) && mob.IsValidTarget(E.Range))
                        {
                            E.Cast();
                        }
                    }
                }
            }
        }

        private static void OnAction(object sender, AfterAttackEventArgs Args)
        {
            if (Args.Target == null || Args.Target.IsDead || !Args.Target.IsValidTarget() ||
                Args.Target.Health <= 0 || Orbwalker.ActiveMode == OrbwalkerMode.None)
            {
                return;
            }

            switch (Args.Target.Type)
            {
                case GameObjectType.AIHeroClient:
                {
                    if (Orbwalker.ActiveMode == OrbwalkerMode.Combo)
                    {
                        if (MyMenuExtensions.ComboOption.UseW && W.IsReady())
                        {
                            var target = (AIHeroClient)Args.Target;
                            if (target != null && target.InAutoAttackRange())
                            {
                                var wPred = W.GetPrediction(target);
                                if (wPred.Hitchance >= HitChance.High)
                                {
                                    W.Cast(wPred.UnitPosition);
                                }
                            }
                        }
                    }
                }
                    break;
            }
        }

        private static double GetRealEDamage(AIBaseClient target)
        {
            if (target != null && !target.IsDead && target.Buffs.Any(b => b.Name.ToLower() == "twitchdeadlyvenom"))
            {
                if (target.HasBuff("KindredRNoDeathBuff"))
                {
                    return 0;
                }

                if (target.HasBuff("UndyingRage") && target.GetBuff("UndyingRage").EndTime - Game.Time > 0.3)
                {
                    return 0;
                }

                if (target.HasBuff("JudicatorIntervention"))
                {
                    return 0;
                }

                if (target.HasBuff("ChronoShift") && target.GetBuff("ChronoShift").EndTime - Game.Time > 0.3)
                {
                    return 0;
                }

                if (target.HasBuff("FioraW"))
                {
                    return 0;
                }

                if (target.HasBuff("ShroudofDarkness"))
                {
                    return 0;
                }

                if (target.HasBuff("SivirShield"))
                {
                    return 0;
                }

                var damage = 0d;

                damage += E.IsReady() ? GetEDMGTwitch(target) : 0d;

                if (target.CharacterName == "Morderkaiser")
                {
                    damage -= target.Mana;
                }

                if (Me.HasBuff("SummonerExhaust"))
                {
                    damage = damage * 0.6f;
                }

                if (target.HasBuff("BlitzcrankManaBarrierCD") && target.HasBuff("ManaBarrier"))
                {
                    damage -= target.Mana / 2f;
                }

                if (target.HasBuff("GarenW"))
                {
                    damage = damage * 0.7f;
                }

                if (target.HasBuff("ferocioushowl"))
                {
                    damage = damage * 0.7f;
                }

                return damage;
            }

            return 0d;
        }

        public static double GetEDMGTwitch(AIBaseClient target)
        {
            if (target.Buffs.All(b => b.Name.ToLower() != "twitchdeadlyvenom"))
            {
                return 0;
            }

            return E.GetDamage(target);
        }

        public static int GetEStackCount(AIBaseClient target)
        {
            if (target == null || target.IsDead ||
                !target.IsValidTarget() ||
                target.Type != GameObjectType.AIMinionClient && target.Type != GameObjectType.AIHeroClient)
            {
                return 0;
            }

            return target.GetBuffCount("twitchdeadlyvenom");
        }
    }
}