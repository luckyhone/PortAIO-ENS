using System;
using System.Linq;
using EnsoulSharp;
using EnsoulSharp.SDK;
using EnsoulSharp.SDK.MenuUI;
using EnsoulSharp.SDK.Utility;
using OlympusAIO.Champions;
using SharpDX;
using MenuManager = OlympusAIO.Helpers.MenuManager;
using Render = LeagueSharpCommon.Render;

namespace OlympusAIO
{
    class OlympusAIO
    {
        public static AIHeroClient objPlayer;

        public static Menu MainMenu;

        public static string[] SupportedChampions =
        {
            "AurelionSol", "Evelynn", "Heimerdinger", "Lissandra", "Poppy", "Teemo",
        };
        public static void Loads()
        {
            GameEvent.OnGameLoad += delegate ()
            {
                objPlayer = ObjectManager.Player;

                SupportedChampionsNotify();

                if (SupportedChampions.All(x => !string.Equals(x, objPlayer.CharacterName, StringComparison.CurrentCultureIgnoreCase)))
                {
                    MainMenu = new Menu("OlympusAIO." + objPlayer.CharacterName + ".NotSupported", "OlympusAIO: " + "Not Supported", true);
                }
                else
                {
                    MainMenu = new Menu("OlympusAIO." + objPlayer.CharacterName, "OlympusAIO: " + objPlayer.CharacterName, true);
                }

                MenuManager.Execute.General();

                switch (objPlayer.CharacterName)
                {
                    case "AurelionSol":
                        AurelionSol.OnLoad();
                        break;
                    case "Evelynn":
                        Evelynn.OnLoad();
                        break;
                    case "Heimerdinger":
                        Heimerdinger.OnLoad();
                        break;
                    case "Lissandra":
                        Lissandra.OnLoad();
                        break;
                    case "Poppy":
                        Poppy.OnLoad();
                        break;
                    case "Teemo":
                        Teemo.OnLoad();
                        break;
                }

                General.Methods.OnLoad();

                MainMenu.Attach();
            };
        }
        private static void SupportedChampionsNotify()
        {
            var drawPos = new Vector2(120, 120);

            Render.Text MainText = new Render.Text("OlympusAIO - Supported Champions", drawPos, 20, new ColorBGRA(170, 255, 47, 255));
            Render.Add(MainText);
            MainText.OnDraw();

            foreach (var champ in SupportedChampions)
            {
                drawPos += new Vector2(0, 30);

                Render.Text SupportingChampions = new Render.Text(champ, drawPos, 20, new ColorBGRA(255, 222, 173, 255));
                Render.Add(SupportingChampions);
                SupportingChampions.OnDraw();

                DelayAction.Add(13000, () => Render.Remove(SupportingChampions));
            }

            DelayAction.Add(13000, () => Render.Remove(MainText));
        }
    }
}