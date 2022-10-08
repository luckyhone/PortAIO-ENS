 using EnsoulSharp;
 using EnsoulSharp.SDK;
 using EnsoulSharp.SDK.MenuUI;
 using EnsoulSharp.SDK.Rendering;
 using SharpDX;

 namespace Flowers_Series.Plugings
{
    using Common;
    using System;
    using System.Linq;
    using static Common.Manager;

    public static class Blitzcrank
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
            Q = new Spell(SpellSlot.Q, 920);
            W = new Spell(SpellSlot.W);
            E = new Spell(SpellSlot.E, 150);
            R = new Spell(SpellSlot.R, 545);

            Q.SetSkillshot(0.25f, 80f, 1800f, true, SpellType.Line);

            var ComboMenu = Menu.Add(new Menu("Blitzcrank_Combo", "Combo"));
            {
                ComboMenu.Add(new MenuBool("Q", "Use Q", true));
                ComboMenu.Add(new MenuBool("W", "Use W", true));
                ComboMenu.Add(new MenuBool("E", "Use E", true));
                ComboMenu.Add(new MenuBool("R", "Use R", true));
            }

            var HarassMenu = Menu.Add(new Menu("Blitzcrank_Harass", "Harass"));
            {
                HarassMenu.Add(new MenuBool("Q", "Use Q", true));
            }

            var KillStealMenu = Menu.Add(new Menu("Blitzcrank_KillSteal", "KillSteal"));
            {
                KillStealMenu.Add(new MenuBool("Q", "Use Q", true));
                KillStealMenu.Add(new MenuBool("R", "Use R", true));
            }

            var MiscMenu = Menu.Add(new Menu("Blitzcrank_Misc", "Misc"));
            {
                MiscMenu.Add(new MenuBool("GapCloser", "Anti Gapcloser", true));
                MiscMenu.Add(new MenuBool("Interrupt", "Interrupt Spells", true));
            }

            var QBlackList = Menu.Add(new Menu("Blitzcrank_QBlackList", "Q BlackList"));
            {
                if (GameObjects.EnemyHeroes.Any())
                {
                    GameObjects.EnemyHeroes.ForEach(i => QBlackList.Add(new MenuBool(i.CharacterName.ToLower(), i.CharacterName, false)));
                }
            }

            var Draw = Menu.Add(new Menu("Blitzcrank_Draw", "Draw"));
            {
                Draw.Add(new MenuBool("Q", "Draw Q Range"));
                Draw.Add(new MenuBool("R", "Draw R Range"));
                Draw.Add(new MenuBool("Auto", "Draw Auto Q Status", true));
                Draw.Add(new MenuBool("Damage", "Draw Combo Damage", true));
            }

            Menu.Add(new MenuKeyBind("Blitzcrank_Key", "Auto Q", Keys.T, KeyBindType.Toggle));

            WriteConsole(GameObjects.Player.CharacterName + " Inject!");

            AntiGapcloser.OnGapcloser += OnGapCloser;
            Interrupter.OnInterrupterSpell += OnInterruptableTarget;
            Game.OnUpdate += OnUpdate;
            Drawing.OnDraw += OnDraw;
        }

        private static void OnDraw(EventArgs args)
        {
            if (Me.IsDead)
            {
                return;
            }

            if (Menu["Blitzcrank_Draw"]["Q"].GetValue<MenuBool>().Enabled && Q.IsReady())
                CircleRender.Draw(Me.Position, Q.Range, Color.AliceBlue);

            if (Menu["Blitzcrank_Draw"]["R"].GetValue<MenuBool>().Enabled && R.IsReady())
                CircleRender.Draw(Me.Position, R.Range, Color.AliceBlue);

            if (Menu["Blitzcrank_Draw"]["Auto"].GetValue<MenuBool>().Enabled)
            {
                var text = "";

                if (Menu["Blitzcrank_Key"].GetValue<MenuKeyBind>().Active)
                    text = "On";
                if (!Menu["Blitzcrank_Key"].GetValue<MenuKeyBind>().Active)
                    text = "Off";

                Drawing.DrawText(Me.HPBarPosition.X + 30, Me.HPBarPosition.Y - 40, System.Drawing.Color.Red, "Auto Q (" + Menu["Blitzcrank_Key"].GetValue<MenuKeyBind>().Key + "): ");
                Drawing.DrawText(Me.HPBarPosition.X + 115, Me.HPBarPosition.Y - 40, System.Drawing.Color.Yellow, text);
            }

            if (Menu["Blitzcrank_Draw"]["Damage"].GetValue<MenuBool>().Enabled)
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

        private static void OnUpdate(EventArgs Args)
        {
            if (Me.IsDead)
            {
                return;
            }

            if (InCombo)
            {
                ComboLogic();
            }

            if (InHarass)
            {
                HarassLogic();
            }

            if (Menu["Blitzcrank_Key"].GetValue<MenuKeyBind>().Active && Q.IsReady())
            {
                QKeyLogic();
            }

            KillStealLogic();
        }

        private static void ComboLogic()
        {
            var target = GetTarget(Q.Range, DamageType.Magical);

            if (CheckTarget(target))
            {
                if (Menu["Blitzcrank_Combo"]["Q"].GetValue<MenuBool>().Enabled && Q.IsReady() && 
                    !Menu["Blitzcrank_QBlackList"][target.CharacterName.ToLower()].GetValue<MenuBool>().Enabled &&
                    target.IsValidTarget(Q.Range))
                {
                    Q.Cast(target);
                }

                if (Menu["Blitzcrank_Combo"]["W"].GetValue<MenuBool>().Enabled && W.IsReady())
                {
                    if (!Menu["Blitzcrank_QBlackList"][target.CharacterName.ToLower()].GetValue<MenuBool>().Enabled && 
                        target.IsValidTarget(Q.Range))
                    {
                        W.Cast();
                    }

                    if (Me.HasBuffOfType(BuffType.Slow))
                    {
                        W.Cast();
                    }
                }

                if (Menu["Blitzcrank_Combo"]["E"].GetValue<MenuBool>().Enabled && E.IsReady() && 
                    target.DistanceToPlayer() <= GetAttackRange(Me) && !Q.IsReady())
                {
                    E.Cast();
                }

                if (Menu["Blitzcrank_Combo"]["R"].GetValue<MenuBool>().Enabled && R.IsReady() && target.IsValidTarget(R.Range))
                {
                    if (target.HasBuff("rocketgrab2"))
                    {
                        R.Cast();
                    }
                    else if (target.Health < R.GetDamage(target))
                    {
                        R.Cast();
                    }
                    else if (target.DistanceToPlayer() >= 500 && target.IsValidTarget(R.Range) &&
                        !Menu["Blitzcrank_QBlackList"][target.CharacterName.ToLower()].GetValue<MenuBool>().Enabled && Q.IsReady() && 
                        Q.GetPrediction(target).Hitchance >= HitChance.VeryHigh)
                    {
                        R.Cast();
                    }
                    else if (Me.CountEnemyHeroesInRange(R.Range) >= 3)
                    {
                        R.Cast();
                    }
                    else if (Me.CountEnemyHeroesInRange(R.Range) >= 2 && Me.IsMelee)
                    {
                        R.Cast();
                    }
                }
            }
        }

        private static void HarassLogic()
        {
            if (Menu["Blitzcrank_Harass"]["Q"].GetValue<MenuBool>().Enabled && Q.IsReady())
            {
                var target = GetTarget(Q, false);

                if (CheckTarget(target) && !Menu["Blitzcrank_QBlackList"][target.CharacterName.ToLower()].GetValue<MenuBool>().Enabled && 
                    target.IsValidTarget(Q.Range))
                {
                    Q.Cast(target);
                }
            }
        }

        private static void QKeyLogic()
        {
            var target = GetTarget(Q, false);

            if (CheckTarget(target) && Q.IsReady() && 
                !Menu["Blitzcrank_QBlackList"][target.CharacterName.ToLower()].GetValue<MenuBool>().Enabled && 
                target.IsValidTarget(Q.Range))
            {
                Q.Cast(target);
            }
        }

        private static void KillStealLogic()
        {
            var target = GetTarget(Q.Range, DamageType.Magical);

            if (CheckTarget(target))
            {
                if (Menu["Blitzcrank_KillSteal"]["Q"].GetValue<MenuBool>().Enabled && Q.IsReady() && 
                    target.Health < Q.GetDamage(target) &&
                    !Menu["Blitzcrank_QBlackList"][target.CharacterName.ToLower()].GetValue<MenuBool>().Enabled && target.IsValidTarget(Q.Range))
                {
                    Q.Cast(target);
                }

                if (Menu["Blitzcrank_KillSteal"]["R"].GetValue<MenuBool>().Enabled && R.IsReady() && target.Health < R.GetDamage(target))
                {
                    R.Cast();
                    return;
                }
            }
        }

        private static void OnInterruptableTarget(AIHeroClient sender, Interrupter.InterruptSpellArgs Args)
        {
            var target = Args.Sender;

            if (Menu["Blitzcrank_Misc"]["Interrupt"].GetValue<MenuBool>().Enabled && target.IsEnemy && target.IsValidTarget(R.Range) && R.IsReady())
            {
                if (Args.DangerLevel >= Interrupter.DangerLevel.High || 
                    target.IsCastingImporantSpell())
                {
                    R.Cast();
                }
            }
        }

        private static void OnGapCloser(AIHeroClient sender, AntiGapcloser.GapcloserArgs Args)
        {
            if (Args.Target.IsMe && Menu["Blitzcrank_Misc"]["GapCloser"].GetValue<MenuBool>().Enabled && 
                E.IsReady() && sender.DistanceToPlayer() <= GetAttackRange(Me))
            {
                E.CastOnUnit(sender);
                Orbwalker.ForceTarget = sender;
            }
        }
    }
}
