using EnsoulSharp;
using EnsoulSharp.SDK;
using static Entropy.AIO.Bases.ChampionBase;
namespace Entropy.AIO.Gangplank.Misc
{
    class Damage
    {
        private static readonly float[] QBaseDamage = {0f, 57f, 120f, 165f, 210f, 255f};
        private static readonly float[] WBaseDamage = {0f, 57f, 120f, 165f, 210f, 255f};
        private static readonly float[] EBaseDamage = {0f, 57f, 120f, 165f, 210f, 255f};
        private static readonly float[] RBaseDamage = {0f, 75f, 100f, 125f};

        private static AIHeroClient LocalPlayer => ObjectManager.Player;

        public static double QDamage(AIBaseClient target)
        {
            var qLevel = Q.Level;

            var qBaseDamage = QBaseDamage[qLevel]                               +
                              0.5f * LocalPlayer.PercentBonusPhysicalDamageMod +
                              0.5f * LocalPlayer.TotalMagicalDamage;

            return LocalPlayer.CalculateDamage(target, DamageType.Magical, qBaseDamage);
        }

        public static double WDamage(AIBaseClient target)
        {
            var wLevel = W.Level;

            var wBaseDamage = WBaseDamage[wLevel]                               +
                              0.5f * LocalPlayer.PercentBonusPhysicalDamageMod +
                              0.5f * LocalPlayer.TotalMagicalDamage;

            return LocalPlayer.CalculateDamage(target, DamageType.Magical, wBaseDamage);
        }

        public static double EDamage(AIBaseClient target)
        {
            var eLevel = E.Level;

            var eBaseDamage = EBaseDamage[eLevel]                               +
                              0.5f * LocalPlayer.PercentBonusPhysicalDamageMod +
                              0.5f * LocalPlayer.TotalMagicalDamage;

            return LocalPlayer.CalculateDamage(target, DamageType.Magical, eBaseDamage);
        }

        public static double RDamage(AIBaseClient target)
        {
            var rLevel = R.Level;

            var rBaseDamage = RBaseDamage[rLevel]                      +
                              LocalPlayer.TotalAttackDamage +
                              0.2f * LocalPlayer.TotalMagicalDamage;

            return LocalPlayer.CalculateDamage(target, DamageType.Magical, rBaseDamage);
        }
    }
}