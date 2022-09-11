using System;
using System.Linq;
using System.Runtime.Serialization;
using EnsoulSharp;
using EnsoulSharp.SDK;
using PortAIO.Library_Ports.Entropy.Lib.Constants;
using PortAIO.Library_Ports.Entropy.Lib.Extensions;
using ObjectManager = EnsoulSharp.ObjectManager;

namespace Entropy.AIO.Darius.Misc.Logics
{
    using static Components;
    using static Bases.ChampionBase;

    public class LaneClear
    {
        public static void ExecuteQ()
        {
            if (!LaneClearMenu.QBool.Enabled || !Q.IsReady())
            {
                return;
            }

            if (ObjectManager.Player.EnemyMinionsCount(Q.Range) < LaneClearMenu.qHit.Value)
            {
                return;
            }

            Q.Cast();
        }

        public static void ExecuteW()
        {
            if (!LaneClearMenu.WBool.Enabled || !W.IsReady())
            {
                return;
            }

            var minion = GameObjects.EnemyMinions.Where(x => x.IsValidTarget(270) &&
                                                                 x.CanKillMinionWithDamage(W.GetDamage(x),
                                                                                           250f +
                                                                                           Game.Ping / 2f)).
                                     OrderBy(x => x.Health).
                                     FirstOrDefault();
            if (minion != null)
            {
                W.Cast();
                if (Environment.TickCount - W.LastCastAttemptTime < 250)
                {
                    Orbwalker.Attack(minion);
                }
            }
        }


        public static void ExecuteWLast()
        {
            if (!LastHitMenu.WBool.Enabled || !W.IsReady())
            {
                return;
            }

            var minion = GameObjects.EnemyMinions.Where(x => x.IsValidTarget(270) &&
                                                                 x.CanKillMinionWithDamage(W.GetDamage(x),
                                                                                           250f +
                                                                                           Game.Ping / 2f)).
                                     OrderBy(x => x.Health).
                                     FirstOrDefault();
            if (minion != null)
            {
                W.Cast();
                if (Environment.TickCount - W.LastCastAttemptTime < 250)
                {
                    Orbwalker.Attack(minion);
                }
            }
        }
    }
}