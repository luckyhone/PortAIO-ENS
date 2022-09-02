using EnsoulSharp;
using EnsoulSharp.SDK;

namespace Entropy.AIO.Aatrox.Misc
{
    using System.Linq;

    class Definitions
    {
        public static bool CanAttackAnyHero
            => Orbwalker.CanAttack() && GameObjects.EnemyHeroes.Any(x => x.IsValidTarget(ObjectManager.Player.GetCurrentAutoAttackRange()));

        public static bool CanAttackAnyMinion
            => Orbwalker.CanAttack() && GameObjects.EnemyMinions.Any(x => x.IsValidTarget(ObjectManager.Player.GetCurrentAutoAttackRange()));

        public static bool CanAttackTurret
            => Orbwalker.CanAttack() && GameObjects.EnemyTurrets.Any(x => x.IsValidTarget(ObjectManager.Player.GetCurrentAutoAttackRange()));
    }
}