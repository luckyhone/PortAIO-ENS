using System;
using System.Collections.Generic;
using System.Linq;
using EnsoulSharp;
using EnsoulSharp.SDK;
using EnsoulSharp.SDK.MenuUI;
using SebbyLib;
using SharpDX;
using SharpDX.Direct3D9;

namespace OneKeyToWin_AIO_Sebby
{
    class Base : Program
    { 
        private static Font TextBold;
        private static float spellFarmTimer = 0;
        public static Menu Local, HarassMenu, FarmMenu;

        public static List<MenuBool> HarassList = new List<MenuBool>();
        
        public static MenuBool supportMode = new MenuBool("supportMode", "Support Mode", true);
        public static MenuBool comboDisableMode = new MenuBool("comboDisableMode", "Disable auto-attack in combo mode", true);
        public static MenuBool manaDisable = new MenuBool("manaDisable", "Disable mana manager in combo", false);
        public static MenuBool harassMixed = new MenuBool("harassMixed", "Spell-harass only in mixed mode", false);

        public static MenuBool spellFarm = new MenuBool("spellFarm", "OKTW spells farm");

        public static MenuList spellFarmMode = new MenuList("spellFarmMode", "SPELLS FARM TOGGLE MODE",
            new[] {"Scroll down", "Scroll press", "Key toggle", "Disable"}, 1);

        public static MenuKeyBind spellFarmKeyToggle =
            new MenuKeyBind("spellFarmKeyToggle", "key toggle", Keys.N, KeyBindType.Toggle);

        public static MenuBool showNot = new MenuBool("showNot", "Show notification", true);
        public static MenuSlider LCminions = new MenuSlider("LCminions", "Lane clear minimum minions", 2, 1, 10);
        public static MenuSlider LCmana = new MenuSlider("LCmana", "LaneClear mana", 50, 0, 100);

        public new static float QMANA = 0, WMANA = 0, EMANA = 0, RMANA = 0;
        
        public static int FarmMinions {get { return LCminions.Value;}}
        public static bool FarmSpells
        {
            get
            {
                return spellFarm.Enabled
                    && Orbwalker.ActiveMode == OrbwalkerMode.LaneClear
                    && Player.ManaPercent > LCmana.Value;
            }
        }

        static Base()
        {
            TextBold = new Font(Drawing.Direct3DDevice9, new FontDescription
                { FaceName = "Impact", Height = 30, Weight = FontWeight.Normal, OutputPrecision = FontPrecision.Default, Quality = FontQuality.ClearType });
            Local = new Menu(Player.CharacterName, Player.CharacterName);
            Local.SetFontColor(Color.Orange);

            HarassMenu = new Menu("harass", "Harass");
            foreach (var enemy in GameObjects.EnemyHeroes)
            {
                var harass = new MenuBool("harass" + enemy.CharacterName, enemy.CharacterName);
                HarassList.Add(harass);
                HarassMenu.Add(harass);
            }

            FarmMenu = new Menu("farm", "Farm");
            FarmMenu.Add(spellFarmMode);
            FarmMenu.Add(spellFarmKeyToggle);
            FarmMenu.Add(showNot);
            FarmMenu.Add(spellFarm);
            FarmMenu.Add(LCminions);
            FarmMenu.Add(LCmana);

            Local.Add(HarassMenu);
            Local.Add(FarmMenu);

            Config.Add(new Menu("extra", "Extra settings OKTW©")
            {
                supportMode,
                comboDisableMode,
                manaDisable,
                harassMixed
            });
            Config.Add(Local);

            spellFarm.AddPermashow();
            harassMixed.AddPermashow();
            
            Orbwalker.OnBeforeAttack += Orbwalking_BeforeAttack;
            Game.OnWndProc += Game_OnWndProc;
            Drawing.OnDraw += OnDraw;
        }

        private static void OnDraw(EventArgs args)
        {
            if (Program.AIOmode != 2 && spellFarmTimer + 1 > Game.Time && showNot.Enabled && spellFarm != null)
            {
                if (spellFarm.Enabled)
                    DrawFontTextScreen(TextBold, "SPELLS FARM ON", Drawing.Width * 0.5f, Drawing.Height * 0.4f, Color.GreenYellow);
                else
                    DrawFontTextScreen(TextBold, "SPELLS FARM OFF", Drawing.Width * 0.5f, Drawing.Height * 0.4f, Color.OrangeRed);
            }
        }

        private static void Game_OnWndProc(GameWndEventArgs args)
        {
            if (Program.AIOmode == 2)
                return;

            if (args.Msg == 0x20a && spellFarmMode.Index == 0 )
            {
                //Config.Item("spellFarm").SetValue(!Config.Item("spellFarm").GetValue<bool>());
                //spellFarmTimer = Game.Time;
                spellFarm.SetValue(!spellFarm.Enabled);
                spellFarmTimer = Game.Time;
            }
            if (args.Msg == 520 && spellFarmMode.Index == 1)
            {
                spellFarm.SetValue(!spellFarm.Enabled);
                spellFarmTimer = Game.Time;
            }
        }

        private static void Orbwalking_BeforeAttack(object sender, BeforeAttackEventArgs args)
        {
            if (Combo && comboDisableMode.Enabled)
            {
                var t = (AIHeroClient)args.Target;
                if (4 * Player.GetAutoAttackDamage(t) < t.Health - OktwCommon.GetIncomingDamage(t) && !t.HasBuff("luxilluminatingfraulein") && !Player.HasBuff("sheen") && !Player.HasBuff("Mastery6261"))
                    args.Process = false;
            }

            if (Orbwalker.ActiveMode == OrbwalkerMode.Harass && supportMode.Enabled)
            {
                if (args.Target.Type == GameObjectType.AIMinionClient) args.Process = false;
            }
        }

        public static bool InHarassList(AIHeroClient t)
        {
            return HarassList.Any(e => e.Enabled && e.Name == "harass" + t.CharacterName);
        }
    }
}