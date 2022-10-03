using EnsoulSharp.SDK;
using EnsoulSharp.SDK.Utility;

namespace Entropy.AIO.Irelia.Logics
{
    #region

    using System.Linq;
    using static Components;
    using static Bases.ChampionBase;

    #endregion

    static class Killsteal
    {
        public static void ExecuteQ()
        {
            if (!KillstealMenu.QBool.Enabled)
            {
                return;
            }

            foreach (var target in GameObjects.EnemyHeroes.Where(t =>
                         t.IsValidTarget(Q.Range)                               &&
                         Q.GetDamage(t) >= t.Health &&
                         !Invulnerable.Check(t, damage: Q.GetDamage(t))))
            {
                Q.CastOnUnit(target);
                return;
            }
        }

        public static void ExecuteE()
        {
            if (!KillstealMenu.EBool.Enabled)
            {
                return;
            }

            foreach (var target in GameObjects.EnemyHeroes.Where(t =>
                         t.IsValidTarget(E.Range)                               &&
                         E.GetDamage(t) >= t.Health &&
                         !Invulnerable.Check(t, damage: E.GetDamage(t))))
            {
                OnInterruptable.useE(target);
                return;
            }
        }
    }
}