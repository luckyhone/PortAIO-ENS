using EnsoulSharp;
using EnsoulSharp.SDK;
using EnsoulSharp.SDK.Utility;

namespace Entropy.AIO.Camille.Logics
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
            if (!Q.IsReady())
            {
                return;
            }

            if (!KillstealMenu.QBool.Enabled)
            {
                return;
            }

            var target = GameObjects.EnemyHeroes.FirstOrDefault(x => x.IsValidTarget(Q.Range)                               &&
                                                                     x.Health <= Q.GetDamage(x) &&
                                                                     !Invulnerable.Check(x, damage: Q.GetDamage(x)));
            if (target == null)
            {
                return;
            }

            Q.Cast();
            Orbwalker.ForceTarget = (target);
        }

        public static void ExecuteW()
        {
            if (!W.IsReady())
            {
                return;
            }

            if (!KillstealMenu.WBool.Enabled)
            {
                return;
            }

            var target = GameObjects.EnemyHeroes.FirstOrDefault(x => x.IsValidTarget(W.Range)                               &&
                                                                     x.Health <= W.GetDamage(x) &&
                                                                     !Invulnerable.Check(x, damage: W.GetDamage(x)));
            if (target == null)
            {
                return;
            }

            if (target.InAutoAttackRange(ObjectManager.Player))
            {
                return;
            }

            W.Cast(target.Position);
        }
    }
}