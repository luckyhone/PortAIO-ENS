using System.Linq;
using EnsoulSharp.SDK;
using Entropy.AIO.Bases;

namespace Entropy.AIO.LeBlanc.Logic
{
    using static Components;
    using static ChampionBase;
    class Killsteal
    {
        internal static void ExecuteQ()
        {
            if (!Q.IsReady() || !KillstealMenu.QBool.Enabled)
            {
                return;
            }

            var target = GameObjects.EnemyHeroes.Where(x => x.IsValidTarget(Q.Range)).FirstOrDefault(x => x.Health < Q.GetDamage(x));

            if (target != null)
            {
                Q.CastOnUnit(target);
            }
        }

        internal static void ExecuteW()
        {
            if (!W.IsReady() || !KillstealMenu.WBool.Enabled)
            {
                return;
            }

            var target = GameObjects.EnemyHeroes.Where(x => x.IsValidTarget(W.Range)).FirstOrDefault(x => x.Health < W.GetDamage(x));

            if (target != null)
            {
                W.Cast(target);
            }
        }

        internal static void ExecuteE()
        {
            if (!E.IsReady() || !KillstealMenu.EBool.Enabled)
            {
                return;
            }

            var target = GameObjects.EnemyHeroes.Where(x => x.IsValidTarget(E.Range)).FirstOrDefault(x => x.Health < E.GetDamage(x));

            if (target != null)
            {
                E.Cast(target);
            }
        }
    }
}