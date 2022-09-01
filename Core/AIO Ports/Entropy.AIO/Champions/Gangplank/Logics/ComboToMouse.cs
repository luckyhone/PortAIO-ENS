using EnsoulSharp;
using EnsoulSharp.SDK;
using Entropy.AIO.Bases;
using Entropy.AIO.Gangplank.Misc;
using static Entropy.AIO.Gangplank.Components;

namespace Entropy.AIO.Gangplank.Logics
{
    using static ChampionBase;
    static class ComboToMouse
    {
        public static void Execute()
        {
            var nearestBarrelToCursor = BarrelManager.GetNearestBarrel(Game.CursorPos);
            if (nearestBarrelToCursor != null)
            {
                var chainedBarrels = BarrelManager.GetChainedBarrels(nearestBarrelToCursor);
                if (chainedBarrels.Count > 1)
                {
                    var barrelToQ = BarrelManager.GetBestBarrelToQ(chainedBarrels);
                    if (barrelToQ != null)
                    {
                        if (Q.IsReady())
                        {
                            Q.Cast(barrelToQ.Object);
                            return;
                        }

                        if (barrelToQ.Object.InAutoAttackRange(Definitions.Player) &&
                            Orbwalker.CanAttack()                                    &&
                            ComboMenu.ExplodeAA.Enabled)
                        {
                            Orbwalker.ForceTarget = (barrelToQ.Object);
                            Orbwalker.Attack(barrelToQ.Object);
                        }
                    }
                }
                else
                {
                    if (nearestBarrelToCursor.Object.Distance(Game.CursorPos) <= Definitions.ChainRadius)
                    {
                        if (E.IsReady() && nearestBarrelToCursor.CanChain)
                        {
                            BarrelManager.CastE(Game.CursorPos);
                        }
                    }
                    else
                    {
                        var bestPos =
                            nearestBarrelToCursor.ServerPosition.Extend(Game.CursorPos, Definitions.ChainRadius - 5);
                        if (E.IsReady() && nearestBarrelToCursor.CanChain)
                        {
                            BarrelManager.CastE(bestPos);
                        }
                    }
                }
            }
            else
            {
                if (Definitions.Player.Distance(Game.CursorPos) <= E.Range && E.IsReady())
                {
                    BarrelManager.CastE(Game.CursorPos);
                }
            }
        }
    }
}