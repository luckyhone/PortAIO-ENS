using EnsoulSharp;
using EnsoulSharp.SDK;
using EnsoulSharp.SDK.Utility;

namespace Entropy.AIO.Neeko.Logics
{
    using static Bases.ChampionBase;

    static class UponDash
    {
        public static void ExecuteE(Dash.DashArgs args,AIBaseClient sender)
        {
            var heroSender = sender as AIHeroClient;

            if (heroSender == null)
            {
                return;
            }

            if (heroSender.CharacterName.Equals("Kalista") || heroSender.CharacterName.Equals("Yasuo"))
            {
                return;
            }

            if (args.EndPos.DistanceToPlayer() > E.Range ||
                Invulnerable.Check(heroSender))
            {
                return;
            }

            E.Cast(args.EndPos);
        }
    }
}