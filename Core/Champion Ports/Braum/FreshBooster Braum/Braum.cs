using System;
using System.Linq;
using EnsoulSharp;
using EnsoulSharp.SDK;
using EnsoulSharp.SDK.MenuUI;
using EnsoulSharp.SDK.Rendering;
using LeagueSharpCommon;
using SharpDX;
using Color = System.Drawing.Color;
using static FreshBooster.FreshCommon;
using Geometry = LeagueSharpCommon.Geometry.Geometry;
using KeyBindType = EnsoulSharp.SDK.MenuUI.KeyBindType;
using Keys = EnsoulSharp.SDK.MenuUI.Keys;
using Menu = EnsoulSharp.SDK.MenuUI.Menu;
using MenuItem = EnsoulSharp.SDK.MenuUI.MenuItem;
using Render = EnsoulSharp.SDK.Render;

namespace FreshBooster.Champion
{
    class Braum
    {
        // Default Setting
        public static int ErrorTime;
        public const string ChampName = "Braum";   // Edit
        public static AIHeroClient Player;
        public static Spell _Q, _W, _E, _R;
        
        // Default Setting

        public static bool QSpell = false;
        public static int SpellTime = 0, RTime;
        public static AIHeroClient FleeQ;
        public static Vector3 QPosition;

        private void SkillSet()
        {
            try
            {
                _Q = new Spell(SpellSlot.Q, 1000f);
                _Q.SetSkillshot(0.25f, 120f, 1400f, true, EnsoulSharp.SDK.SpellType.Line);
                _W = new Spell(SpellSlot.W, 650);
                _E = new Spell(SpellSlot.E, 0);
                _R = new Spell(SpellSlot.R, 1250f);
                _R.SetSkillshot(0.5f, 120f, 1200f, false, EnsoulSharp.SDK.SpellType.Line);
            }
            catch (Exception)
            {
                if (NowTime() > ErrorTime)
                {
                    Game.Print(ChampName + " in FreshBooster isn't Load. Error Code 01");
                    ErrorTime = TickCount(10000);
                }
            }
        }
        private void Menu()
        {
            try
            {
                var Combo = new Menu("Combo", "Combo");
                {
                    Combo.Add(new MenuBool("Braum_CUse_Q", "Use Q").SetValue(true));
                    Combo.Add(new MenuSlider("Braum_CUse_Q_Hit", "Q HitChance",3, 1, 6));
                    Combo.Add(new MenuBool("Braum_CUse_R", "Use R").SetValue(true));
                    Combo.Add(new MenuKeyBind("CKey", "Combo Key",Keys.Space, KeyBindType.Press));
                }
                _MainMenu.Add(Combo);

                var Harass = new Menu("Harass", "Harass");
                {
                    Harass.Add(new MenuBool("Braum_HUse_Q", "Use Q").SetValue(true));
                    Harass.Add(new MenuBool("Braum_Auto_HEnable", "Auto Harass").SetValue(true));
                    Harass.Add(new MenuSlider("Braum_HMana", "Min. Mana %",50, 0, 100));
                    Harass.Add(new MenuKeyBind("HKey", "Harass Key",Keys.C, KeyBindType.Press));
                }
                _MainMenu.Add(Harass);

                var KillSteal = new Menu("KillSteal", "KillSteal");
                {
                    KillSteal.Add(new MenuBool("Braum_KUse_Q", "Use Q").SetValue(true));
                    KillSteal.Add(new MenuBool("Braum_KUse_R", "Use R").SetValue(true));
                }
                _MainMenu.Add(KillSteal);

                var Misc = new Menu("Misc", "Misc");
                {
                    Misc.Add(new MenuKeyBind("Braum_Flee", "Flee Key",Keys.G, KeyBindType.Press));
                    Misc.Add(new MenuBool("Braum_AutoW", "Auto W").SetValue(true));
                    Misc.Add(new MenuBool("Braum_AutoE", "Auto E").SetValue(true));
                    var interrupt = Misc.Add(new Menu("Interrupt", "Interrupt")); 
                    var antigap = Misc.Add(new Menu("Anti-GapCloser", "Anti-GapCloser")); 
                    interrupt.Add(new MenuBool("Braum_InterR", "Use R").SetValue(true));
                    antigap.Add(new MenuBool("Braum_GapQ", "Use Q").SetValue(true));
                    antigap.Add(new MenuBool("Braum_GapR", "Use R").SetValue(true));
                }
                _MainMenu.Add(Misc);

                var Draw = new Menu("Draw", "Draw");
                {
                    Draw.Add(new MenuBool("Braum_Draw_Q", "Draw Q").SetValue(false));
                    Draw.Add(new MenuBool("Braum_Draw_W", "Draw W").SetValue(false));
                    Draw.Add(new MenuBool("Braum_Draw_R", "Draw R").SetValue(false));
                    Draw.Add(new MenuBool("Braum_Indicator", "Draw Damage Indicator").SetValue(true));
                }
                _MainMenu.Add(Draw);
            }
            catch (Exception)
            {
                if (NowTime() > ErrorTime)
                {
                    Game.Print(ChampName + " in FreshBooster isn't Load. Error Code 02");
                    ErrorTime = TickCount(10000);
                }
            }
        }
        public static void Drawing_OnDraw(EventArgs args)
        {
            try
            {
                if (Player.IsDead) return;
                if (_MainMenu["Braum_Draw_Q"].GetValue<MenuBool>().Enabled)
                    CircleRender.Draw(Player.Position, _Q.Range, Color.White.ToSharpDxColor(), 1);
                if (_MainMenu["Braum_Draw_W"].GetValue<MenuBool>().Enabled)
                    CircleRender.Draw(Player.Position, _W.Range, Color.White.ToSharpDxColor(), 1);
                if (_MainMenu["Braum_Draw_R"].GetValue<MenuBool>().Enabled)
                    CircleRender.Draw(Player.Position, _R.Range, Color.White.ToSharpDxColor(), 1);
                var QTarget = TargetSelector.GetTarget(_Q.Range, DamageType.Magical);
                if (QTarget != null)
                    Drawing.DrawCircle(QTarget.Position, 150,1, Color.Green);
            }
            catch (Exception)
            {
                if (NowTime() > ErrorTime)
                {
                    Game.Print(ChampName + " in FreshBooster isn't Load. Error Code 03");
                    ErrorTime = TickCount(10000);
                }
            }
        }

        static float getComboDamage(AIBaseClient enemy)
        {
            try
            {
                if (enemy != null)
                {
                    float damage = 0;
                    if (_Q.IsReady())
                        damage += _Q.GetDamage(enemy);
                    if (_R.IsReady())
                        damage += _R.GetDamage(enemy);
                    if (!Player.Spellbook.IsAutoAttack)
                        damage += (float)Player.GetAutoAttackDamage(enemy, true);
                    return damage;
                }
                return 0;
            }
            catch (Exception)
            {
                if (NowTime() > ErrorTime)
                {
                    Game.Print(ChampName + " in FreshBooster isn't Load. Error Code 04");
                    ErrorTime = TickCount(10000);
                }
                return 0;
            }
        }
        public static void Drawing_OnEndScene(EventArgs args)
        {
            try
            {
            }
            catch (Exception)
            {
                if (NowTime() > ErrorTime)
                {
                    Game.Print(ChampName + " in FreshBooster isn't Load. Error Code 05");
                    ErrorTime = TickCount(10000);
                }
            }
        }

        // OnLoad
        public Braum()
        {
            Player = ObjectManager.Player;
            SkillSet();
            Menu();
            Game.OnUpdate += OnGameUpdate;
            Drawing.OnDraw += Drawing_OnDraw;
            Drawing.OnEndScene += Drawing_OnEndScene;
            AntiGapcloser.OnGapcloser += AntiGapcloser_OnEnemyGapcloser;
            AIBaseClient.OnProcessSpellCast += OnProcessSpell;
            Interrupter.OnInterrupterSpell += Interrupter2_OnInterruptableTarget;
            Orbwalker.OnBeforeAttack += Orbwalking_BeforeAttack;
            AIBaseClient.OnIssueOrder += Obj_AI_Base_OnIssueOrder;
            GameObject.OnCreate += GameObject_OnCreate;
            GameObject.OnDelete += GameObject_OnDelete;
        }

        private static void OnGameUpdate(EventArgs args)
        {
            try
            {
                if (Player.IsDead) return;
                // Set Target
                var QTarget = TargetSelector.GetTarget(_Q.Range, DamageType.Magical);
                var RTarget = TargetSelector.GetTarget(_R.Range, DamageType.Magical);

                // Kill
                if (_MainMenu["Braum_KUse_Q"].GetValue<MenuBool>().Enabled && QTarget != null && _Q.IsReady() && _Q.GetDamage(QTarget) > QTarget.Health)
                {
                    _Q.CastIfHitchanceEquals(QTarget, HitChance.Low);
                    return;
                }
                if (_MainMenu["Braum_KUse_R"].GetValue<MenuBool>().Enabled && QTarget != null && _R.IsReady() && _R.GetDamage(RTarget) > RTarget.Health)
                {
                    _R.CastIfHitchanceEquals(QTarget, HitChance.VeryHigh);
                    return;
                }

                // Flee
                if (_MainMenu["Braum_Flee"].GetValue<MenuKeyBind>().Active)
                {
                    MovingPlayer(Game.CursorPos);
                    if (QTarget != null && _Q.IsReady())
                        _Q.CastIfHitchanceEquals(QTarget, HitChance.Low);
                }

                // Combo
                if (Orbwalker.ActiveMode == OrbwalkerMode.Combo)
                {
                    if (_MainMenu["Braum_CUse_R"].GetValue<MenuBool>().Enabled && _R.IsReady() && RTarget != null)
                        _R.CastIfHitchanceEquals(RTarget, HitChance.VeryHigh);
                    if (_MainMenu["Braum_CUse_Q"].GetValue<MenuBool>().Enabled && _Q.IsReady() && QTarget != null)
                        _Q.CastIfHitchanceEquals(QTarget,HitChance.VeryHigh);
                }

                // Harass
                if ((Orbwalker.ActiveMode == OrbwalkerMode.Harass || _MainMenu["Braum_Auto_HEnable"].GetValue<MenuBool>().Enabled)
                    && _MainMenu["Braum_HMana"].GetValue<MenuSlider>().Value < Player.ManaPercent)
                {
                    if (_MainMenu["Braum_HUse_Q"].GetValue<MenuBool>().Enabled && _Q.IsReady() && QTarget != null)
                        _Q.CastIfHitchanceEquals(QTarget, HitChance.VeryHigh);
                }
            }
            catch (Exception e)
            {
                if (NowTime() > ErrorTime)
                {
                    Game.Print(ChampName + " in FreshBooster isn't Load. Error Code 06");
                    Console.WriteLine(e);
                    ErrorTime = TickCount(10000);
                }
            }
        }
        private void AntiGapcloser_OnEnemyGapcloser(AIHeroClient sender, AntiGapcloser.GapcloserArgs args)
        {
            try
            {
                if (_MainMenu["Anti-GapCloser"]["Braum_GapQ"].GetValue<MenuBool>().Enabled && _Q.IsReady() && sender.Position.Distance(Player.Position) < _Q.Range)
                {
                    _Q.CastIfHitchanceEquals(sender, HitChance.Low);
                    return;
                }
                if (_MainMenu["Anti-GapCloser"]["Braum_GapR"].GetValue<MenuBool>().Enabled && _R.IsReady() && sender.Position.Distance(Player.Position) < _R.Range)
                {
                    _R.CastIfHitchanceEquals(sender, HitChance.VeryHigh);
                    return;
                }
            }
            catch (Exception)
            {
                if (NowTime() > ErrorTime)
                {
                    Game.Print(ChampName + " in FreshBooster isn't Load. Error Code 07");
                    ErrorTime = TickCount(10000);
                }
            }
        }
        private static void OnProcessSpell(AIBaseClient sender, AIBaseClientProcessSpellCastEventArgs args)
        {
            try
            {
                if (!(sender is AIHeroClient) || Player.IsRecalling())
                    return;
                // Auto W
                if (_MainMenu["Braum_AutoW"].GetValue<MenuBool>().Enabled && _W.IsReady())
                {
                    if (!(sender is AIHeroClient) || !sender.IsEnemy)
                        return;
                    if (args.Target != null)
                        if (args.SData.Name.ToLower().Contains("attack") && args.Target.Position.Distance(Player.Position) < _W.Range)
                            if (args.Target.IsAlly && args.Target is AIHeroClient)
                            {
                                if (args.Target.IsMe && Player.HealthPercent < 20)
                                {
                                    _W.CastOnUnit((AIBaseClient)args.Target);
                                }
                                else
                                {
                                    _W.CastOnUnit((AIBaseClient)args.Target);
                                }
                            }
                }
                // Auto E
                if (_MainMenu["Braum_AutoE"].GetValue<MenuBool>().Enabled && _E.IsReady())
                {
                    if (!(sender is AIHeroClient) || !sender.IsEnemy || !Orbwalker.CanAttack())
                        return;
                    var enemyskill = new Geometry.Polygon.Rectangle(args.Start, args.End, args.SData.CastRadius + 20);
                    var myteam = HeroManager.Allies.Where(f => f.Distance(Player.Position) < 200);
                    var count = myteam.Count(f => enemyskill.IsInside(f.Position));
                    if (args.Target != null && args.Target.Position.Distance(Player.Position) < 200)
                    {
                        if (args.Target.Name == Player.Name && Player.HealthPercent < 20)
                        {
                            _E.Cast(sender.Position);
                        }
                        else if (args.Target.Position.Distance(Player.Position) < 200 && args.Target is AIHeroClient)
                        {
                            if (_W.IsReady() && args.Target.Position.Distance(Player.Position) < _W.Range)
                                _W.CastOnUnit((AIBaseClient)args.Target);
                            _E.Cast(sender.Position);
                        }
                    }
                    else if (args.Target == null)
                    {
                        if (Player.HealthPercent < 20 && count == 1)
                        {
                            _E.Cast(sender.Position);
                        }
                        else if (count >= 2)
                        {
                            _E.Cast(sender.Position);
                        }
                    }
                }
            }
            catch (Exception)
            {
                if (NowTime() > ErrorTime)
                {
                    Game.Print(ChampName + " in FreshBooster isn't Load. Error Code 08");
                    ErrorTime = TickCount(10000);
                }
            }

        }
        private static void Interrupter2_OnInterruptableTarget(AIHeroClient sender, Interrupter.InterruptSpellArgs args)
        {
            try
            {
                if (_MainMenu["Interrupt"]["Braum_InterR"].GetValue<MenuBool>().Enabled && _R.IsReady() && sender.IsEnemy && sender.Position.Distance(Player.Position) < _R.Range * 0.9)
                    _R.CastIfHitchanceEquals(sender, HitChance.VeryHigh);
            }
            catch (Exception)
            {
                if (NowTime() > ErrorTime)
                {
                    Game.Print(ChampName + " in FreshBooster isn't Load. Error Code 09");
                    ErrorTime = TickCount(10000);
                }
            }

        }
        public static void Orbwalking_BeforeAttack(object sender, BeforeAttackEventArgs args)
        {
            try
            {

            }
            catch (Exception)
            {
                if (NowTime() > ErrorTime)
                {
                    Game.Print(ChampName + " in FreshBooster isn't Load. Error Code 10");
                    ErrorTime = TickCount(10000);
                }
            }

        }
        private static void Obj_AI_Base_OnIssueOrder(AIBaseClient sender, AIBaseClientIssueOrderEventArgs args)
        {
            try
            {

            }
            catch (Exception)
            {
                if (NowTime() > ErrorTime)
                {
                    Game.Print(ChampName + " in FreshBooster isn't Load. Error Code 11");
                    ErrorTime = TickCount(10000);
                }
            }

        }
        private static void GameObject_OnCreate(GameObject obj, EventArgs args)
        {
            try
            {

            }
            catch (Exception e)
            {
                Console.Write(e);
                Game.Print("FreshPoppy is not working. plz send message by KorFresh (Code 13)");
            }
        }

        private static void GameObject_OnDelete(GameObject obj, EventArgs args)
        {
            try
            {

            }
            catch (Exception e)
            {
                Console.Write(e);
                Game.Print("FreshPoppy is not working. plz send message by KorFresh (Code 14)");
            }
        }
    }
}