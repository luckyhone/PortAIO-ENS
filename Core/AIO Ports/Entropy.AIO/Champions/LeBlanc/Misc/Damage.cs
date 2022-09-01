using EnsoulSharp;
using EnsoulSharp.SDK;
using Entropy.AIO.Bases;

namespace Entropy.AIO.LeBlanc.Misc
{
    using static ChampionBase;
    class Damage
    {
        private static AIHeroClient LocalPlayer => ObjectManager.Player;
        private static readonly float[] QBaseDamage       = {0f, 55f, 80f, 105f, 130f, 155f};
        private static readonly float[] WBaseDamage       = {0f, 85f, 125f, 165f, 205f, 245f};
        private static readonly float[] EBaseDamage       = {0f, 40f, 60f, 80f, 100f, 120f};
        private static readonly float[] EAdditionalDamage = {0f, 60f, 90f, 120f, 150f, 180f};

        private static readonly float[] RQBaseDamage = {0f, 70f, 140f, 210f};
        private static readonly float[] RWBaseDamage = {0f, 150f, 300f, 450f};
        private static readonly float[] REBaseDamage = {0f, 70f, 140f, 210f};

        private static float QMarkDamage =>
            QBaseDamage[Q.Level] + 0.4f * LocalPlayer.TotalMagicalDamage;

        private static float RQMarkDamage =>
            (RQBaseDamage[R.Level] + 0.4f * LocalPlayer.TotalMagicalDamage) * 2f;

        private static float REAdditionalDamage =>
            (REBaseDamage[R.Level] + 0.4f * LocalPlayer.TotalMagicalDamage) * 2f;

        public static double QDamage(AIBaseClient target)
        {
            var qLevel = Q.Level;

            var qBaseDamage = QBaseDamage[qLevel] +
                              0.4f * LocalPlayer.TotalMagicalDamage;

            if (target.HasBuff("LeblancQMark"))
            {
                qBaseDamage += QMarkDamage;
            }
            else if (target.HasBuff("LeblancRQMark"))
            {
                qBaseDamage += QMarkDamage * 2;
            }

            return LocalPlayer.CalculateDamage(target, DamageType.Magical, qBaseDamage);
        }

        public static double WDamage(AIBaseClient target)
        {
            var wLevel = W.Level;

            var wBaseDamage = WBaseDamage[wLevel] +
                              0.6f * LocalPlayer.TotalMagicalDamage;

            if (target.HasBuff("LeblancQMark"))
            {
                wBaseDamage += QMarkDamage;
            }
            else if (target.HasBuff("LeblancRQMark"))
            {
                wBaseDamage += QMarkDamage * 2;
            }

            return LocalPlayer.CalculateDamage(target, DamageType.Magical, wBaseDamage);
        }

        public static double EDamage(AIBaseClient target)
        {
            var eLevel = E.Level;

            var eBaseDamage = EBaseDamage[eLevel] +
                              0.3f * LocalPlayer.TotalMagicalDamage;

            if (Components.DrawsMenu.RootDanage.Enabled)
            {
                eBaseDamage += EAdditionalDamage[eLevel] +
                               0.7f * LocalPlayer.TotalMagicalDamage;
            }

            if (target.HasBuff("LeblancQMark"))
            {
                eBaseDamage += QMarkDamage;
            }
            else if (target.HasBuff("LeblancRQMark"))
            {
                eBaseDamage += QMarkDamage * 2;
            }

            return LocalPlayer.CalculateDamage(target, DamageType.Magical, eBaseDamage);
        }

        public static double RDamage(AIBaseClient target)
        {
            var rLevel = R.Level;

            var rBaseDamage = 0f;

            switch (Definitions.GetRState())
            {
                case RState.NULL:
                case RState.RQ:
                    rBaseDamage = RQBaseDamage[rLevel] + 0.4f * LocalPlayer.TotalMagicalDamage;
                    break;
                case RState.RW:
                    rBaseDamage = RWBaseDamage[rLevel] + 0.75f * LocalPlayer.TotalMagicalDamage;
                    break;
                case RState.RE:
                    rBaseDamage = REBaseDamage[rLevel] + 0.4f * LocalPlayer.TotalMagicalDamage;
                    if (Components.DrawsMenu.RootDanage.Enabled)
                    {
                        rBaseDamage += REAdditionalDamage;
                    }

                    break;
            }

            if (target.HasBuff("LeblancQMark"))
            {
                rBaseDamage += QMarkDamage;
            }
            else if (target.HasBuff("LeblancRQMark"))
            {
                rBaseDamage += QMarkDamage * 2;
            }

            return LocalPlayer.CalculateDamage(target, DamageType.Magical, rBaseDamage);
        }
    }
}