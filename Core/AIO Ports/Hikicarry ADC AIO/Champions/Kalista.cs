using System;
using EnsoulSharp;
using EnsoulSharp.SDK;
using EnsoulSharp.SDK.MenuUI;
using HikiCarry.Core.Utilities;
using SharpDX;
using Utilities = HikiCarry.Core.Utilities.Utilities;

 namespace HikiCarry.Champions
{
    class Kalista
    {
        internal static Spell Q;
        internal static Spell W;
        internal static Spell E;
        internal static Spell R;

        public Kalista()
        {
            Q = new Spell(SpellSlot.Q, 1150f);
            W = new Spell(SpellSlot.W, 5000f);
            E = new Spell(SpellSlot.E, 1000f);
            R = new Spell(SpellSlot.R, 1500f);

            Q.SetSkillshot(0.25f, 40f, 1200f, true, SpellType.Line);

            var comboMenu = new Menu("Combo Settings", "Combo Settings");
            {
                comboMenu.Add(new MenuBool("qCombo", "Use Q",true).SetValue(true));
                comboMenu.Add(new MenuBool("eCombo", "Use E",true).SetValue(true));
                comboMenu.Add(new MenuKeyBind("combo", "Combo!", Keys.Space, KeyBindType.Press));
                Initializer.Config.Add(comboMenu);
            }

            var harassMenu = new Menu("Harass Settings", "Harass Settings");
            {
                harassMenu.Add(new MenuBool("qHarass", "Use Q",true).SetValue(true));
                harassMenu.Add(new MenuBool("eHarass", "Use E",true).SetValue(true));
                harassMenu.Add(new MenuSlider("eSpearCount", "If Enemy Spear Count >= ", 3, 0, 10));
                harassMenu.Add(new MenuSlider("manaHarass", "Harass Mana Manager", 20, 1, 99));
                Initializer.Config.Add(harassMenu);
            }

            var laneMenu = new Menu("LaneClear Settings", "LaneClear Settings");
            {
                laneMenu.Add(new MenuBool("eClear", "Use E", true).SetValue(true));
                laneMenu.Add(new MenuSlider("eClearCount", "If Can Kill Minion >= ", 2, 1, 5));
                laneMenu.Add(new MenuSlider("manaClear", "Clear Mana Manager", 20, 1, 99));
                Initializer.Config.Add(laneMenu);
            }

            var jungMenu = new Menu("JungleClear Settings", "JungleClear Settings");
            {
                jungMenu.Add(new MenuBool("qJungle", "Use Q", true).SetValue(true));
                jungMenu.Add(new MenuBool("eJungle", "Use E", true).SetValue(true));
                jungMenu.Add(new MenuSlider("manaJungle", "Jungle Mana Manager", 20, 1, 99));
                Initializer.Config.Add(jungMenu);
            }

            var ksMenu = new Menu("KillSteal Settings", "KillSteal Settings");
            {
                ksMenu.Add(new MenuBool("qKS", "Use Q", true).SetValue(true));
                ksMenu.Add(new MenuBool("eKS", "Use E", true).SetValue(true));
                Initializer.Config.Add(ksMenu);
            }

            var miscMenu = new Menu("Miscellaneous", "Miscellaneous");
            {
                var lastJoke = new Menu("Last Joke Settings", "Last Joke Settings");
                {
                    lastJoke.Add(new MenuBool("last.joke", "Last Joke", true).SetValue(true));
                    lastJoke.Add(new MenuSlider("last.joke.hp", "Kalista HP Percent", 2, 1, 99));
                    miscMenu.Add(lastJoke);
                }
                miscMenu.Add(new MenuBool("qImmobile", "Auto Q to Immobile Target", true).SetValue(true));
                Initializer.Config.Add(miscMenu);
            }

            var wCombo = new Menu("Wombo Combo with R", "Wombo Combo with R"); // beta
            {
                var balista = new Menu("Balista", "Balista");
                {
                    balista.Add(new MenuBool("use.balista", "Balista Active", true).SetValue(true));
                    balista.Add(new MenuSlider("balista.maxrange", "Balista Max Range", 700, 100, 1500));
                    balista.Add(new MenuSlider("balista.minrange", "Balista Min Range", 700, 100, 1500));
                    wCombo.Add(balista);
                }
                var skalista = new Menu("Skalista", "Skalista");
                {
                    skalista.Add(new MenuBool("use.skalista", "SKalista Active", true).SetValue(true));
                    skalista.Add(new MenuSlider("skalista.maxrange", "SKalista Max Range", 700, 100, 1500));
                    skalista.Add(new MenuSlider("skalista.minrange", "SKalista Min Range", 700, 100, 1500));
                    wCombo.Add(skalista);
                }
                Initializer.Config.Add(wCombo);
            }

            var drawMenu = new Menu("Draw Settings", "Draw Settings");
            {
                var drawDamageMenu = new MenuBool("RushDrawEDamage", "Combo Damage").SetValue(true);
                var drawFill = new MenuColor("RushDrawEDamageFill", "Combo Damage Fill",Color.Gold);

                var damageDraws = drawMenu.Add(new Menu("Damage Draws", "Damage Draws"));
                damageDraws.Add(drawDamageMenu);
                damageDraws.Add(drawFill);
                
                Initializer.Config.Add(drawMenu);
            }

            Initializer.Config.Add(new MenuBool("saveSupport", "Save Support [R]",true).SetValue(true));
            Initializer.Config.Add(new MenuSlider("savePercent", "Save Support Health Percent",10, 1, 99));
            Initializer.Config.Add(new MenuList("calculator", "E Damage Calculator",new[] { "Custom Calculator", "Common Calculator" }));

            Game.OnUpdate += KalistaOnUpdate;
        }

        private void KalistaOnUpdate(EventArgs args)
        {
            switch (Orbwalker.ActiveMode)
            {
                case OrbwalkerMode.Combo:
                    Combo();
                    break;
                case OrbwalkerMode.Harass:
                    Harass();
                    break;
                case OrbwalkerMode.LaneClear:
                    Clear();
                    Jungle();
                    break;
            }
            if (Initializer.Config["Wombo Combo with R"]["Balista"]["use.balista"].GetValue<MenuBool>().Enabled)
            {
                KalistaLogics.Balista(Initializer.Config["Wombo Combo with R"]["Balista"]["balista.minrange"].GetValue<MenuSlider>().Value, Initializer.Config["Wombo Combo with R"]["Balista"]["balista.maxrange"].GetValue<MenuSlider>().Value, R);
            }
            if (Initializer.Config["Wombo Combo with R"]["Skalista"]["use.skalista"].GetValue<MenuBool>().Enabled)
            {
                KalistaLogics.SKalista(Initializer.Config["Wombo Combo with R"]["Skalista"]["skalista.minrange"].GetValue<MenuSlider>().Value, Initializer.Config["Wombo Combo with R"]["Skalista"]["skalista.maxrange"].GetValue<MenuSlider>().Value, R);
            }
            if (Utilities.Enabled("qKS"))
            {
                KalistaLogics.KillStealWithPierce();
            }
            if (Utilities.Enabled("eKS"))
            {
                KalistaLogics.KillStealWithRend();
            }
            if (Utilities.Enabled("qImmobile"))
            {
                KalistaLogics.ImmobilePierce();
            }
            if (Utilities.Enabled("saveSupport"))
            {
                KalistaLogics.SupportProtector(R);
            }
        }
        private static void Combo()
        {
            if (Q.IsReady() && Utilities.Enabled("qCombo"))
            {
                KalistaLogics.PierceCombo();
            }
            if (E.IsReady() && Utilities.Enabled("eCombo"))
            {
                KalistaLogics.RendCombo();
            }
        }

        private static void Harass()
        {
            if (ObjectManager.Player.ManaPercent < Utilities.Slider("manaHarass"))
            {
                return;
            }
            if (Q.IsReady() && Utilities.Enabled("qHarass"))
            {
                KalistaLogics.PierceCombo();
            }
            if (E.IsReady() && Utilities.Enabled("eHarass"))
            {
                KalistaLogics.RendHarass(Utilities.Slider("eSpearCount"));
            }
        }

        private static void Clear()
        {
            if (ObjectManager.Player.ManaPercent < Utilities.Slider("manaClear"))
            {
                return;
            }


            if (E.IsReady() && Utilities.Enabled("eClear"))
            {
                KalistaLogics.RendClear(Utilities.Slider("eClearCount"));
            }
        }

        private static void Jungle()
        {
            if (ObjectManager.Player.ManaPercent < Utilities.Slider("manaJungle"))
            {
                return;
            }

            if (Q.IsReady() && Utilities.Enabled("qJungle"))
            {
                KalistaLogics.PierceJungleClear(Q);
            }
            if (E.IsReady() && Utilities.Enabled("eJungle"))
            {
                KalistaLogics.RendJungleClear();
            }
        }
    }
}
