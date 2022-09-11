using EnsoulSharp;
using EnsoulSharp.SDK;

namespace Entropy.AIO.Darius.Misc.Logics
{
    using Misc;
    using static Components;
    using static Bases.ChampionBase;

    static class Combo
    {
        public static void ExecuteE()
        {
            if (!ComboMenu.EBool.Enabled || !E.IsReady())
            {
                return;
            }

            var target = TargetSelector.GetTarget(E.Range,DamageType.Physical);
            if (target == null)
            {
                return;
            }

            if (ComboMenu.EAA.Enabled && ObjectManager.Player.InAutoAttackRange(target))
            {
                return;
            }

            E.Cast(target);
        }

        public static void ExecuteQ()
        {
            if (!ComboMenu.QBool.Enabled || !Q.IsReady())
            {
                return;
            }

            var target = TargetSelector.GetTarget(Q.Range,DamageType.Physical);
            if (target == null)
            {
                return;
            }

            if (ComboMenu.QAA.Enabled && target.DistanceToPlayer() <= 260)
            {
                return;
            }

            if (ComboMenu.QChecks.Enabled && Definitions.AllowQ == false)
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