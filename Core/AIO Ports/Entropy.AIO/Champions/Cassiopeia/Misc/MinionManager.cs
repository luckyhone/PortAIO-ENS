using System.Collections.Generic;
using System.Linq;
using EnsoulSharp;
using EnsoulSharp.SDK;
using Entropy.AIO.Bases;
using PortAIO.Library_Ports.Entropy.Lib.Extensions;

namespace Entropy.AIO.Cassiopeia.Misc
{
    public class MinionManager
    {
        public static List<AIMinionClient> KillableMinions = new List<AIMinionClient>();
        public static List<AIMinionClient> AACheck         = new List<AIMinionClient>();
        public static AIMinionClient       GetBestLasthitMinion => KillableMinions.FirstOrDefault();

        public static AIMinionClient AACheckMinion => AACheck.FirstOrDefault();

        public static void MinionList()
        {
            KillableMinions = GameObjects.EnemyMinions.Where(x =>
                    x.IsValidTarget(ChampionBase.E.Range) &&
                    x.CanKillMinionWithDamage(ChampionBase.E.GetDamage(x),
                        125f                        +
                        x.DistanceToPlayer() / 2.5f +
                        Game.Ping      / 2f)).
                OrderBy(x => x.MaxHealth).
                ToList();

            AACheck = GameObjects.EnemyMinions.Where(x =>
                    x.IsValidTarget(ChampionBase.E.Range) &&
                    x.CanKillMinionWithDamage(
                        (float)(ChampionBase.E.GetDamage(x) +
                                ObjectManager.Player.GetAutoAttackDamage(x)),
                        125f + x.DistanceToPlayer() / 2.5f + Game.Ping / 2f)).
                OrderBy(x => x.MaxHealth).
                ToList();
        }
    }
}