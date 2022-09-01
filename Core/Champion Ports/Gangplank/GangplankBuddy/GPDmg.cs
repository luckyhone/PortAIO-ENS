using EnsoulSharp;
using EnsoulSharp.SDK;

namespace GangplankBuddy
{
    class GPDmg
    {
        public static float QDamage(AIBaseClient target)
        {
            return (float) ObjectManager.Player.CalculateDamage(target, DamageType.Physical,
                (float) (new double[] {20, 45, 70, 95, 120}[
                             ObjectManager.Player.Spellbook.GetSpell(SpellSlot.Q).Level - 1] +
                         1 * (ObjectManager.Player.TotalAttackDamage)));
        }

        public static double EDamage(AIBaseClient target, float dmg)
        {
            return ObjectManager.Player.CalculateDamage(target, DamageType.Physical,
                (float) (!target.IsMinion()
                    ? (new double[] {80, 110, 140, 170, 200}[
                        ObjectManager.Player.Spellbook.GetSpell(SpellSlot.E).Level - 1])
                    : 0 + (dmg)));
        }
    }
}