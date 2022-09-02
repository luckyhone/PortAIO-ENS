using System;
using System.Drawing;
using EnsoulSharp;
using EnsoulSharp.SDK;
using EnsoulSharp.SDK.Utility;
using StormAIO.Champions;
using StormAIO.utilities;

namespace StormAIO
{
    internal static class Program
    {
        public static void Loads()
        {
            GameEvent.OnGameLoad += GameEventOnOnGameLoad;
        }

        private static void GameEventOnOnGameLoad()
        {
            var delay = Game.Time > 7 ? 0f : 1750f;
            DelayAction.Add((int) delay, BootStrap);
        }

        private static void BootStrap()
        {
            try
            {
                /*if (!Checker.ServerStatus() || !Checker.IsUpdatetoDate())
                    return;*/
                new MainMenu();
                new Drawbackground();
                switch (ObjectManager.Player.CharacterName)
                {
                    case "Yone":
                        new Yone();
                        break;
                    case "Warwick":
                        new Warwick();
                        break;
                    case "Akali":
                        new Akali();
                        break;
                    case "Yorick":
                        new Yorick();
                        break;
                    case "KogMaw":
                        new Kowmaw();
                        break;
                    case "DrMundo":
                        //new Drmundo();
                        break;
                    case "Rengar":
                        new Rengar();
                        break;
                    case "Garen":
                        new Garen();
                        break;
                    case "Ashe":
                        new Ashe();
                        break;
                    case "Urgot":
                        new Urgot();
                        break;
                    case "Lucian":
                        new Lucian();
                        break;
                    case "Chogath":
                        new Chogath();
                        break;
                    case "Zed":
                        new Zed();
                        break;
                    case "Maokai":
                        new Maokai();
                        break;
                    case "Vladimir":
                        new Vladimir();
                        break;
                    case "Twitch":
                        new Twitch();
                        break;
                    case "Katarina":
                        new Katarina();
                        break;
                }
            }
            catch (Exception error)
            {
                Game.Print("Failed to load reload or Check ur Console");
                Console.WriteLine(@"Failed To load: " + error);
            }
            new Emote();
            new SkinChanger();
            new AutoLeveler();
            new StarterItem();
            new ArrowDrawer();
                new DrawText("SpellFarm", MainMenu.Key, MainMenu.SpellFarm, Color.GreenYellow,
                    Color.Red);
            new DrawText2("Skin index", SkinChanger.SkinMeun, 100);
            new Rundown();
        }
    }
}