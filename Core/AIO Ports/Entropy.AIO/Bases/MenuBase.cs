using System.Drawing;
using EnsoulSharp;
using EnsoulSharp.SDK;
using EnsoulSharp.SDK.MenuUI;

namespace Entropy.AIO.Bases
{
    static class MenuBase
    {
        public static Menu Root { get; private set; }
        public static Menu General { get; private set; }
        public static Menu Drawings { get; private set; }
        public static Menu Champion { get; private set; }
        public static Menu Gapcloser { get; set; }
        public static Menu Interrupter { get; set; }

        public static void Initialize()
        {
            General = new Menu("general", "General Menu")
            {
                Components.General.CastOnSmallJungleMinionsMenuBool,

                new Menu("stormrazor", "Stormrazor Menu")
                {
                    new MenuSeparator("stormsep", "Stop AA'ing until it procs in:"),
                    Components.General.StormrazorComboMenubool,
                    Components.General.StormrazorLaneClearMenubool,
                    Components.General.StormrazorHarassMenubool,
                    Components.General.StormrazorLasthitMenubool
                }
            };

            Drawings = new Menu("Drawings", "Drawing Menu")
            {
                Components.DrawingMenu.SharpDXMode,
                Components.DrawingMenu.CircleThickness,
                Components.DrawingMenu.ColorQ,
                Components.DrawingMenu.ColorW,
                Components.DrawingMenu.ColorE,
                Components.DrawingMenu.ColorR,
                Components.DrawingMenu.ColorExtra
            };

            //This is for each champion have its configuration file
            Root = new Menu("aio", "Entropy.AIO", true)
            {
                Components.General.OrbwalkerOnlyMenuBool.SetTooltip("F5 to enable"),
                General,
                Drawings
            };

            Champion = new Menu(ObjectManager.Player.CharacterName.ToLower(), ObjectManager.Player.CharacterName);

            if (ObjectManager.Player.MaxMana >= 200)
            {
                General.Add(Components.General.IgnoreManaManagerBlue);
            }

            Root.Add(new MenuSeparator("sep1"," - "));

            Root.Add(Champion);

            var a = new MenuSeparator("12312312", $"AIO - {ObjectManager.Player.CharacterName}");
            a.AddPermashow($"AIO - {ObjectManager.Player.CharacterName}",Color.Red.ToSharpDxColor());

            Root.Attach();
        }
    }
}