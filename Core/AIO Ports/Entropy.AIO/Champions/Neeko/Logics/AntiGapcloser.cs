using EnsoulSharp;
using EnsoulSharp.SDK;

namespace Entropy.AIO.Neeko.Logics
{
    using static Components;
    using static Bases.ChampionBase;

    static class AntiGapcloser
    {
        public static void ExecuteE(EnsoulSharp.SDK.AntiGapcloser.GapcloserArgs args,AIHeroClient sender)
        {
            if (!GapCloserMenu.EBool.Enabled)
            {
                return;
            }
            

            switch (args.Type)
            {
                case EnsoulSharp.SDK.AntiGapcloser.GapcloserType.Targeted when args.Target.IsMe:
                    E.Cast(args.StartPosition);
                    break;

                case EnsoulSharp.SDK.AntiGapcloser.GapcloserType.UnknowDash when args.EndPosition.DistanceToPlayer() <= ObjectManager.Player.GetCurrentAutoAttackRange():
                    E.Cast(args.EndPosition);
                    break;
            }
        }

        public static void ExecuteW(EnsoulSharp.SDK.AntiGapcloser.GapcloserArgs args,AIHeroClient sender)
        {
            if (!GapCloserMenu.WBool.Enabled)
            {
                return;
            }
            

            switch (args.Type)
            {
                case EnsoulSharp.SDK.AntiGapcloser.GapcloserType.Targeted when args.Target.IsMe:
                    W.Cast();
                    break;

                case EnsoulSharp.SDK.AntiGapcloser.GapcloserType.UnknowDash when args.EndPosition.DistanceToPlayer() <= ObjectManager.Player.GetCurrentAutoAttackRange():
                    W.Cast();
                    break;
            }
        }
    }
}