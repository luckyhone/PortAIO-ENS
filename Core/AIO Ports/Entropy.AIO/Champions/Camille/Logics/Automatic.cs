using EnsoulSharp;
using EnsoulSharp.SDK;

namespace Entropy.AIO.Camille.Logics
{
    #region

    using Misc;
    using static Bases.ChampionBase;

    #endregion

    static class Automatic
    {
        public static void WMagnet()
        {
            var target = TargetSelector.GetTarget(W.Range,DamageType.Physical);
            if (target == null)
            {
                return;
            }

            var actualEnd = ObjectManager.Player.Position + Definitions.WDirection * W.Range;
            var movePos   = target.Position.Extend(actualEnd, -490);
            Orbwalker.Move(movePos);
        }
    }
}