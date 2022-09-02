using EnsoulSharp;
using EnsoulSharp.SDK;

namespace Entropy.AIO.Camille
{
    #region

    using Misc;
    using static Bases.ChampionBase;

    #endregion

    class Spells
    {
        public Spells()
        {
            Initialize();
        }

        private static void Initialize()
        {
            Q = new Spell(SpellSlot.Q, 300f);
            W = new Spell(SpellSlot.W, 610f);
            E = new Spell(SpellSlot.E, 1000f);
            R = new Spell(SpellSlot.R, 475f);

            W.SetSkillshot(0.25f, 80f, float.MaxValue, false,SpellType.Cone);

            Q.SetCustomDamage(Damage.Q);
            W.SetCustomDamage(Damage.W);
        }
    }
}