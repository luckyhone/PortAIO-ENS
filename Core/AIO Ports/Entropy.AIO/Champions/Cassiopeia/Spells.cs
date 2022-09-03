using EnsoulSharp;
using EnsoulSharp.SDK;
using Damage = Entropy.AIO.Cassiopeia.Misc.Damage;
namespace Entropy.AIO.Cassiopeia
{
    using static Bases.ChampionBase;

    class Spells
    {
        public Spells()
        {
            Initialize();
        }

        public static Spell Flash  { get; set; }
        public static Spell FlashR { get; set; }

        private static void Initialize()
        {
            Q     = new Spell(SpellSlot.Q, 850);
            W     = new Spell(SpellSlot.W, 850);
            E     = new Spell(SpellSlot.E, 760);
            R     = new Spell(SpellSlot.R, 750);
            Flash = new Spell(ObjectManager.Player.GetSpellSlotFromName("SummonerFlash"), 400f);

            Q.SetSkillshot(0.85f, 340f, float.MaxValue, false,SpellType.Circle,HitChance.VeryHigh);
            W.SetSkillshot(.25f, 180f, 1500, false,SpellType.Circle,HitChance.VeryHigh);
            R.SetSkillshot(0.5f, 80f, float.MaxValue, false,SpellType.Cone, HitChance.VeryHigh);

            Q.SetCustomDamage(Damage.Q);
            W.SetCustomDamage(Damage.W);
            E.SetCustomDamage(Damage.E);
            R.SetCustomDamage(Damage.R);

            FlashR = new Spell(SpellSlot.Q, R.Range + 410);
            FlashR.SetSkillshot(0.5f, 80f, float.MaxValue, false,SpellType.Cone, HitChance.VeryHigh);
        }
    }
}