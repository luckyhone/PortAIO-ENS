using EnsoulSharp;
using EnsoulSharp.SDK;

namespace iDZed.Utils
{
    internal static class ZedDamage
    {
        private static float GetPassiveDamage(AIHeroClient target)
        {
            double totalDamage = 0;

            if (ObjectManager.Player.Level > 16)
            {
                double targetHealth = target.MaxHealth * .1;
                totalDamage += ObjectManager.Player.CalculateDamage(target, DamageType.Magical, targetHealth);
            }
            else if (ObjectManager.Player.Level > 6)
            {
                double targetHealth = target.MaxHealth * 0.8;
                totalDamage += ObjectManager.Player.CalculateDamage(target, DamageType.Magical, targetHealth);
            }
            else
            {
                double targetHealth = target.MaxHealth * 0.6;
                totalDamage += ObjectManager.Player.CalculateDamage(target, DamageType.Magical, targetHealth);
            }

            return (float) totalDamage;
        }

        private static float GetDeathmarkDamage(AIHeroClient target)
        {
            double totalDamage = 0;

            if (Zed._spells[SpellSlot.R].IsReady() || target.HasBuff("zedulttargetmark"))
            {
                totalDamage += Zed._spells[SpellSlot.R].GetDamage(target);

                switch (Zed._spells[SpellSlot.R].Level)
                {
                    case 1:
                        totalDamage += totalDamage * 1.2;
                        break;
                    case 2:
                        totalDamage += totalDamage * 1.35;
                        break;
                    case 3:
                        totalDamage += totalDamage * 1.5;
                        break;
                }
            }

            return (float) totalDamage;
        }

        public static float GetTotalDamage(AIHeroClient target)
        {
            double totalDamage = 0;

            if (Zed._spells[SpellSlot.Q].IsReady()) // TODO calculate 2 or 3 q's depending on shadows kappa
            {
                totalDamage += Zed._spells[SpellSlot.Q].GetDamage(target) * 2; // shadow logic pls
            }

            if (Zed._spells[SpellSlot.E].IsReady())
            {
                totalDamage += Zed._spells[SpellSlot.E].GetDamage(target) * 2; // Same shadow situation
            }

            if (target.HealthPercent <= 50)
            {
                totalDamage += GetPassiveDamage(target);
            }

            if (Zed._spells[SpellSlot.R].IsReady())
            {
                totalDamage += GetDeathmarkDamage(target);
            }

            return (float) totalDamage;
        }
    }
}