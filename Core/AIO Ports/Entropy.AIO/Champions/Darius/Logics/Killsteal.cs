using System.Linq;
using EnsoulSharp.SDK;
using EnsoulSharp.SDK.Utility;

namespace Entropy.AIO.Darius.Misc.Logics
{
    using Bases;
    using Components = Components;

    static class Killsteal
    {
        public static void ExecuteR()
        {
            if (!Components.KillstealMenu.KSR.Enabled)
            {
                return;
            }

            if (!ChampionBase.R.IsReady())
            {
                return;
            }

            foreach (var target in GameObjects.EnemyHeroes.Where(t =>
                         t.IsValidTarget(ChampionBase.R.Range) &&
                         ChampionBase.R.GetDamage(t) >=
                         t.Health &&
                         !Invulnerable.Check(
                             t,
                             damage: ChampionBase.R.GetDamage(t))))
            {
                ChampionBase.R.CastOnUnit(target);
                break;
            }
        }
    }
}