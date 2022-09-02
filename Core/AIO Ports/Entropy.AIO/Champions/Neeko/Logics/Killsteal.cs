using System.Linq;
using EnsoulSharp.SDK;
using EnsoulSharp.SDK.Utility;

namespace Entropy.AIO.Neeko.Logics
{
    using static Components;
    using static Bases.ChampionBase;

    static class Killsteal
    {
        public static void ExecuteQ()
        {
            if (!KillstealMenu.QBool.Enabled)
            {
                return;
            }

            foreach (var target in GameObjects.EnemyHeroes.Where(t =>
                         t.IsValidTarget(Q.Range)                              &&
                         Q.GetDamage(t) >= t.Health &&
                         !Invulnerable.Check(t, damage: Q.GetDamage(t))))
            {
                Q.Cast(target);
                break;
            }
        }

        public static void ExecuteE()
        {
            if (!KillstealMenu.EBool.Enabled)
            {
                return;
            }

            foreach (var target in GameObjects.EnemyHeroes.Where(t =>
                         t.IsValidTarget(E.Range)                              &&
                         E.GetDamage(t) >= t.Health &&
                         !Invulnerable.Check(t, damage: E.GetDamage(t))))
            {
                E.Cast(target);
                break;
            }
        }
    }
}