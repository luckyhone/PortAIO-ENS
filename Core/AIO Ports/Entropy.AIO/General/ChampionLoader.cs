using System;
using System.Reflection;
using EnsoulSharp;
using EnsoulSharp.SDK.Utility;
using Entropy.AIO.Bases;

namespace Entropy.AIO.General
{
    static class ChampionLoader
    {
        //Version: MajorPatch.ChampionAdded.FeatureAdded.BugFix
        public const string VERSION = "1.17.2.0";
        private static AIHeroClient Player => ObjectManager.Player;

        //Commit: ChampName:Description(day/month/year)
        //If more than 1 champ use NEW: Champion1, Champion2 Added!(day/month/year)
        private const string Commit = "Added Neeko! (01/09/2022)";

        public static void Initialize()
        {
            if (Components.General.OrbwalkerOnlyMenuBool.Enabled)
            {
                Console.WriteLine($@"AIO Version {VERSION} - {Commit} Loaded! (Orbwalker Only)");
                return;
            }

            try
            {
                var name = Player.CharacterName;
                // Special Cases
                // ReSharper disable once SwitchStatementMissingSomeCases
                switch (name)
                {
                    case "Leblanc":
                        name = "LeBlanc";
                        break;
                }

                var path = $"Entropy.AIO.{name}.{name}";
                var type = Type.GetType(path, true);

                Activator.CreateInstance(type);
                Console.WriteLine($@"AIO Version {VERSION} - {Commit} Loaded!");
            }
            catch (Exception e)
            {
                switch (e)
                {
                    case TargetInvocationException _:
                        Console.WriteLine($@"AIO: Error occurred while trying to load {Player.CharacterName}.{e}");
                        break;
                    case TypeLoadException _:
                        Console.WriteLine($@"AIO: {Player.CharacterName} is not supported.");
                        break;
                }
            }
        }
    }
}