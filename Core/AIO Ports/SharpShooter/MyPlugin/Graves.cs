namespace SharpShooter.MyPlugin
{
    #region

    using System;
    using System.Linq;

    using EnsoulSharp;
    using EnsoulSharp.SDK;
    using EnsoulSharp.SDK.Utility;

    using SharpShooter.MyBase;
    using SharpShooter.MyCommon;

    using static SharpShooter.MyCommon.MyMenuExtensions;

    #endregion

    public class Graves : MyLogic
    {
        public Graves()
        {
            Initializer();
        }

        private static void Initializer()
        {
            Q = new Spell(SpellSlot.Q, 800f);
            Q.SetSkillshot(0.25f, 40f, 3000f, false,SpellType.Line);

            W = new Spell(SpellSlot.W, 900f);
            W.SetSkillshot(0.25f, 250f, 1000f, false,SpellType.Circle);

            E = new Spell(SpellSlot.E, 425f);

            R = new Spell(SpellSlot.R, 1050f);
            R.SetSkillshot(0.25f, 60f, 2100f, false,SpellType.Line);

            ComboOption.AddMenu();
            ComboOption.AddQ();
            ComboOption.AddW();
            ComboOption.AddE();
            ComboOption.AddBool("ComboEReset", "Use E|Reset Attack");
            ComboOption.AddBool("ComboECheck", "Use E|Check Safe");
            ComboOption.AddSliderBool("ComboRCount", "Use R| When Min Hit Count >= x", 4, 1, 5);

            HarassOption.AddMenu();
            HarassOption.AddQ();
            HarassOption.AddMana();
            HarassOption.AddTargetList();

            LaneClearOption.AddMenu();
            LaneClearOption.AddSliderBool("LaneClearQCount", "Use Q| Min Hit Count >= x", 3, 1, 5, true);
            LaneClearOption.AddE();
            LaneClearOption.AddMana();

            JungleClearOption.AddMenu();
            JungleClearOption.AddQ();
            JungleClearOption.AddW();
            JungleClearOption.AddE();
            JungleClearOption.AddMana();

            KillStealOption.AddMenu();
            KillStealOption.AddQ();
            KillStealOption.AddW();
            KillStealOption.AddR();
            KillStealOption.AddTargetList();

            //GapcloserOption.AddMenu();

            MiscOption.AddMenu();
            MiscOption.AddBasic();

            DrawOption.AddMenu();
            DrawOption.AddQ(Q);
            DrawOption.AddW(W);
            DrawOption.AddE(E);
            DrawOption.AddR(R);
            DrawOption.AddDamageIndicatorToHero(true, true, false, true, true);

            Game.OnUpdate += OnUpdate;
            Orbwalker.OnAfterAttack += OnAction;
            //Gapcloser.OnGapcloser += OnGapcloser;
            //AIBaseClient.OnBasicAttack += OnBasicAttack;
            Spellbook.OnCastSpell += OnCastSpell;
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
            if (KillStealOption.UseQ && Q.IsReady())
            {
                foreach (
                    var target in
                    GameObjects.EnemyHeroes.Where(
                        x => x.IsValidTarget(Q.Range) && x.Health < Me.GetSpellDamage(x, SpellSlot.Q)))
                {
                    if (target.IsValidTarget(Q.Range) && !target.IsUnKillable())
                    {
                        CastQ(target);
                    }
                }
            }

            if (KillStealOption.UseW && W.IsReady())
            {
                foreach (
                    var target in
                    GameObjects.EnemyHeroes.Where(
                        x => x.IsValidTarget(W.Range) && x.Health < Me.GetSpellDamage(x, SpellSlot.W)))
                {
                    if (target.IsValidTarget(W.Range) && !target.IsUnKillable())
                    {
                        var wPred = W.GetPrediction(target);

                        if (wPred.Hitchance >= HitChance.High)
                        {
                            W.Cast(wPred.UnitPosition);
                        }
                    }
                }
            }


            if (KillStealOption.UseR && R.IsReady())
            {
                foreach (
                    var target in
                    GameObjects.EnemyHeroes.Where(
                        x => x.IsValidTarget(R.Range) &&
                             KillStealOption.GetKillStealTarget(x.CharacterName.ToLower()) &&
                             x.Health < Me.GetSpellDamage(x, SpellSlot.R) &&
                             x.DistanceToPlayer() >
                             Me.AttackRange + Me.BoundingRadius + Me.BoundingRadius + 30))
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

            if (target.IsValidTarget(R.Range))
            {
                if (ComboOption.UseQ && Q.IsReady() && target.IsValidTarget(Q.Range))
                {
                    CastQ(target);
                }

                if (ComboOption.UseE && E.IsReady() && target.IsValidTarget(800f) &&
                    !target.IsValidTarget(Me.AttackRange + Me.BoundingRadius + target.BoundingRadius + 50))
                {
                    ELogic(target);
                }

                if (ComboOption.UseW && W.IsReady() && target.IsValidTarget(W.Range) &&
                    (target.DistanceToPlayer() <= target.AttackRange + 70 ||
                     target.DistanceToPlayer() >= Me.AttackRange + Me.BoundingRadius - target.BoundingRadius + 15 + 80))
                {
                    var wPred = W.GetPrediction(target);

                    if (wPred.Hitchance >= HitChance.High)
                    {
                        W.Cast(wPred.UnitPosition);
                    }
                }

                if (ComboOption.GetSliderBool("ComboRCount").Enabled && R.IsReady() && target.IsValidTarget(R.Range))
                {
                    var rPred = R.GetPrediction(target);

                    if (rPred.Hitchance >= HitChance.Medium)
                    {
                        if (rPred.AoeTargetsHitCount >= ComboOption.GetSliderBool("ComboRCount").Value)
                        {
                            R.Cast(rPred.CastPosition);
                        }
                    }
                }
            }
        }

        private static void Harass()
        {
            if (HarassOption.HasEnouguMana())
            {
                var target = HarassOption.GetTarget(Q.Range);

                if (HarassOption.UseQ && Q.IsReady() && target.IsValidTarget(Q.Range))
                {
                    CastQ(target);
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
            if (LaneClearOption.HasEnouguMana())
            {
                if (LaneClearOption.GetSliderBool("LaneClearQCount").Enabled && Q.IsReady())
                {
                    var minions = GameObjects.EnemyMinions.Where(x => x.IsValidTarget(Q.Range) && x.IsMinion()).ToList();

                    if (minions.Any())
                    {
                        var qFarm = Q.GetLineFarmLocation(minions);

                        if (qFarm.MinionsHit >= LaneClearOption.GetSliderBool("LaneClearQCount").Value)
                        {
                            Q.Cast(qFarm.Position);
                        }
                    }
                }
            }
        }

        private static void JungleClear()
        {
            if (JungleClearOption.HasEnouguMana())
            {
                if (JungleClearOption.UseQ && Q.IsReady())
                {
                    var mobs = GameObjects.Jungle.Where(x => x.IsValidTarget(Q.Range) && x.GetJungleType() != JungleType.Unknown).ToList();

                    if (mobs.Any())
                    {
                        var qFarm = Q.GetLineFarmLocation(mobs);

                        if (qFarm.MinionsHit >= 1)
                        {
                            Q.Cast(qFarm.Position);
                        }
                    }
                }
            }
        }


        private static void OnAction(object sender, AfterAttackEventArgs Args)
        {
            

            if (Args.Target == null || Args.Target.IsDead || !Args.Target.IsValidTarget() || Args.Target.Health <= 0 || !E.IsReady())
            {
                return;
            }

            switch (Args.Target.Type)
            {
                case GameObjectType.AIHeroClient:
                    {
                        if (Orbwalker.ActiveMode == OrbwalkerMode.Combo)
                        {
                            if (ComboOption.UseE && ComboOption.GetBool("ComboEReset").Enabled)
                            {
                                var target = (AIHeroClient)Args.Target;
                                if (target != null && !target.IsDead && target.IsValidTarget())
                                {
                                    ELogic(target);
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
                            if (mob != null && mob.IsValidTarget() && mob.GetJungleType() != JungleType.Unknown)
                            {
                                if (JungleClearOption.HasEnouguMana() && JungleClearOption.UseE)
                                {
                                    var mobs =
                                        GameObjects.Jungle.Where(x => x.IsValidTarget(800) && x.GetJungleType() != JungleType.Unknown)
                                            .Where(x => x.GetJungleType() != JungleType.Unknown)
                                            .ToList();

                                    if (mobs.Any() && mobs.FirstOrDefault() != null)
                                    {
                                        ELogic(mobs.FirstOrDefault());
                                    }
                                }
                            }
                        }
                    }
                    break;
            }
        }

        //private static void OnGapcloser(AIHeroClient target, GapcloserArgs Args)
        //{
        //    if (W.IsReady() && target != null && target.IsValidTarget(W.Range))
        //    {
        //        switch (Args.Type)
        //        {
        //            case SpellType.Melee:
        //                if (target.IsValidTarget(target.AttackRange + target.BoundingRadius + 100))
        //                {
        //                    var wPred = W.GetPrediction(target);
        //                    W.Cast(wPred.UnitPosition);
        //                }
        //                break;
        //            case SpellType.Dash:
        //            case SpellType.SkillShot:
        //            case SpellType.Targeted:
        //                {
        //                    var wPred = W.GetPrediction(target);
        //                    W.Cast(wPred.UnitPosition);
        //                }
        //                break;
        //        }
        //    }
        //}

        private static void OnBasicAttack(AIBaseClient sender, AIBaseClientProcessSpellCastEventArgs Args)
        {
            if (!sender.IsMe)
            {
                return;
            }

            if (!E.IsReady())
            {
                return;
            }

            if (Orbwalker.ActiveMode != OrbwalkerMode.Combo && Orbwalker.ActiveMode != OrbwalkerMode.LaneClear)
            {
                return;
            }

            var target = (AttackableUnit)Args.Target;
            if (target == null || !target.IsValidTarget())
            {
                return;
            }

            if (!Orbwalker.CanAttack() || Me.IsWindingUp || !target.IsValidTarget(Me.AttackRange + Me.BoundingRadius + target.BoundingRadius - 20))
            {
                return;
            }

            if (Orbwalker.ActiveMode == OrbwalkerMode.Combo && ComboOption.UseE &&
                ComboOption.GetBool("ComboEReset").Enabled && target.Type == GameObjectType.AIHeroClient)
            {
                DelayAction.Add(0, () =>
                {
                    if (ELogic((AIHeroClient)target))
                    {
                        Orbwalker.ResetAutoAttackTimer();
                    }
                });
            }
            else if (Orbwalker.ActiveMode == OrbwalkerMode.LaneClear && JungleClearOption.HasEnouguMana() &&
                     JungleClearOption.UseE && target is AIMinionClient)
            {
                var mob = (AIMinionClient)target;
                if (mob != null && mob.GetJungleType() != JungleType.Unknown && mob.GetJungleType() != JungleType.Small)
                {
                    DelayAction.Add(0, () =>
                    {
                        if (ELogic(mob))
                        {
                            Orbwalker.ResetAutoAttackTimer();
                        }
                    });
                }
            }
        }

        private static void OnCastSpell(Spellbook sender, SpellbookCastSpellEventArgs Args)
        {
            if (Args.Slot == SpellSlot.E)
            {
                DelayAction.Add(0, Orbwalker.ResetAutoAttackTimer);
            }
        }

        private static bool ELogic(AIBaseClient target)
        {
            if (!E.IsReady())
            {
                return false;
            }

            var ePosition = Me.Position.Extend(Game.CursorPos, E.Range);
            if (ePosition.IsUnderEnemyTurret() && Me.HealthPercent <= 50)
            {
                return false;
            }

            if (ComboOption.GetBool("ComboECheck").Enabled && Orbwalker.ActiveMode == OrbwalkerMode.Combo)
            {
                if (GameObjects.EnemyHeroes.Count(x => !x.IsDead && x.Distance(ePosition) <= 550) >= 3)
                {
                    return false;
                }

                //Catilyn W
                if (ObjectManager
                        .Get<GameObject>()
                        .FirstOrDefault(
                            x =>
                                x != null && x.IsValid &&
                                x.Name.ToLower().Contains("yordletrap_idle_red.troy") &&
                                x.Position.Distance(ePosition) <= 100) != null)
                {
                    return false;
                }

                //Jinx E
                if (ObjectManager.Get<AIMinionClient>()
                        .FirstOrDefault(x => x.IsValid && x.IsEnemy && x.Name == "k" &&
                                             x.Position.Distance(ePosition) <= 100) != null)
                {
                    return false;
                }

                //Teemo R
                if (ObjectManager.Get<AIMinionClient>()
                        .FirstOrDefault(x => x.IsValid && x.IsEnemy && x.Name == "Noxious Trap" &&
                                             x.Position.Distance(ePosition) <= 100) != null)
                {
                    return false;
                }
            }

            if (target.Distance(ePosition) > Me.AttackRange + Me.BoundingRadius + target.BoundingRadius + 15)
            {
                return false;
            }

            if (/*target.Health < Me.GetAutoAttackDamage(target) * 2 &&*/
                target.Distance(Me.Position.Extend(Game.CursorPos, E.Range)) <= Me.AttackRange + Me.BoundingRadius + target.BoundingRadius - 20)
            {
                return E.Cast(Me.Position.Extend(Game.CursorPos, E.Range));
            }

            if (/*!Me.HasBuff("gravesbasicattackammo2") && Me.HasBuff("gravesbasicattackammo1") &&*/
                     target.Distance(Me.Position.Extend(Game.CursorPos, E.Range)) <= Me.AttackRange + Me.BoundingRadius + target.BoundingRadius - 20)
            {
                return E.Cast(Me.Position.Extend(Game.CursorPos, E.Range));
            }

            if (/*!Me.HasBuff("gravesbasicattackammo2") && !Me.HasBuff("gravesbasicattackammo1") &&*/
                     target.IsValidTarget(Me.AttackRange + Me.BoundingRadius + target.BoundingRadius - 20))
            {
                return E.Cast(Me.Position.Extend(Game.CursorPos, E.Range));
            }

            return false;
        }

        private static void CastQ(AIBaseClient target)
        {
            if (target == null || !target.IsValidTarget(Q.Range))
            {
                return;
            }

            var from = Me.PreviousPosition.ToVector2();
            var to = target.PreviousPosition.ToVector2();
            var direction = (from - to).Normalized();
            var distance = from.Distance(to);

            for (var d = 0; d < distance; d = d + 20)
            {
                var point = from + d * direction;
                var flags = NavMesh.GetCollisionFlags(point.ToVector3());

                if (flags.HasFlag(CollisionFlags.Building) || flags.HasFlag(CollisionFlags.Wall))
                {
                    return;
                }
            }

            var qPred = Q.GetPrediction(target);
            if (qPred.Hitchance >= HitChance.High)
            {
                Q.Cast(qPred.UnitPosition);
            }
        }
    }
}