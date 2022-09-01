using EnsoulSharp;
using EnsoulSharp.SDK;
using Entropy.AIO.Bases;

namespace Entropy.AIO.Ahri.Misc
{
    class Damage
    {
        private static readonly float[] QBaseDamage = {0f, 40f, 65f, 90f, 115f, 140f};
        private static readonly float[] WBaseDamage = {0f, 40f, 65f, 90f, 115f, 140f};
        private static readonly float[] EBaseDamage = {0f, 60f, 90f, 120f, 150f, 180f};
        private static readonly float[] RBaseDamage = {0f, 60f, 90f, 120f};

        private static AIHeroClient LocalPlayer => ObjectManager.Player;

        public static double Q(AIBaseClient target)
        {
            var qLevel = ChampionBase.Q.Level;

            var qBaseDamage = QBaseDamage[qLevel] +
                              0.35f * LocalPlayer.TotalMagicalDamage;

            return (float)(LocalPlayer.CalculateDamage(target, DamageType.Magical, qBaseDamage) + qBaseDamage);
        }

        public static double W(AIBaseClient target)
        {
            var wLevel = ChampionBase.W.Level;

            var wBaseDamage = WBaseDamage[wLevel] +
                              0.3f * LocalPlayer.TotalMagicalDamage;

            return (float)LocalPlayer.CalculateDamage(target, DamageType.Magical, wBaseDamage);
        }

        public static double E(AIBaseClient target)
        {
            var eLevel = ChampionBase.E.Level;

            var eBaseDamage = EBaseDamage[eLevel] +
                              0.4f * LocalPlayer.TotalMagicalDamage;

            return (float)LocalPlayer.CalculateDamage(target, DamageType.Magical, eBaseDamage);
        }

        public static double R(AIBaseClient target)
        {
            var rLevel = ChampionBase.R.Level;

            var rBaseDamage = RBaseDamage[rLevel] +
                              0.35f * LocalPlayer.TotalMagicalDamage;

            return (float)LocalPlayer.CalculateDamage(target, DamageType.Magical, rBaseDamage * 3);
        }
        
    }
}