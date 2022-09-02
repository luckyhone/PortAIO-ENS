using EnsoulSharp;
using EnsoulSharp.SDK;

namespace Entropy.AIO.Aatrox.Logics
{
    using static Components;
    using static Bases.ChampionBase;

    class AntiGapcloser
    {
        public static void ExecuteW(AIHeroClient sender, EnsoulSharp.SDK.AntiGapcloser.GapcloserArgs args)
        {
            if (!GapCloserMenu.WBool.Enabled)
            {
                return;
            }
            

            if (args.EndPosition.DistanceToPlayer() > W.Range)
            {
                return;
            }

            switch (args.Type)
            {
                case EnsoulSharp.SDK.AntiGapcloser.GapcloserType.Targeted when args.Target.IsMe:
                    W.Cast(args.StartPosition);
                    break;
                case EnsoulSharp.SDK.AntiGapcloser.GapcloserType.UnknowDash when args.EndPosition.DistanceToPlayer() <= args.StartPosition.DistanceToPlayer():
                    W.Cast(args.EndPosition);
                    break;
            }
        }
    }
}