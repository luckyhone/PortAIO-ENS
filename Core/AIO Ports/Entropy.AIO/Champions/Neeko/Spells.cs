using EnsoulSharp;
using EnsoulSharp.SDK;
using Entropy.AIO.Bases;

namespace Entropy.AIO.Neeko
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
            Q = new Spell(SpellSlot.Q, 800);
            W = new Spell(SpellSlot.W);
            E = new Spell(SpellSlot.E, 1000);
            R = new Spell(SpellSlot.R, 525 + ObjectManager.Player.BoundingRadius);

            Q.SetSkillshot(0.25f,
                225f,
                1000f,
                false,
                SpellType.Circle,
                HitChance.Medium); // Missilespeed not accurate since it works the same as Corki Q, based on distance.
            E.SetSkillshot(0.25f, 120f, 1000f, false,SpellType.Line);

            Q.SetCustomDamage(Misc.Damage.Q);
            E.SetCustomDamage(Misc.Damage.E);
            R.SetCustomDamage(Misc.Damage.R);
        }
    }
}