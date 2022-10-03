using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using EnsoulSharp;
using hikiMarksmanRework.Core.Spells;
using hikiMarksmanRework.Core.Utilitys;
namespace hikiMarksmanRework.Core.Calculators
{
    class GravesCalculator
    {
        public static float GravesTotalDamage(AIHeroClient enemy)
        {
            if (GravesSpells.Q.IsReady() && Helper.Enabled("graves.q.combo"))
            {
                return GravesSpells.Q.GetDamage(enemy);
            }
            if (GravesSpells.W.IsReady() && Helper.Enabled("graves.w.combo"))
            {
                return GravesSpells.W.GetDamage(enemy);
            }
            if (GravesSpells.R.IsReady() && Helper.Enabled("graves.r.combo"))
            {
                return GravesSpells.R.GetDamage(enemy);
            }
            return 0;
        }
    }
}