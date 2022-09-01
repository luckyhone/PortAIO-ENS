using EnsoulSharp;
using EnsoulSharp.SDK;
using Entropy.AIO.Bases;
using Definitions = Entropy.AIO.Gangplank.Misc.Definitions;

namespace Entropy.AIO.Gangplank
{
    using static ChampionBase;
    class Spells
    {
        public Spells()
        {
            Initialize();
        }

        private static void Initialize()
        {
            Q = new Spell(SpellSlot.Q, 625f);
            W = new Spell(SpellSlot.W);
            E = new Spell(SpellSlot.E, 1000f);
            R = new Spell(SpellSlot.R, float.MaxValue);

            E.SetSkillshot(0.25f, Definitions.ExplosionRadius * 2, float.MaxValue, false,SpellType.Circle);
            R.SetSkillshot(0.25f, 1050f                       * 2, float.MaxValue, false,SpellType.Circle);

            Q.SetCustomDamage(Misc.Damage.QDamage);
            W.SetCustomDamage(Misc.Damage.WDamage);
            E.SetCustomDamage(Misc.Damage.EDamage);
            R.SetCustomDamage(Misc.Damage.RDamage);
        }
    }
}