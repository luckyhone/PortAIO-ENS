using System.Linq;
using EnsoulSharp;
using EnsoulSharp.SDK;

namespace Entropy.AIO.Cassiopeia.Logics
{
    using Misc;
    using static Bases.ChampionBase;
    using static Components;


    static class LaneClear
    {
        private static AIHeroClient LocalPlayer => ObjectManager.Player;
        public static void ExecuteEPassive()
        {
            var LasthitEMenu = LaneClearMenu.EPassive.Enabled;
            if (!LasthitEMenu                       ||
                E.Level                 <= 0        ||
                LocalPlayer.Mana <= E.Mana ||
                !E.IsReady())
            {
                return;
            }

            if (MinionManager.AACheckMinion != null)
            {
                Lasthit.possibleTarget = MinionManager.AACheckMinion;
            }


            if (MinionManager.GetBestLasthitMinion == null)
            {
                return;
            }

            E.Cast(MinionManager.GetBestLasthitMinion);
        }

        public static void ExecuteEPushLast()
        {
            var LasthitEMenu = LaneClearMenu.EPassive.Enabled;
            if (!LasthitEMenu                       ||
                E.Level                 <= 0        ||
                LocalPlayer.Mana <= E.Mana ||
                !E.IsReady())
            {
                return;
            }

            if (MinionManager.AACheckMinion != null)
            {
                Lasthit.possibleTarget = MinionManager.AACheckMinion;
            }


            if (MinionManager.GetBestLasthitMinion == null)
            {
                return;
            }

            E.Cast(MinionManager.GetBestLasthitMinion);
        }

        public static void ExecuteEPush()
        {
            var UseE = LaneClearMenu.EBool.Enabled;
            if (!UseE                               ||
                E.Level                 <= 0        ||
                LocalPlayer.Mana <= E.Mana ||
                !E.IsReady())
            {
                return;
            }


            var targetPoison = GameObjects.EnemyMinions.FirstOrDefault(x =>
                                                                           x.IsValidTarget(E.Range) &&
                                                                           Definitions.CachedPoisoned.ContainsKey((uint)x.NetworkId));
            if (targetPoison != null)
            {
                E.Cast(targetPoison);
            }

            if (LaneClearMenu.EPoisonBool.Enabled)
            {
                return;
            }

            var target = GameObjects.EnemyMinions.FirstOrDefault(x =>
                                                                     x.IsValidTarget(E.Range));
            if (target != null)
            {
                E.Cast(target);
            }
        }

        public static void ExecuteQPush()
        {
            if (!LaneClearMenu.QBool.Enabled)
            {
                return;
            }

            var minions      = GameObjects.EnemyMinions.Where(x => x.IsValidTarget(Q.Range)).ToList();
            var farmLocation = Q.GetCircularFarmLocation(minions, Q.Width);
            if (farmLocation.MinionsHit >= LaneClearMenu.hitsQ.Value)
            {
                Q.Cast(farmLocation.Position);
            }
        }
    }
}