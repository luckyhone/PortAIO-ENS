using EnsoulSharp;
using EnsoulSharp.SDK;

namespace Entropy.AIO.Neeko.Misc
{
    using Bases;
    class Damage
    {
        private static readonly float[] QBaseDamage = {0f, 70f, 115f, 160f, 205f, 250f};
        private static readonly float[] EBaseDamage = {0f, 80f, 115f, 150f, 185f, 220f};
        private static readonly float[] RBaseDamage = {0f, 250f, 425f, 650f};

        public static double Q(AIBaseClient target)
        {
            var qLevel = ChampionBase.Q.Level;

            var qDamage = QBaseDamage[qLevel] + 0.5f * ObjectManager.Player.TotalMagicalDamage;

            return ObjectManager.Player.CalculateDamage(target, DamageType.Magical, qDamage);
        }

        public static double E(AIBaseClient target)
        {
            var eLevel = ChampionBase.E.Level;

            var eDamage = EBaseDamage[eLevel] + 0.4f * ObjectManager.Player.TotalMagicalDamage;

            return ObjectManager.Player.CalculateDamage(target, DamageType.Magical, eDamage);
        }

        public static double R(AIBaseClient target)
        {
            var rLevel = ChampionBase.R.Level;

            var rDamage = RBaseDamage[rLevel] + 1.3f * ObjectManager.Player.TotalMagicalDamage;

            return ObjectManager.Player.CalculateDamage(target, DamageType.Magical, rDamage);
        }
    }
}