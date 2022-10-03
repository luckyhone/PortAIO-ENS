using EnsoulSharp;
using EnsoulSharp.SDK;
using static PortAIO.Library_Ports.Entropy.Lib.Extensions.MinionExtensions;

namespace Entropy.AIO.Irelia.Misc
{
    #region

    using static Bases.ChampionBase;

    #endregion

    class Damage
    {
        private static readonly float[] QBaseDamage  = {0f, 5f, 25f, 45f, 65f, 85f};
        private static readonly float[] QBonusDamage = {0f, 45f, 60f, 75f, 90f, 105f};
        private static readonly float[] WBaseDamage  = {0, 10, 25, 40, 55, 70};
        private static readonly float[] EBaseDamage  = {0, 70, 110, 150, 190, 230};
        private static readonly float[] RBaseDamage  = {0, 125, 225, 325};

        private static AIHeroClient LocalPlayer => ObjectManager.Player;

        private static double PassiveDamage(AIBaseClient target)
        {
            var ireliaPassiveDamage =
                (3.235f                                +
                 0.765f * LocalPlayer.Level +
                 0.04f  * LocalPlayer.GetBonusPhysicalDamage()) *
                LocalPlayer.GetBuffCount("ireliapassivestacks");

            return (float)LocalPlayer.CalculateDamage(target, DamageType.Magical, ireliaPassiveDamage);
        }

        private static double Sheen(AIBaseClient target)
        {
            float damage = 0;
            if (LocalPlayer.HasItem(ItemId.Sheen) && Irelia.sheenTimer < Game.Time)
            {
                var item = new Items.Item((int)ItemId.Sheen, 600);
                if (item.IsReady && LocalPlayer.HasBuff("sheen"))
                {
                    damage = (float)LocalPlayer.CalculateDamage(target, DamageType.Physical, LocalPlayer.BaseAttackDamage);
                }
            }

            if (LocalPlayer.HasItem(ItemId.Trinity_Force) && Irelia.sheenTimer < Game.Time)
            {
                var item = new Items.Item((int)ItemId.Trinity_Force, 600);
                if (item.IsReady && !LocalPlayer.HasBuff("TrinityForce"))
                {
                    damage = (float)(LocalPlayer.CalculateDamage(target,
                                         DamageType.Physical,
                                         LocalPlayer.BaseAttackDamage) *
                                     2f);
                }
            }

            return damage;
        }

        public static double QDamage(AIBaseClient target)
        {
            var qLevel = Q.Level;
            var baseDamage = (double)QBaseDamage[qLevel] + .6f * LocalPlayer.TotalAttackDamage;
            baseDamage = (float)LocalPlayer.CalculateDamage(target, DamageType.Physical, baseDamage);
            if (target is AIMinionClient minion && minion.IsMinion())
            {
                baseDamage += (43 + 12 * qLevel + LocalPlayer.GetBonusPhysicalDamage());
            }

            baseDamage += Sheen(target);
            return baseDamage;
        }


        public static double WDamage(AIBaseClient target)
        {
            var wLevel = W.Level;

            var wBaseDamage = WBaseDamage[wLevel]                             +
                              0.5f * LocalPlayer.TotalAttackDamage +
                              0.4f * LocalPlayer.TotalMagicalDamage;

            return LocalPlayer.CalculateDamage(target, DamageType.Physical, wBaseDamage);
        }

        public static double EDamage(AIBaseClient target)
        {
            var eLevel = E.Level;

            var eBaseDamage = EBaseDamage[eLevel] + 0.8f * LocalPlayer.TotalMagicalDamage;

            return LocalPlayer.CalculateDamage(target, DamageType.Magical, eBaseDamage);
        }

        public static double RDamage(AIBaseClient target)
        {
            var rLevel = R.Level;

            var rBaseDamage = RBaseDamage[rLevel] + 0.7f * LocalPlayer.TotalMagicalDamage;

            return LocalPlayer.CalculateDamage(target, DamageType.Magical, rBaseDamage);
        }
    }
}