using EnsoulSharp;
using EnsoulSharp.SDK;

namespace Entropy.AIO.Darius.Misc.Logics
{
    using static Components;
    using static Bases.ChampionBase;

    class AntiGapcloser
    {
        public static void ExecuteE(AIBaseClient sender, EnsoulSharp.SDK.AntiGapcloser.GapcloserArgs args)
        {
            if (!GapCloserMenu.EBool.Enabled)
            {
                return;
            }
            

            if (sender.DistanceToPlayer() > E.Range)
            {
                return;
            }

            switch (args.Type)
            {
                case EnsoulSharp.SDK.AntiGapcloser.GapcloserType.Targeted when args.Target.IsMe:
                    E.Cast(args.StartPosition);
                    break;
                case EnsoulSharp.SDK.AntiGapcloser.GapcloserType.UnknowDash when args.EndPosition.DistanceToPlayer() <= args.StartPosition.DistanceToPlayer():
                    E.Cast(sender);
                    break;
            }
        }
    }
}