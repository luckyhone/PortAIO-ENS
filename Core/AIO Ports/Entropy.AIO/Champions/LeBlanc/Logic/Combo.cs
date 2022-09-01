using EnsoulSharp.SDK;
using Entropy.AIO.Bases;
using Entropy.AIO.LeBlanc.Misc;

namespace Entropy.AIO.LeBlanc.Logic
{
    using static Components;
    using static ChampionBase;
    static class Combo
    {
        internal static void ExecuteQ()
        {
            if (!ComboMenu.QBool.Enabled)
            {
                return;
            }

            var target = Definitions.GetMarkedTarget;
            if (target != null && target.IsValid && target.DistanceToPlayer() <= Q.Range)
            {
                Q.CastOnUnit(target);
            }

            target = TargetSelector.GetTarget(Q.Range,DamageType.Magical);
            if (target == null || !target.IsValid)
            {
                return;
            }

            Q.CastOnUnit(target);
        }

        internal static void ExecuteW()
        {
            if (!Definitions.HasW1)
            {
                if (!ComboMenu.WReturnBool.Enabled || Q.IsReady() || E.IsReady() || R.IsReady())
                {
                    return;
                }

                W.Cast();
                return;
            }

            if (!ComboMenu.WBool.Enabled)
            {
                return;
            }

            var target = Definitions.GetMarkedTarget;
            if (target != null && target.IsValid && target.DistanceToPlayer() <= W.Range + W.Width)
            {
                W.Cast(target);
                return;
            }

            target = TargetSelector.GetTarget(W.Range,DamageType.Magical);
            if (target != null && target.IsValid)
            {
                W.Cast(target);
                return;
            }

            if (!ComboMenu.WGCBool.Enabled)
            {
                return;
            }

            target = TargetSelector.GetTarget(W.Range + Q.Range,DamageType.Magical);
            if (target == null || !target.IsValid)
            {
                return;
            }

            W.Cast(target.Position);
        }

        internal static void ExecuteE()
        {
            if (!ComboMenu.EBool.Enabled)
            {
                return;
            }

            var target = Definitions.GetMarkedTarget;
            if (target != null && target.IsValid && target.DistanceToPlayer() <= E.Range)
            {
                E.Cast(target);
                return;
            }

            target = TargetSelector.GetTarget(E.Range,DamageType.Magical);
            if (target == null || !target.IsValid)
            {
                return;
            }

            E.Cast(target);
        }

        internal static void ExecuteR()
        {
            if (!ComboMenu.RBool.Enabled)
            {
                return;
            }

            var target = Definitions.GetMarkedTarget;

            if (target == null || !target.IsValid || target.DistanceToPlayer() > 700f)
            {
                switch (Definitions.GetRMode)
                {
                    case RMode.NULL:
                    case RMode.RQ when (int) Definitions.GetRState() == (int) RMode.RQ:
                        target = TargetSelector.GetTarget(Q.Range,DamageType.Magical);
                        if (target == null || !target.IsValid)
                        {
                            return;
                        }

                        Spells.RQ.CastOnUnit(target);
                        break;
                    case RMode.RE when (int) Definitions.GetRState() == (int) RMode.RE:
                        target = TargetSelector.GetTarget(E.Range,DamageType.Magical);

                        if (target == null || !target.IsValid)
                        {
                            return;
                        }

                        Spells.RE.Cast(target);
                        break;
                }
            }
        }
    }
}