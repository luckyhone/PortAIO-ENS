using EnsoulSharp.SDK;
using Entropy.AIO.Bases;

namespace Entropy.AIO.Ahri.Logics
{
    using static ChampionBase;
    class AntiGapcloser
    {
        public static void ExecuteE(EnsoulSharp.SDK.AntiGapcloser.GapcloserArgs args)
        {
            if (!Components.GapCloserMenu.EBool.Enabled)
            {
                return;
            }

            if (!Components.GapCloserMenu.EGapcloser.Enabled)
            {
                return;
            }

            switch (args.Type)
            {
                case EnsoulSharp.SDK.AntiGapcloser.GapcloserType.Targeted when args.Target.IsMe:
                case EnsoulSharp.SDK.AntiGapcloser.GapcloserType.UnknowDash when args.EndPosition.DistanceToPlayer() <= 250:
                    E.Cast(args.Target);
                    break;
            }
        }
    }
}