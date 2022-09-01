using System;
using System.Linq;
using EnsoulSharp;
using EnsoulSharp.SDK;
using EnsoulSharp.SDK.MenuUI;
using EnsoulSharp.SDK.Utility;
using PortAIO;
using SharpDX;
using SPrediction;

namespace Kimbaeng_KarThus
{
    internal class Program
    {
        public static Menu _menu;

        private static AIHeroClient Player;

        public static Spell Q;

        public static Spell W;

        public static Spell E;

        public static Spell R;

        private static bool _comboE;

        private static Vector3 PingLocation;

        private static int LastPingT = 0;

        private const float SpellQWidth = 160f;

        public static SpellSlot IgniteSlot;

        public static void Loads()
        {
            Game_OnGameLoad();
        }

        static void Game_OnGameLoad()
        {
            if (ObjectManager.Player.CharacterName != "Karthus") return;

            Player = ObjectManager.Player;
            IgniteSlot = ObjectManager.Player.GetSpellSlot("SummonerDot");
            Q = new Spell(SpellSlot.Q, 875f);
            W = new Spell(SpellSlot.W, 990f);
            E = new Spell(SpellSlot.E, 425f);
            R = new Spell(SpellSlot.R, float.MaxValue);

            Q.SetSkillshot(1f, 150f, float.MaxValue, false, SpellType.Circle);
            W.SetSkillshot(0.25f, 5f, float.MaxValue, false, SpellType.Line);
            E.SetSkillshot(1f, 520f, float.MaxValue, false, SpellType.Circle);
            R.SetSkillshot(3f, float.MaxValue, float.MaxValue, false, SpellType.Circle);


            (_menu = new Menu("kimbaengkarthus","Kimbaeng Karthus", true)).Attach();
            
            var HitchanceMenu = _menu.Add(new Menu("Hitchances", "Hitchance"));
            HitchanceMenu.Add(
                new MenuList("Hitchance", "Hitchance",new[] { "Low", "Medium", "High", "VeryHigh", "Impossible" }, 3));

            var comboMenu = _menu.Add(new Menu("combo", "Combo"));
            comboMenu.Add(new MenuBool("useQ", "Use Q").SetValue(true));
            comboMenu.Add(new MenuBool("useW", "Use W").SetValue(true));
            comboMenu.Add(new MenuBool("useE", "Use E").SetValue(true));
            comboMenu.Add(new MenuBool("comboAA", "Use AA").SetValue(false));
            comboMenu.Add(new MenuSeparator("string", "if No Mana(100↓), Allow Use AA"));
            comboMenu.Add(new MenuBool("UseI", "Use Ignite").SetValue(true));

            var harassMenu = _menu.Add(new Menu("Harass", "Harass"));
            harassMenu.Add(new MenuBool("useQHarass", "UseQ").SetValue(true));
            harassMenu.Add(new MenuBool("useEHarass", "UseE").SetValue(true));
            harassMenu.Add(new MenuBool("harassAA", "Use AA").SetValue(false));
            harassMenu.Add(new MenuBool("autoqh", "Auto Q Harass").SetValue(false));
            harassMenu.Add(new MenuSlider("harassmana", "Mana %",50));

            var LastHitMenu = _menu.Add(new Menu("LastHit", "LastHit"));
            LastHitMenu.Add(new MenuBool("useqlasthit", "Use Q Lasthit").SetValue(true));
            LastHitMenu.Add(new MenuBool("laneq", "Use Q Laneclear").SetValue(true));


            var MiscMenu = _menu.Add(new Menu("Misc", "Misc"));
            var ultMenu = MiscMenu.Add(new Menu("Ult", "Ult"));
            ultMenu.Add(new MenuBool("NotifyUlt", "Notify Ult Text").SetValue(true));
            ultMenu.Add(new MenuBool("NotifyPing", "Notify Ult Ping").SetValue(true));

            MiscMenu.Add(new MenuBool("estate", "Auto E if No Target").SetValue(true));

            var DrawMenu = _menu.Add(new Menu("drawing","Draw"));
            DrawMenu.Add(new MenuBool("noDraw", "Disable Drawing").SetValue(true));
            DrawMenu.Add(new MenuBool("drawQ", "DrawQ").SetValue(true));// System.Drawing.Color.Goldenrod)));
            DrawMenu.Add(new MenuBool("drawW", "DrawW").SetValue(false));// System.Drawing.Color.Goldenrod)));
            DrawMenu.Add(new MenuBool("drawE", "DrawE").SetValue(false));// System.Drawing.Color.Goldenrod)));
            
            Drawing.OnDraw += Drawing_Ondraw;
            Game.OnUpdate += Game_OnUpdate;
            Orbwalker.OnBeforeAttack += Orbwalking_BeforeAttack;

            Game.Print("Kimbaeng<font color=\"#030066\">Karthus</font> Loaded");
            Game.Print("If You like this Assembly plz <font color=\"#1DDB16\">Upvote</font> XD ");
        }

        private static void Game_OnUpdate(EventArgs args)
        {
            if (_menu["Misc"]["Ult"]["NotifyUlt"].GetValue<MenuBool>().Enabled)
            {
                AutoUlt();
            }

            if (_menu["Misc"]["Ult"]["NotifyPing"].GetValue<MenuBool>().Enabled)
            {
                NotifyPing();
            }

            if (_menu["autoqh"].GetValue<MenuBool>().Enabled)
            {
                Harass();
            }
            if (Orbwalker.ActiveMode == OrbwalkerMode.Combo)
            {
                Orbwalker.AttackEnabled = (_menu["comboAA"].GetValue<MenuBool>().Enabled || ObjectManager.Player.Mana < 100);
                try
                {
                    Combo();
                }
                catch (Exception)
                {
                }
            }


            if (Orbwalker.ActiveMode == OrbwalkerMode.Harass)
            {
                Orbwalker.AttackEnabled = (_menu["harassAA"].GetValue<MenuBool>().Enabled);
                Harass();
                LastHit();
            }

            if (Orbwalker.ActiveMode == OrbwalkerMode.LaneClear)
            {
                LaneClear();
            }
            if (Orbwalker.ActiveMode == OrbwalkerMode.LastHit)
            {
                LastHit();
            }
            RegulateEState();
            PassiveForm();
        }

        private static void Orbwalking_BeforeAttack(object sender, BeforeAttackEventArgs args)
        {
            if (Orbwalker.ActiveMode == OrbwalkerMode.Combo)
            {
                args.Process = !Q.IsReady();
            }
            else if (Orbwalker.ActiveMode == OrbwalkerMode.LastHit)
            {
                var farmQ = _menu["useqlasthit"].GetValue<MenuBool>().Enabled;
                args.Process = !(farmQ && Q.IsReady());
            }
        }

        private static void Drawing_Ondraw(EventArgs args)
        {
            if (_menu["noDraw"].GetValue<MenuBool>().Enabled)
            {
                return;
            }

            var qValue = _menu["drawQ"].GetValue<MenuBool>().Enabled;
            var wValue = _menu["drawW"].GetValue<MenuBool>().Enabled;
            var eValue = _menu["drawE"].GetValue<MenuBool>().Enabled;

            if (qValue)
            {
                if (Q.Instance.Level != 0)
                    LeagueSharpCommon.Render.Circle.DrawCircle(ObjectManager.Player.Position, Q.Range, System.Drawing.Color.Goldenrod);
            }

            if (wValue)
            {
                if (W.Instance.Level != 0)
                    LeagueSharpCommon.Render.Circle.DrawCircle(ObjectManager.Player.Position, W.Range, System.Drawing.Color.Goldenrod);
            }

            if (eValue)
            {
                if (E.Instance.Level != 0)
                    LeagueSharpCommon.Render.Circle.DrawCircle(ObjectManager.Player.Position, E.Range, System.Drawing.Color.Goldenrod);
            }
        }
        
        private static void AutoUlt()
        {
            if (!R.IsReady()) return;
            var pos = Drawing.WorldToScreen(ObjectManager.Player.Position)[1] + 20;
            foreach (var hero in
                     ObjectManager.Get<AIHeroClient>()
                         .Where(
                             x => ObjectManager.Player.GetSpellDamage(x, SpellSlot.R) >= x.Health && x.IsValidTarget()))
            {
                Drawing.DrawText(
                    Drawing.WorldToScreen(Player.Position)[0] - 30,
                    pos,
                    System.Drawing.Color.Gold,
                    "Can Kill : " + hero.CharacterName);
                pos += 20;
            }
        }

        private static void NotifyPing()
        {
            if (R.Instance.Level == 0)
            {
                return;
            }
            else
                foreach (var enemy in
                         GameObjects.EnemyHeroes.Where(
                             t =>
                                 ObjectManager.Player.Spellbook.CanUseSpell(SpellSlot.R) == SpellState.Ready && t.IsValidTarget()
                                 && R.GetDamage(t) > t.Health && t.Distance(ObjectManager.Player.Position) > Q.Range))
                {
                    Ping(enemy.Position);
                }
        }
        
        private static void LastHit()
        {
            if (!Orbwalker.CanMove()) return;

            var minions = MinionManager.GetMinions(Player.ServerPosition, Q.Range, MinionManager.MinionTypes.All, MinionManager.MinionTeam.NotAlly);
            minions.RemoveAll(x => x.MaxHealth <= 5);

            foreach (var minion in
                     minions.Where(
                         x => ObjectManager.Player.GetSpellDamage(x, SpellSlot.Q, 1) >=
                             SebbyLib.HealthPrediction.GetHealthPrediction(x, (int)(Q.Delay * 1000))))
                if (_menu["useqlasthit"].GetValue<MenuBool>().Enabled)
                {
                    Q.Cast(minion);
                }
        }

        private static void LaneClear()
        {
            var minions = MinionManager.GetMinions(Player.ServerPosition, Q.Range, MinionManager.MinionTypes.All, MinionManager.MinionTeam.Enemy, MinionManager.MinionOrderTypes.Health);

            if (!minions.Any())
                return;

            var minion = minions.First();

            if (Q.IsReady() && minion.IsValidTarget(Q.Range) && _menu["laneq"].GetValue<MenuBool>().Enabled)
            {
                Q.Cast(minion.ServerPosition);
            }
        }
        
        private static void RegulateEState(bool ignoreTargetChecks = false)
        {
            if (_menu["estate"].GetValue<MenuBool>().Enabled)
            {
                if (!E.IsReady() || IsInPassiveForm()
                                 || ObjectManager.Player.Spellbook.GetSpell(SpellSlot.E).ToggleState != (SpellToggleState) 2) return;
                var target = TargetSelector.GetTarget(E.Range, DamageType.Magical);
                var minions = MinionManager.GetMinions(
                    ObjectManager.Player.ServerPosition,
                    E.Range,
                    MinionManager.MinionTypes.All,
                    MinionManager.MinionTeam.NotAlly);

                if (!ignoreTargetChecks && (target != null || (!_comboE && minions.Count != 0))) return;
                E.CastOnUnit(ObjectManager.Player);
                _comboE = false;
            }
        }

        private static bool IsInPassiveForm()
        {
            return ObjectManager.Player.IsZombie();
        }

        private static void PassiveForm()
        {
            if (Player.IsZombie())
            {
                var Target = TargetSelector.GetTarget(Q.Range, DamageType.Magical);

                if (Target != null)
                {
                    Combo();
                }
                else
                {
                    LaneClear();
                }
            }

        }
        
        private static void Ping(Vector3 position)
        {
            if (Environment.TickCount - LastPingT < 30 * 1000)
            {
                return;
            }

            LastPingT = Environment.TickCount;
            PingLocation = position;
            SimplePing();

            DelayAction.Add(150, SimplePing);
            DelayAction.Add(300, SimplePing);
            DelayAction.Add(400, SimplePing);
            DelayAction.Add(800, SimplePing);
        }
        
        private static void SimplePing()
        {
            Game.ShowPing(PingCategory.Fallback, PingLocation, true);
        }
        
        private static float TotalDamage(AIHeroClient hero)
        {
            var damage = 0d;
            if (Q.IsReady())
            {
                damage += Q.GetDamage(hero);
            }
            if (W.IsReady())
            {
                damage += W.GetDamage(hero);
            }
            if (E.IsReady())
            {
                damage += W.GetDamage(hero);
            }
            if (R.IsReady())
            {
                damage += R.GetDamage(hero);
            }
            return (float)damage;
        }

        private static void Combo()
        {
            var wTarget = TargetSelector.GetTarget(W.Range, DamageType.Magical);
            var eTarget = TargetSelector.GetTarget(E.Range, DamageType.Magical);
            var UseQ = _menu["combo"]["useQ"].GetValue<MenuBool>().Enabled;
            var UseW = _menu["combo"]["useW"].GetValue<MenuBool>().Enabled;
            var UseE = _menu["combo"]["useE"].GetValue<MenuBool>().Enabled;

            if (wTarget != null && UseW && W.IsReady())
            {
                W.Cast(wTarget, false, true);
            }

            if (eTarget != null && UseE && E.IsReady() && ObjectManager.Player.Spellbook.GetSpell(SpellSlot.E).ToggleState == (SpellToggleState) 1)
            {
                if (ObjectManager.Player.Distance(eTarget.ServerPosition) <= E.Range)
                {
                    _comboE = true;
                    E.Cast();
                }
            }

            else if (eTarget == null && ObjectManager.Player.Spellbook.GetSpell(SpellSlot.E).ToggleState != (SpellToggleState) 1)
            {
                E.Cast();
            }
            if (UseQ && Q.IsReady()) 
            {
                var HC = SebbyLib.HitChance.Impossible;
                switch (_menu["Hitchances"]["Hitchance"].GetValue<MenuList>().Index)
                {
                    case 0: //Low
                        HC = SebbyLib.HitChance.Low;
                        break;
                    case 1: //Medium
                        HC = SebbyLib.HitChance.Medium;
                        break;
                    case 2: //High
                        HC = SebbyLib.HitChance.High;
                        break;
                    case 3: //Very High
                        HC = SebbyLib.HitChance.VeryHigh;
                        break;
                    case 4: //impossable
                        HC = SebbyLib.HitChance.Impossible;
                        break;
                }
                var qTarget = TargetSelector.GetTarget(Q.Range, DamageType.Magical);
                if (qTarget != null)
                {
                    Q.CastIfHitchanceEquals(qTarget, (HitChance) HC);
                }
            }

            if (IgniteSlot != SpellSlot.Unknown &&
    ObjectManager.Player.Spellbook.CanUseSpell(IgniteSlot) == SpellState.Ready &&
    ObjectManager.Player.Distance(wTarget.ServerPosition) < 600 &&
    Player.GetSummonerSpellDamage(wTarget, SummonerSpell.Ignite) > wTarget.Health)
            {
                ObjectManager.Player.Spellbook.CastSpell(IgniteSlot, wTarget);
            }

        }
        
        private static void Harass()
        {
            var eTarget = TargetSelector.GetTarget(E.Range, DamageType.Magical);
            var UseQ = _menu["Harass"]["useQHarass"].GetValue<MenuBool>().Enabled;
            var UseE = _menu["Harass"]["useEHarass"].GetValue<MenuBool>().Enabled;

            var HC = SebbyLib.HitChance.VeryHigh;
            if (UseQ && Q.IsReady())
            {
                switch (_menu["Hitchances"]["Hitchance"].GetValue<MenuList>().Index)
                {
                    case 0: //Low
                        HC = SebbyLib.HitChance.Low;
                        break;
                    case 1: //Medium
                        HC = SebbyLib.HitChance.Medium;
                        break;
                    case 2: //High
                        HC = SebbyLib.HitChance.High;
                        break;
                    case 3: //Very High
                        HC = SebbyLib.HitChance.VeryHigh;
                        break;
                    case 4: //impossable
                        HC = SebbyLib.HitChance.Impossible;
                        break;
                }
                var qTarget = TargetSelector.GetTarget(Q.Range, DamageType.Magical);
                if (qTarget != null)
                {
                    Q.CastIfHitchanceEquals(qTarget, (HitChance) HC);
                }

            }

            if (eTarget != null && UseE && E.IsReady() && ObjectManager.Player.Spellbook.GetSpell(SpellSlot.E).ToggleState == (SpellToggleState) 1)
            {
                E.Cast();
            }
            else if (eTarget == null && ObjectManager.Player.Spellbook.GetSpell(SpellSlot.E).ToggleState != (SpellToggleState) 1)
            {
                E.Cast();
            }
        }
    }
}