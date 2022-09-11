using System.Linq;
using EnsoulSharp;
using EnsoulSharp.SDK;

namespace Entropy.AIO.Darius
{
    class Definitions
    {
        public static bool  AllowQ = true;
        public static float LastW  = 0f;

        public static bool CanAttackAnyHero
            => Orbwalker.CanAttack() && GameObjects.EnemyHeroes.Any(x => x.IsValidTarget(ObjectManager.Player.GetCurrentAutoAttackRange()));

        public static bool CanAttackAnyMinion
            => Orbwalker.CanAttack() && GameObjects.EnemyMinions.Any(x => x.IsValidTarget(ObjectManager.Player.GetCurrentAutoAttackRange()));

        public static bool CanAttackTurret
            => Orbwalker.CanAttack() && GameObjects.EnemyTurrets.Any(x => x.IsValidTarget(ObjectManager.Player.GetCurrentAutoAttackRange()));
    }
}