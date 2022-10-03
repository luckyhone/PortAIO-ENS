using EnsoulSharp;
using EnsoulSharp.SDK;

namespace Entropy.AIO.Irelia.Logics
{
    #region

    using System.Linq;
    using Misc;
    using static Bases.ChampionBase;
    using static Components;

    #endregion

    static class Flee
    {
        public static void Execute()
        {
            if (!FleeMenu.QBool.Enabled)
            {
                return;
            }

            if (!FleeMenu.marked.Enabled)
            {
                var target = GameObjects.EnemyMinions.Where(x => x.IsValidTarget(Q.Range) &&
                                                                 Game.CursorPos.DistanceToPlayer() >
                                                                 x.Distance(Game.CursorPos)).
                    OrderByDescending(x => x.HasBuff("ireliamark") || Damage.QDamage(x) >= x.Health).
                    ThenBy(x => x.DistanceToPlayer()).
                    FirstOrDefault();
                if (target != null)
                {
                    Q.CastOnUnit(target);
                }
            }

            if (FleeMenu.marked.Enabled)
            {
                var target = GameObjects.EnemyMinions.Where(x => x.IsValidTarget(Q.Range) &&
                                                                 Game.CursorPos.DistanceToPlayer() >
                                                                 x.Distance(Game.CursorPos) &&
                                                                 (x.HasBuff("ireliamark") || Damage.QDamage(x) >= x.Health)).
                    OrderBy(x => x.DistanceToPlayer()).
                    FirstOrDefault();
                if (target != null)
                {
                    Q.CastOnUnit(target);
                }
            }
        }
    }
}