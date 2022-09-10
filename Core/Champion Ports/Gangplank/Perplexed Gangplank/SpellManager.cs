using EnsoulSharp;
using EnsoulSharp.SDK;

namespace Perplexed_Gangplank
{
    public static class SpellManager
    {
        public static float ExplosionRadius = 355f;
        public static float ChainRadius = 690f;

        public static Spell Q, W, E, E2, R;

        public static void Initialize()
        {
            Q = new Spell(SpellSlot.Q, 625);
            W = new Spell(SpellSlot.W);
            E = new Spell(SpellSlot.E, 1000);
            E2 = new Spell(SpellSlot.E, E.Range + ExplosionRadius);
            R = new Spell(SpellSlot.R, float.MaxValue);

            E.SetSkillshot(0.25f, ExplosionRadius, float.MaxValue, false, SpellType.Circle);
            R.SetSkillshot(0.25f, 1050, float.MaxValue, false, SpellType.Circle);
        }
    }
}