using EnsoulSharp.SDK;

namespace Entropy.AIO.Bases
{
    abstract class ChampionBase
    {
        public static Spell Q { get; set; }
        public static Spell W { get; set; }
        public static Spell E { get; set; }
        public static Spell R { get; set; }
    }
}