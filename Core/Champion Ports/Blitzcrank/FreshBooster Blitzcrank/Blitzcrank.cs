using System;
using EnsoulSharp;
using EnsoulSharp.SDK;
using EnsoulSharp.SDK.MenuUI;
using static FreshBooster.FreshCommon;
using Color = System.Drawing.Color;
using Render = EnsoulSharp.SDK.Render;

namespace FreshBooster.Champion
{
    class Blitzcrank
    {
        // Default Setting
        public static int ErrorTime;
        public const string ChampName = "Blitzcrank";   // Edit
        public static AIHeroClient Player;
        public static Spell _Q, _W, _E, _R;
        
        // Default Setting

        private void SkillSet()
        {
            try
            {
                _Q = new Spell(SpellSlot.Q, 950f);
                _Q.SetSkillshot(0.25f, 70f, 1800f, true, EnsoulSharp.SDK.SpellType.Line);
                _W = new Spell(SpellSlot.W, 700f);
                _E = new Spell(SpellSlot.E, 150f);
                _R = new Spell(SpellSlot.R, 540f);
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
                    Combo.Add(new MenuBool("Blitzcrank_CUse_Q", "Use Q").SetValue(true));
                    Combo.Add(new MenuBool("Blitzcrank_CUse_W", "Use W").SetValue(true));
                    Combo.Add(new MenuBool("Blitzcrank_CUse_E", "Use E").SetValue(true));
                    Combo.Add(new MenuBool("Blitzcrank_CUse_R", "Use R").SetValue(true));
                    Combo.Add(new MenuSlider("Blitzcrank_CUseQ_Hit", "Q HitChance",6, 1, 6));
                    Combo.Add(new MenuKeyBind("CKey", "Combo Key",Keys.Space, KeyBindType.Press));
                }
                _MainMenu.Add(Combo);

                var Harass = new Menu("Harass", "Harass");
                {

                    Harass.Add(new MenuBool("Blitzcrank_HUse_Q", "Use Q").SetValue(true));
                    Harass.Add(new MenuBool("Blitzcrank_HUse_W", "Use W").SetValue(true));
                    Harass.Add(new MenuBool("Blitzcrank_HUse_E", "Use E").SetValue(true));
                    Harass.Add(new MenuSlider("Blitzcrank_AManarate", "Mana %",20));
                    Harass.Add(new MenuKeyBind("HKey", "Harass Key",Keys.C, KeyBindType.Press));
                }
                _MainMenu.Add(Harass);

                var KillSteal = new Menu("KillSteal", "KillSteal");
                {
                    KillSteal.Add(new MenuBool("Blitzcran_KUse_Q", "Use Q").SetValue(true));                    
                    KillSteal.Add(new MenuBool("Blitzcran_KUse_R", "Use R").SetValue(true));
                }
                _MainMenu.Add(KillSteal);

                var setGrab = new Menu("SetGrab", "SetGrab");
                var interrupt = new Menu("Interrupt", "Interrupt");
                var Misc = new Menu("Misc", "Misc");
                {
                    foreach (var enemy in ObjectManager.Get<AIHeroClient>())
                    {
                        if (enemy.Team != Player.Team)
                        {
                            setGrab.Add(new MenuList("Blitzcrank_GrabSelect" + enemy.CharacterName, enemy.CharacterName,new[] { "Enable", "Dont", "Auto" }));
                        }
                    }

                    Misc.Add(setGrab);
                    Misc.Add(interrupt);
                    interrupt.Add(new MenuBool("Blitzcrank_InterQ", "Use Q").SetValue(true));
                    interrupt.Add(new MenuBool("Blitzcrank_InterE", "Use E").SetValue(true));
                    interrupt.Add(new MenuBool("Blitzcrank_InterR", "Use R").SetValue(true));
                    Misc.Add(new MenuBool("Blitzcrank_GrabDash", "Grab to dashing enemy").SetValue(true));
                }
                _MainMenu.Add(Misc);

                var Draw = new Menu("Draw", "Draw");
                {
                    Draw.Add(new MenuBool("Blitzcrank_Draw_Q", "Draw Q").SetValue(false));
                    Draw.Add(new MenuBool("Blitzcrank_Draw_R", "Draw R").SetValue(false));
                    Draw.Add(new MenuBool("Blitzcrank_Indicator", "Draw Damage Indicator").SetValue(true));
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
                if (_MainMenu["Blitzcrank_Draw_Q"].GetValue<MenuBool>().Enabled)
                    Render.Circle.DrawCircle(Player.Position, _Q.Range, Color.White, 1);
                if (_MainMenu["Blitzcrank_Draw_R"].GetValue<MenuBool>().Enabled)
                    Render.Circle.DrawCircle(Player.Position, _R.Range, Color.White, 1);
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
                    if (_E.IsReady())
                        damage += _E.GetDamage(enemy);
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
        public Blitzcrank()
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
            //ObjectManager.Player.OnIssueOrder += Obj_AI_Base_OnIssueOrder;
        }


        private static void OnGameUpdate(EventArgs args)
        {
            try
            {
                if (Player.IsDead) return;
                var QTarget = TargetSelector.GetTarget(_Q.Range, DamageType.Magical);
                var WTarget = TargetSelector.GetTarget(1500, DamageType.Physical);
                var RTarget = TargetSelector.GetTarget(_R.Range, DamageType.Magical);

                if (_MainMenu["Misc"]["Blitzcrank_GrabDash"].GetValue<MenuBool>().Enabled && _Q.IsReady())
                    if (QTarget != null && _Q.GetPrediction(QTarget, false).Hitchance == HitChance.Dash)
                        _Q.CastIfHitchanceEquals(QTarget, HitChance.Dash);

                //killsteal
                if (_MainMenu["KillSteal"]["Blitzcran_KUse_Q"].GetValue<MenuBool>().Enabled && QTarget != null && QTarget.Health < _Q.GetDamage(QTarget) && _Q.IsReady())
                {
                    _Q.CastIfHitchanceEquals(QTarget, HitChance.VeryHigh);
                    return;
                }
                if (_MainMenu["KillSteal"]["Blitzcran_KUse_R"].GetValue<MenuBool>().Enabled && RTarget != null && RTarget.Health < _E.GetDamage(RTarget) && _R.IsReady())
                {
                    _R.Cast();
                    return;
                }

                if (QTarget == null) return;    // auto grab
                foreach (var enemy in ObjectManager.Get<AIHeroClient>())
                {
                    if (enemy.Team != Player.Team && QTarget != null
                        && _MainMenu["Misc"]["SetGrab"]["Blitzcrank_GrabSelect" + enemy.CharacterName].GetValue<MenuList>().Index == 2 && _Q.IsReady()
                        && QTarget.CharacterName == enemy.CharacterName)
                    {
                        if (QTarget.CanMove && QTarget.Distance(Player.Position) < _Q.Range * 0.9)
                            _Q.CastIfHitchanceEquals(QTarget, Hitchance("Blitzcrank_CUseQ_Hit"));
                        if (!QTarget.CanMove)
                            _Q.CastIfHitchanceEquals(QTarget, Hitchance("Blitzcrank_CUseQ_Hit"));
                    }
                }

                // Combo
                if (_MainMenu["Combo"]["CKey"].GetValue<MenuKeyBind>().Active)
                {
                    if (_MainMenu["Combo"]["Blitzcrank_CUse_Q"].GetValue<MenuBool>().Enabled && _Q.IsReady() && QTarget != null
                        && _MainMenu["Misc"]["SetGrab"]["Blitzcrank_GrabSelect" + QTarget.CharacterName].GetValue<MenuList>().Index != 1)
                    {
                        _Q.CastIfHitchanceEquals(QTarget, Hitchance("Blitzcrank_CUseQ_Hit"));
                    }
                    if (_MainMenu["Combo"]["Blitzcrank_CUse_W"].GetValue<MenuBool>().Enabled && _W.IsReady() && WTarget != null)
                        _W.Cast(Player, true);
                    if (_MainMenu["Combo"]["Blitzcrank_CUse_E"].GetValue<MenuBool>().Enabled && _E.IsReady() && QTarget.Distance(Player.ServerPosition) < 230)
                        _E.Cast(Player);
                    if (_MainMenu["Combo"]["Blitzcrank_CUse_R"].GetValue<MenuBool>().Enabled && _R.IsReady() && RTarget != null)
                        _R.Cast();
                }

                // Harass
                if (_MainMenu["Harass"]["HKey"].GetValue<MenuKeyBind>().Active && _MainMenu["Blitzcrank_AManarate"].GetValue<MenuSlider>().Value < Player.ManaPercent)
                {
                    if (_MainMenu["Harass"]["Blitzcrank_HUse_Q"].GetValue<MenuBool>().Enabled && _Q.IsReady() && QTarget != null
                        && _MainMenu["Misc"]["SetGrab"]["Blitzcrank_GrabSelect" + QTarget.CharacterName].GetValue<MenuList>().Index != 1)
                    {
                        _Q.CastIfHitchanceEquals(QTarget, Hitchance("Blitzcrank_CUseQ_Hit"));
                    }
                    if (_MainMenu["Harass"]["Blitzcrank_HUse_W"].GetValue<MenuBool>().Enabled && _W.IsReady() && WTarget != null)
                        _W.Cast(Player, true);
                    if (_MainMenu["Harass"]["Blitzcrank_HUse_E"].GetValue<MenuBool>().Enabled && _E.IsReady() && QTarget.Distance(Player.ServerPosition) < 230)
                        _E.Cast(Player);
                }
            }
            catch (Exception)
            {
                if (NowTime() > ErrorTime)
                {
                    Game.Print(ChampName + " in FreshBooster isn't Load. Error Code 06");
                    ErrorTime = TickCount(10000);
                }
            }
        }
        private void AntiGapcloser_OnEnemyGapcloser(AIHeroClient sender, AntiGapcloser.GapcloserArgs args)
        {
            try
            {

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
        private static void Interrupter2_OnInterruptableTarget(AIHeroClient sender, Interrupter.InterruptSpellArgs args)
        {
            try
            {
                if (Player.IsDead)
                    return;
                if (!sender.IsEnemy || !sender.IsValid())
                    return;

                if (_MainMenu["Misc"]["Interrupt"]["Blitzcrank_InterQ"].GetValue<MenuBool>().Enabled && _Q.IsReady())
                {
                    if (sender.Distance(Player.ServerPosition) <= _Q.Range)
                        _Q.Cast(sender);
                }
                if (_MainMenu["Misc"]["Interrupt"]["Blitzcrank_InterR"].GetValue<MenuBool>().Enabled && _R.IsReady())
                {
                    if (sender.Distance(Player.ServerPosition) <= _R.Range)
                        _R.Cast();
                }
                if (_MainMenu["Misc"]["Interrupt"]["Blitzcrank_InterR"].GetValue<MenuBool>().Enabled && _E.IsReady())
                {
                    if (sender.Distance(Player.ServerPosition) <= _E.Range)
                        _E.CastOnUnit(Player);
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
        private static void Obj_AI_Base_OnIssueOrder(AIBaseClient sender, AIBaseClientPlayAnimationEventArgs args)
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
    }
}