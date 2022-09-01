using System.Collections.Generic;
using EnsoulSharp;
using EnsoulSharp.SDK.MenuUI;
using SharpDX;

namespace Entropy.AIO.Bases
{
    class Components
    {
        public static class General
        {
            public static readonly MenuBool OrbwalkerOnlyMenuBool = new MenuBool("orbwalker", "Only Orbwalker", false);

            public static readonly MenuBool CastOnSmallJungleMinionsMenuBool =
                new MenuBool("junglesmall", "Cast to small minions too in JungleClear", false);

            public static readonly MenuBool StormrazorComboMenubool = new MenuBool("Combo", "Combo", false);
            public static readonly MenuBool StormrazorLaneClearMenubool = new MenuBool("LaneClear", "LaneClear", false);
            public static readonly MenuBool StormrazorHarassMenubool = new MenuBool("Harass", "Harass", false);
            public static readonly MenuBool StormrazorLasthitMenubool = new MenuBool("Lasthit", "Lasthit", false);

            public static readonly MenuBool IgnoreManaManagerBlue =
                new MenuBool("nomanagerifblue", "Ignore ManaManagers if you have Blue Buff", false);
        }

        private static AIHeroClient Player => ObjectManager.Player;
        public static class DrawingMenu
        {
            public static readonly MenuBool SharpDXMode = new MenuBool("sharpDXMode", "SharpDX Mode", false);
            public static readonly MenuSlider CircleThickness = new MenuSlider("CircleThickness", "Circle Thickness", 1, 1, 10);

            public static readonly MenuColor ColorQ = new MenuColor("color1", "Color Q", new Color(255, 0, 0));
            public static readonly MenuColor ColorW = new MenuColor("color2", "Color W", new Color(0, 255, 0));
            public static readonly MenuColor ColorE = new MenuColor("color3", "Color E", new Color(0, 0, 255));
            public static readonly MenuColor ColorR = new MenuColor("color4", "Color R", new Color(255, 255, 255));
            public static readonly MenuColor ColorExtra = new MenuColor("color5", "Color Extra", new Color(0, 0, 0));

            public static readonly MenuBool QBool = new MenuBool($"{Player.CharacterName}.QR", "Draw Q", false);
            public static readonly MenuBool WBool = new MenuBool($"{Player.CharacterName}.WR", "Draw W", false);

            public static readonly MenuBool EBool = new MenuBool($"{Player.CharacterName}.ER",
                "Draw E",
                Player.CharacterName == "Twitch" ||
                Player.CharacterName == "Kalista");

            public static readonly MenuBool RBool = new MenuBool($"{Player.CharacterName}.RR", "Draw R", false);

            public static readonly MenuBool QDamageBool =
                new MenuBool($"{Player.CharacterName}.QD", "Draw Q Damage", false);

            public static readonly MenuBool WDamageBool =
                new MenuBool($"{Player.CharacterName}.WD", "Draw W Damage", false);

            public static readonly MenuBool EDamageBool = new MenuBool($"{Player.CharacterName}.ED",
                "Draw E Damage",
                Player.CharacterName == "Twitch" ||
                Player.CharacterName == "Kalista");

            public static readonly MenuBool RDamageBool =
                new MenuBool($"{Player.CharacterName}.RD", "Draw R Damage", false);

            public static readonly MenuSliderButton AutoDamageSliderBool =
                new MenuSliderButton($"{Player.CharacterName}.autos", "Include x Autos' damage", 1, 1, 10,false);
        }

        public static class Gapcloser
        {
            public static Dictionary<string, Menu> AntiGapcloserSpellSlot = new Dictionary<string, Menu>();
            public static Dictionary<string, Menu> EnemyChampionName = new Dictionary<string, Menu>();
            public static Dictionary<string, MenuBool> EnemySpell = new Dictionary<string, MenuBool>();
        }
    }
}