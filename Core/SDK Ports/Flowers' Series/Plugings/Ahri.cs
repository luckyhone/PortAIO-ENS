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
    public class Ahri
    {
        private static Spell Q;
        private static Spell W;
        private static Spell E;
        private static Spell R;
        private static MissileClient QObj, QReturn;
        private static HpBarDraw DrawHpBar = new HpBarDraw();

        private static AIHeroClient Me => Program.Me;
        private static Menu Menu => Program.Menu;
        
        public static void Init()
        {
            Q = new Spell(SpellSlot.Q, 870f);
            W = new Spell(SpellSlot.W, 700f);
            E = new Spell(SpellSlot.E, 970f);
            R = new Spell(SpellSlot.R, 450f);

            Q.SetSkillshot(0.25f, 90, 1550, false, SpellType.Line);
            E.SetSkillshot(0.25f, 60, 1550, true, SpellType.Line);

            var ComboMenu = Menu.Add(new Menu("Ahri_Combo", "Combo"));
            {
                ComboMenu.Add(new MenuBool("Q", "Use Q", true));
                ComboMenu.Add(new MenuBool("W", "Use W", true));
                ComboMenu.Add(new MenuBool("E", "Use E", true));
                ComboMenu.Add(new MenuBool("R", "Use R", true));
                ComboMenu.Add(new MenuBool("RMax", "Use R| Max Dash", true));
                ComboMenu.Add(new MenuBool("RTurret", "Use R| Dont Dash To Enemy Turret", true));
                ComboMenu.Add(new MenuBool("RWall", "Use R| Check Wall", true));
            }

            var HarassMenu = Menu.Add(new Menu("Ahri_Harass", "Harass"));
            {
                HarassMenu.Add(new MenuBool("Q", "Use Q", true));
                HarassMenu.Add(new MenuBool("W", "Use W"));
                HarassMenu.Add(new MenuBool("E", "Use E"));
                HarassMenu.Add(new MenuSlider("Mana", "When Player ManaPercent >= %", 60));
            }

            var LaneClearMenu = Menu.Add(new Menu("Ahri_LaneClear", "Lane Clear"));
            {
                LaneClearMenu.Add(new MenuSliderButton("Q", "Use Q| Min Hit Minions >= x", 3, 1, 5, true));
                LaneClearMenu.Add(new MenuBool("W", "Use W", true));
                LaneClearMenu.Add(new MenuSlider("Mana", "When Player ManaPercent >= %", 60));
            }

            var JungleMenu = Menu.Add(new Menu("Ahri_JungleClear", "Jungle Clear"));
            {
                JungleMenu.Add(new MenuBool("Q", "Use Q", true));
                JungleMenu.Add(new MenuBool("W", "Use W", true));
                JungleMenu.Add(new MenuBool("E", "Use E", true));
                JungleMenu.Add(new MenuSlider("Mana", "When Player ManaPercent >= %", 30));
            }

            var KillStealMenu = Menu.Add(new Menu("Ahri_KillSteal", "Kill Steal"));
            {
                KillStealMenu.Add(new MenuBool("Q", "Use Q", true));
                KillStealMenu.Add(new MenuBool("W", "Use W", true));
                KillStealMenu.Add(new MenuBool("E", "Use E", true));
            }

            var MiscMenu = Menu.Add(new Menu("Ahri_Misc", "Misc"));
            {
                MiscMenu.Add(new MenuBool("AutoQ", "Auto Q| When Enemies Can't Move!", true));
                MiscMenu.Add(new MenuBool("EInt", "Use E| Interrupt Spell", true));
                MiscMenu.Add(new MenuBool("EGap", "Use E| Anti GapCloser", true));
                MiscMenu.Add(new MenuSeparator("EGapName", "GapCloser Select"));
                if (GameObjects.EnemyHeroes.Any())
                {
                    GameObjects.EnemyHeroes.ForEach(i => MiscMenu.Add(new MenuBool(i.CharacterName.ToLower(), i.CharacterName, true)));
                }
            }

            var Draw = Menu.Add(new Menu("Ahri_Draw", "Draw"));
            {
                Draw.Add(new MenuBool("Q", "Q Range"));
                Draw.Add(new MenuBool("W", "W Range"));
                Draw.Add(new MenuBool("E", "E Range"));
                Draw.Add(new MenuBool("R", "R Range"));
                Draw.Add(new MenuBool("DrawDamage", "Draw Combo Damage", true));
            }

            Menu.Add(new MenuSeparator("Check", "Please Check Your Evade# or EzEvade"));
            Menu.Add(new MenuSeparator("Check1", "About Use R Evade Logic"));
            Menu.Add(new MenuSeparator("Check2", "I Suggest Set It Off!"));

            Interrupter.OnInterrupterSpell += OnInterruptableTarget;
            AntiGapcloser.OnGapcloser += OnGapCloser;
            GameObject.OnCreate += OnCreate;
            GameObject.OnDelete += OnDelete;
            Game.OnUpdate += OnUpdate;
            Drawing.OnDraw += OnDraw;
            Drawing.OnEndScene += OnEndScene;
        }

        private static void OnInterruptableTarget(AIHeroClient sender, Interrupter.InterruptSpellArgs Args)
        {
            if (Menu["Ahri_Misc"].GetValue<MenuBool>("EInt").Enabled && sender.IsEnemy && sender.IsValidTarget(E.Range) && E.IsReady())
            {
                if (Args.DangerLevel >= Interrupter.DangerLevel.High ||
                    sender.IsCastingImporantSpell())
                {
                    E.Cast(Args.Sender);
                }
            }
        }

        private static void OnGapCloser(AIHeroClient sender, AntiGapcloser.GapcloserArgs Args)
        {
            if (sender.IsFacing(Me) && Menu["Ahri_Misc"].GetValue<MenuBool>("EGap").Enabled &&
                E.IsReady() && sender.DistanceToPlayer() <= 300 &&
                Menu["Ahri_Misc"].GetValue<MenuBool>(sender.CharacterName.ToLower()).Enabled)
            {
                E.Cast(sender);
            }
        }

        private static void OnCreate(GameObject sender, EventArgs args)
        {
            if(sender is MissileClient)
            {
                var QMissleClient = sender as MissileClient;

                if (QMissleClient.SpellCaster.IsMe && QMissleClient.IsValid)
                {
                    var Name = QMissleClient.SData.Name;

                    if (Name.Contains("AhriOrbMissile"))
                    {
                        QObj = QMissleClient;
                    }

                    if (Name.Contains("AhriOrbReturn"))
                    {
                        QReturn = QMissleClient;
                    }
                }
            }
        }

        private static void OnDelete(GameObject sender, EventArgs args)
        {
            if (sender is MissileClient)
            {
                var QMissleClient = sender as MissileClient;

                if (QMissleClient.SpellCaster.IsMe)
                {
                    var Name = QMissleClient.SData.Name;

                    if (Name.Contains("AhriOrbMissile") && QMissleClient.IsValid)
                    {
                        QObj = null;
                    }

                    if (Name.Contains("AhriOrbReturn") && QMissleClient.IsValid)
                    {
                        QReturn = null;
                    }
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
                ComboLogic();
            }

            if (InHarass && !Me.IsUnderEnemyTurret())
            {
                HarassLogic();
            }

            if (InClear)
            {
                LaneClearLogic();
                JungleClearLogic();
            }

            KillStealLogic();
            AutoQLogic();
        }
        
        private static void ComboLogic()
        {
            var target = GetTarget(900f, DamageType.Magical);

            if (CheckTarget(target))
            {
                if (Menu["Ahri_Combo"].GetValue<MenuBool>("R").Enabled && R.IsReady() && target.IsValidTarget(900f))
                {
                    RLogic(target);
                }

                if (Menu["Ahri_Combo"].GetValue<MenuBool>("E").Enabled && E.IsReady() && target.IsValidTarget(E.Range - 50))
                {
                    E.Cast(target);
                }

                if (Menu["Ahri_Combo"].GetValue<MenuBool>("W").Enabled && W.IsReady() && target.IsValidTarget(R.Range))//target.IsValidTarget(W.Range))
                {
                    W.Cast();
                }

                if (Menu["Ahri_Combo"].GetValue<MenuBool>("Q").Enabled && Q.IsReady() && target.IsValidTarget(Q.Range - 50))
                {
                    Q.Cast(target);
                }
            }
        }

        private static void RLogic( AIHeroClient target)
        {
            if (CheckTarget(target) && target.IsValidTarget(900f))
            {
                Vector3 RDashPos = Vector3.Zero;

                if (Menu["Ahri_Combo"].GetValue<MenuBool>("RMax").Enabled)
                {
                    RDashPos = Me.Position.Extend(Game.CursorPos, 450f);
                }
                else
                {
                    RDashPos = Game.CursorPos;
                }

                if ((RDashPos.IsWall() && Menu["Ahri_Combo"].GetValue<MenuBool>("RWall").Enabled) || 
                    (RDashPos.IsUnderEnemyTurret() && Menu["Ahri_Combo"].GetValue<MenuBool>("RTurret").Enabled) ||
                    (target.Health < GetDamage(target, false, true, true, true, false) && target.IsValidTarget(650)))
                {
                    return;
                }

                if (Me.HasBuff("AhriTumble"))
                {
                    var buffTime = Me.GetBuff("AhriTumble").EndTime;

                    if (buffTime - Game.Time <= 3)
                    {
                        R.Cast(RDashPos);
                    }

                    if (QReturn != null && QReturn.IsValid)
                    {
                        var ReturnPos = QReturn.Position;

                        if (target.DistanceToPlayer() > ReturnPos.DistanceToPlayer())
                        {
                            var targetdis = target.Position.Distance(ReturnPos);
                            var QReturnEnd = QReturn.EndPosition;

                            if (targetdis < Q.Range)
                            {
                                var CastPos = QReturnEnd.Extend(target.Position, target.Position.Distance(RDashPos));

                                if ((CastPos.IsWall() && Menu["Ahri_Combo"].GetValue<MenuBool>("RWall").Enabled) ||
                                    Me.Position.Distance(CastPos) > R.Range
                                    || CastPos.CountEnemyHeroesInRange(R.Range) > 2 ||
                                    (RDashPos.IsUnderEnemyTurret() && Menu["Ahri_Combo"].GetValue<MenuBool>("RTurret").Enabled))
                                {
                                    return;
                                }

                                R.Cast(CastPos);

                                return;
                            }
                        }
                    }
                    else
                    {
                        if (!Q.IsReady() && RDashPos.CountEnemyHeroesInRange(R.Range) <= 2 && target.IsValidTarget(800f))
                        {
                            if (Game.CursorPos.Distance(target.Position) > target.DistanceToPlayer() && target.IsValidTarget(R.Range))
                            {
                                R.Cast(RDashPos);
                            }
                            else if (Game.CursorPos.Distance(target.Position) < target.DistanceToPlayer() && !target.IsValidTarget(R.Range) && target.IsValidTarget(800f))
                            {
                                R.Cast(RDashPos);
                            }
                        }
                    }
                }
                else
                {
                    var AllDamage = GetDamage(target, false);
                    var QDamage = Q.GetDamage(target) * 2;
                    var WDamage = W.GetDamage(target);
                    var RDamage = R.GetDamage(target) * 3;

                    if (target.IsValidTarget(800) && target.Distance(RDashPos) <= R.Range)
                    {
                        if (RDashPos.CountEnemyHeroesInRange(R.Range) > 2 || 
                            target.CountAllyHeroesInRange(R.Range) > 2)
                        {
                            return;
                        }

                        if (target.Health >= AllDamage && target.Health <= QDamage + WDamage + RDamage)
                        {
                            R.Cast(RDashPos);
                        }
                        else if (target.Health < RDamage + QDamage)
                        {
                            R.Cast(RDashPos);
                        }
                        else if (target.Health < RDamage + WDamage)
                        {
                            R.Cast(RDashPos);
                        }
                    }
                }
            }
        }

        private static void HarassLogic()
        {
            if (Me.ManaPercent >= Menu["Ahri_Harass"].GetValue<MenuSlider>("Mana").Value)
            {
                var target = GetTarget(Q.Range, DamageType.Magical);

                if (CheckTarget(target))
                {
                    if (Menu["Ahri_Harass"].GetValue<MenuBool>("E").Enabled && E.IsReady() && target.IsValidTarget(E.Range))
                    {
                        E.Cast(target);
                    }

                    if (Menu["Ahri_Harass"].GetValue<MenuBool>("W").Enabled && W.IsReady() && target.IsValidTarget(R.Range))//target.IsValidTarget(W.Range))
                    {
                        W.Cast();
                    }

                    if (Menu["Ahri_Harass"].GetValue<MenuBool>("Q").Enabled && Q.IsReady() && target.IsValidTarget(Q.Range))
                    {
                        Q.Cast(target);
                    }
                }

            }
        }

        private static void LaneClearLogic()
        {
            if (Me.ManaPercent >= Menu["Ahri_LaneClear"].GetValue<MenuSlider>("Mana").Value)
            {
                var minions = GetMinions(Me.Position, Q.Range);

                if (minions.Count() > 0)
                {
                    if (Menu["Ahri_LaneClear"].GetValue<MenuSliderButton>("Q").Enabled && Q.IsReady())
                    {
                        var QFarm = Q.GetLineFarmLocation(minions, Q.Width);

                        if (QFarm.MinionsHit >= Menu["Ahri_LaneClear"].GetValue<MenuSliderButton>("Q").Value)
                        {
                            Q.Cast(QFarm.Position);
                        }
                    }

                    if (Menu["Ahri_LaneClear"].GetValue<MenuBool>("W").Enabled && W.IsReady() && QObj == null && QReturn == null)
                    {
                        var Wminions = GetMinions(Me.Position, W.Range);

                        if (Wminions.Count() >= 3)
                        {
                            W.Cast();
                        }
                    }
                }
            }
        }

        private static void JungleClearLogic()
        {
            if (Me.ManaPercent >= Menu["Ahri_JungleClear"].GetValue<MenuSlider>("Mana").Value)
            {
                var mobs = GetMobs(Me.Position, Q.Range, true);

                if (mobs.Count() > 0)
                {
                    var mob = mobs.FirstOrDefault();

                    if (Menu["Ahri_JungleClear"].GetValue<MenuBool>("E").Enabled && E.IsReady())
                    {
                        E.Cast(mob);
                        return;
                    }

                    if (Menu["Ahri_JungleClear"].GetValue<MenuBool>("W").Enabled && W.IsReady())
                    {
                        W.Cast(mob);
                        return;
                    }

                    if (Menu["Ahri_JungleClear"].GetValue<MenuBool>("Q").Enabled && Q.IsReady())
                    {
                        Q.Cast(mob);
                        return;
                    }
                }
            }
        }

        private static void KillStealLogic()
        {
            foreach (var target in GetEnemies(Q.Range))
            {
                if (CheckTarget(target))
                {
                    if (Menu["Ahri_KillSteal"].GetValue<MenuBool>("Q").Enabled && Q.IsReady() && target.IsValidTarget(Q.Range) && Q.GetDamage(target) > target.Health)
                    {
                        Q.Cast(target);
                        return;
                    }

                    if (Menu["Ahri_KillSteal"].GetValue<MenuBool>("W").Enabled && W.IsReady() && target.IsValidTarget(W.Range) && W.GetDamage(target) > target.Health)
                    {
                        W.Cast();
                        return;
                    }

                    if (Menu["Ahri_KillSteal"].GetValue<MenuBool>("E").Enabled && E.IsReady() && target.IsValidTarget(E.Range) && E.GetDamage(target) > target.Health)
                    {
                        E.Cast(target);
                        return;
                    }
                }
            }
        }

        private static void AutoQLogic()
        {
            if (Menu["Ahri_Misc"].GetValue<MenuBool>("AutoQ").Enabled && Q.IsReady() && !Me.IsUnderEnemyTurret())
            {
                foreach (var target in GetEnemies(Q.Range).Where(x => x.IsValidTarget(Q.Range) && !x.HasBuffOfType(BuffType.SpellShield) && !CanMove(x)))
                {
                    if (CheckTarget(target))
                    {
                        Q.Cast(target);
                        return;
                    }
                }
            }
        }

        private static void OnEndScene(EventArgs args)
        {
            if (Me.IsDead)
            {
                return;
            }
            if (Menu["Ahri_Draw"].GetValue<MenuBool>("DrawDamage").Enabled)
            {
                foreach (var e in ObjectManager.Get<AIHeroClient>().Where(e => e.IsValidTarget() && e.IsValid && !e.IsDead && !e.IsZombie()))
                {
                    HpBarDraw.Unit = e;
                    HpBarDraw.DrawDmg((float) GetDamage(e), new SharpDX.ColorBGRA(255, 204, 0, 170));
                }
            }
        }

        private static void OnDraw(EventArgs args)
        {
            if (Me.IsDead)
                return;

            if (Menu["Ahri_Draw"].GetValue<MenuBool>("Q").Enabled && Q.IsReady())
                CircleRender.Draw(Me.Position, Q.Range, Color.BlueViolet);

            if (Menu["Ahri_Draw"].GetValue<MenuBool>("W").Enabled && W.IsReady())
                CircleRender.Draw(Me.Position, W.Range, Color.BlueViolet);

            if (Menu["Ahri_Draw"].GetValue<MenuBool>("E").Enabled && E.IsReady())
                CircleRender.Draw(Me.Position, E.Range, Color.BlueViolet);

            if (Menu["Ahri_Draw"].GetValue<MenuBool>("R").Enabled && R.IsReady())
                CircleRender.Draw(Me.Position, 450f, Color.BlueViolet);
        }
    }
}