using System;
using System.Collections.Generic;
using System.Linq;
using EnsoulSharp;
using EnsoulSharp.SDK;
using EnsoulSharp.SDK.MenuUI;
using EnsoulSharp.SDK.Rendering;
using SharpDX;
using SharpShooter.MyBase;
using SharpShooter.MyCommon;

namespace SharpShooter.MyPlugin
{
    public class Draven : MyLogic
    {
        private static Dictionary<GameObject, int> AxeList { get; } = new Dictionary<GameObject, int>();

        private static Vector3 OrbwalkerPoint { get; set; } = Game.CursorPos;

        private static int AxeCount => (Me.HasBuff("dravenspinning") ? 1 : 0) + (Me.HasBuff("dravenspinningleft") ? 1 : 0) + AxeList.Count;

        private static int lastCatchTime { get; set; }

        public Draven()
        {
            Initializer();
        }

        private static void Initializer()
        {
            Q = new Spell(SpellSlot.Q);

            W = new Spell(SpellSlot.W);

            E = new Spell(SpellSlot.E, 950f);
            E.SetSkillshot(0.25f, 100f, 1400f, false, SpellType.Line);

            R = new Spell(SpellSlot.R, 3000f);
            R.SetSkillshot(0.4f, 160f, 2000f, false, SpellType.Line);

            MyMenuExtensions.ComboOption.AddMenu();
            MyMenuExtensions.ComboOption.AddQ();
            MyMenuExtensions.ComboOption.AddW();
            MyMenuExtensions.ComboOption.AddE();
            MyMenuExtensions.ComboOption.AddR();
            MyMenuExtensions.ComboOption.AddBool("RSolo", "Use R | Solo Ks Mode");
            MyMenuExtensions.ComboOption.AddBool("RTeam", "Use R| Team Fight");

            MyMenuExtensions.HarassOption.AddMenu();
            MyMenuExtensions.HarassOption.AddQ();
            MyMenuExtensions.HarassOption.AddE();
            MyMenuExtensions.HarassOption.AddMana();
            MyMenuExtensions.HarassOption.AddTargetList();

            MyMenuExtensions.LaneClearOption.AddMenu();
            MyMenuExtensions.LaneClearOption.AddQ();
            MyMenuExtensions.LaneClearOption.AddSliderBool("LaneClearECount", "Use E| Min Hit Count >= x", 4, 1, 7, true);
            MyMenuExtensions.LaneClearOption.AddMana();

            MyMenuExtensions.JungleClearOption.AddMenu();
            MyMenuExtensions.JungleClearOption.AddQ();
            MyMenuExtensions.JungleClearOption.AddW();
            MyMenuExtensions.JungleClearOption.AddE();
            MyMenuExtensions.JungleClearOption.AddMana();

            MyMenuExtensions.KillStealOption.AddMenu();
            MyMenuExtensions.KillStealOption.AddE();
            MyMenuExtensions.KillStealOption.AddR();
            MyMenuExtensions.KillStealOption.AddTargetList();

            MyMenuExtensions.AxeOption.AddMenu();
            MyMenuExtensions.AxeOption.AddList("CatchMode", "Catch Axe Mode: ", new[] { "All", "Only Combo", "Off" });
            MyMenuExtensions.AxeOption.AddSlider("CatchRange", "Catch Axe Range(Cursor center)", 2000, 180, 3000);
            MyMenuExtensions.AxeOption.AddSlider("CatchCount", "Max Axe Count <= x", 2, 1, 3);
            MyMenuExtensions.AxeOption.AddBool("CatchWSpeed", "Use W| When Axe Too Far");
            MyMenuExtensions.AxeOption.AddBool("NotCatchKS", "Dont Catch| If Target Can KillAble(1-3 AA)");
            MyMenuExtensions.AxeOption.AddBool("NotCatchTurret", "Dont Catch| If Axe Under Enemy Turret");
            MyMenuExtensions.AxeOption.AddSliderBool("NotCatchMoreEnemy", "Dont Catch| If Enemy Count >= x", 3, 1, 5, true);
            MyMenuExtensions.AxeOption.AddBool("CancelCatch", "Enabled Cancel Catch Axe Key");
            MyMenuExtensions.AxeOption.AddKey("CancelKey1", "Cancel Catch Key 1", Keys.G, KeyBindType.Press);
            MyMenuExtensions.AxeOption.AddBool("CancelKey2", "Cancel Catch Key 2(is right click)");
            MyMenuExtensions.AxeOption.AddBool("CancelKey3", "Cancel Catch Key 3(is mouse scroll)", false);
            MyMenuExtensions.AxeOption.AddSeperator("Set Orbwalker->Misc->Hold Radius to 0 (will better)");

            //GapcloserOption.AddMenu();

            MyMenuExtensions.MiscOption.AddMenu();
            MyMenuExtensions.MiscOption.AddBasic();
            MyMenuExtensions.MiscOption.AddW();
            MyMenuExtensions.MiscOption.AddBool("W", "WSlow", "Auto W| When Player Have Debuff(Slow)");
            MyMenuExtensions.MiscOption.AddR();
            MyMenuExtensions.MiscOption.AddSlider("R", "GlobalRMin", "Global -> Cast R Min Range", 1000, 500, 2500);
            MyMenuExtensions.MiscOption.AddSlider("R", "GlobalRMax", "Global -> Cast R Max Range", 3000, 1500, 3500);
            MyMenuExtensions.MiscOption.AddKey("R", "SemiRKey", "Semi-manual R Key", Keys.T, KeyBindType.Press);

            MyMenuExtensions.DrawOption.AddMenu();
            MyMenuExtensions.DrawOption.AddE(E);
            MyMenuExtensions.DrawOption.AddR(R);
            MyMenuExtensions.DrawOption.AddBool("AxeRange", "Draw Catch Axe Range");
            MyMenuExtensions.DrawOption.AddBool("AxePosition", "Draw Axe Position");
            MyMenuExtensions.DrawOption.AddDamageIndicatorToHero(true, false, true, true, true);

            MyMenuExtensions.AxeOption.GetKey("CancelKey1").ValueChanged += OnCancelValueChange;

            Game.OnUpdate += OnUpdate;
            Game.OnWndProc += OnWndProc;
            GameObject.OnCreate += (sender, args) => OnCreate(sender);
            GameObject.OnDelete += (sender, args) => OnDestroy(sender);
            //Gapcloser.OnGapcloser += OnGapcloser;
            Orbwalker.OnBeforeAttack += OnAction;
            Drawing.OnDraw += OnRender;
        }

        private static void OnCancelValueChange(object sender, EventArgs e)
        {
            var key = sender as MenuKeyBind;
            if (key != null && key.Active)
            {
                if (MyMenuExtensions.AxeOption.GetBool("CancelCatch").Enabled)
                {
                    if (Variables.GameTimeTickCount - lastCatchTime > 1800)
                    {
                        lastCatchTime = Variables.GameTimeTickCount;
                    }
                }
            }
        }

        private static void OnUpdate(EventArgs args)
        {
            foreach (var sender in AxeList.Where(x => x.Key.IsDead || !x.Key.IsValid).Select(x => x.Key))
            {
                AxeList.Remove(sender);
            }

            if (Me.IsDead || Me.IsRecalling())
            {
                return;
            }

            if (Me.IsWindingUp)
            {
                return;
            }

            R.Range = MyMenuExtensions.MiscOption.GetSlider("R", "GlobalRMax").Value;

            CatchAxeEvent();
            KillStealEvent();
            AutoUseEvent();

            switch (Orbwalker.ActiveMode)
            {
                case OrbwalkerMode.Combo:
                    ComboEvent();
                    break;
                case OrbwalkerMode.Harass:
                    HarassEvent();
                    break;
                case OrbwalkerMode.LaneClear:
                    ClearEvent();
                    break;
            }
        }

        private static void CatchAxeEvent()
        {
            if (AxeList.Count == 0)
            {
                Orbwalker.SetOrbwalkerPosition(Vector3.Zero);
                return;
            }

            if (MyMenuExtensions.AxeOption.GetList("CatchMode").Index == 2 ||
                MyMenuExtensions.AxeOption.GetList("CatchMode").Index == 1 && Orbwalker.ActiveMode != OrbwalkerMode.Combo)
            {
                Orbwalker.SetOrbwalkerPosition(Vector3.Zero);
                return;
            }

            var catchRange = MyMenuExtensions.AxeOption.GetSlider("CatchRange").Value;

            var bestAxe =
                AxeList.Where(x => !x.Key.IsDead && x.Key.IsValid && x.Key.Position.DistanceToCursor() <= catchRange)
                    .OrderBy(x => x.Value)
                    .ThenBy(x => x.Key.Position.DistanceToPlayer())
                    .ThenBy(x => x.Key.Position.DistanceToCursor())
                    .FirstOrDefault();

            if (bestAxe.Key != null)
            {
                if (MyMenuExtensions.AxeOption.GetBool("NotCatchTurret").Enabled &&
                    (Me.IsUnderEnemyTurret() && bestAxe.Key.Position.IsUnderEnemyTurret() ||
                     bestAxe.Key.Position.IsUnderEnemyTurret() && !Me.IsUnderEnemyTurret()))
                {
                    return;
                }

                if (MyMenuExtensions.AxeOption.GetSliderBool("NotCatchMoreEnemy").Enabled &&
                    (bestAxe.Key.Position.CountEnemyHeroesInRange(350) >=
                     MyMenuExtensions.AxeOption.GetSliderBool("NotCatchMoreEnemy").Value ||
                     GameObjects.EnemyHeroes.Count(x => x.Distance(bestAxe.Key.Position) < 350 && x.IsMelee) >=
                     MyMenuExtensions.AxeOption.GetSliderBool("NotCatchMoreEnemy").Value - 1))
                {
                    return;
                }

                if (MyMenuExtensions.AxeOption.GetBool("NotCatchKS").Enabled && Orbwalker.ActiveMode == OrbwalkerMode.Combo)
                {
                    var target = MyTargetSelector.GetTarget(800);

                    if (target != null && target.IsValidTarget(800) &&
                        target.DistanceToPlayer() > target.BoundingRadius + Me.BoundingRadius + 200 &&
                        target.Health < Me.GetAutoAttackDamage(target) * 2.5 - 80)
                    {
                        Orbwalker.SetOrbwalkerPosition(Vector3.Zero);
                        return;
                    }
                }

                if (MyMenuExtensions.AxeOption.GetBool("CatchWSpeed").Enabled && W.IsReady() &&
                    bestAxe.Key.Position.DistanceToPlayer() / Me.MoveSpeed * 1000 >= bestAxe.Value - Variables.GameTimeTickCount)
                {
                    W.Cast();
                }

                if (bestAxe.Key.Position.DistanceToPlayer() > 100)
                {
                    if (Variables.GameTimeTickCount - lastCatchTime > 1800)
                    {
                        if (Orbwalker.ActiveMode != OrbwalkerMode.None)
                        {
                            Orbwalker.SetOrbwalkerPosition(bestAxe.Key.Position);
                        }
                        else
                        {
                            Me.IssueOrder(GameObjectOrder.MoveTo, bestAxe.Key.Position);
                        }
                    }
                    else
                    {
                        if (Orbwalker.ActiveMode != OrbwalkerMode.None)
                        {
                            Orbwalker.SetOrbwalkerPosition(Vector3.Zero);
                        }
                    }
                }
                else
                {
                    if (Orbwalker.ActiveMode != OrbwalkerMode.None)
                    {
                        Orbwalker.SetOrbwalkerPosition(Vector3.Zero);
                    }
                }
            }
            else
            {
                if (Orbwalker.ActiveMode != OrbwalkerMode.None)
                {
                    Orbwalker.SetOrbwalkerPosition(Vector3.Zero);
                }
            }
        }

        private static void KillStealEvent()
        {
            if (MyMenuExtensions.KillStealOption.UseE && E.IsReady())
            {
                foreach (
                    var target in
                    GameObjects.EnemyHeroes.Where(
                        x =>
                            x.IsValidTarget(E.Range) && x.Health < Me.GetSpellDamage(x, SpellSlot.E) &&
                            !x.IsUnKillable()))
                {
                    if (target.IsValidTarget(E.Range))
                    {
                        var ePred = E.GetPrediction(target);

                        if (ePred.Hitchance >= HitChance.High)
                        {
                            E.Cast(ePred.CastPosition);
                        }
                    }
                }
            }

            if (MyMenuExtensions.KillStealOption.UseR && R.IsReady())
            {
                foreach (
                    var target in
                    GameObjects.EnemyHeroes.Where(
                        x =>
                            x.IsValidTarget(R.Range) &&
                            MyMenuExtensions.KillStealOption.GetKillStealTarget(x.CharacterName) &&
                            x.Health <
                            Me.GetSpellDamage(x, SpellSlot.R) +
                            Me.GetSpellDamage(x, SpellSlot.R) && !x.IsUnKillable()))
                {
                    if (target.IsValidTarget(R.Range) && !target.IsValidTarget(MyMenuExtensions.MiscOption.GetSlider("R", "GlobalRMin").Value))
                    {
                        var rPred = R.GetPrediction(target);

                        if (rPred.Hitchance >= HitChance.High)
                        {
                            R.Cast(rPred.CastPosition);
                        }
                    }
                }
            }
        }

        private static void AutoUseEvent()
        {
            if (MyMenuExtensions.MiscOption.GetKey("R", "SemiRKey").Active)
            {
                Me.IssueOrder(GameObjectOrder.MoveTo, Game.CursorPos);

                if (Me.Spellbook.GetSpell(SpellSlot.R).Level > 0 && R.IsReady())
                {
                    var target = MyTargetSelector.GetTarget(R.Range);
                    if (target.IsValidTarget(R.Range) && !target.IsValidTarget(MyMenuExtensions.MiscOption.GetSlider("R", "GlobalRMin").Value))
                    {
                        var rPred = R.GetPrediction(target);

                        if (rPred.Hitchance >= HitChance.High)
                        {
                            R.Cast(rPred.CastPosition);
                        }
                    }
                }
            }

            if (MyMenuExtensions.MiscOption.GetBool("W", "WSlow").Enabled && W.IsReady() && Me.HasBuffOfType(BuffType.Slow))
            {
                W.Cast();
            }
        }

        private static void ComboEvent()
        {
            var target = MyTargetSelector.GetTarget(E.Range);

            if (target != null && target.IsValidTarget(E.Range))
            {
                if (MyMenuExtensions.ComboOption.UseW && W.IsReady() && !Me.HasBuff("dravenfurybuff"))
                {
                    if (target.DistanceToPlayer() >= 600)
                    {
                        W.Cast();
                    }
                    else
                    {
                        if (target.Health <
                            (AxeCount > 0
                                ? Me.GetSpellDamage(target, SpellSlot.Q) * 5
                                : Me.GetAutoAttackDamage(target) * 5))
                        {
                            W.Cast();
                        }
                    }
                }

                if (MyMenuExtensions.ComboOption.UseE && E.IsReady())
                {
                    if (!target.InAutoAttackRange() ||
                        target.Health <
                        (AxeCount > 0
                            ? Me.GetSpellDamage(target, SpellSlot.Q) * 3
                            : Me.GetAutoAttackDamage(target) * 3) || Me.HealthPercent < 40)
                    {
                        var ePred = E.GetPrediction(target);

                        if (ePred.Hitchance >= HitChance.High)
                        {
                            E.Cast(ePred.CastPosition);
                        }
                    }
                }

                if (MyMenuExtensions.ComboOption.UseR && R.IsReady() && !target.IsValidTarget(MyMenuExtensions.MiscOption.GetSlider("R", "GlobalRMin").Value))
                {
                    if (MyMenuExtensions.ComboOption.GetBool("RSolo").Enabled)
                    {
                        if (target.Health <
                            Me.GetSpellDamage(target, SpellSlot.R) +
                            Me.GetSpellDamage(target, SpellSlot.R) +
                            (AxeCount > 0
                                ? Me.GetSpellDamage(target, SpellSlot.Q) * 2
                                : Me.GetAutoAttackDamage(target) * 2) +
                            (E.IsReady() ? Me.GetSpellDamage(target, SpellSlot.E) : 0) &&
                            target.Health >
                            (AxeCount > 0
                                ? Me.GetSpellDamage(target, SpellSlot.Q) * 3
                                : Me.GetAutoAttackDamage(target) * 3) &&
                            (Me.CountEnemyHeroesInRange(1000) == 1 ||
                             Me.CountEnemyHeroesInRange(1000) == 2 && Me.HealthPercent >= 60))
                        {
                            var rPred = R.GetPrediction(target);

                            if (rPred.Hitchance >= HitChance.High)
                            {
                                R.Cast(rPred.CastPosition);
                            }
                        }
                    }

                    if (MyMenuExtensions.ComboOption.GetBool("RTeam").Enabled)
                    {
                        if (Me.CountAllyHeroesInRange(1000) <= 3 && Me.CountEnemyHeroesInRange(1000) <= 3)
                        {
                            var rPred = R.GetPrediction(target);

                            if (rPred.Hitchance >= HitChance.Medium)
                            {
                                if (rPred.AoeTargetsHitCount >= 3)
                                {
                                    R.Cast(rPred.CastPosition);
                                }
                                else if (rPred.AoeTargetsHitCount >= 2)
                                {
                                    R.Cast(rPred.CastPosition);
                                }
                            }
                        }
                        else if (Me.CountAllyHeroesInRange(1000) <= 2 && Me.CountEnemyHeroesInRange(1000) <= 4)
                        {
                            var rPred = R.GetPrediction(target);

                            if (rPred.Hitchance >= HitChance.Medium)
                            {
                                if (rPred.AoeTargetsHitCount >= 3)
                                {
                                    R.Cast(rPred.CastPosition);
                                }
                            }
                        }
                    }
                }
            }
        }

        private static void HarassEvent()
        {
            if (MyMenuExtensions.HarassOption.HasEnouguMana() && MyMenuExtensions.HarassOption.UseE && E.IsReady())
            {
                var target = MyMenuExtensions.HarassOption.GetTarget(E.Range);

                if (target != null && target.IsValidTarget(E.Range))
                {
                    var ePred = E.GetPrediction(target);

                    if (ePred.Hitchance >= HitChance.High ||
                        ePred.Hitchance >= HitChance.Medium && ePred.AoeTargetsHitCount > 1)
                    {
                        E.Cast(ePred.CastPosition);
                    }
                }
            }
        }

        private static void ClearEvent()
        {
            if (MyManaManager.SpellHarass && Me.CountEnemyHeroesInRange(E.Range) > 0)
            {
                HarassEvent();
            }

            if (MyManaManager.SpellFarm)
            {
                LaneClearEvent();
                JungleClearEvent();
            }
        }

        private static void LaneClearEvent()
        {
            if (MyMenuExtensions.LaneClearOption.HasEnouguMana())
            {
                if (MyMenuExtensions.LaneClearOption.UseQ && Q.IsReady() && AxeCount < 2 && Orbwalker.CanAttack())
                {
                    var minions = GameObjects.EnemyMinions.Where(x => x.IsValidTarget(600) && x.IsMinion()).ToList();

                    if (minions.Any() && minions.Count >= 2)
                    {
                        Q.Cast();
                    }
                }

                if (MyMenuExtensions.LaneClearOption.GetSliderBool("LaneClearECount").Enabled && E.IsReady())
                {
                    var minions = GameObjects.EnemyMinions.Where(x => x.IsValidTarget(E.Range) && x.IsMinion()).ToList();

                    if (minions.Any() && minions.Count >= MyMenuExtensions.LaneClearOption.GetSliderBool("LaneClearECount").Value)
                    {
                        var eFarm = E.GetLineFarmLocation(minions);

                        if (eFarm.MinionsHit >= MyMenuExtensions.LaneClearOption.GetSliderBool("LaneClearECount").Value)
                        {
                            E.Cast(eFarm.Position);
                        }
                    }
                }
            }
        }

        private static void JungleClearEvent()
        {
            if (MyMenuExtensions.JungleClearOption.HasEnouguMana())
            {
                var mobs = GameObjects.Jungle.Where(x => x.IsValidTarget(E.Range) && x.GetJungleType() != JungleType.Unknown)
                                             .OrderByDescending(x => x.MaxHealth)
                                             .ToList();

                if (mobs.Any())
                {
                    var mob = mobs.FirstOrDefault();

                    if (MyMenuExtensions.JungleClearOption.UseE && E.IsReady() && mob != null && mob.IsValidTarget(E.Range))
                    {
                        E.CastIfHitchanceEquals(mob, HitChance.Medium);
                    }

                    if (MyMenuExtensions.JungleClearOption.UseW && W.IsReady() && !Me.HasBuff("dravenfurybuff") && AxeCount > 0)
                    {
                        foreach (
                            var m in
                            mobs.Where(
                                x =>
                                    x.DistanceToPlayer() <= 600 && !x.Name.ToLower().Contains("mini") &&
                                    !x.Name.ToLower().Contains("crab") && x.MaxHealth > 1500 &&
                                    x.Health > Me.GetAutoAttackDamage(x) * 2))
                        {
                            if (m.IsValidTarget(600))
                            {
                                W.Cast();
                            }
                        }
                    }

                    if (MyMenuExtensions.JungleClearOption.UseQ && Q.IsReady() && AxeCount < 2 && Orbwalker.CanAttack())
                    {
                        if (mobs.Count >= 2)
                        {
                            Q.Cast();
                        }

                        if (mobs.Count == 1 && mob != null && mob.InAutoAttackRange() && mob.Health > Me.GetAutoAttackDamage(mob) * 5)
                        {
                            Q.Cast();
                        }
                    }
                }
            }
        }

        private static void OnWndProc(GameWndEventArgs Args)
        {
            if (MyMenuExtensions.AxeOption.GetBool("CancelCatch").Enabled)
            {
                if (MyMenuExtensions.AxeOption.GetBool("CancelKey2").Enabled && (Args.Msg == 516 || Args.Msg == 517))
                {
                    if (Variables.GameTimeTickCount - lastCatchTime > 1800)
                    {
                        lastCatchTime = Variables.GameTimeTickCount;
                    }
                }

                if (MyMenuExtensions.AxeOption.GetBool("CancelKey3").Enabled && Args.Msg == 519)
                {
                    if (Variables.GameTimeTickCount - lastCatchTime > 1800)
                    {
                        lastCatchTime = Variables.GameTimeTickCount;
                    }
                }
            }
        }

        private static void OnCreate(GameObject sender)
        {
            if (sender != null && sender.Name.Contains("Draven_") && sender.Name.Contains("_Q_reticle_self"))
            {
                AxeList.Add(sender, Variables.GameTimeTickCount + 1800);
            }
        }

        private static void OnDestroy(GameObject sender)
        {
            if (sender != null && sender.Name.Contains("Draven_") && sender.Name.Contains("_Q_reticle_self"))
            {
                if (AxeList.Any(o => o.Key.NetworkId == sender.NetworkId))
                {
                    AxeList.Remove(sender);
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
        //                if (target.IsValidTarget(target.AttackRange + target.BoundingRadius + 100))
        //                {
        //                    var ePred = E.GetPrediction(target);
        //                    E.Cast(ePred.UnitPosition);
        //                }
        //                break;
        //            case SpellType.Dash:
        //            case SpellType.SkillShot:
        //            case SpellType.Targeted:
        //                {
        //                    var ePred = E.GetPrediction(target);
        //                    E.Cast(ePred.UnitPosition);
        //                }
        //                break;
        //        }
        //    }
        //}

        private static void OnAction(object sender, BeforeAttackEventArgs Args)
        {
            if (Args.Target == null || Args.Target.IsDead || !Args.Target.IsValidTarget() || Args.Target.Health <= 0 || !Q.IsReady())
            {
                return;
            }

            switch (Args.Target.Type)
            {
                case GameObjectType.AIHeroClient:
                {
                    var target = (AIHeroClient)Args.Target;
                    if (target != null && target.InAutoAttackRange())
                    {
                        if (Orbwalker.ActiveMode == OrbwalkerMode.Combo)
                        {
                            if (MyMenuExtensions.ComboOption.UseQ && MyMenuExtensions.AxeOption.GetSlider("CatchCount").Value >= AxeCount)
                            {
                                Q.Cast();
                            }
                        }
                        else if (Orbwalker.ActiveMode == OrbwalkerMode.Harass || Orbwalker.ActiveMode == OrbwalkerMode.LaneClear &&
                                 MyManaManager.SpellHarass)
                        {
                            if (MyMenuExtensions.HarassOption.HasEnouguMana() && MyMenuExtensions.HarassOption.GetHarassTargetEnabled(target.CharacterName))
                            {
                                if (MyMenuExtensions.HarassOption.UseQ)
                                {
                                    if (AxeCount < 2)
                                    {
                                        Q.Cast();
                                    }
                                }
                            }
                        }
                    }
                }
                    break;
            }
        }

        private static void OnRender(EventArgs args)
        {
            if (Me.IsDead || MenuGUI.IsChatOpen || MenuGUI.IsShopOpen)
            {
                return;
            }

            if (MyMenuExtensions.DrawOption.GetBool("AxeRange").Enabled)
            {
                CircleRender.Draw(Game.CursorPos, MyMenuExtensions.AxeOption.GetSlider("CatchRange").Value, System.Drawing.Color.FromArgb(0, 255, 161).ToSharpDxColor(), 1);
            }

            if (MyMenuExtensions.DrawOption.GetBool("AxePosition").Enabled)
            {
                foreach (var axe in AxeList.Where(x => !x.Key.IsDead && x.Key.IsValid).Select(x => x.Key))
                {
                    if (axe != null && axe.IsValid)
                    {
                        CircleRender.Draw(axe.Position, 130, System.Drawing.Color.FromArgb(86, 0, 255).ToSharpDxColor(), 1);
                    }
                }
            }
        }
    }
}