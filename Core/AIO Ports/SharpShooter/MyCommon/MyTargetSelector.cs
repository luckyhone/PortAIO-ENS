using System.Collections.Generic;
using System.Linq;
using EnsoulSharp;
using EnsoulSharp.SDK;

namespace SharpShooter.MyCommon
{
    public class MyTargetSelector
    {
        public static AIHeroClient GetTarget(float range, bool ForcusOrbwalkerTarget = true, bool checkKillAble = true, bool checkShield = false)
        {
            var selectTarget = TargetSelector.SelectedTarget;

            if (selectTarget != null && selectTarget.IsValidTarget(range))
            {
                if (!checkKillAble || !selectTarget.IsUnKillable())
                {
                    if (!checkShield || !selectTarget.HaveShiledBuff())
                    {
                        return selectTarget;
                    }
                }
            }

            var orbTarget = Orbwalker.GetTarget() as AIHeroClient;

            if (ForcusOrbwalkerTarget && orbTarget != null && orbTarget.IsValidTarget(range) && orbTarget.InAutoAttackRange())
            {
                if (!checkKillAble || !orbTarget.IsUnKillable())
                {
                    if (!checkShield || !orbTarget.HaveShiledBuff())
                    {
                        return orbTarget;
                    }
                }
            }

            var finallyTarget = TargetSelector.GetTargets(range,DamageType.Mixed).FirstOrDefault();

            if (finallyTarget != null && finallyTarget.IsValidTarget(range))
            {
                if (!checkKillAble || !finallyTarget.IsUnKillable())
                {
                    if (!checkShield || !finallyTarget.HaveShiledBuff())
                    {
                        return finallyTarget;
                    }
                }
            }

            return null;
        }

        public static List<AIHeroClient> GetTargets(float range, bool checkKillAble = true, bool checkShield = false)
        {
            return
                TargetSelector.GetTargets(range,DamageType.Mixed)
                    .Where(x => !checkKillAble || !x.IsUnKillable())
                    .Where(x => !checkShield || !x.HaveShiledBuff())
                    .ToList();
        }
    }
}