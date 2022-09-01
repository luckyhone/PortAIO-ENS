using System;
using System.Reflection;
using EnsoulSharp.SDK;

namespace Entropy.Awareness
{
    static class Program
    {
        private const string Commit = "REWORK!!";

        public static void Loads()
        {
            GameEvent.OnGameLoad += Loading_OnLoadingComplete;
        }

        /// <summary>
        ///     Initializes the Awareness
        /// </summary>
        private static void Loading_OnLoadingComplete()
        {
            try
            {
                Bootstrap.Initialize();
            }
            catch (Exception e)
            {
                Console.WriteLine(@"Failed to start the Awareness");
            }

            Console.WriteLine($@"Awareness Version {Assembly.GetExecutingAssembly().GetName().Version} - {Commit} Loaded!");
        }
    }
}