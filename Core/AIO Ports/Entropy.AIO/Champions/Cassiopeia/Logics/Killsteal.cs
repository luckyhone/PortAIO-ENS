using System.Linq;
using EnsoulSharp.SDK;
using EnsoulSharp.SDK.Utility;

namespace Entropy.AIO.Cassiopeia.Logics
{
    using Bases;
    static class Killsteal
    {
        public static void ExecuteR()
        {
            foreach (var target in GameObjects.EnemyHeroes.Where(t =>
                                                                     t.IsValidTarget(ChampionBase.R.Range) &&
                                                                     ChampionBase.R.GetDamage(t) >=
                                                                     t.Health &&
                                                                     !Invulnerable.Check(
                                                                         t,
                                                                         damage: ChampionBase.R.GetDamage(t))))
            {
                ChampionBase.R.Cast(target);
                break;
            }
        }

        public static void ExecuteE()
        {
            foreach (var target in GameObjects.EnemyHeroes.Where(t =>
                                                                     t.IsValidTarget(ChampionBase.E.Range) &&
                                                                     ChampionBase.E.GetDamage(t) >=
                                                                     t.Health &&
                                                                     !Invulnerable.Check(
                                                                         t,
                                                                         damage: ChampionBase.E.GetDamage(t))))
            {
                ChampionBase.E.Cast(target);
                break;
            }
        }

        public static void ExecuteQ()
        {
            foreach (var target in GameObjects.EnemyHeroes.Where(t =>
                                                                     t.IsValidTarget(ChampionBase.Q.Range) &&
                                                                     ChampionBase.Q.GetDamage(t) >=
                                                                     t.Health &&
                                                                     !Invulnerable.Check(
                                                                         t,
                                                                         damage: ChampionBase.Q.GetDamage(t))))
            {
                ChampionBase.Q.Cast(target);
                break;
            }
        }

        public static void ExecuteW()
        {
            foreach (var target in GameObjects.EnemyHeroes.Where(t =>
                                                                     t.IsValidTarget(ChampionBase.W.Range) &&
                                                                     ChampionBase.W.GetDamage(t) >=
                                                                     t.Health &&
                                                                     !Invulnerable.Check(
                                                                         t,
                                                                         damage: ChampionBase.W.GetDamage(t))))
            {
                ChampionBase.W.Cast(target);
                break;
            }
        }
    }
}