using System.Linq;
using EnsoulSharp;
using EnsoulSharp.SDK;

namespace Entropy.AIO.Darius.Misc.Logics
{
    using Misc;
    using static Components;
    using static Bases.ChampionBase;

    public class JungleClear
    {
        public static void ExecuteQ()
        {
            if (!JungleClearMenu.QBool.Enabled || !Q.IsReady())
            {
                return;
            }

            var target = GameObjects.Jungle.FirstOrDefault(x => x.IsValidTarget(Q.Range));
            if (target == null)
            {
                return;
            }

            if (ObjectManager.Player.HasBuff("dariusnoxiantacticsonh") || Game.Time < Definitions.LastW)
            {
                return;
            }

            if (!ComboMenu.QAA.Enabled && target.DistanceToPlayer() <= 260 && W.IsReady() && ComboMenu.WBool.Enabled)
            {
                return;
            }

            Q.Cast();
        }
    }
}