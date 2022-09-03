using System;
using System.Linq;
using EnsoulSharp;
using EnsoulSharp.SDK;
using EnsoulSharp.SDK.MenuUI;
using SharpShooter.MyBase;
using SharpShooter.MyCommon;

namespace SharpShooter.MyPlugin
{
    public class KogMaw : MyLogic
    {
        private static int GetRCount => Me.HasBuff("kogmawlivingartillerycost") ? Me.GetBuffCount("kogmawlivingartillerycost") : 0;

        private static float wRange => 500f + new[] {0, 130, 150, 170, 190, 210}[Me.Spellbook.GetSpell(SpellSlot.W).Level] + Me.BoundingRadius;
        private static float rRange => new[] { 1200, 1200, 1500, 1800 }[Me.Spellbook.GetSpell(SpellSlot.R).Level];

        public KogMaw()
        {
            Initializer();
        }

        private static void Initializer()
        {
            Q = new Spell(SpellSlot.Q, 950f);
            Q.SetSkillshot(0.25f, 70f, 1650f, true, SpellType.Line);

            W = new Spell(SpellSlot.W, wRange);

            E = new Spell(SpellSlot.E, 1200f);
            E.SetSkillshot(0.25f, 120f, 1400f, false, SpellType.Line);

            R = new Spell(SpellSlot.R, rRange);
            R.SetSkillshot(1.20f, 120f, float.MaxValue, false, SpellType.Circle);

            MyMenuExtensions.ComboOption.AddMenu();
            MyMenuExtensions.ComboOption.AddQ();
            MyMenuExtensions.ComboOption.AddW();
            MyMenuExtensions.ComboOption.AddE();
            MyMenuExtensions.ComboOption.AddR();
            MyMenuExtensions.ComboOption.AddSlider("ComboRLimit", "Use R|Max Buff Count < x", 3, 0, 10);
            MyMenuExtensions.ComboOption.AddBool("ComboROnlyOutAARange", "Use R|Only Target Out AA Range", false);
            MyMenuExtensions.ComboOption.AddSlider("ComboRHP", "Use R|target HealthPercent <= x%", 70, 1, 101);
            MyMenuExtensions.ComboOption.AddBool("ComboForcus", "Forcus Spell on Orbwalker Target", false);

            MyMenuExtensions.HarassOption.AddMenu();
            MyMenuExtensions.HarassOption.AddQ();
            MyMenuExtensions.HarassOption.AddE();
            MyMenuExtensions.HarassOption.AddR();
            MyMenuExtensions.HarassOption.AddSlider("HarassRLimit", "Use R|Max Buff Count < x", 5, 0, 10);
            MyMenuExtensions.HarassOption.AddMana();
            MyMenuExtensions.HarassOption.AddTargetList();

            MyMenuExtensions.LaneClearOption.AddMenu();
            MyMenuExtensions.LaneClearOption.AddQ();
            MyMenuExtensions.LaneClearOption.AddE();
            MyMenuExtensions.LaneClearOption.AddSlider("LaneClearECount", "Use E|Min Hit Count >= x", 3, 1, 5);
            MyMenuExtensions.LaneClearOption.AddR();
            MyMenuExtensions.LaneClearOption.AddSlider("LaneClearRLimit", "Use R|Max Buff Count < x", 4, 0, 10);
            MyMenuExtensions.LaneClearOption.AddMana();

            MyMenuExtensions.JungleClearOption.AddMenu();
            MyMenuExtensions.JungleClearOption.AddQ();
            MyMenuExtensions.JungleClearOption.AddW();
            MyMenuExtensions.JungleClearOption.AddE();
            MyMenuExtensions.JungleClearOption.AddR();
            MyMenuExtensions.JungleClearOption.AddSlider("JungleClearRLimit", "Use R|Max Buff Count < x", 5, 0, 10);
            MyMenuExtensions.JungleClearOption.AddMana();

            MyMenuExtensions.KillStealOption.AddMenu();
            MyMenuExtensions.KillStealOption.AddQ();
            MyMenuExtensions.KillStealOption.AddE();
            MyMenuExtensions.KillStealOption.AddSliderBool("KillStealRCount", "Use R|Max Buff Count < x", 3, 0, 10);
            MyMenuExtensions.KillStealOption.AddBool("KillStealOutAARange", "Only Target Out of AA Range");
            MyMenuExtensions.KillStealOption.AddTargetList();

            //GapcloserOption.AddMenu();

            MyMenuExtensions.MiscOption.AddMenu();
            MyMenuExtensions.MiscOption.AddBasic();
            MyMenuExtensions.MiscOption.AddE();
            MyMenuExtensions.MiscOption.AddBool("E", "AutoE", "Auto E| Anti Gapcloser");
            MyMenuExtensions.MiscOption.AddR();
            MyMenuExtensions.MiscOption.AddKey("R", "SemiR", "Semi-manual R Key", Keys.T, KeyBindType.Press);

            MyMenuExtensions.DrawOption.AddMenu();
            MyMenuExtensions.DrawOption.AddQ(Q);
            MyMenuExtensions.DrawOption.AddW(W);
            MyMenuExtensions.DrawOption.AddE(E);
            MyMenuExtensions.DrawOption.AddR(R);
            MyMenuExtensions.DrawOption.AddDamageIndicatorToHero(true, true, true, true, true);

            GameEvent.OnGameTick += OnUpdate;
            Orbwalker.OnAfterAttack += OnAction;
            //Gapcloser.OnGapcloser += OnGapcloser;
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

            if (W.Level > 0)
            {
                W.Range = wRange;
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
            if (R.IsReady())
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
                        if (MyMenuExtensions.KillStealOption.GetBool("KillStealOutAARange").Enabled && target.InAutoAttackRange())
                        {
                            return;
                        }

                        var qPred = Q.GetPrediction(target);

                        if (qPred.Hitchance >= HitChance.Medium)
                        {
                            Q.Cast(qPred.CastPosition);
                        }
                    }
                }
            }

            if (MyMenuExtensions.KillStealOption.UseE && E.IsReady())
            {
                foreach (
                    var target in
                    GameObjects.EnemyHeroes.Where(
                        x => x.IsValidTarget(E.Range) && x.Health < Me.GetSpellDamage(x, SpellSlot.E)))
                {
                    if (target.IsValidTarget(E.Range) && !target.IsUnKillable())
                    {
                        if (MyMenuExtensions.KillStealOption.GetBool("KillStealOutAARange").Enabled && target.InAutoAttackRange())
                        {
                            return;
                        }

                        var ePred = E.GetPrediction(target);

                        if (ePred.Hitchance >= HitChance.High)
                        {
                            E.Cast(ePred.CastPosition);
                        }
                    }
                }
            }

            if (MyMenuExtensions.KillStealOption.GetSliderBool("KillStealRCount").Enabled && R.IsReady())
            {
                foreach (
                    var target in
                    GameObjects.EnemyHeroes.Where(
                        x => x.IsValidTarget(R.Range) && MyMenuExtensions.KillStealOption.GetKillStealTarget(x.CharacterName) && 
                        x.Health < Me.GetSpellDamage(x, SpellSlot.R)))
                {
                    if (target.IsValidTarget(R.Range) && !target.IsUnKillable() &&
                        GetRCount < MyMenuExtensions.KillStealOption.GetSliderBool("KillStealRCount").Value)
                    {
                        if (MyMenuExtensions.KillStealOption.GetBool("KillStealOutAARange").Enabled && target.InAutoAttackRange())
                        {
                            return;
                        }

                        var rPred = R.GetPrediction(target);

                        if (rPred.Hitchance >= HitChance.High)
                        {
                            R.Cast(rPred.CastPosition);
                        }
                    }
                }
            }
        }

        private static void Combo()
        {
            var target = MyTargetSelector.GetTarget(R.Range, MyMenuExtensions.ComboOption.GetBool("ComboForcus").Enabled);

            if (target.IsValidTarget(R.Range) && !target.IsUnKillable())
            {
                if (MyMenuExtensions.ComboOption.UseR && R.IsReady() && MyMenuExtensions.ComboOption.GetSlider("ComboRLimit").Value > GetRCount &&
                    target.IsValidTarget(R.Range) && target.HealthPercent <= MyMenuExtensions.ComboOption.GetSlider("ComboRHP").Value &&
                    (!MyMenuExtensions.ComboOption.GetBool("ComboROnlyOutAARange").Enabled ||
                     MyMenuExtensions.ComboOption.GetBool("ComboROnlyOutAARange").Enabled && !target.InAutoAttackRange()))
                {
                    var rPred = R.GetPrediction(target);

                    if (rPred.Hitchance >= HitChance.High)
                    {
                        R.Cast(rPred.CastPosition);
                    }
                }

                if (MyMenuExtensions.ComboOption.UseQ && Q.IsReady() && target.IsValidTarget(Q.Range))
                {
                    var qPred = Q.GetPrediction(target);

                    if (qPred.Hitchance >= HitChance.Medium)
                    {
                        Q.Cast(qPred.CastPosition);
                    }
                }

                if (MyMenuExtensions.ComboOption.UseE && E.IsReady() && target.IsValidTarget(E.Range))
                {
                    var ePred = E.GetPrediction(target);

                    if (ePred.Hitchance >= HitChance.High)
                    {
                        E.Cast(ePred.CastPosition);
                    }
                }

                if (MyMenuExtensions.ComboOption.UseW && W.IsReady() && target.IsValidTarget(W.Range) &&
                    !target.InAutoAttackRange() && Orbwalker.CanAttack())
                {
                    W.Cast();
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
                    if (MyMenuExtensions.HarassOption.UseR && R.IsReady() && MyMenuExtensions.HarassOption.GetSlider("HarassRLimit").Value > GetRCount &&
                        target.IsValidTarget(R.Range))
                    {
                        var rPred = R.GetPrediction(target);

                        if (rPred.Hitchance >= HitChance.High)
                        {
                            R.Cast(rPred.CastPosition);
                        }
                    }

                    if (MyMenuExtensions.HarassOption.UseQ && Q.IsReady() && target.IsValidTarget(Q.Range))
                    {
                        var qPred = Q.GetPrediction(target);

                        if (qPred.Hitchance >= HitChance.Medium)
                        {
                            Q.Cast(qPred.CastPosition);
                        }
                    }

                    if (MyMenuExtensions.HarassOption.UseE && E.IsReady() && target.IsValidTarget(E.Range))
                    {
                        var ePred = E.GetPrediction(target);

                        if (ePred.Hitchance >= HitChance.High)
                        {
                            E.Cast(ePred.UnitPosition);
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
                var minions = GameObjects.EnemyMinions.Where(x => x.IsValidTarget(R.Range) && x.IsMinion()).ToList();

                if (minions.Any())
                {
                    if (MyMenuExtensions.LaneClearOption.UseR && R.IsReady() && MyMenuExtensions.LaneClearMenu["LaneClearRLimit"].GetValue<MenuSlider>().Value > GetRCount)
                    {
                        var rMinion =
                            minions.FirstOrDefault(x => x.DistanceToPlayer() > Me.AttackRange + Me.BoundingRadius);

                        if (rMinion != null && rMinion.IsValidTarget(R.Range))
                        {
                            var input = R.GetPrediction(rMinion);
                            if (input.Hitchance >= HitChance.High)
                            {
                                R.Cast(rMinion);
                            }
                        }
                    }

                    if (MyMenuExtensions.LaneClearOption.UseE && E.IsReady())
                    {
                        var eMinions = minions.Where(x => x.IsValidTarget(E.Range)).ToList();
                        var eFarm = E.GetLineFarmLocation(eMinions);

                        if (eFarm.MinionsHit >= MyMenuExtensions.LaneClearMenu["LaneClearECount"].GetValue<MenuSlider>().Value)
                        {
                            E.Cast(eFarm.Position);
                        }
                    }

                    if (MyMenuExtensions.LaneClearOption.UseQ && Q.IsReady())
                    {
                        var qMinion =
                            minions.Where(x => x.IsValidTarget(Q.Range))
                                .FirstOrDefault(
                                    x =>
                                        x.Health < Me.GetSpellDamage(x, SpellSlot.Q) &&
                                        x.Health > Me.GetAutoAttackDamage(x));

                        if (qMinion != null && qMinion.IsValidTarget(Q.Range))
                        {
                            var input = Q.GetPrediction(qMinion);
                            if (input.Hitchance >= HitChance.High)
                            {
                                Q.Cast(qMinion);
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
                var mobs = GameObjects.Jungle.Where(x => x.IsValidTarget(R.Range) && x.GetJungleType() != JungleType.Unknown).ToList();

                if (mobs.Any())
                {
                    var bigmob = mobs.FirstOrDefault(x => !x.Name.ToLower().Contains("mini"));

                    if (MyMenuExtensions.JungleClearOption.UseR && R.IsReady() &&
                        MyMenuExtensions.JungleClearOption.GetSlider("JungleClearRLimit").Value > GetRCount &&
                        bigmob != null && (!bigmob.InAutoAttackRange() || !Orbwalker.CanAttack()))
                    {
                        var input = R.GetPrediction(bigmob);
                        if (input.Hitchance >= HitChance.High)
                        {
                            R.Cast(bigmob.Position);
                        }
                    }

                    if (MyMenuExtensions.JungleClearOption.UseE && E.IsReady())
                    {
                        if (bigmob != null && bigmob.IsValidTarget(E.Range) && (!bigmob.InAutoAttackRange() || !Orbwalker.CanAttack()))
                        {
                            var input = R.GetPrediction(bigmob);
                            E.Cast(bigmob.Position);
                        }
                        else
                        {
                            var eMobs = mobs.Where(x => x.IsValidTarget(E.Range)).ToList();
                            var eFarm = E.GetLineFarmLocation(eMobs);

                            if (eFarm.MinionsHit >= 2)
                            {
                                E.Cast(eFarm.Position);
                            }
                        }
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

                            if (target != null && !target.IsDead)
                            {
                                if (MyMenuExtensions.ComboOption.UseW && W.IsReady() && target.IsValidTarget(W.Range))
                                {
                                    W.Cast();
                                }
                                else if (MyMenuExtensions.ComboOption.UseQ && Q.IsReady() && target.IsValidTarget(Q.Range))
                                {
                                    var qPred = Q.GetPrediction(target);

                                    if (qPred.Hitchance >= HitChance.Medium)
                                    {
                                        Q.Cast(qPred.CastPosition);
                                    }
                                }
                                else if (MyMenuExtensions.ComboOption.UseE && E.IsReady() && target.IsValidTarget(E.Range))
                                {
                                    var ePred = E.GetPrediction(target);

                                    if (ePred.Hitchance >= HitChance.High)
                                    {
                                        E.Cast(ePred.UnitPosition);
                                    }
                                }
                            }
                        }
                    }
                    break;
                case GameObjectType.AIMinionClient:
                    {
                        if (Orbwalker.ActiveMode == OrbwalkerMode.LaneClear && MyMenuExtensions.JungleClearOption.HasEnouguMana())
                        {
                            var mobs = GameObjects.Jungle.Where(x => x.IsValidTarget(R.Range) && x.GetJungleType() != JungleType.Unknown).ToList();

                            if (mobs.Any())
                            {
                                var mob = mobs.FirstOrDefault();
                                var bigmob = mobs.FirstOrDefault(x => !x.Name.ToLower().Contains("mini"));

                                if (MyMenuExtensions.JungleClearOption.UseW && W.IsReady() && bigmob != null && bigmob.IsValidTarget(W.Range))
                                {
                                    W.Cast();
                                }
                                else if (MyMenuExtensions.JungleClearOption.UseE && E.IsReady())
                                {
                                    if (bigmob != null && bigmob.IsValidTarget(E.Range))
                                    {
                                        E.Cast(bigmob);
                                    }
                                    else
                                    {
                                        var eMobs = mobs.Where(x => x.IsValidTarget(E.Range)).ToList();
                                        var eFarm = E.GetLineFarmLocation(eMobs);

                                        if (eFarm.MinionsHit >= 2)
                                        {
                                            E.Cast(eFarm.Position);
                                        }
                                    }
                                }
                                else if (MyMenuExtensions.JungleClearOption.UseQ && Q.IsReady() && mob != null && mob.IsValidTarget(Q.Range))
                                {
                                    Q.Cast(mob);
                                }
                            }
                        }
                    }
                    break;
            }
        }

        //private static void OnGapcloser(AIHeroClient target, GapcloserArgs Args)
        //{
        //    if (MiscOption.GetBool("E", "AutoE").Enabled && E.IsReady() && target.IsValidTarget(E.Range))
        //    {
        //        if (E.IsReady() && target != null && target.IsValidTarget(E.Range))
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
        //                    {
        //                        var ePred = E.GetPrediction(target);
        //                        E.Cast(ePred.UnitPosition);
        //                    }
        //                    break;
        //            }
        //        }
        //    }
        //}
    }
}