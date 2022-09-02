using System;
using ADCCOMMON;
using EnsoulSharp;
using EnsoulSharp.SDK;
using PortAIO.Library_Ports.Entropy.Lib.Constants;
using PortAIO.Library_Ports.Entropy.Lib.Geometry;

namespace Entropy.AIO.Aatrox.Logics
{
    using System.Linq;
    using static Components;
    using static Bases.ChampionBase;
    using static Extensions;

    static class Harass
    {
        private static AIHeroClient LocalPlayer => ObjectManager.Player;
        public static void ExecuteEAA()
        {
            if (!HarassMenu.EBool.Enabled || !E.IsReady() || Q.IsReady() || Environment.TickCount - Q.LastCastAttemptTime <= 1200)
            {
                return;
            }

            var target = TargetSelector.GetTarget(E.Range + 100,DamageType.Physical);
            if (target == null)
            {
                return;
            }

            if (GameObjectExtensions.DistanceToPlayer(target) > LocalPlayer.GetCurrentAutoAttackRange(target))
            {
                E.Cast(target.Position);
            }
        }

        public static void ExecuteE()
        {
            if (!HarassMenu.EBool.Enabled || !HarassMenu.EQBool.Enabled || !E.IsReady() || Environment.TickCount - Q.LastCastAttemptTime > 450)
            {
                return;
            }

            var target = TargetSelector.GetTarget(Q.Range,DamageType.Physical);
            if (target == null)
            {
                return;
            }

            if (target.Buffs.Any(x => x.Type == BuffType.Stun) ||
                target.Path.Length <= 1                        ||
                GameObjectExtensions.DistanceToPlayer(target) <= 200 &&
                GameObjectExtensions.DistanceToPlayer(target) >= 50  &&
                target.IsFacing(LocalPlayer))
            {
                if (Q.Name == "AatroxQ2")
                {
                    if (GameObjectExtensions.DistanceToPlayer(target) < 400)
                    {
                        E.Cast(LocalPlayer.Extend(target,-400));
                        E.Cast(LocalPlayer.Extend(target, -400));
                    }
                }

                if (Q.Name == "AatroxQ3")
                {
                    if (GameObjectExtensions.DistanceToPlayer(target) < E.Range)
                    {
                        E.Cast(LocalPlayer.Extend(target, -E.Range));
                    }
                }
            }
        }

        public static void ExecuteEGap()
        {
            if (!HarassMenu.EBool.Enabled || !E.IsReady() || Environment.TickCount - Q.LastCastAttemptTime > 450)
            {
                return;
            }

            var target = TargetSelector.GetTarget(E.Range + Q.Range,DamageType.Physical);
            if (target == null)
            {
                return;
            }

            var RangeChecks = PosAfterTime(target, Q.Delay + E.Delay);

            if (Q.GetPrediction(target).
                  Hitchance <
                HitChance.Medium)
            {
                return;
            }

            if (Q.Name == "AatroxQ2")
            {
                if (Distance.DistanceToPlayer(Prediction.GetPrediction(target,Q.Delay).CastPosition) < 500)
                {
                    return;
                }
            }

            if (Q.Name == "AatroxQ3")
            {
                if (Distance.DistanceToPlayer(RangeChecks) < E.Range)
                {
                    return;
                }
            }

            if (Q.Name == "AatroxQ")
            {
                if (Distance.DistanceToPlayer(RangeChecks) < E.Range)
                {
                    return;
                }
            }

            if (GameObjectExtensions.DistanceToPlayer(target) <= Q.Range && Q.Name != "AatroxQ")
            {
                return;
            }

            if (!(Distance.DistanceToPlayer(RangeChecks) <= Q.Range + E.Range))
            {
                return;
            }

            if (!(GameObjectExtensions.DistanceToPlayer(target) > LocalPlayer.GetCurrentAutoAttackRange(target)) &&
                Environment.TickCount - E.LastCastAttemptTime >= 500)
            {
                return;
            }

            E.Cast(target.Position);
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

            if (GameObjectExtensions.DistanceToPlayer(target) <= LocalPlayer.GetCurrentAutoAttackRange(target))
            {
                return;
            }

            if (Q.Type == SpellType.Circle && Q.GetPrediction(target).Hitchance == HitChance.OutOfRange)
            {
                return;
            }

            if (Distance.DistanceToPlayer(Prediction.GetPrediction(target,Q.Delay).CastPosition) > Q.Range)
            {
                return;
            }

            Q.Cast(target);
        }

        public static void ExecuteQGap()
        {
            if (!HarassMenu.QBool.Enabled || !Q.IsReady())
            {
                return;
            }

            if (!HarassMenu.EBool.Enabled || !E.IsReady())
            {
                return;
            }


            var target = TargetSelector.GetTarget(E.Range + Q.Range,DamageType.Physical);
            if (target == null)
            {
                return;
            }

            var RangeChecks = PosAfterTime(target, Q.Delay + E.Delay);

            if (Spells.DashQ.GetPrediction(target).
                       Hitchance <
                HitChance.Medium)
            {
                return;
            }

            if (Q.Name == "AatroxQ")
            {
                if (Distance.DistanceToPlayer(RangeChecks) < 700)
                {
                    return;
                }
            }

            if (Q.Name == "AatroxQ2")
            {
                if (Distance.DistanceToPlayer(RangeChecks) < 500)
                {
                    return;
                }
            }

            if (Q.Name == "AatroxQ3")
            {
                if (Distance.DistanceToPlayer(RangeChecks) < 400)
                {
                    return;
                }
            }

            if (!(Distance.DistanceToPlayer(RangeChecks) <= Q.Range + E.Range))
            {
                return;
            }

            if (!(GameObjectExtensions.DistanceToPlayer(target) > LocalPlayer.GetCurrentAutoAttackRange(target)) &&
                Environment.TickCount - E.LastCastAttemptTime >= 500)
            {
                return;
            }

            if (Q.Cast(Spells.DashQ.GetPrediction(target).CastPosition))
            {
                E.Cast(target.Position);
            }
        }

        public static void ExecuteW()
        {
            if (!HarassMenu.WBool.Enabled || !W.IsReady())
            {
                return;
            }

            var target = TargetSelector.GetTarget(W.Range,DamageType.Physical);
            if (target == null)
            {
                return;
            }

            W.Cast(target);
        }
    }
}