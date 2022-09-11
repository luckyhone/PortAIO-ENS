using EnsoulSharp;
using EnsoulSharp.SDK;

namespace Entropy.AIO.Darius.Misc.Logics
{
    using Misc;
    using static Components;
    using static Bases.ChampionBase;

    static class Harass
    {
        public static void ExecuteE()
        {
            if (!HarassMenu.EBool.Enabled || !E.IsReady())
            {
                return;
            }

            var target = TargetSelector.GetTarget(E.Range,DamageType.Physical);
            if (target == null)
            {
                return;
            }

            if (HarassMenu.EAA.Enabled && ObjectManager.Player.InAutoAttackRange(target))
            {
                return;
            }

            E.Cast(target);
        }

        public static void ExecuteQ()
        {
            if (!HarassMenu.QBool.Enabled || !Q.IsReady())
            {
                return;
            }

            var target = TargetSelector.GetTarget(Q.Range,DamageType.Physical);
            if (target == null)
            {
                return;
            }

            if (HarassMenu.QAA.Enabled && target.DistanceToPlayer() <= 260)
            {
                return;
            }

            if (HarassMenu.QChecks.Enabled && Definitions.AllowQ == false)
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