using System.Linq;
using EnsoulSharp.SDK;
using EzAIO.Bases;

namespace Entropy.AIO.Neeko.Logics
{
    using Misc;
    using static ChampionBases;
    using static Components;
    static class Automatic
    {
        public static void EOnImmobile()
        {
            if (!AutomaticMenu.EImmobileBool.Enabled)
            {
                return;
            }

            foreach (var target in GameObjects.EnemyHeroes.Where(t => t.IsStunned && t.IsValidTarget(E.Range)))
            {
                E.Cast(target.Position);
            }
        }

        public static void ExecuteR()
        {
            if (!AutomaticMenu.RTargetsNear.Enabled)
            {
                return;
            }

            if (AutomaticMenu.ROnlyInShape.Enabled && !Definitions.InShape())
            {
                return;
            }

            if (GameObjects.EnemyHeroes.Count(t => t.IsValidTarget(R.Range)) >= AutomaticMenu.RTargetsNear.Value)
            {
                R.Cast();
            }
        }
    }
}