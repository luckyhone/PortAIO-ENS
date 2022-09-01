using System;
using System.Linq;
using EnsoulSharp;
using EnsoulSharp.SDK;
using EnsoulSharp.SDK.MenuUI;
using EnsoulSharp.SDK.Rendering;
using Flowers_Series.Common;
using ShadowTracker;
using SharpDX;
using static Flowers_Series.Common.Manager;
namespace Flowers_Series.Plugings
{
    public class Vayne
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
            Q = new Spell(SpellSlot.Q, 800f);
            W = new Spell(SpellSlot.W);
            E = new Spell(SpellSlot.E, 650f);
            R = new Spell(SpellSlot.R);

            E.SetTargetted(0.25f, 1600f);

            var ComboMenu = Menu.Add(new Menu("Vayne_Combo", "Combo"));
            {
                ComboMenu.Add(new MenuSeparator("QLogic", "Q Logic"));
                ComboMenu.Add(new MenuBool("Q", "Use Q", true));
                ComboMenu.Add(new MenuBool("AQA", "A-Q-A", true));
                ComboMenu.Add(new MenuBool("SafeCheck", "Safe Q Check", true));
                ComboMenu.Add(new MenuBool("QTurret", "Dont Cast In Turret", true));
                ComboMenu.Add(new MenuSeparator("ELogic", "E Logic"));
                ComboMenu.Add(new MenuBool("E", "Use E", true));
                ComboMenu.Add(new MenuSeparator("RLogic", "R Logic"));
                ComboMenu.Add(new MenuBool("R", "Use R", true));
                ComboMenu.Add(new MenuSlider("RCount", "When Enemies Counts >= ", 2, 1, 5));
                ComboMenu.Add(new MenuSlider("RHp", "Or Player HealthPercent <= %", 45));
            }

            var HarassMenu = Menu.Add(new Menu("Vayne_Harass", "Harass"));
            {
                HarassMenu.Add(new MenuBool("Q", "Use Q", true));
                HarassMenu.Add(new MenuBool("SafeCheck", "Safe Q Check", true));
                HarassMenu.Add(new MenuBool("QTurret", "Dont Cast In Turret", true));
                HarassMenu.Add(new MenuBool("E", "Use E | Only Target have 2 Passive"));
            }

            var LaneClearMenu = Menu.Add(new Menu("Vayne_LaneClear", "LaneClear"));
            {
                LaneClearMenu.Add(new MenuBool("Q", "Use Q", true));
                LaneClearMenu.Add(new MenuBool("QTurret", "Use Q To Attack Tower", true));
                LaneClearMenu.Add(new MenuSlider("Mana", "Min LaneClear Mana >= %", 50));
            }

            var JungleClearMenu = Menu.Add(new Menu("Vayne_JungleClear", "JungleClear"));
            {
                JungleClearMenu.Add(new MenuBool("Q", "Use Q", true));
                JungleClearMenu.Add(new MenuBool("E", "Use E", true));
                JungleClearMenu.Add(new MenuSlider("Mana", "Min JungleClear Mana >= %", 30));
            }

            var AutoMenu = Menu.Add(new Menu("Vayne_Auto", "Auto"));
            {
                AutoMenu.Add(new MenuSeparator("ELogic", "E Logic"));
                AutoMenu.Add(new MenuBool("E", "Use E"));
                if (GameObjects.EnemyHeroes.Any())
                {
                    GameObjects.EnemyHeroes.ForEach(i => AutoMenu.Add(new MenuBool("CastE" + i.CharacterName, "Cast To :" + i.CharacterName, AutoEnableList.Contains(i.CharacterName))));
                }
                AutoMenu.Add(new MenuSeparator("RLogic", "R Logic"));
                AutoMenu.Add(new MenuBool("R", "Use R", true));
                AutoMenu.Add(new MenuSlider("RCount", "When Enemies Counts >= ", 3, 1, 5));
                AutoMenu.Add(new MenuSlider("RRange", "Search Enemies Range ", 600, 500, 1200));
            }

            var EMenu = Menu.Add(new Menu("Vayne_E", "E Settings"));
            {
                EMenu.Add(new MenuBool("Gapcloser", "Anti Gapcloser", true));
                EMenu.Add(new MenuBool("AntiAlistar", "Anti Alistar", true));
                EMenu.Add(new MenuBool("AntiRengar", "Anti Rengar", true));
                EMenu.Add(new MenuBool("AntiKhazix", "Anti Khazix", true));
                EMenu.Add(new MenuBool("Interrupt", "Interrupt Danger Spells", true));
                EMenu.Add(new MenuBool("Under", "Dont Cast In Turret", true));
                EMenu.Add(new MenuSlider("Push", "Push Tolerance", -30, -100, 100));
            }

            var Draw = Menu.Add(new Menu("Vayne_Draw", "Draw"));
            {
                Draw.Add(new MenuBool("E", "Draw E Range"));
                Draw.Add(new MenuBool("Damage", "Draw Combo Damage", true));
            }

            WriteConsole(GameObjects.Player.CharacterName + " Inject!");

            GameObject.OnCreate += OnCreate;
            Game.OnUpdate += OnUpdate;
            Orbwalker.OnAfterAttack += OnAction;
            Orbwalker.OnBeforeAttack += OnBeforeAttack;
            Interrupter.OnInterrupterSpell += OnInterruptableTarget;
            AntiGapcloser.OnGapcloser += OnGapCloser;
            AIBaseClient.OnProcessSpellCast += OnProcessSpellCast;
            Drawing.OnDraw += OnDraw;
            Drawing.OnEndScene += OnEndScene;
        }

        private static void OnCreate(GameObject sender, EventArgs args)
        {
            if (Menu["Vayne_E"].GetValue<MenuBool>("Gapcloser").Enabled && E.IsReady())
            {
                var Rengar = GameObjects.EnemyHeroes.Find(heros => heros.CharacterName.Equals("Rengar"));
                var Khazix = GameObjects.EnemyHeroes.Find(heros => heros.CharacterName.Equals("Khazix"));

                if (Rengar != null && Menu["Vayne_E"].GetValue<MenuBool>("AntiRengar").Enabled)
                {
                    if (sender.Name == ("Rengar_LeapSound.troy") && sender.Distance(Me) < E.Range)
                        E.CastOnUnit(Rengar);
                }

                if (Khazix != null && Menu["Vayne_E"].GetValue<MenuBool>("AntiKhazix").Enabled)
                {
                    if (sender.Name == ("Khazix_Base_E_Tar.troy") && sender.Distance(Me) <= 300)
                        E.CastOnUnit(Khazix);
                }
            }
        }

        private static void OnUpdate(EventArgs args)
        {
            if (Me.IsDead)
                return;

            if (!E.IsReady())
            {
                return;
            }

            if (InCombo && Menu["Vayne_Combo"].GetValue<MenuBool>("E").Enabled && E.IsReady())
            {
                var target = GetTarget(E.Range);

                if (CheckTarget(target))
                {
                    ELogic(target);
                }
            }

            if (InHarass && Menu["Vayne_Harass"].GetValue<MenuBool>("E").Enabled && E.IsReady())
            {
                var target = GetTarget(E.Range);

                if (CheckTarget(target) && GetPassive(target) == 2)
                {
                    E.CastOnUnit(target);
                }
            }

            if (InClear && Menu["Vayne_JungleClear"].GetValue<MenuBool>("E").Enabled && E.IsReady() && 
                Me.ManaPercent >= Menu["Vayne_JungleClear"].GetValue<MenuSlider>("Mana").Value)
            {
                var mob = GetMobs(Me.Position, E.Range, true).FirstOrDefault();

                if (mob != null && mob.IsValidTarget(E.Range))
                {
                    E.CastOnUnit(mob);
                }
            }

            if (Menu["Vayne_Auto"].GetValue<MenuBool>("E").Enabled && !InCombo)
            {
                var target = GetTarget(E.Range);

                if (CheckTarget(target) && Menu["Vayne_Auto"].GetValue<MenuBool>("CastE" + target.CharacterName).Enabled)
                {
                    ELogic(target);
                }
            }
        }
        

        private static void OnBeforeAttack(object sender, BeforeAttackEventArgs Args)
        {
            if (!Q.IsReady())
            {
                return;
            }

            BeforeQLogic(Args);
        }

        private static void BeforeQLogic(BeforeAttackEventArgs Args)
        {
            if (!(Args.Target is AIHeroClient))
            {
                return;
            }

            var target = Args.Target as AIHeroClient;

            if (InCombo && Args.Target is AIHeroClient && Menu["Vayne_Combo"].GetValue<MenuBool>("Q").Enabled)
            {
                if (CheckTarget(target) && target.IsValidTarget(Q.Range) && Q.IsReady())
                {
                    var AfterQPosition = Me.Position + (Game.CursorPos - Me.Position).Normalized() * 250;
                    var Distance = target.Position.Distance(AfterQPosition);

                    if (Menu["Vayne_Combo"].GetValue<MenuBool>("QTurret").Enabled && AfterQPosition.IsUnderEnemyTurret())
                    {
                        return;
                    }

                    if (Menu["Vayne_Combo"].GetValue<MenuBool>("SafeCheck").Enabled && AfterQPosition.CountEnemyHeroesInRange(300) >= 3)
                    {
                        return;
                    }

                    if (target.DistanceToPlayer() >= 600 && Distance <= 600)
                    {
                        Q.Cast(Game.CursorPos);
                        return;
                    }
                }
            }

            if (InHarass && Args.Target is AIHeroClient && Menu["Vayne_Harass"].GetValue<MenuBool>("Q").Enabled)
            {
                if (CheckTarget(target) && target.IsValidTarget(800) && Q.IsReady())
                {
                    var AfterQPosition = Me.Position + (Game.CursorPos - Me.Position).Normalized() * 250;
                    var Distance = target.Position.Distance(AfterQPosition);

                    if (Menu["Vayne_Harass"].GetValue<MenuBool>("QTurret").Enabled && AfterQPosition.IsUnderEnemyTurret())
                    {
                        return;
                    }

                    if (Menu["Vayne_Harass"].GetValue<MenuBool>("SafeCheck").Enabled && AfterQPosition.CountEnemyHeroesInRange(300) >= 2)
                    {
                        return;
                    }

                    if (target.DistanceToPlayer() >= 600 && Distance <= 600)
                    {
                        Q.Cast(Game.CursorPos);
                        return;
                    }
                }
            }
        }

        private static void OnAction(object sender, AfterAttackEventArgs Args)
        {
            if (!Q.IsReady())
            {
                return;
            }

            AfterQLogic(Args);
        }

        private static void AfterQLogic(AfterAttackEventArgs Args)
        {
            if (InCombo && Args.Target is AIHeroClient && Menu["Vayne_Combo"].GetValue<MenuBool>("Q").Enabled && Menu["Vayne_Combo"].GetValue<MenuBool>("AQA").Enabled)
            {
                var target = Args.Target as AIHeroClient;

                if (CheckTarget(target) && target.IsValidTarget(Q.Range) && Q.IsReady())
                {
                    var AfterQPosition = Me.Position + (Game.CursorPos - Me.Position).Normalized() * 250;
                    var Distance = target.Position.Distance(AfterQPosition);

                    if (Menu["Vayne_Combo"].GetValue<MenuBool>("QTurret").Enabled && AfterQPosition.IsUnderEnemyTurret())
                    {
                        return;
                    }

                    if (Menu["Vayne_Combo"].GetValue<MenuBool>("SafeCheck").Enabled && AfterQPosition.CountEnemyHeroesInRange(300) >= 3)
                    {
                        return;
                    }

                    if (Distance <= 650 && Distance >= 300)
                    {
                        Q.Cast(Game.CursorPos);
                        return;
                    }
                }
            }
            else if (InClear)
            {
                if (Args.Target is AITurretClient && Menu["Vayne_LaneClear"].GetValue<MenuBool>("Q").Enabled
                                                 && Me.ManaPercent >= Menu["Vayne_LaneClear"].GetValue<MenuSlider>("Mana").Value &&
                                                 Menu["Vayne_LaneClear"].GetValue<MenuBool>("QTurret").Enabled && Me.CountEnemyHeroesInRange(900) == 0 && Q.IsReady())
                {
                    Q.Cast(Game.CursorPos);
                }

                if (Args.Target is AIMinionClient)
                {
                    LaneClearQ(Args);
                    JungleQ(Args);
                }
            }
        }

        private static void LaneClearQ(AfterAttackEventArgs Args)
        {
            if (Menu["Vayne_LaneClear"].GetValue<MenuBool>("Q").Enabled && Me.ManaPercent >= Menu["Vayne_LaneClear"].GetValue<MenuSlider>("Mana").Value)
            {
                var minions = GetMinions(Me.Position, GetAttackRange(Me) + 175).Where(m => m.Health < (Q.GetDamage(m) + Me.GetAutoAttackDamage(m)));

                if (minions.Count() > 0 && Args.Target.NetworkId != minions.FirstOrDefault().NetworkId)
                {
                    Q.Cast(Game.CursorPos);
                    Orbwalker.ForceTarget = minions.FirstOrDefault();
                }
            }
        }

        private static void JungleQ(AfterAttackEventArgs Args)
        {
            if (Menu["Vayne_JungleClear"].GetValue<MenuBool>("Q").Enabled && Me.ManaPercent >= Menu["Vayne_JungleClear"].GetValue<MenuSlider>("Mana").Value)
            {
                var mobs = GetMobs(Me.Position, GetAttackRange(Me), true);

                if (mobs.Count() > 0 && Q.IsReady())
                {
                    Q.Cast(Game.CursorPos);
                }
            }
        }

        private static void OnInterruptableTarget(AIHeroClient sender, Interrupter.InterruptSpellArgs Args)
        {
            if (Menu["Vayne_E"].GetValue<MenuBool>("Interrupt").Enabled && E.IsReady() && Args.Sender.IsValidTarget(E.Range))
            {
                if (Args.Sender.IsCastingImporantSpell())
                {
                    E.CastOnUnit(Args.Sender);
                }

                if (Args.DangerLevel >= Interrupter.DangerLevel.Medium)
                {
                    E.CastOnUnit(Args.Sender);
                }
            }
        }

        private static void OnGapCloser(AIHeroClient sender, AntiGapcloser.GapcloserArgs Args)
        {
            try
            {
                if (sender.IsFacing(Me))
                {
                    if (Menu["Vayne_E"].GetValue<MenuBool>("Gapcloser").Enabled && E.IsReady())
                    {
                        if (Menu["Vayne_E"].GetValue<MenuBool>("AntiAlistar").Enabled &&
                            sender.CharacterName == "Alistar" && Args.Type == AntiGapcloser.GapcloserType.Targeted)
                        {
                            E.CastOnUnit(sender);
                        }
                        else if (Args.EndPosition.DistanceToPlayer() <= 250 && Args.Target.IsValid)
                        {
                            E.CastOnUnit(sender);
                        }
                    }
                }
            }catch(Exception ){}
        }

        private static void OnProcessSpellCast(AIBaseClient sender, AIBaseClientProcessSpellCastEventArgs args)
        {
            if (!R.IsReady())
            {
                return;
            }

            if (InCombo && Menu["Vayne_Combo"].GetValue<MenuBool>("R").Enabled)
            {
                if (Me.CountEnemyHeroesInRange(800) >= Menu["Vayne_Combo"].GetValue<MenuSlider>("RCount").Value)
                {
                    R.Cast();
                }

                if (Me.CountEnemyHeroesInRange(GetAttackRange(Me)) >= 1 && Me.HealthPercent <= Menu["Vayne_Combo"].GetValue<MenuSlider>("RHp").Value)
                {
                    R.Cast();
                }
            }

            if (Menu["Vayne_Auto"].GetValue<MenuBool>("R").Enabled &&
                Me.CountEnemyHeroesInRange(Menu["Vayne_Auto"].GetValue<MenuSlider>("RRange").Value) >= 
                Menu["Vayne_Auto"].GetValue<MenuSlider>("RCount").Value)
            {
                R.Cast();
            }
        }

        private static void OnEndScene(EventArgs args)
        {
            if (Me.IsDead) return;
            
            if (Menu["Vayne_Draw"].GetValue<MenuBool>("Damage").Enabled)
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

        private static void OnDraw(EventArgs args)
        {
            if (Me.IsDead)
                return;

            if (Menu["Vayne_Draw"].GetValue<MenuBool>("E").Enabled && E.IsReady())
            {
                CircleRender.Draw(Me.Position, E.Range, Color.AliceBlue);
            }
        }
        
        private static void ELogic(AIBaseClient target)
        {
            if (Menu["Vayne_E"].GetValue<MenuBool>("Under").Enabled && Me.IsUnderEnemyTurret())
            {
                return;
            }

            if (target != null && target.IsHPBarRendered)
            {
                var EPred = E.GetPrediction(target);
                var PD = 425 + Menu["Vayne_E"].GetValue<MenuSlider>("Push").Value;
                var PP = EPred.UnitPosition.Extend(Me.Position, -PD);

                for (int i = 1; i < PD; i += (int)target.BoundingRadius)
                {
                    var VL = EPred.UnitPosition.Extend(Me.Position, -i);
                    var J4 = ObjectManager.Get<AIBaseClient>().Any(f => f.Distance(PP) <= target.BoundingRadius && f.Name.ToLower() == "beacon");
                    var CF = NavMesh.GetCollisionFlags(VL);

                    if (CF.HasFlag(CollisionFlags.Wall) || CF.HasFlag(CollisionFlags.Building) || J4)
                    {
                        E.CastOnUnit(target);
                        return;
                    }
                }
            }
        }

        private static int GetPassive(AIBaseClient target)
        {
            int counts = 0;

            if (target != null && target.IsValidTarget())
            {
                foreach (var buff in target.Buffs)
                {
                    if (buff.Name.ToLower() == "vaynesilvereddebuff")
                    {
                        counts = buff.Count;
                    }
                }
            }

            return counts;
        }
    }
}