using EnsoulSharp;
using EnsoulSharp.SDK;

namespace Entropy.AIO.Aatrox.Misc
{
    using static Bases.ChampionBase;

    class Damage
    {
        public static readonly float[] Q1 =
        {
            0f, 10f, 25f, 40f, 55f, 70f
        };

        public static readonly float[] Q1Bonus =
        {
            0f, 0.55f, 0.6f, 0.65f, 0.7f, 0.75f
        };

        public static readonly float[] Q2 =
        {
            0f, 12.5f, 31.25f, 50f, 68.75f, 87.5f
        };

        public static readonly float[] Q2Bonus =
        {
            0f, 0.6875f, 75f, 0.8125f, 0.875f, 0.9375f
        };

        public static readonly float[] Q3 =
        {
            0f, 15f, 37.5f, 60f, 82.5f, 105f
        };

        public static readonly float[] Q3Bonus =
        {
            0f, 0.825f, 0.9f, 0.975f, 1.05f, 1.125f
        };

        public static readonly float[] WBase =
        {
            0f, 30f, 40f, 50f, 60f, 70f
        };


        public static double GetQDamage(AIBaseClient target)
        {
            double damage = 0f;

            var QBasePhysicalDamage = 0f;
            var QBonus              = 0f;

            if (Q.Name == "AatroxQ")
            {
                QBasePhysicalDamage = Q1[Q.Level];
                QBonus              = Q1Bonus[Q.Level];
            }

            if (Q.Name == "AatroxQ2")
            {
                QBasePhysicalDamage = Q2[Q.Level];
                QBonus              = Q2Bonus[Q.Level];
            }

            if (Q.Name == "AatroxQ3")
            {
                QBasePhysicalDamage = Q3[Q.Level];
                QBonus              = Q3Bonus[Q.Level];
            }

            damage = LocalPlayer.CalculateDamage(target,
                                                          DamageType.Physical,
                                                          QBasePhysicalDamage +
                                                          LocalPlayer.TotalAttackDamage * QBonus);

            return damage;
        }

        public static double GetWDamage(AIBaseClient target)
        {
            double damage = 0f;

            damage = LocalPlayer.CalculateDamage(target,
                                                          DamageType.Physical,
                                                          WBase[W.Level] +
                                                          LocalPlayer.FlatPhysicalDamageMod * 0.4f);

            return damage;
        }

        private static AIHeroClient LocalPlayer => ObjectManager.Player;
    }
}