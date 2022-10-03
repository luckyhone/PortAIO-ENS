using EnsoulSharp;
using EnsoulSharp.SDK;
using Entropy.AIO.Utility;

namespace Entropy.AIO.Irelia.Misc
{
    
    using System.Collections.Generic;
    using System.Linq;
    using static Components;
    using static Bases.ChampionBase;

    public class MinionManager
    {
        public static List<AIMinionClient> KillableMinions = new List<AIMinionClient>();
        public static AIMinionClient       GetBestLasthitMinion => KillableMinions.FirstOrDefault();

        public static AIMinionClient BestJump(AIBaseClient target)
        {
            var bestJump = GameObjects.EnemyMinions.Where(x => x.IsValidTarget(Q.Range)     &&
                                                                   Q.CanExecute(x)              &&
                                                                   x.Distance(target) < Q.Range &&
                                                                   !x.Position.IsUnderEnemyTurret()).
                                       OrderBy(x => x.MaxHealth).
                                       ToList();
            return bestJump.FirstOrDefault();
        }

        public static AIMinionClient BestGap(AIBaseClient target)
        {
            var bestJump = GameObjects.EnemyMinions.Where(x => x.IsValidTarget(Q.Range)                     &&
                                                                   (Q.CanExecute(x) || x.HasBuff("ireliamark")) &&
                                                                   x.Distance(target) < Q.Range).
                                       OrderBy(x => x.MaxHealth).
                                       ToList();
            return bestJump.OrderBy(x => x.Distance(target)).FirstOrDefault();
        }

        public static void MinionList()
        {
            if (LaneClearMenu.QTurret.Enabled)
            {
                KillableMinions = GameObjects.EnemyMinions.Where(x => x.IsValidTarget(Q.Range)         &&
                                                                          !x.Position.IsUnderEnemyTurret() &&
                                                                          x.DistanceToPlayer() >
                                                                          (LaneClearMenu.qAA.Enabled ? 250 : 0) &&
                                                                          x.Position.CountEnemyHeroesInRange(
                                                                              LaneClearMenu.qRange.Value) ==
                                                                          0 &&
                                                                          Q.CanExecute(x)).
                                              OrderBy(x => x.MaxHealth).
                                              ToList();
            }
            else
            {
                KillableMinions = GameObjects.EnemyMinions.Where(x => x.IsValidTarget(Q.Range) &&
                                                                          x.DistanceToPlayer() >
                                                                          (LaneClearMenu.qAA.Enabled ? 250 : 0) &&
                                                                          x.Position.CountEnemyHeroesInRange(
                                                                              LaneClearMenu.qRange.Value) ==
                                                                          0 &&
                                                                          Q.CanExecute(x)).
                                              OrderBy(x => x.MaxHealth).
                                              ToList();
            }
        }
    }
}