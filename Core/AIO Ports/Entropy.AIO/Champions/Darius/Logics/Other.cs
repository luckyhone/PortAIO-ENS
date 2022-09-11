using EnsoulSharp;
using EnsoulSharp.SDK;

namespace Entropy.AIO.Darius.Misc.Logics
{
    using static Components;
    using static Bases.ChampionBase;

    static class Other
    {
        public static void Magnet()
        {
            if (!ComboMenu.QLock.Active || !ObjectManager.Player.HasBuff("dariusqcast"))
            {
                return;
            }

            var target = TargetSelector.GetTarget(Q.Range,DamageType.Physical);
            if (target == null)
            {
                return;
            }

            if (target.DistanceToPlayer() <= 250)
            {
                Orbwalker.Move(ObjectManager.Player.Position.Extend(target.Position, -Q.Range));
            }
        }
    }
}