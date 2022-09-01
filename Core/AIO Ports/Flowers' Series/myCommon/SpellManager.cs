using EnsoulSharp;
using EnsoulSharp.SDK;

namespace ADCCOMMON
{

    public static class SpellManager
    {
        public static void PredCast(Spell spell, AIBaseClient target, bool isAOE = false)
        {
            if (!spell.IsReady())
            {
                return;
            }

            var pred = spell.GetPrediction(target, isAOE);

            if (pred.Hitchance >= HitChance.VeryHigh)
            {
                spell.Cast(pred.CastPosition);
            }
        }
    }
}