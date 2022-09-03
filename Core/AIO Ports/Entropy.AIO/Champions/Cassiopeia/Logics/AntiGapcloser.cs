using EnsoulSharp;
using EnsoulSharp.SDK;

namespace Entropy.AIO.Cassiopeia.Logics
{
    using Bases;
    using static Components;

    static class AntiGapcloser
    {
        public static void ExecuteR(AIHeroClient sender, EnsoulSharp.SDK.AntiGapcloser.GapcloserArgs args)
        {
            if (!GapCloserMenu.RBool.Enabled)
            {
                return;
            }
            

            switch (args.Type)
            {
                case EnsoulSharp.SDK.AntiGapcloser.GapcloserType.Targeted when args.Target.IsMe:
                case EnsoulSharp.SDK.AntiGapcloser.GapcloserType.UnknowDash
                    when args.EndPosition.DistanceToPlayer() <= ObjectManager.Player.GetCurrentAutoAttackRange():
                    ChampionBase.R.Cast(sender.Position);
                    break;
            }
        }
    }
}