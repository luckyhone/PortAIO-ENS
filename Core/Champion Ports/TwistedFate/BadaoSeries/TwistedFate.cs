﻿using System;
using System.Collections.Generic;
using System.Drawing;
using System.Linq;
using EnsoulSharp;
using EnsoulSharp.SDK;
using EnsoulSharp.SDK.MenuUI;
using ShadowTracker;
using SPrediction;

namespace BadaoSeries.Plugin
{
    internal class TwistedFate : AddUI
    {
        private static int cardtick, yellowtick;
        public static bool helpergold, helperblue, helperred;
        private static bool isobvious { get { return Environment.TickCount - cardtick <= 500; } }
        private static bool IsPickingCard { get { return Player.HasBuff("pickacard_tracker"); } }
        private static bool CanUseR2 { get { return R.IsReady() && Player.HasBuff("destiny_marker"); } }
        private static bool CanUseR1 { get { return R.IsReady() && !Player.HasBuff("destiny_marker"); } }
        private static bool PickACard { get { return W.Instance.Name == "PickACard"; } }
        private static bool GoldCard { get { return W.Instance.Name == "goldcardlock"; } }
        private static bool BlueCard { get { return W.Instance.Name == "bluecardlock"; } }
        private static bool RedCard { get { return W.Instance.Name == "redcardlock"; } }
        private static bool HasBlue { get { return Player.HasBuff("bluecardpreattack"); } }
        private static bool HasRed { get { return Player.HasBuff("redcardpreattack"); } }
        private static bool HasGold { get { return Player.HasBuff("goldcardpreattack"); } }
        private static string HasACard
        {
            get
            {
                if (Player.HasBuff("bluecardpreattack"))
                    return "blue";
                if (Player.HasBuff("goldcardpreattack"))
                    return "gold";
                if (Player.HasBuff("redcardpreattack"))
                    return "red";
                return "none";
            }
        }
        public TwistedFate()
        {
            Q = new Spell(SpellSlot.Q, 1400);
            W = new Spell(SpellSlot.W);
            E = new Spell(SpellSlot.E);
            R = new Spell(SpellSlot.R);
            Q.SetSkillshot(0.25f, 40, 1000, false, SpellType.Line);
            Q.DamageType = W.DamageType = E.DamageType = DamageType.Magical;
            Q.MinHitChance = HitChance.High;
            
            Menu Combo = new Menu("Combo", "Combo");
            {
                Bool(Combo, "Qc", "Q", true);
                Bool(Combo, "Qafterattackc", "Q after attack", true);
                Bool(Combo, "Qimmobilec", "Q on immobile", true);
                Slider(Combo, "Qhitc", "Q if will hit", 2, 1, 3);
                Bool(Combo, "Wc", "W", true);
                Bool(Combo, "pickgoldc", "Pick gold card while using R", true);
                Bool(Combo, "dontpickyellow1stc", "don't pick gold at 1st turn", false);
                MainMenu.Add(Combo);
            }
            Menu Harass = new Menu("Harass", "Harass");
            {
                Bool(Harass, "Qh", "Q", true);
                Bool(Harass, "Qafterattackh", "Q after attack", true);
                Bool(Harass, "Qimmobileh", "Q on immobile", true);
                Slider(Harass, "Qhith", "Q if will hit", 2, 1, 3);
                Bool(Harass, "Wh", "W", true);
                List(Harass, "Wcolorh", "W card type", new[] { "blue", "red", "gold" });
                Slider(Harass, "manah", "Min mana", 40, 0, 100);
                MainMenu.Add(Harass);
            }
            Menu Clear = new Menu("Clear", "Clear");
            {
                Bool(Clear, "Qj", "Q", true);
                Slider(Clear, "Qhitj", "Q if will hit", 2, 1, 3);
                Bool(Clear, "Wj", "W", true);
                List(Clear, "Wcolorj", "W card type", new[] { "blue", "red" });
                Slider(Clear, "wmanaj", "mana only W blue", 0, 0, 100);
                Slider(Clear, "manaj", "Min mana", 40, 0, 100);
                MainMenu.Add(Clear);
            }
            Menu Auto = new Menu("Auto", "Auto");
            {
                Bool(Auto, "throwyellowa", "gapclose + interrupt: throw gold card", true);
                Bool(Auto, "killsteala", "KillSteal Q", true);
                MainMenu.Add(Auto);
            }
            Menu Helper = new Menu("Helper", "Pick card Helper");
            {
                Bool(Helper, "enableh", "Enabale", true);
                KeyBind(Helper, "pickyellowh", "Pick Yellow", Keys.W, KeyBindType.Toggle);
                KeyBind(Helper, "pickblueh", "Pick Blue", Keys.G, KeyBindType.Toggle);
                KeyBind(Helper, "pickredh", "Pick Red", Keys.H, KeyBindType.Toggle);
                MainMenu.Add(Helper);
            }
            Menu drawMenu = new Menu("Draw", "Draw");
            {
                Bool(drawMenu, "Qd", "Q");
                Bool(drawMenu, "Rd", "R");
                Bool(drawMenu, "Hpd", "Damage Indicator").ValueChanged += TwistedFate_ValueChanged;
                MainMenu.Add(drawMenu);
            }
            Game.OnUpdate += OnUpdate;
            Drawing.OnDraw += OnDraw;
            //Obj_AI_Base.OnSpellCast += OnProcessSpellCast;
            //GameObject.OnCreate += OnCreate;
            AntiGapcloser.OnGapcloser += Gapcloser_OnGapCloser;
            Interrupter.OnInterrupterSpell += InterruptableSpell_OnInterruptableTarget;
            Drawing.OnEndScene += Drawing_OnEndScene;
            Orbwalker.OnBeforeAttack += Orbwalking_BeforeAttack;
            Orbwalker.OnAfterAttack += Orbwalking_AfterAttack;
            Orbwalker.OnAttack += Orbwalking_OnAttack;
            //LeagueSharp.Common.Utility.HpBar//DamageIndicator.DamageToUnit = TwistedFateDamage;
            //LeagueSharp.Common.Utility.HpBar//DamageIndicator.Enabled = drawhp;
            //Custom//DamageIndicator.Initialize(TwistedFateDamage);
            //Custom//DamageIndicator.Enabled = drawhp;
        }

        private void TwistedFate_ValueChanged(MenuBool menuitem, EventArgs args)
        {
            
        }

        private void InterruptableSpell_OnInterruptableTarget(AIHeroClient sender, Interrupter.InterruptSpellArgs args)
        {
            if (!Enable)
                return;
            if (sender.IsEnemy && Player.InAutoAttackRange(sender))
            {
                if (HasGold)
                {
                    Player.IssueOrder(GameObjectOrder.AttackUnit, sender);
                }
            }
        }

        private void Gapcloser_OnGapCloser(AIHeroClient sender, AntiGapcloser.GapcloserArgs args)
        {
            if (!Enable)
                return;
            if (sender.IsEnemy && Player.InAutoAttackRange(sender))
            {
                if (HasGold)
                {
                    Player.IssueOrder(GameObjectOrder.AttackUnit, sender);
                }
            }
        }

        private void Drawing_OnEndScene(EventArgs args)
        {
            if (!Enable)
                return;
            if (Player.IsDead)
                return;
            if (drawr)
            {
                LeagueSharpCommon.Render.Circle.DrawCircle(Player.Position, 5500, Color.Aqua,5,true);
            }
        }

        private void OnDraw(EventArgs args)
        {
            if (!Enable)
                return;
            if (Player.IsDead)
                return;
            if (drawq)
                LeagueSharpCommon.Render.Circle.DrawCircle(Player.Position, Q.Range, Color.Aqua);
        }

        private void Orbwalking_BeforeAttack(object sender, BeforeAttackEventArgs args)
        {
            if (!Enable)
                return;
            var mode = new OrbwalkerMode[] { OrbwalkerMode.Harass, OrbwalkerMode.Combo };
            if (IsPickingCard && mode.Contains(Orbwalker.ActiveMode)) args.Process = false;
            else if (HasACard != "none" && !GameObjects.EnemyHeroes.Contains(args.Target) && Orbwalker.ActiveMode == OrbwalkerMode.Harass)
            {
                args.Process = false;
                var target = TargetSelector.GetTarget(Player.GetRealAutoAttackRange(Player), DamageType.Magical);
                if (target.IsValidTarget() && !target.IsZombie())
                    Player.IssueOrder(GameObjectOrder.AttackUnit, target);
            }
            else if (HasACard != "none" && HasRed && Orbwalker.ActiveMode == OrbwalkerMode.LaneClear)
            {
                args.Process = false;
                IDictionary<AIMinionClient, int> creeps = new Dictionary<AIMinionClient, int>();
                foreach (var x in ObjectManager.Get<AIMinionClient>().Where(x => x.Team != Player.Team && x.Team != GameObjectTeam.Neutral  && Player.InAutoAttackRange(x)))
                {
                    creeps.Add(x, ObjectManager.Get<AIMinionClient>().Count(y => y.Team != Player.Team && y.Team != GameObjectTeam.Neutral && y.IsValidTarget() && y.Distance(x.Position) <= 300));
                }
                foreach (var x in ObjectManager.Get<AIMinionClient>().Where(x => x.Team == GameObjectTeam.Neutral && Player.InAutoAttackRange(x)))
                {
                    creeps.Add(x, ObjectManager.Get<AIMinionClient>().Count(y => y.Team == GameObjectTeam.Neutral && y.IsValidTarget() && y.Distance(x.Position) <= 300));
                }
                var minion = creeps.OrderByDescending(x => x.Value).FirstOrDefault();
                Player.IssueOrder(GameObjectOrder.AttackUnit, minion.Key);
            }
        }

        private void Orbwalking_AfterAttack(object sender, AfterAttackEventArgs e)
        {
            if (!Enable)
                return;
            if (e.Target.IsValidTarget() && (e.Target as AIHeroClient) != null && ((AIHeroClient) e.Target).IsValid)
            {
                if (Environment.TickCount - yellowtick <= 1500)
                    return;
                if (Orbwalker.ActiveMode == OrbwalkerMode.Combo && comboqafterattack)
                {
                    if (e.Target.IsValidTarget() )
                    {
                        var Target = TargetSelector.GetTarget(Q.Range, DamageType.Magical);
                        if (Target.IsValidTarget() && !Target.IsZombie())
                            Q.Cast(Q.GetPrediction(Target).CastPosition);
                    }
                }
                if (Orbwalker.ActiveMode == OrbwalkerMode.Harass && harassqafterattack && Player.ManaPercent >= harassmana)
                {
                    if (e.Target.IsValidTarget())
                    {
                        var Target = TargetSelector.GetTarget(Q.Range, DamageType.Magical);
                        if (Target.IsValidTarget() && !Target.IsZombie())
                            Q.Cast(Q.GetPrediction(Target).CastPosition);
                    }
                }
            }
        }

        private void Orbwalking_OnAttack(object sender, AttackingEventArgs e)
        {
            if (!Enable)
                return;
            if (HasGold)
                yellowtick = Environment.TickCount;
        }

        private static bool dontbeobvious { get { return MainMenu["dontpickyellow1stc"].GetValue<MenuBool>().Enabled; } }
        private static bool comboq { get { return MainMenu["Qc"].GetValue<MenuBool>().Enabled; } }
        private static bool comboqafterattack { get { return  MainMenu["Qafterattackc"].GetValue<MenuBool>().Enabled; } }
        private static bool comboqimmobile { get { return  MainMenu["Qimmobilec"].GetValue<MenuBool>().Enabled; } }
        private static int comboqhit { get { return  MainMenu["Qhitc"].GetValue<MenuSlider>().Value; } }
        private static bool combow { get { return  MainMenu["Wc"].GetValue<MenuBool>().Enabled; } }
        private static bool combopickgold { get { return  MainMenu["pickgoldc"].GetValue<MenuBool>().Enabled; } }
        private static bool harassq { get { return  MainMenu["Qh"].GetValue<MenuBool>().Enabled; } }
        private static bool harassqafterattack { get { return  MainMenu["Qafterattackh"].GetValue<MenuBool>().Enabled; } }
        private static bool harassqimmobile { get { return  MainMenu["Qimmobileh"].GetValue<MenuBool>().Enabled; } }
        private static int harassqhit { get { return  MainMenu["Qhith"].GetValue<MenuSlider>().Value; } }
        private static bool harassw { get { return  MainMenu["Wh"].GetValue<MenuBool>().Enabled; } }
        private static int harasswcolor { get { return  MainMenu["Wcolorh"].GetValue<MenuList>().Index; } }
        private static int harassmana { get { return  MainMenu["manah"].GetValue<MenuSlider>().Value; } }
        private static bool clearq { get { return  MainMenu["Qj"].GetValue<MenuBool>().Enabled; } }
        private static int clearqhit { get { return  MainMenu["Qhitj"].GetValue<MenuSlider>().Value; } }
        private static bool clearw { get { return  MainMenu["Wj"].GetValue<MenuBool>().Enabled; } }
        private static int clearwcolor { get { return  MainMenu["Wcolorj"].GetValue<MenuList>().Index; } }
        private static int clearwmana { get { return  MainMenu["wmanaj"].GetValue<MenuSlider>().Value; } }
        private static int clearmana { get { return  MainMenu["manaj"].GetValue<MenuSlider>().Value; } }
        private static bool autothrowyellow { get { return  MainMenu["throwyellowa"].GetValue<MenuBool>().Enabled; } }
        private static bool autokillsteal { get { return  MainMenu["killsteala"].GetValue<MenuBool>().Enabled; } }
        private static bool helperenable { get { return  MainMenu["enableh"].GetValue<MenuBool>().Enabled; } }
        private static bool helperpickyellow
        {
            get { return  MainMenu["pickyellowh"].GetValue<MenuKeyBind>().Active; }
            set
            {
                var key = MainMenu["pickyellowh"].GetValue<MenuKeyBind>().Key;
                var type = MainMenu["pickyellowh"].GetValue<MenuKeyBind>().Type;
                //MainMenu["pickyellowh"].GetValue<MenuKeyBind>().Key =  <KeyBind>(new KeyBind(key, KeyBindType.Toggle, value));
            }
        }
        private static bool helperpickblue
        {
            get { return  MainMenu["pickblueh"].GetValue<MenuKeyBind>().Active; }
            set
            {
                var key = MainMenu["pickblueh"].GetValue<MenuKeyBind>().Key;
                var type = MainMenu["pickblueh"].GetValue<MenuKeyBind>().Type;
                //MainMenu.Item("pickblueh").SetValue<KeyBind>(new KeyBind(key, KeyBindType.Toggle, value));
            }
        }
        private static bool helperpickred
        {
            get { return  MainMenu["pickredh"].GetValue<MenuKeyBind>().Active; }
            set
            {
                var key = MainMenu["pickredh"].GetValue<MenuKeyBind>().Key;
                var type = MainMenu["pickredh"].GetValue<MenuKeyBind>().Type;
                //MainMenu.Item("pickredh").SetValue<KeyBind>(new KeyBind(key, KeyBindType.Toggle, value));
            }
        }
        
        private static bool drawq { get { return  MainMenu["Qd"].GetValue<MenuBool>().Enabled; } }
        private static bool drawr { get { return  MainMenu["Rd"].GetValue<MenuBool>().Enabled; } }
        private static bool drawhp { get { return  MainMenu["Hpd"].GetValue<MenuBool>().Enabled; } }
        private static void AutoHelper()
        {
            if (autokillsteal && Q.IsReady())
            {
                foreach (var x in GameObjects.EnemyHeroes.Where(x => x.IsValidTarget(Q.Range) && Player.GetSpellDamage(x, SpellSlot.Q) > x.Health))
                {
                    Q.Cast(x);
                }
            }
            if (helperenable)
            {
                if (helperpickblue || helperpickred || helperpickyellow)
                {
                    //EloBuddy.Player.IssueOrder(GameObjectOrder.MoveTo,Game.CursorPos);
                    if (!IsPickingCard && PickACard && Environment.TickCount - cardtick >= 500)
                    {
                        cardtick = Environment.TickCount;
                        W.Cast();
                    }
                    if (helperpickyellow && GoldCard) W.Cast();
                    if (helperpickblue && BlueCard) W.Cast();
                    if (helperpickred && RedCard) W.Cast();
                }
                if (HasGold)
                    helperpickyellow = false;
                if (HasBlue)
                    helperpickblue = false;
                if (HasRed)
                    helperpickred = false;
            }
            if (combow && Player.HasBuff("destiny_marker") && combopickgold && W.IsReady())
            {
                if (!IsPickingCard && PickACard && Environment.TickCount - cardtick >= 500)
                {
                    cardtick = Environment.TickCount;
                    W.Cast();
                }
                if (IsPickingCard && GoldCard)
                    W.Cast();
            }
        }

        private void OnUpdate(EventArgs args)
        {
            if (!Enable)
            {
                //LeagueSharp.Common.Utility.HpBar//DamageIndicator.Enabled = false;
                //Custom//DamageIndicator.Enabled = false;
                return;
            }
            AutoHelper();
            if (Orbwalker.ActiveMode == OrbwalkerMode.Combo)
                Combo();
            if (Orbwalker.ActiveMode == OrbwalkerMode.Harass && Player.ManaPercent >= harassmana)
                Harass();
            if (Orbwalker.ActiveMode == OrbwalkerMode.LaneClear)
                Clear();
        }
        private static void Combo()
        {
            if (Q.IsReady() && comboq)
            {
                if (comboqimmobile)
                {
                    foreach (var x in GameObjects.EnemyHeroes.Where(x => x.IsValidTarget() && !x.IsZombie()))
                        Q.CastIfHitchanceEquals(x, HitChance.Immobile);
                }
                {
                    var target = TargetSelector.GetTarget(Q.Range, DamageType.Magical);
                    if (target.IsValidTarget() && !target.IsZombie())
                        Q.CastIfWillHit(target, comboqhit);
                }
            }
            if (combow && W.IsReady())
            {
                var target = TargetSelector.GetTarget(900, DamageType.Magical);
                if (target.IsValidTarget() && !target.IsZombie())
                {
                    if (!IsPickingCard && PickACard && Environment.TickCount - cardtick >= 500)
                    {
                        cardtick = Environment.TickCount;
                        W.Cast();
                    }
                    if (IsPickingCard)
                    {
                        if (Player.Mana >= Q.Instance.ManaCost)
                        {
                            if (GoldCard && !(dontbeobvious && isobvious))
                                W.Cast();
                        }
                        else if (GameObjects.AllyHeroes.Any(x => x.IsValidTarget(800, false)))
                        {
                            if (GoldCard && !(dontbeobvious && isobvious))
                                W.Cast();
                        }
                        else if (BlueCard)
                        {
                            W.Cast();
                        }
                    }
                }
            }
        }
        private static void Harass()
        {
            if (Q.IsReady() && harassq && Player.ManaPercent >= clearmana)
            {
                if (harassqimmobile)
                {
                    foreach (var x in GameObjects.EnemyHeroes.Where(x => x.IsValidTarget() && !x.IsZombie()))
                        Q.CastIfHitchanceEquals(x, HitChance.Immobile);
                }
                {
                    var target = TargetSelector.GetTarget(Q.Range, DamageType.Magical);
                    Q.CastIfWillHit(target, harassqhit);
                }
            }
            if (harassw && W.IsReady())
            {
                var target = TargetSelector.GetTarget(900, DamageType.Magical);
                if (target.IsValidTarget() && !target.IsZombie())
                {
                    if (!IsPickingCard && PickACard && Environment.TickCount - cardtick >= 500 && Player.ManaPercent >= clearmana)
                    {
                        cardtick = Environment.TickCount;
                        W.Cast();
                    }
                    if (IsPickingCard)
                    {
                        switch (harasswcolor)
                        {
                            case 0:
                                if (BlueCard)
                                    W.Cast();
                                break;
                            case 1:
                                if (RedCard)
                                    W.Cast();
                                break;
                            case 2:
                                if (GoldCard)
                                    W.Cast();
                                break;
                        }
                    }
                }
            }
        }
        private static void Clear()
        {
            if (Q.IsReady() && clearq && Player.ManaPercent >= clearmana)
            {
                var farm = Q.GetLineFarmLocation(MinionManager.GetMinions(Q.Range, MinionManager.MinionTypes.All));
                if (farm.MinionsHit >= clearqhit)
                    Q.Cast(farm.Position);
            }
            if (W.IsReady() && clearw)
            {
                var target = ObjectManager.Get<AIMinionClient>().Where(x => x.Team != Player.Team && x.IsValidTarget() && Player.InAutoAttackRange(x));
                if (target.Any())
                {
                    if (!IsPickingCard && PickACard && Environment.TickCount - cardtick >= 500 && Player.ManaPercent >= clearmana)
                    {
                        cardtick = Environment.TickCount;
                        W.Cast();
                    }
                    if (IsPickingCard)
                    {
                        if (clearwmana > Player.Mana * 100 / Player.MaxMana)
                        {
                            if (BlueCard)
                                W.Cast();
                        }
                        else
                        {
                            switch (clearwcolor)
                            {
                                case 0:
                                    if (BlueCard)
                                        W.Cast();
                                    break;
                                case 1:
                                    if (RedCard)
                                        W.Cast();
                                    break;
                            }
                        }
                    }
                }
            }
        }
        private static float TwistedFateDamage(AIHeroClient target)
        {
            var Qdamage = (float)Player.GetSpellDamage(target, Q.Slot);
            var Wdamage = (float)Player.GetSpellDamage(target, W.Slot);
            float x = 0;
            if ((W.IsReady() || HasACard != "none") && Q.IsReady())
            {
                if ((Player.Mana >= Q.Instance.ManaCost + W.Instance.ManaCost) || (Player.Mana >= Q.Instance.ManaCost && HasACard != "none"))
                {
                    x = x + Qdamage + Wdamage;
                }
                else if (Player.Mana >= Q.Instance.ManaCost)
                {
                    x = x + Qdamage;
                }
                else if (Player.Mana >= W.Instance.ManaCost || HasACard != "none")
                {
                    x = x + Wdamage;
                }
            }
            else if (Q.IsReady())
            {
                x = x + Qdamage;
            }
            else if (W.IsReady() || HasACard != "none")
            {
                x = x + Wdamage;
            }
            if (LichBane.IsReady)
            {
                x = x + (float)Player.CalculateDamage(target, DamageType.Magical, 0.75 * Player.BaseAttackDamage + 0.5 * Player.FlatMagicDamageMod);
            }
            else if (TrinityForce.IsReady)
            {
                x = x + (float)Player.CalculateDamage(target, DamageType.Magical, 2 * Player.BaseAttackDamage);
            }
            else if (IcebornGauntlet.IsReady)
            {
                x = x + (float)Player.CalculateDamage(target, DamageType.Magical, 1.25 * Player.BaseAttackDamage);
            }
            else if (Sheen.IsReady)
            {
                x = x + (float)Player.CalculateDamage(target, DamageType.Magical, 1 * Player.BaseAttackDamage);
            }
            if (LudensEcho.IsReady)
            {
                x = x + (float)Player.CalculateDamage(target, DamageType.Magical, 100 + 0.1 * Player.FlatMagicDamageMod);
            }
            x = x + (float)Player.GetAutoAttackDamage(target, true);
            return x;
        }
        private static void checkbuff()
        {
            var temp = Player.Buffs.Aggregate("", (current, buff) => current + ("( " + buff.Name + " , " + buff.Count + " )"));
            if (temp != null)
                Game.Print(temp);
        }
    }
}