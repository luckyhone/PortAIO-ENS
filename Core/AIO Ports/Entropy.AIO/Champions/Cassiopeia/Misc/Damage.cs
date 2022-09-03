using EnsoulSharp;
using EnsoulSharp.SDK;

namespace Entropy.AIO.Cassiopeia.Misc
{
    using Bases;
    class Damage
    {
        private static readonly float[] QBaseDamage = {0, 75, 120, 165, 210, 255};
        private static readonly float[] WBaseDamage = {0, 20, 30, 40, 50, 60};
        private static readonly float[] EBaseDamage = {0, 10, 30, 50, 70, 90};
        private static readonly float[] RBaseDamage = {0, 150, 250, 350};

        private static AIHeroClient LocalPlayer => ObjectManager.Player;

        public static double Q(AIBaseClient target)
        {
            var qLevel = ChampionBase.Q.Level;

            var qBaseDamage = QBaseDamage[qLevel] + 0.8f * LocalPlayer.TotalMagicalDamage;

            return LocalPlayer.CalculateDamage(target, DamageType.Magical, qBaseDamage);
        }

        public static double W(AIBaseClient target)
        {
            var wLevel = ChampionBase.W.Level;

            var wBaseDamage = WBaseDamage[wLevel] + .15f * LocalPlayer.TotalMagicalDamage;

            return LocalPlayer.CalculateDamage(target, DamageType.Magical, wBaseDamage * 2);
        }

        public static double E(AIBaseClient target)
        {
            var eLevel = ChampionBase.E.Level;

            var eBaseDamage = 48                                  +
                              4    * LocalPlayer.Level +
                              .10f * LocalPlayer.TotalMagicalDamage;

            if (Definitions.CachedPoisoned.ContainsKey((uint)target.NetworkId))
            {
                eBaseDamage += EBaseDamage[eLevel] + .60f * LocalPlayer.TotalMagicalDamage;
            }
            
            return LocalPlayer.CalculateDamage(target, DamageType.Magical, eBaseDamage);
        }

        public static double R(AIBaseClient target)
        {
            var rLevel = ChampionBase.R.Level;

            var rBaseDamage = RBaseDamage[rLevel] + 0.50f * LocalPlayer.TotalMagicalDamage;

            return LocalPlayer.CalculateDamage(target, DamageType.Magical, rBaseDamage);
        }
    }
}