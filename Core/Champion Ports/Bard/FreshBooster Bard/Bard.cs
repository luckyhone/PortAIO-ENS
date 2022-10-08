using System;
using System.Linq;
using EnsoulSharp;
using EnsoulSharp.SDK;
using EnsoulSharp.SDK.MenuUI;
using EnsoulSharp.SDK.Rendering;
using Render = EnsoulSharp.SDK.Render;
using Color = System.Drawing.Color;
using static FreshBooster.FreshCommon;

namespace FreshBooster.Champion
{
    class Bard
    {
        // Default Setting
        public static int ErrorTime;
        public const string ChampName = "Bard";   // Edit
        public static AIHeroClient Player;
        public static Spell _Q, _W, _E, _R;
        
        // Default Setting

        public static int cnt = 0;
        public static AIBaseClient BardQTarget1, BardQTarget2;
        public static Geometry.Rectangle Range1, Range2;
        public static AIHeroClient RRange;
        public static int RCnt;

        private void SkillSet()
        {
            try
            {
                _Q = new Spell(SpellSlot.Q, 950f);
                _Q.SetSkillshot(0.4f, 120f, 1400f, false, EnsoulSharp.SDK.SpellType.Line);
                _W = new Spell(SpellSlot.W, 1000);
                _E = new Spell(SpellSlot.E, 900);
                _R = new Spell(SpellSlot.R, 3400f);
                _R.SetSkillshot(0.5f, 340f, 1400f, false, EnsoulSharp.SDK.SpellType.Circle);
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
                    Combo.Add(new MenuBool("Bard_CUse_Q", "Use Q").SetValue(true));
                    Combo.Add(new MenuSlider("Bard_CUse_Q_Hit", "Q HitChance",3, 1, 6));
                    Combo.Add(new MenuBool("Bard_CUse_OnlyQ", "Only use Q sturn").SetValue(true));
                    Combo.Add(new MenuKeyBind("CKey", "Combo Key",Keys.Space, KeyBindType.Press));
                }
                _MainMenu.Add(Combo);

                var Harass = new Menu("Harass", "Harass");
                {

                    Harass.Add(new MenuBool("Bard_HUse_Q", "Use Q").SetValue(true));
                    Harass.Add(new MenuBool("Bard_HUse_OnlyQ", "Only use Q sturn").SetValue(true));
                    Harass.Add(new MenuSlider("Bard_AManarate", "Mana %",20));
                    Harass.Add(new MenuBool("Bard_Auto_HEnable", "Auto Harass").SetValue(true));
                    Harass.Add(new MenuKeyBind("HKey", "Harass Key",Keys.C, KeyBindType.Press));
                }
                _MainMenu.Add(Harass);

                var KillSteal = new Menu("KillSteal", "KillSteal");
                {
                    KillSteal.Add(new MenuBool("Bard_KUse_Q", "Use Q").SetValue(true));
                }
                _MainMenu.Add(KillSteal);
                var healW = new Menu("healW", "Heal W");
                var Misc = new Menu("Misc", "Misc");
                {
                    healW.Add(new MenuSlider("Bard_HealWMin", "Min HP %",20, 0, 100));
                    healW.Add(new MenuBool("Bard_HealWMinEnable", "Enable").SetValue(true));
                    Misc.Add(healW);
                    Misc.Add(new MenuBool("Bard_Anti", "Anti-Gabcloser Q").SetValue(true));
                    Misc.Add(new MenuBool("Bard_Inter", "Interrupt R").SetValue(true));
                    Misc.Add(new MenuKeyBind("BardRKey", "Ult R",Keys.G, KeyBindType.Press));
                }
                _MainMenu.Add(Misc);

                var Draw = new Menu("Draw", "Draw");
                {
                    Draw.Add(new MenuBool("Bard_Draw_Q", "Draw Q").SetValue(false));
                    Draw.Add(new MenuBool("Bard_Draw_W", "Draw W").SetValue(false));
                    Draw.Add(new MenuBool("Bard_Draw_E", "Draw E").SetValue(false));
                    Draw.Add(new MenuBool("Bard_Draw_R", "Draw R").SetValue(false));
                    Draw.Add(new MenuBool("Bard_Draw_R1", "Draw R in MiniMap").SetValue(false));
                    Draw.Add(new MenuBool("Bard_Indicator", "Draw Damage Indicator").SetValue(true));
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
                if (_MainMenu["Bard_Draw_Q"].GetValue<MenuBool>().Enabled)
                    CircleRender.Draw(Player.Position, _Q.Range, Color.White.ToSharpDxColor(), 1);
                if (_MainMenu["Bard_Draw_W"].GetValue<MenuBool>().Enabled)
                    CircleRender.Draw(Player.Position, _W.Range, Color.White.ToSharpDxColor(), 1);
                if (_MainMenu["Bard_Draw_E"].GetValue<MenuBool>().Enabled)
                    CircleRender.Draw(Player.Position, _W.Range, Color.White.ToSharpDxColor(), 1);
                if (_MainMenu["Bard_Draw_R"].GetValue<MenuBool>().Enabled)
                    CircleRender.Draw(Player.Position, _R.Range, Color.White.ToSharpDxColor(), 1);
                if (_MainMenu["Bard_Draw_R1"].GetValue<MenuBool>().Enabled)
                    LeagueSharpCommon.Utility.DrawCircle(Player.Position, _R.Range, Color.White, 1, 23, true);
                var QTarget = TargetSelector.GetTarget(_Q.Range, DamageType.Magical);
                if (QTarget != null)
                    Drawing.DrawCircle(QTarget.Position, 150,1, Color.Green);
                if (_MainMenu["BardRKey"].GetValue<MenuKeyBind>().Active)
                {
                    CircleRender.Draw(Game.CursorPos, 340, Color.Aqua.ToSharpDxColor());
                    //Drawing.DrawCircle(Game.CursorPos, _R.Range, Color.AliceBlue);
                }                
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
        public Bard()
        {
            Player = ObjectManager.Player;
            SkillSet();
            Menu();
            Game.OnUpdate += OnGameUpdate;
            Drawing.OnDraw += Drawing_OnDraw;
            Drawing.OnEndScene += Drawing_OnEndScene;
            AntiGapcloser.OnGapcloser += AntiGapcloser_OnEnemyGapcloser;
            AIBaseClient.OnDoCast += OnProcessSpell;
            Interrupter.OnInterrupterSpell += Interrupter2_OnInterruptableTarget;
            Orbwalker.OnBeforeAttack += Orbwalking_BeforeAttack;
            GameObject.OnCreate += GameObject_OnCreate;
            GameObject.OnDelete += GameObject_OnDelete;
            Spellbook.OnCastSpell += Spellbook_OnCastSpell;
        }


        private static void OnGameUpdate(EventArgs args)
        {
            try
            {
                if (Player.IsDead) return;

                // Set Target
                var QTarget = TargetSelector.GetTarget(_Q.Range, DamageType.Magical);

                //Kill
                if (_MainMenu["KillSteal"]["Bard_KUse_Q"].GetValue<MenuBool>().Enabled)
                    if (QTarget != null)
                        if (_Q.IsReady())
                            if (QTarget.Health < _Q.GetDamage(QTarget))
                                _Q.CastIfHitchanceEquals(QTarget, HitChance.High);

                // W
                if (_MainMenu["Misc"]["healW"]["Bard_HealWMinEnable"].GetValue<MenuBool>().Enabled && !Player.IsRecalling())
                {
                    var ally = GameObjects.AllyHeroes.OrderBy(f => f.Health).FirstOrDefault(f => f.Distance(Player.Position) < _W.Range && !f.IsDead && !f.IsZombie() && f.HealthPercent < _MainMenu["Misc"]["healW"]["Bard_HealWMin"].GetValue<MenuSlider>().Value);
                    if (ally != null && _W.IsReady() && !ally.InFountain())
                        _W.CastOnUnit(ally);
                }

                //R
                if (_MainMenu["Misc"]["BardRKey"].GetValue<MenuKeyBind>().Active && _R.IsReady())
                {
                    RCnt = 0;
                    var range = GameObjects.Heroes.OrderBy(f => Game.CursorPos.Distance(f.Position) < 340f);
                    foreach (var item in range)
                    {
                        RCnt++;
                    }
                    if (RCnt == 0)
                        return;
                    var target = range.FirstOrDefault(f => f.Distance(Game.CursorPos) < 340f);
                    if (target != null)
                    {
                        _R.SetSkillshot(Player.Distance(target.Position) * 3400 / 1.4f, 340f, 1400f, false,
                            EnsoulSharp.SDK.SpellType.Circle);

                        _R.CastIfHitchanceEquals(target, HitChance.Medium);
                    }
                }

                // Combo
                if (_MainMenu["Combo"]["CKey"].GetValue<MenuKeyBind>().Active)
                {
                    if (_MainMenu["Combo"]["Bard_CUse_Q"].GetValue<MenuBool>().Enabled && _Q.IsReady() && QTarget != null)
                    {
                        BardQ(QTarget, true);
                        if (_MainMenu["Combo"]["Bard_CUse_OnlyQ"].GetValue<MenuBool>().Enabled)
                        {
                            if (cnt == 2)
                                if (BardQTarget1 is AIHeroClient || BardQTarget2 is AIHeroClient)
                                    _Q.CastIfHitchanceEquals(BardQTarget1, HitChance.High);
                            if (cnt == 1 && BardQTarget1 is AIHeroClient && BardQTarget1.Position.Extend(Player.Position, -450).IsWall())
                                _Q.CastIfHitchanceEquals(BardQTarget1, HitChance.High);
                        }
                        else
                        {
                            BardQ(QTarget, false);
                            if (BardQTarget1 == QTarget || BardQTarget2 == QTarget)
                                _Q.CastIfHitchanceEquals(BardQTarget1, HitChance.High);
                        }
                    }
                }
                /*
                // Harass
                if (_MainMenu["Harass"]["HKey"].GetValue<MenuKeyBind>().Active || _MainMenu["Harass"]["Bard_Auto_HEnable"].GetValue<MenuBool>().Enabled)
                {
                    if (_MainMenu["Harass"]["Bard_HUse_Q"].GetValue<MenuBool>().Enabled && _Q.IsReady() && QTarget != null && _MainMenu["Harass"]["Bard_AManarate"].GetValue<MenuSlider>().Value < Player.ManaPercent)
                    {
                        BardQ(QTarget, true);
                        // Sturn
                        if (_MainMenu["Harass"]["Bard_HUse_OnlyQ"].GetValue<MenuBool>().Enabled)
                        {
                            if (cnt == 2 && Range2 != null)
                                if ((BardQTarget1 is AIHeroClient && BardQTarget1 != Player) || (BardQTarget2 is AIHeroClient && BardQTarget2 != Player))
                                {
                                    _Q.CastIfHitchanceEquals(BardQTarget1, Hitchance("Bard_CUse_Q_Hit"));
                                }

                            if (cnt == 1 && BardQTarget1 is AIHeroClient && BardQTarget1.Position.Extend(Player.Position, -400).IsWall())
                            {
                                _Q.CastIfHitchanceEquals(BardQTarget1, Hitchance("Bard_CUse_Q_Hit"));
                            }
                        }
                        else
                        {
                            BardQ(QTarget, false);
                            if (BardQTarget1 == QTarget || BardQTarget2 == QTarget)
                            {
                                _Q.CastIfHitchanceEquals(BardQTarget1, Hitchance("Bard_CUse_Q_Hit"));
                            }
                        }
                    }
                }*/
            }
            catch (Exception e )
            {
                if (NowTime() > ErrorTime)
                {
                    Game.Print(ChampName + " in FreshBooster isn't Load. Error Code 06");
                    Console.WriteLine(e);
                    ErrorTime = TickCount(10000);
                }
            }
        }
        private void AntiGapcloser_OnEnemyGapcloser(AIHeroClient sender, AntiGapcloser.GapcloserArgs gapcloser)
        {
            try
            {
                if (_MainMenu["Bard_Anti"].GetValue<MenuBool>().Enabled && _Q.IsReady() && sender.Distance(Player.Position) < _Q.Range)
                    _Q.CastIfHitchanceEquals(sender, HitChance.Medium);
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
        private static void Spellbook_OnCastSpell(Spellbook sender, SpellbookCastSpellEventArgs args)
        {

        }
        private static void Interrupter2_OnInterruptableTarget(AIHeroClient sender, Interrupter.InterruptSpellArgs args)
        {
            try
            {
                if (_MainMenu["Bard_Inter"].GetValue<MenuBool>().Enabled && _R.IsReady() && sender.Distance(Player.Position) < _R.Range)
                {
                    _R.SetSkillshot(Player.Distance(sender.Position) * 3400 / 1.5f, 340f, 1400f, false, EnsoulSharp.SDK.SpellType.Circle);
                    _R.CastIfHitchanceEquals(sender, HitChance.Medium);
                }
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
        private void Orbwalking_BeforeAttack(object sender, BeforeAttackEventArgs e)
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

        private static void BardQ(AIHeroClient Target, bool Type, bool Draw = false)
        {
            // Type 0: no sturn / 1: only sturn
            // If Draw is true, return draw
            /* return
            target1, target2, type
            */            
            Range1 = new Geometry.Rectangle(Player.Position, Player.Position.Extend(Target.Position, _Q.Range), _Q.Width);
            Range2 = null;
            if (Draw)
                Range1.Draw(Color.Red);
            cnt = 0;
            BardQTarget1 = Player;
            BardQTarget2 = Player;
            foreach (var item in ObjectManager.Get<AIBaseClient>().OrderBy(f => f.Distance(f.Position)))
            {
                if (item.Distance(Player.Position) < _Q.Range)
                    if (item is AIHeroClient || item is AIMinionClient)
                        if (item.IsEnemy && !item.IsDead)
                        {
                            if (cnt == 2)
                                break;
                            if (cnt == 0 && Range1.IsInside(item.Position))
                            {
                                BardQTarget1 = item;
                                Range2 = new Geometry.Rectangle(Player.Position.Extend(BardQTarget1.Position, Player.Distance(BardQTarget1.Position)),
                                    Player.Position.Extend(BardQTarget1.Position, Player.Distance(BardQTarget1.Position) + 450), _Q.Width);
                                if (Draw)
                                    Range2.Draw(Color.Yellow);
                                cnt++;
                            }
                            if (cnt == 1 && Range2.IsInside(item.Position))
                            {
                                BardQTarget2 = item;
                                cnt++;
                            }
                        }
            }
        }
    }
}