using System;
using EnsoulSharp;
using PortAIO.Properties;

namespace UniversalMinimapHack
{
    public class Program
    {
        public static void Loads()
        {
            Game_OnGameLoad();
        }

        private static void Game_OnGameLoad()
        {
            Resources.ResourceManager.IgnoreCase = true;
            MinimapHack.Instance().Load();
            Print("Loaded!");
        }

        public static void Print(string msg)
        {
            Game.Print(
                "<font color='#ff3232'>Universal</font><font color='#d4d4d4'>MinimapHack:</font> <font color='#FFFFFF'>" +
                msg + "</font>");
        }

        public static string Format(float f)
        {
            TimeSpan t = TimeSpan.FromSeconds(f);
            if (t.Minutes < 1)
            {
                return t.Seconds + "";
            }
            if (t.Seconds >= 10)
            {
                return t.Minutes + ":" + t.Seconds;
            }
            return t.Minutes + ":0" + t.Seconds;
        }
    }
}