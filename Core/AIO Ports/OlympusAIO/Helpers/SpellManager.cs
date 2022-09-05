using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

using EnsoulSharp;
using EnsoulSharp.SDK;

using OlympusAIO.General;

namespace OlympusAIO.Helpers
{
    class SpellManager
    {
        public static Spell Q { get; set; }
        public static Spell Q2 { get; set; }
        public static Spell W { get; set; }
        public static Spell W2 { get; set; }
        public static Spell E { get; set; }
        public static Spell E2 { get; set; }
        public static Spell R { get; set; }
        public static Spell R2 { get; set; }

        public static List<float> LastCastTime = new List<float> { 0f, 0f, 0f, 0f };

        public static List<string> SpellList = new List<string> { "Q", "W", "E", "R" };

        public static float LastMove;
    }
}
