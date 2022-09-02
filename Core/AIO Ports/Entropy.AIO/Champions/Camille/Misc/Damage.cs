using EnsoulSharp;
using EnsoulSharp.SDK;

namespace Entropy.AIO.Camille.Misc
{
    #region

    using Bases;

    #endregion
    
    class Damage
    {
        private static readonly float[] QChargedBaseDamage = {0, 0.40f, 0.50f, 0.60f, 0.70f, 0.80f};
        private static readonly float[] QBaseDamage        = {0, 0.20f, 0.25f, 0.30f, 0.35f, 0.40f};
        private static readonly float[] WBaseDamage        = {0, 70f, 100f, 130f, 160f, 190f};
        private static readonly float[] WConeBonus         = {0, 0.06f, 0.065f, 0.07f, 0.075f, 0.08f};
        private static AIHeroClient LocalPlayer => ObjectManager.Player;

        public static double Q(AIBaseClient target)
        {
            var qLevel = ChampionBase.Q.Level;
            double damage = LocalPlayer.TotalAttackDamage;
            var qBonus = Definitions.QState == QState.Charged
                ? QChargedBaseDamage[qLevel]
                : QBaseDamage[qLevel];
            damage += damage * qBonus;

            if (Definitions.QState == QState.Charged)
            {
                damage *= 2;
                damage =  LocalPlayer.CalculateDamage(target, DamageType.Physical, damage);
                var trueDamagePct = 0.36f + 0.04f * LocalPlayer.Level;
                if (trueDamagePct > 1f)
                {
                    trueDamagePct = 1f;
                }

                var damageAsTrue     = damage * trueDamagePct;
                var damageAsPhysical = damage - damageAsTrue;

                damage = damageAsPhysical + damageAsTrue;
            }
            else
            {
                damage = LocalPlayer.CalculateDamage(target, DamageType.Physical, damage);
            }

            return damage;
        }

        public static double W(AIBaseClient target)
        {
            var wLevel         = ChampionBase.W.Level;
            var baseDamage     = WBaseDamage[wLevel];
            var wBonus         = LocalPlayer.PercentBonusPhysicalDamageMod * 0.60f;
            var damage         = baseDamage + wBonus;
            var multiplier     = LocalPlayer.PercentBonusPhysicalDamageMod / 100f;
            var coneBonus      = WConeBonus[wLevel];
            var extraConeBonus = 0.033f * multiplier;

            coneBonus += extraConeBonus;
            var bonusDamage = target.MaxHealth * coneBonus;
            damage += bonusDamage;
            return LocalPlayer.CalculateDamage(target, DamageType.Physical, damage);
        }
    }
}