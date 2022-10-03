using EnsoulSharp;
using hikiMarksmanRework.Core.Spells;
using hikiMarksmanRework.Core.Utilitys;

namespace hikiMarksmanRework.Core.Calculators
{
    class LucianCalculator
    {
        public static float LucianTotalDamage(AIHeroClient enemy)
        {
            if (LucianSpells.Q.IsReady() && Helper.LEnabled("lucian.q.combo"))
            {
                return LucianSpells.Q.GetDamage(enemy);
            }
            if (LucianSpells.W.IsReady() && Helper.LEnabled("lucian.w.combo"))
            {
                return LucianSpells.W.GetDamage(enemy);
            }
            if (LucianSpells.R.IsReady() && Helper.LEnabled("lucian.r.combo"))
            {
                return LucianSpells.R.GetDamage(enemy);
            }
            return 0;
        }
    }
}