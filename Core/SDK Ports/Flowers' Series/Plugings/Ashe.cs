 using EnsoulSharp;
 using EnsoulSharp.SDK;
 using EnsoulSharp.SDK.MenuUI;
 using EnsoulSharp.SDK.Rendering;
 using HERMES_Kalista.MyLogic.Others;
 using SharpDX;

 namespace Flowers_Series.Plugings
{
    using Common;
    using System;
    using System.Linq;
    using static Common.Manager;

    public static class Ashe
    {
        private static Spell Q;
        private static Spell W;
        private static Spell E;
        private static Spell R;
        private static HpBarDraw HpBarDraw = new HpBarDraw();

        private static Menu Menu => Program.Menu;
        private static AIHeroClient Me => Program.Me;

        public static void Init()
        {
            Q = new Spell(SpellSlot.Q, GetAttackRange(Me));
            W = new Spell(SpellSlot.W, 1255f);
            E = new Spell(SpellSlot.E, 5000f);
            R = new Spell(SpellSlot.R, 2000f);

            W.SetSkillshot(0.25f, 60f, 2000f, true, SpellType.Cone);
            E.SetSkillshot(0.25f, 300f, 1400f, false, SpellType.Line);
            R.SetSkillshot(0.25f, 130f, 1600f, true, SpellType.Line);

            var ComboMenu = Menu.Add(new Menu("Ashe_Combo", "Combo"));
            {
                ComboMenu.Add(new MenuBool("Q", "Use Q", true));
                ComboMenu.Add(new MenuBool("SaveMana", "Save Mana To Cast W&R", true));
                ComboMenu.Add(new MenuBool("W", "Use W", true));
                ComboMenu.Add(new MenuBool("R", "Use R", true));
            }

            var HarassMenu = Menu.Add(new Menu("Ashe_Harass", "Harass"));
            {
                HarassMenu.Add(new MenuBool("W", "Use W", true));
                HarassMenu.Add(new MenuSlider("WMana", "Min Harass Mana", 60));
            }

            var ClearMenu = Menu.Add(new Menu("Ashe_Clear", "Clear"));
            {
                ClearMenu.Add(new MenuSeparator("LaneClear Settings", "LaneClear Settings"));
                ClearMenu.Add(new MenuBool("LCW", "Use W", true));
                ClearMenu.Add(new MenuSlider("LCWMana", "If Player ManaPercent >= %", 50, 0, 100));
                ClearMenu.Add(new MenuSlider("LCWCount", "If W CanHit Counts >= ", 3, 1, 10));
                ClearMenu.Add(new MenuSeparator("JungleClear Settings", "JungleClear Settings"));
                ClearMenu.Add(new MenuSliderButton("JCQ", "Use Q | If Player ManaPercent >= %", 30, 0, 100, true));
                ClearMenu.Add(new MenuSliderButton("JCW", "Use W | If Player ManaPercent >= %", 30, 0, 100, true));
            }

            var EMenu = Menu.Add(new Menu("Ashe_ESettings", "E Settings"));
            {
                EMenu.Add(new MenuBool("AutoE", "Auto E", true));
            }

            var RMenu = Menu.Add(new Menu("Ashe_RSettings", "R Settings"));
            {
                RMenu.Add(new MenuBool("AutoR", "Auto", true));
                RMenu.Add(new MenuBool("Interrupt", "Interrupt Danger Spells", true));
                RMenu.Add(new MenuKeyBind("R", "R Key", Keys.T, KeyBindType.Press));
                RMenu.Add(new MenuSliderButton("AntiGapCloser", "Anti GapCloser | If Player HealthPercent <= %", 20, 0, 80, true));
                RMenu.Add(new MenuSeparator("AntiGapCloserRList", "AntiGapCloser R List:"));
                if (GameObjects.EnemyHeroes.Any())
                {
                    GameObjects.EnemyHeroes.ForEach(i => RMenu.Add(new MenuBool(i.CharacterName.ToLower(), i.CharacterName, AutoEnableList.Contains(i.CharacterName))));
                }
            }

            var KillStealMenu = Menu.Add(new Menu("Ashe_KillSteal", "KillSteal"));
            {
                KillStealMenu.Add(new MenuBool("KSW", "KillSteal W", true));
                KillStealMenu.Add(new MenuBool("KSR", "KillSteal R", true));
                KillStealMenu.Add(new MenuSeparator("KillStealRList", "KillSteal R List:"));
                if (GameObjects.EnemyHeroes.Any())
                {
                    GameObjects.EnemyHeroes.ForEach(i => KillStealMenu.Add(new MenuBool(i.CharacterName.ToLower(), i.CharacterName, AutoEnableList.Contains(i.CharacterName))));
                }
            }

            var DrawMenu = Menu.Add(new Menu("Ashe_Draw", "Drawings"));
            {
                DrawMenu.Add(new MenuBool("W", "W"));
                DrawMenu.Add(new MenuBool("Damage", "Draw Combo Damage", true));
            }

            WriteConsole(GameObjects.Player.CharacterName + " Inject!");

            AntiGapcloser.OnGapcloser += OnGapCloser;
            Interrupter.OnInterrupterSpell += OnInterruptableTarget;
            Game.OnUpdate += OnUpdate;
            Drawing.OnDraw += Drawing_OnDraw;
            Orbwalker.OnBeforeAttack += OnBeforeAction;
            Orbwalker.OnAfterAttack += OnAfterAction;
        }

        private static void OnAfterAction(object sender, AfterAttackEventArgs Args)
        {
            if (InCombo)
                {
                    var target = GetTarget(Q);

                    if (CheckTarget(target))
                    {
                        if (Menu["Ashe_Combo"]["Q"].GetValue<MenuBool>().Enabled && target.IsValidTarget(GetAttackRange(Me)) && Q.IsReady() && Me.HasBuff("asheqcastready"))
                        {
                            if (Menu["Ashe_Combo"]["SaveMana"].GetValue<MenuBool>().Enabled && Me.Mana < R.Instance.ManaCost + W.Instance.ManaCost + Q.Instance.ManaCost)
                            {
                                return;
                            }

                            Q.Cast();
                            Orbwalker.ResetAutoAttackTimer();
                            return;
                        }

                        if (Menu["Combo"]["W"].GetValue<MenuBool>().Enabled && W.IsReady() && target.IsValidTarget(GetAttackRange(Me)))
                        {
                            var WPred = W.GetPrediction(target);

                            if (WPred.Hitchance >= HitChance.VeryHigh)
                            {
                                if (W.Cast(WPred.CastPosition))
                                {
                                    Orbwalker.ResetAutoAttackTimer();;
                                    return;
                                }
                            }
                        }
                    }
                }

                if (InClear)
                {
                    var Mobs = GetMobs(Me.Position, GetAttackRange(Me));

                    foreach (var mob in Mobs)
                    {
                        if (mob != null)
                        {
                            if (Menu["Ashe_Clear"]["JCQ"].GetValue<MenuSliderButton>().Enabled &&
                                mob.IsValidTarget(GetAttackRange(Me)) && Q.IsReady() && 
                                Me.HasBuff("asheqcastready") && mob.Health > Me.GetAutoAttackDamage(mob) * 2)
                            {
                                if (Menu["Ashe_Clear"]["JCQ"].GetValue<MenuSliderButton>().Value < Me.ManaPercent)
                                {
                                    Q.Cast();
                                    Orbwalker.ResetAutoAttackTimer();
                                    return;
                                }
                            }
                        }
                    }
                }
        }

        private static void OnBeforeAction(object sender, BeforeAttackEventArgs Args)
        {
            if (InCombo)
            {
                var target = GetTarget(GetAttackRange(Me));

                if (Menu["Ashe_Combo"]["Q"].GetValue<MenuBool>().Enabled && CheckTarget(target) &&
                    target.IsValidTarget(GetAttackRange(Me)) && 
                    Q.IsReady() && Me.HasBuff("asheqcastready"))
                {
                    if (Menu["Ashe_Combo"]["SaveMana"].GetValue<MenuBool>().Enabled && Me.Mana <
                        R.Instance.ManaCost + W.Instance.ManaCost + Q.Instance.ManaCost)
                    {
                        return;
                    }

                    Q.Cast();
                    return;
                }
            }

            if (Args.Target is AIMinionClient && InClear)
            {
                var Mobs = GetMobs(Me.Position, GetAttackRange(Me));

                foreach (var mob in Mobs)
                {
                    if (mob != null)
                    {
                        if (Menu["Ashe_Clear"]["JCQ"].GetValue<MenuSliderButton>().Enabled && mob.IsValidTarget(GetAttackRange(Me)) &&
                            Q.IsReady() && Me.HasBuff("asheqcastready") && mob.Health > Me.GetAutoAttackDamage(mob) * 2)
                        {
                            if (Menu["Ashe_Clear"]["JCQ"].GetValue<MenuSliderButton>().Value < Me.ManaPercent)
                            {
                                Q.Cast();
                                return;
                            }
                        }
                    }
                }
            }
        }


        private static void OnInterruptableTarget(AIHeroClient sender, Interrupter.InterruptSpellArgs Args)
        {
            if (Menu["Ashe_RSettings"]["Interrupt"].GetValue<MenuBool>().Enabled && R.IsReady() && Args.Sender.IsEnemy &&
                Args.DangerLevel >= Interrupter.DangerLevel.High && 
                Args.Sender.IsValidTarget(R.Range))
            {
                var RPred = R.GetPrediction(Args.Sender);

                if (RPred.Hitchance >= HitChance.VeryHigh)
                {
                    R.Cast(RPred.CastPosition);
                    return;
                }
            }
        }

        private static void OnGapCloser(AIHeroClient sender, AntiGapcloser.GapcloserArgs Args)
        {
            if (sender.IsEnemy && sender is AIHeroClient)
            {
                if (Menu["Ashe_RSettings"]["AntiGapCloser"].GetValue<MenuSliderButton>().Enabled && 
                    Me.HealthPercent <= Menu["Ashe_RSettings"]["AntiGapCloser"].GetValue<MenuSliderButton>().Value && 
                    R.IsReady() && 
                    Menu["Ashe_RSettings"][sender.CharacterName.ToLower()].GetValue<MenuBool>().Enabled &&
                    Args.EndPosition.DistanceToPlayer() <= 300 && 
                    sender.IsValidTarget(R.Range))
                {
                    R.Cast(sender);
                }
            }
        }

        private static void OnUpdate(EventArgs args)
        {
            Q.Range = GetAttackRange(Me);

            if (Me.IsDead)
            {
                return;
            }

            SeMiRLogic();

            if (InCombo)
            {
                ComboLogic();
            }

            if (InHarass)
            {
                HarassLogic();
            }

            if (InClear)
            {
                LaneLogic();
                JungleLogic();
            }

            AutoRLogic();
            KillStealLogic();

            if (Menu["Ashe_ESettings"]["AutoE"].GetValue<MenuBool>().Enabled && E.IsReady())
            {
                var target = GetTarget(1000);

                if (CheckTarget(target))
                {
                    var EPred = E.GetPrediction(target);
                    var Col = NavMesh.GetCollisionFlags(EPred.CastPosition);

                    if (Col == CollisionFlags.Grass && !target.IsVisible)
                    {
                        E.Cast(EPred.CastPosition);
                    }
                }
            }
        }

        private static void SeMiRLogic()
        {
            if (Menu["Ashe_RSettings"]["R"].GetValue<MenuKeyBind>().Active && R.IsReady())
            {
                var select = TargetSelector.SelectedTarget;
                var target = TargetSelector.GetTarget(R.Range,DamageType.Mixed);
                
                if (select != null && !target.HasBuffOfType(BuffType.SpellShield) && target.IsValidTarget(R.Range))
                {
                    var RPred = R.GetPrediction(target);

                    if (RPred.Hitchance >= HitChance.VeryHigh)
                    {
                        R.Cast(RPred.CastPosition);
                        return;
                    }
                }
                else if (select == null && target != null && !target.HasBuffOfType(BuffType.SpellShield) && target.IsValidTarget(R.Range))
                {
                    var RPred = R.GetPrediction(target);

                    if (RPred.Hitchance >= HitChance.VeryHigh)
                    {
                        R.Cast(RPred.CastPosition);
                        return;
                    }
                }
            }
        }

        private static void ComboLogic()
        {
            if (Menu["Ashe_Combo"]["W"].GetValue<MenuBool>().Enabled && W.IsReady() && !Me.HasBuff("AsheQAttack"))
            {
                var target = GetTarget(W);

                if (CheckTarget(target))
                {
                    var WPred = W.GetPrediction(target);

                    if (WPred.Hitchance >= HitChance.VeryHigh)
                    {
                        W.Cast(WPred.CastPosition);
                    }
                }
            }

            if (Menu["Ashe_Combo"]["R"].GetValue<MenuBool>().Enabled && R.IsReady())
            {
                foreach (var target in GameObjects.EnemyHeroes.Where(x => x.IsValidTarget(1200) && !x.IsDead && !x.IsZombie() && x.IsHPBarRendered))
                {
                    if (target != null)
                    {
                        if (target.IsValidTarget(600) && Me.CountEnemyHeroesInRange(600) >= 3 && target.CountAllyHeroesInRange(200) >= 2)
                        {
                            var RPred = R.GetPrediction(target);

                            if (RPred.Hitchance >= HitChance.VeryHigh)
                            {
                                R.Cast(RPred.CastPosition);
                            }
                        }

                        if (target.DistanceToPlayer() > GetAttackRange(Me) && target.DistanceToPlayer() <= 700 &&
                            target.Health > Me.GetAutoAttackDamage(target) &&
                            target.Health < R.GetDamage(target) + Me.GetAutoAttackDamage(target) * 3 && 
                            !target.HasBuffOfType(BuffType.SpellShield))
                        {
                            var RPred = R.GetPrediction(target);

                            if (RPred.Hitchance >= HitChance.VeryHigh)
                            {
                                R.Cast(RPred.CastPosition);
                            }
                        }

                        if (target.DistanceToPlayer() <= 1000 && 
                            (!target.CanMove || target.HasBuffOfType(BuffType.Stun) || R.GetPrediction(target).Hitchance == HitChance.Immobile))
                        {
                            R.Cast(target);
                        }

                        if (Me.CountEnemyHeroesInRange(800) == 1 && target.IsValidTarget(800) && 
                            target.Health <= Me.GetAutoAttackDamage(target) * 4 + R.GetDamage(target))
                        {
                            var RPred = R.GetPrediction(target);

                            if (RPred.Hitchance >= HitChance.VeryHigh)
                            {
                                R.Cast(RPred.CastPosition);
                            }
                        }
                    }
                }
            }
        }

        private static void HarassLogic()
        {
            if (Menu["Ashe_Harass"]["W"].GetValue<MenuBool>().Enabled && Menu["Ashe_Harass"]["WMana"].GetValue<MenuSlider>().Value <= Me.ManaPercent && W.IsReady() && !Me.HasBuff("AsheQAttack"))
            {
                var target = GetTarget(W);

                if (CheckTarget(target))
                {
                    var WPred = W.GetPrediction(target);

                    if (WPred.Hitchance >= HitChance.VeryHigh)
                    {
                        W.Cast(WPred.CastPosition);
                    }
                }
            }
        }

        private static void LaneLogic()
        {
            var Minions = GetMinions(Me.Position, W.Range);

            if (Minions.Count() > 0)
            {
                if (Menu["Ashe_Clear"]["LCW"].GetValue<MenuBool>().Enabled && W.IsReady() && 
                    Menu["Ashe_Clear"]["LCWMana"].GetValue<MenuSlider>().Value <= Me.ManaPercent)
                {
                    var WFarm = W.GetLineFarmLocation(Minions);

                    if (WFarm.MinionsHit >= Menu["Ashe_Clear"]["LCWCount"].GetValue<MenuSlider>().Value)
                    {
                        W.Cast(WFarm.Position);
                    }
                }
            }
        }

        private static void JungleLogic()
        {
            var Mobs = GetMobs(Me.Position, W.Range);

            if (Mobs.Count() > 0)
            {
                if (Menu["Ashe_Clear"]["JCW"].GetValue<MenuSliderButton>().Enabled &&
                    Me.ManaPercent >= Menu["Ashe_Clear"]["JCW"].GetValue<MenuSliderButton>().Value &&
                    !Me.HasBuff("AsheQAttack"))
                {
                    W.Cast(Mobs.FirstOrDefault().Position);
                }
            }
        }

        private static void AutoRLogic()
        {
            if (Menu["Ashe_RSettings"]["AutoR"].GetValue<MenuBool>().Enabled && R.IsReady())
            {
                foreach (var target in GameObjects.EnemyHeroes.Where(x => x.IsValidTarget(R.Range) && !x.IsDead && !x.IsZombie() && x.IsHPBarRendered))
                {
                    if (target != null)
                    {
                        if (target.DistanceToPlayer() > GetAttackRange(Me) && target.DistanceToPlayer() <= 700 &&
                            target.Health > Me.GetAutoAttackDamage(target) && 
                            target.Health < R.GetDamage(target) + Me.GetAutoAttackDamage(target) * 3 && 
                            !target.HasBuffOfType(BuffType.SpellShield))
                        {
                            var RPred = R.GetPrediction(target);

                            if (RPred.Hitchance >= HitChance.VeryHigh)
                            {
                                R.Cast(RPred.CastPosition);
                                return;
                            }
                        }
                    }
                }
            }
        }

        private static void KillStealLogic()
        {
            if (Menu["Ashe_KillSteal"]["KSW"].GetValue<MenuBool>().Enabled && W.IsReady())
            {
                foreach (var target in GameObjects.EnemyHeroes.Where(x => x.IsValidTarget(W.Range) && !x.IsDead && !x.IsZombie() && x.IsHPBarRendered))
                {
                    if (target.IsValid && target.Health < W.GetDamage(target))
                    {
                        if(target.DistanceToPlayer() <= GetAttackRange(Me) && Me.HasBuff("AsheQAttack"))
                        {
                            return;
                        }

                        var WPred = W.GetPrediction(target);

                        if (WPred.Hitchance >= HitChance.VeryHigh)
                        {
                            W.Cast(WPred.CastPosition);
                            return;
                        }
                    }
                }
            }

            if (Menu["Ashe_KillSteal"]["KSR"].GetValue<MenuBool>().Enabled && R.IsReady())
            {
                foreach (var target in GameObjects.EnemyHeroes.Where(x => x.IsValidTarget(2000) && !x.IsDead && !x.IsZombie() && x.IsHPBarRendered && Menu["Ashe_KillSteal"][x.CharacterName.ToLower()].GetValue<MenuBool>().Enabled))
                {
                    if (target.DistanceToPlayer() > 800 && target.IsValid && target.Health < R.GetDamage(target) && !target.HasBuffOfType(BuffType.SpellShield))
                    {
                        var RPred = R.GetPrediction(target);

                        if (RPred.Hitchance >= HitChance.VeryHigh)
                        {
                            R.Cast(RPred.CastPosition);
                            return;
                        }
                    }
                }
            }
        }

        private static void Drawing_OnDraw(EventArgs Args)
        {
            if (Me.IsDead)
                return;

            if (Menu["Ashe_Draw"]["W"].GetValue<MenuBool>().Enabled && W.IsReady())
            {
                CircleRender.Draw(Me.Position, W.Range, Color.OrangeRed, 2);
            }

            if (Menu["Ashe_Draw"]["Damage"].GetValue<MenuBool>().Enabled)
            {
                foreach (var target in GameObjects.EnemyHeroes.Where(x => x.IsValidTarget() && !x.IsDead && !x.IsZombie()))
                {
                    if (target != null)
                    {
                        HpBarDraw.Unit = target;

                        HpBarDraw.DrawDmg((float)GetDamage(target), new SharpDX.ColorBGRA(255, 200, 0, 170));
                    }
                }
            }
        }
    }
}
