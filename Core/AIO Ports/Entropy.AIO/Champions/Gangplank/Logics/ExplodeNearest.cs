using EnsoulSharp.SDK;
using Entropy.AIO.Gangplank.Misc;
using static Entropy.AIO.Bases.ChampionBase;
namespace Entropy.AIO.Gangplank.Logics
{
    static class ExplodeNearest
    {
        public static void Execute()
        {
            var barrel = BarrelManager.GetNearestBarrel();
            if (barrel != null)
            {
                if (barrel.CanQ)
                {
                    if (Q.IsReady())
                    {
                        Q.Cast(barrel.Object);
                        return;
                    }

                    if (barrel.Object.InAutoAttackRange(Definitions.Player) &&
                        Orbwalker.CanAttack()                                 &&
                        Components.ComboMenu.ExplodeAA.Enabled)
                    {
                        Orbwalker.ForceTarget = (barrel.Object);
                        Orbwalker.Attack(barrel.Object);
                    }
                }
            }
        }
    }
}