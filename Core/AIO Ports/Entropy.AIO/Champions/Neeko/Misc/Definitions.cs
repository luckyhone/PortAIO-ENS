using EnsoulSharp;

namespace Entropy.AIO.Neeko.Misc
{
    static class Definitions
    {
        /// <summary>
        ///     Returns true if the player is disguised as another champion.
        /// </summary>
        public static bool InShape()
        {
            return ObjectManager.Player.CharacterName != "Neeko";
        }
    }
}