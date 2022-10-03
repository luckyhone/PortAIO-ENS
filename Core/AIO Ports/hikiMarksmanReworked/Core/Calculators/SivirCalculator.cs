using EnsoulSharp;
using hikiMarksmanRework.Core.Spells;
using hikiMarksmanRework.Core.Utilitys;

namespace hikiMarksmanRework.Core.Calculators
{
    class SivirCalculator
    {
        public static float SivirTotalDamage(AIHeroClient enemy)
        {
            if (SivirSpells.Q.IsReady() && Helper.SEnabled("sivir.q.combo"))
            {
                return SivirSpells.Q.GetDamage(enemy);
            }

            if (SivirSpells.W.IsReady() && Helper.SEnabled("sivir.w.combo"))
            {
                return SivirSpells.W.GetDamage(enemy);
            }

            return 0;
        }
    }
}