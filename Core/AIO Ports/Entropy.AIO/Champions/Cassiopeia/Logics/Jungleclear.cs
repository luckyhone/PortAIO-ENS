using System.Linq;
using EnsoulSharp;
using EnsoulSharp.SDK;
using PortAIO.Library_Ports.Entropy.Lib.Extensions;

namespace Entropy.AIO.Cassiopeia.Logics
{
    using Misc;
    using static Components;
    using static Bases.ChampionBase;

    static class JungleClear
    {
        private static AIHeroClient LocalPlayer => ObjectManager.Player;
        public static void ExecuteEPushLast()
        {
            var LasthitEMenu = JungleClearMenu.EBool.Enabled;
            if (!LasthitEMenu                       ||
                E.Level                 <= 0        ||
                LocalPlayer.Mana <= E.Mana ||
                !E.IsReady())
            {
                return;
            }

            var targets = GameObjects.Jungle.Where(m => m.IsValidTarget() && m.DistanceToPlayer() <= E.Range).ToList();
            var target = targets.Where(x =>
                                           x.CanKillMinionWithDamage(E.GetDamage(x),
                                                                     125f + x.DistanceToPlayer() / 2.5f + Game.Ping / 2f)).
                                 MaxBy(x => x.MaxHealth);
            if (target == null)
            {
                return;
            }

            E.Cast(target);
        }

        public static void ExecuteEPush()
        {
            var UseE = JungleClearMenu.EBool.Enabled;
            if (!UseE                               ||
                E.Level                 <= 0        ||
                LocalPlayer.Mana <= E.Mana ||
                !E.IsReady())
            {
                return;
            }


            var targetPoison = GameObjects.Jungle.FirstOrDefault(x =>
                                                                            x.IsValidTarget(E.Range) &&
                                                                            Definitions.CachedPoisoned.ContainsKey((uint)x.NetworkId));
            if (targetPoison != null)
            {
                E.Cast(targetPoison);
            }

            var target = GameObjects.Jungle.FirstOrDefault(x =>
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

            var minions = GameObjects.Jungle.FirstOrDefault(x => x.IsValidTarget(Q.Range));
            if (minions != null)
            {
                var qinpunt = Q.GetPrediction(minions);
                if (qinpunt.Hitchance >= HitChance.Low)
                {
                    Q.Cast(qinpunt.CastPosition);
                }
            }
        }
    }
}