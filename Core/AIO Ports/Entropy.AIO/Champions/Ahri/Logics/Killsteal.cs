using System.Linq;
using EnsoulSharp.SDK;
using EnsoulSharp.SDK.Utility;
using Entropy.AIO.Bases;
using Entropy.AIO.Utility;

namespace Entropy.AIO.Ahri.Logics
{
    using static ChampionBase;
    class Killsteal
    {
        public static void ExecuteQ()
        {
            if (!Components.KillstealMenu.QBool.Enabled)
            {
                return;
            }

            foreach (var target in GameObjects.EnemyHeroes.Where(t => Q.CanExecute(t)          &&
                                                                      t.IsValidTarget(Q.Range) &&
                                                                      !t.IsInvulnerable))
            {
                Q.Cast(target);
                break;
            }
        }

        public static void ExecuteE()
        {
            if (!Components.KillstealMenu.EBool.Enabled)
            {
                return;
            }

            foreach (var target in GameObjects.EnemyHeroes.Where(t => E.CanExecute(t)          &&
                                                                      t.IsValidTarget(E.Range) &&
                                                                      !t.IsInvulnerable))
            {
                E.Cast(target);
                break;
            }
        }
    }
}