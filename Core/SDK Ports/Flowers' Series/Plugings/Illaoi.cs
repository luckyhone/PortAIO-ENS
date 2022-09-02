using System;
using System.Linq;
using EnsoulSharp;
using EnsoulSharp.SDK;
using EnsoulSharp.SDK.MenuUI;
using EnsoulSharp.SDK.Rendering;
using Flowers_Series.Common;
using SharpDX;
using static Flowers_Series.Common.Manager;

namespace Flowers_Series.Plugings
{
    public class Illaoi
    {
        private static Spell Q;
        private static Spell W;
        private static Spell E;
        private static Spell R;
        private static HpBarDraw DrawHpBar = new HpBarDraw();

        private static AIHeroClient Me => Program.Me;

        private static Menu Menu => Program.Menu;

        public static void Init()
        {
            Q = new Spell(SpellSlot.Q, 800f);
            W = new Spell(SpellSlot.W, 400f);
            E = new Spell(SpellSlot.E, 900f);
            R = new Spell(SpellSlot.R, 450f);

            Q.SetSkillshot(0.75f, 100f, float.MaxValue, false, SpellType.Line);
            E.SetSkillshot(0.066f, 50f, 1900f, true, SpellType.Line);

            var ComboMenu = Menu.Add(new Menu("Illaoi_Combo", "Combo"));
            {
                ComboMenu.Add(new MenuBool("Q", "Use Q", true));
                ComboMenu.Add(new MenuBool("QGhost", "Use Q | To Ghost", true));
                ComboMenu.Add(new MenuBool("W", "Use W", true));
                ComboMenu.Add(new MenuBool("WOutRange", "Use W | Out of Attack Range"));
                ComboMenu.Add(new MenuBool("WUlt", "Use W | Ult Active", true));
                ComboMenu.Add(new MenuBool("E", "Use E", true));
                ComboMenu.Add(new MenuBool("R", "Use R", true));
                ComboMenu.Add(new MenuBool("RSolo", "Use R | 1v1 Mode", true));
                ComboMenu.Add(new MenuSlider("RCount", "Use R | Counts Enemies >=", 2, 1, 5));
            }

            var HarassMenu = Menu.Add(new Menu("Illaoi_Harass", "Harass"));
            {
                HarassMenu.Add(new MenuBool("Q", "Use Q", true));
                HarassMenu.Add(new MenuBool("W", "Use W", true));
                HarassMenu.Add(new MenuBool("WOutRange", "Use W | Only Out of Attack Range", true));
                HarassMenu.Add(new MenuBool("E", "Use E", true));
                HarassMenu.Add(new MenuBool("Ghost", "Attack Ghost", true));
                HarassMenu.Add(new MenuSlider("Mana", "Harass Mode | Min ManaPercent >= %", 60));
            }

            var LaneClearMenu = Menu.Add(new Menu("Illaoi_LaneClear", "Lane Clear"));
            {
                LaneClearMenu.Add(new MenuSliderButton("Q", "Use Q | Min Hit Count >= ", 3, 1, 5, true));
                LaneClearMenu.Add(new MenuSlider("Mana", "LaneClear Mode | Min ManaPercent >= %", 60));
            }

            var JungleClearMenu = Menu.Add(new Menu("Illaoi_JungleClear", "Jungle Clear"));
            {
                JungleClearMenu.Add(new MenuBool("Q", "Use Q", true));
                JungleClearMenu.Add(new MenuBool("W", "Use W", true));
                JungleClearMenu.Add(new MenuBool("Item", "Use Item", true));
                JungleClearMenu.Add(new MenuSlider("Mana", "JungleClear Mode | Min ManaPercent >= %", 60));
            }

            var KillStealMenu = Menu.Add(new Menu("Illaoi_KillSteal", "Kill Steal"));
            {
                KillStealMenu.Add(new MenuBool("Q", "Use Q", true));
                KillStealMenu.Add(new MenuBool("E", "Use E", true));
                KillStealMenu.Add(new MenuBool("R", "Use R", true));
                KillStealMenu.Add(new MenuSeparator("RList", "R List"));
                if (GameObjects.EnemyHeroes.Any())
                {
                    GameObjects.EnemyHeroes.ForEach(i => KillStealMenu.Add(new MenuBool(i.CharacterName.ToLower(), i.CharacterName)));
                }
            }

            var EBlacklist = Menu.Add(new Menu("Illaoi_EBlackList", "E BlackList"));
            {
                EBlacklist.Add(new MenuSeparator("Adapt", "Only Adapt to Harass & KillSteal & Anti GapCloser Mode!"));
                if (GameObjects.EnemyHeroes.Any())
                {
                    GameObjects.EnemyHeroes.ForEach(i => EBlacklist.Add(new MenuBool(i.CharacterName.ToLower(), i.CharacterName, false)));
                }
            }

            var MiscMenu = Menu.Add(new Menu("Illaoi_Misc", "Misc"));
            {
                MiscMenu.Add(new MenuBool("EGap", "Use E Anti GapCloset", true));
            }

            var DrawMenu = Menu.Add(new Menu("Illaoi_Draw", "Draw"));
            {
                DrawMenu.Add(new MenuBool("Q", "Q Range"));
                DrawMenu.Add(new MenuBool("W", "W Range"));
                DrawMenu.Add(new MenuBool("E", "E Range"));
                DrawMenu.Add(new MenuBool("R", "R Range"));
                DrawMenu.Add(new MenuBool("DrawDamage", "Draw Combo Damage", true));
            }

            WriteConsole(GameObjects.Player.CharacterName + " Inject!");

            AntiGapcloser.OnGapcloser += OnGapCloser;
            AIBaseClient.OnDoCast += OnSpellCast;
            Orbwalker.OnBeforeAttack += OnAction;
            Orbwalker.OnAfterAttack += OnAfterAttack;
            Game.OnUpdate += OnUpdate;
            Drawing.OnDraw += OnDraw;
            Drawing.OnEndScene += OnEndScene;
        }

        private static void OnEndScene(EventArgs args)
        {
            if (Me.IsDead)
            {
                return;
            }
            if (Menu["Illaoi_Draw"].GetValue<MenuBool>("DrawDamage").Enabled)
            {
                foreach (var target in ObjectManager.Get<AIHeroClient>().Where(e => e.IsValidTarget() && e.IsValid && !e.IsDead && !e.IsZombie()))
                {
                    HpBarDraw.Unit = target;
                    HpBarDraw.DrawDmg((float)GetDamage(target), new SharpDX.ColorBGRA(255, 204, 0, 170));
                }
            }
        }

        private static void OnUpdate(EventArgs args)
        {
            if (Me.IsDead)
            {
                return;
            }

            if (InCombo)
            {
                Combo();
            }

            if (InHarass)
            {
                Harass();
            }

            if (InClear)
            {
                Lane();
                Jungle();
            }

            if (InLastHit)
            {
                LastHitLogic();
            }

            KillSteal();
        }

        private static void OnDraw(EventArgs args)
        {
            if (Me.IsDead)
            {
                return;
            }

            if (Menu["Illaoi_Draw"].GetValue<MenuBool>("Q").Enabled && Q.IsReady())
                CircleRender.Draw(Me.Position, Q.Range, Color.AliceBlue, 2);

            if (Menu["Illaoi_Draw"].GetValue<MenuBool>("W").Enabled && (W.IsReady() || Me.HasBuff("IllaoiW")))
                CircleRender.Draw(Me.Position, W.Range, Color.LightSeaGreen, 2);

            if (Menu["Illaoi_Draw"].GetValue<MenuBool>("E").Enabled && E.IsReady())
                CircleRender.Draw(Me.Position, E.Range, Color.LightYellow, 2);

            if (Menu["Illaoi_Draw"].GetValue<MenuBool>("R").Enabled && E.IsReady())
                CircleRender.Draw(Me.Position, R.Range, Color.OrangeRed, 2);
        }

        private static void Combo()
        {
            var target = GetTarget(Q.Range, DamageType.Physical);
            var Ghost = ObjectManager.Get<AIMinionClient>().Where(x => x.IsEnemy && x.IsHPBarRendered && x.IsValidTarget(Q.Range) && x.HasBuff("illaoiespirit")).FirstOrDefault();
            if (CheckTarget(target) && target.IsValidTarget(Q.Range))
            {
                if (Menu["Illaoi_Combo"].GetValue<MenuBool>("Q").Enabled && Q.IsReady() && target.IsValidTarget(Q.Range) && Q.CanCast(target) && !((W.IsReady() || Me.HasBuff("IllaoiW")) && target.IsValidTarget(W.Range)))
                {
                    Q.Cast(target);
                }

                if (Menu["Illaoi_Combo"].GetValue<MenuBool>("E").Enabled && E.IsReady() && !InAutoAttackRange(target) && target.IsValidTarget(E.Range))
                {
                    E.Cast(target);
                }

                if (Menu["Illaoi_Combo"].GetValue<MenuBool>("R").Enabled && R.IsReady() && target.IsValidTarget(R.Range))
                {
                    if (Menu["Illaoi_Combo"].GetValue<MenuBool>("RSolo").Enabled && target.Health <= GetDamage(target) + Me.GetAutoAttackDamage(target) * 3 && Me.CountEnemyHeroesInRange(R.Range) == 1)
                    {
                        R.Cast(target);
                    }

                    if (Me.CountEnemyHeroesInRange(R.Range - 50) >= Menu["Illaoi_Combo"].GetValue<MenuSlider>("RCount").Value)
                    {
                        R.Cast();
                    }
                }
            }
            else if (target == null && Ghost != null)
            {
                if (Q.IsReady() && Menu["Illaoi_Combo"].GetValue<MenuBool>("Q").Enabled && Menu["Illaoi_Combo"].GetValue<MenuBool>("QGhost").Enabled)
                {
                    Q.Cast(Ghost.Position);
                }
            }
        }

        private static void Harass()
        {
            if (Me.ManaPercent >= Menu["Illaoi_Harass"]["Mana"].GetValue<MenuSlider>().Value && !Me.IsUnderEnemyTurret())
            {
                var target = GetTarget(Q.Range, DamageType.Physical);
                var Ghost = ObjectManager.Get<AIMinionClient>().Where(x => x.IsEnemy && x.IsHPBarRendered && x.IsValidTarget(Q.Range)).FirstOrDefault(x => x.HasBuff("illaoiespirit"));

                if (CheckTarget(target) && target.IsValidTarget(Q.Range))
                {
                    if (Menu["Illaoi_Harass"].GetValue<MenuBool>("E").Enabled && E.IsReady() && E.CanCast(target) && !Menu["Illaoi_EBlackList"].GetValue<MenuBool>(target.CharacterName.ToLower()).Enabled && !InAutoAttackRange(target) && target.IsValidTarget(E.Range))
                    {
                        E.Cast(target);
                    }

                    if (Menu["Illaoi_Harass"].GetValue<MenuBool>("Q").Enabled && Q.IsReady() && target.IsValidTarget(Q.Range) && !(E.IsReady() && Menu["Illaoi_Harass"].GetValue<MenuBool>("E").Enabled && E.GetPrediction(target).Hitchance >= HitChance.VeryHigh))
                    {
                        Q.Cast(target);
                    }

                    if (Menu["Illaoi_Harass"].GetValue<MenuBool>("W").Enabled && W.IsReady() && Menu["Illaoi_Harass"].GetValue<MenuBool>("WOutRange").Enabled && !InAutoAttackRange(target) && target.IsValidTarget(W.Range))
                    {
                        W.Cast();
                    }
                }
                else if (target == null && Ghost != null)
                {
                    if (Q.IsReady() && Menu["Illaoi_Harass"].GetValue<MenuBool>("Q").Enabled)
                        Q.Cast(Ghost);

                    if (W.IsReady() && Menu["Illaoi_Harass"].GetValue<MenuBool>("W").Enabled)
                    {
                        W.Cast();
                    }

                    if (Menu["Illaoi_Harass"].GetValue<MenuBool>("Ghost").Enabled)
                    {
                        Orbwalker.ForceTarget = Ghost;
                    }
                }
            }
        }

        private static void Lane()
        {
            if (Me.ManaPercent >= Menu["Illaoi_LaneClear"].GetValue<MenuSlider>("Mana").Value)
            {
                if (Menu["Illaoi_LaneClear"].GetValue<MenuSliderButton>("Q").Enabled && Q.IsReady())
                {
                    var Minions = GetMinions(Me.Position, Q.Range);

                    if (Minions.Count() > 0)
                    {
                        var QFarm = Q.GetLineFarmLocation(Minions, Q.Width);

                        if (QFarm.MinionsHit >= Menu["Illaoi_LaneClear"].GetValue<MenuSliderButton>("Q").Value)
                        {
                            Q.Cast(QFarm.Position);
                        }
                    }
                }
            }
        }

        private static void Jungle()
        {
            var Mobs = GetMobs(Me.Position, Q.Range, true);

            if (Mobs.Count() > 0)
            {
                if (Me.ManaPercent >= Menu["Illaoi_JungleClear"].GetValue<MenuSlider>("Mana").Value)
                {
                    if (Menu["Illaoi_JungleClear"].GetValue<MenuBool>("Q").Enabled && Q.IsReady() && !Orbwalker.IsAutoAttack(Me.CharacterName))
                    {
                        Q.Cast(Mobs.FirstOrDefault());
                    }
                }
            }
        }

        private static void LastHitLogic()
        {
            var Ghost = ObjectManager.Get<AIMinionClient>().Where(x => x.IsEnemy && x.IsHPBarRendered && x.IsValidTarget(GetAttackRange(Me))).FirstOrDefault(x => x.HasBuff("illaoiespirit"));

            if (Ghost != null)
            {
                Orbwalker.ForceTarget = Ghost;
            }
        }

        private static void KillSteal()
        {
            if (Menu["Illaoi_KillSteal"].GetValue<MenuBool>("Q").Enabled && Q.IsReady())
            {
                var qt = GameObjects.EnemyHeroes.Where(x => x.IsValidTarget(Q.Range) && x.Health < Q.GetDamage(x)).FirstOrDefault();

                if (CheckTarget(qt))
                {
                    Q.Cast(qt);
                    return;
                }
            }

            if (Menu["Illaoi_KillSteal"].GetValue<MenuBool>("E").Enabled && E.IsReady())
            {
                var et = GameObjects.EnemyHeroes.Where(x => !Menu["Illaoi_EBlackList"].GetValue<MenuBool>(x.CharacterName.ToLower()).Enabled && x.IsValidTarget(E.Range) && x.Health < E.GetDamage(x)).FirstOrDefault();

                if (CheckTarget(et))
                {
                    E.Cast(et);
                    return;
                }
            }

            if (Menu["Illaoi_KillSteal"].GetValue<MenuBool>("R").Enabled && R.IsReady())
            {
                var rt = GameObjects.EnemyHeroes.Where(x => x.IsValidTarget(R.Range - 50) && x.Health < R.GetDamage(x) && Menu["Illaoi_KillSteal"].GetValue<MenuBool>(x.CharacterName.ToLower()).Enabled).FirstOrDefault();

                if (CheckTarget(rt))
                {
                    R.Cast(rt);
                    return;
                }
            }
        }

        private static void OnAfterAttack(object sender, AfterAttackEventArgs e)
        {
            if (InCombo)
            {
                var target = GetTarget(W.Range, DamageType.Physical);

                if (CheckTarget(target))
                {
                    if (Menu["Illaoi_Combo"].GetValue<MenuBool>("W").Enabled && W.IsReady() && target.IsValidTarget(W.Range))
                    {
                        if (Menu["Illaoi_Combo"].GetValue<MenuBool>("WOutRange").Enabled && !InAutoAttackRange(target))
                        {
                            W.Cast();
                        }

                        if (Menu["Illaoi_Combo"].GetValue<MenuBool>("WUlt").Enabled && Me.HasBuff("IllaoiR"))
                        {
                            W.Cast();
                        }
                    }
                }
            }

            if (InHarass && !Me.IsUnderEnemyTurret())
            {
                if (Me.ManaPercent >= Menu["Illaoi_Harass"].GetValue<MenuSlider>("Mana").Value)
                {
                    var target = GetTarget(W.Range, DamageType.Physical);

                    if (CheckTarget(target))
                    {
                        if (Menu["Illaoi_Harass"].GetValue<MenuBool>("W").Enabled && W.IsReady() && target.IsValidTarget(W.Range))
                        {
                            if (Menu["Illaoi_Harass"].GetValue<MenuBool>("WOutRange").Enabled && !InAutoAttackRange(target))
                            {
                                W.Cast();
                            }
                            else if (!Menu["Illaoi_Harass"].GetValue<MenuBool>("WOutRange").Enabled)
                            {
                                W.Cast();
                            }
                        }
                    }
                }
            }

            if (InClear)
            {
                if (Me.ManaPercent >= Menu["Illaoi_JungleClear"].GetValue<MenuSlider>("Mana").Value)
                {
                    var Mobs = GetMobs(Me.Position, W.Range, true);
                    if (Mobs.Count() > 0)
                    {
                        if (Menu["Illaoi_JungleClear"].GetValue<MenuBool>("W").Enabled && W.IsReady() && !Orbwalker.IsAutoAttack(Me.CharacterName))
                        {
                            W.Cast();
                        }
                    }
                }
            }
        }

        private static void OnAction(object sender, BeforeAttackEventArgs Args)
        {
            var target = GetTarget(W.Range, DamageType.Physical);

            if (InHarass && Me.HasBuff("IllaoiW") && CheckTarget(target) && Args.Target is AIMinionClient)
            {
                Args.Process = false;
            }
            else
            {
                Args.Process = true;
            }
        }

        private static void OnSpellCast(AIBaseClient sender, AIBaseClientProcessSpellCastEventArgs args)
        {
            if (InCombo)
            {
                var target = GetTarget(W.Range, DamageType.Physical);

                if (CheckTarget(target))
                {
                    if (Menu["Illaoi_Combo"].GetValue<MenuBool>("W").Enabled && W.IsReady() && target.IsValidTarget(W.Range))
                    {
                        if (Menu["Illaoi_Combo"].GetValue<MenuBool>("WOutRange").Enabled && !InAutoAttackRange(target))
                        {
                            W.Cast();
                        }
                        else if (!Menu["Illaoi_Combo"].GetValue<MenuBool>("WOutRange").Enabled)
                        {
                            W.Cast();
                        }

                        if (Menu["Illaoi_Combo"].GetValue<MenuBool>("WUlt").Enabled && Me.HasBuff("IllaoiR"))
                        {
                            W.Cast();
                        }
                    }
                }
            }
        }

        private static void OnGapCloser(AIHeroClient sender, AntiGapcloser.GapcloserArgs Args)
        {
            if (Menu["Illaoi_Misc"].GetValue<MenuBool>("EGap").Enabled)
            {
                if (sender.IsEnemy && (Args.EndPosition.DistanceToPlayer() <= 200 || sender.DistanceToPlayer() <= 250) && !Menu["Illaoi_EBlackList"].GetValue<MenuBool>(sender.CharacterName.ToLower()).Enabled)
                {
                    E.Cast(sender);
                }
            }
        }
    }
}