using System.Collections.Generic;
using System.Linq;
using EnsoulSharp;
using EnsoulSharp.SDK;
using ExorAIO.Utilities;

namespace ExorAIO.Champions.Kalista
{

    /// <summary>
    ///     The settings class.
    /// </summary>
    internal class Targets
    {
        #region Public Properties

        /// <summary>
        ///     The valid harassable heroes.
        /// </summary>
        public static List<AIHeroClient> Harass => GameObjects.EnemyHeroes.ToList().FindAll(Kalista.IsPerfectRendTarget);

        /// <summary>
        ///     The jungle minion targets.
        /// </summary>
        public static List<AIMinionClient> JungleMinions
            =>
                GameObjects.Jungle.Where(
                    m =>
                    m.IsValidTarget(Vars.E.Range)
                    && (!GameObjects.JungleSmall.Contains(m) || m.Name.Equals("Sru_Crab"))).ToList();

        /// <summary>
        ///     The minions target.
        /// </summary>
        public static List<AIMinionClient> Minions
            => GameObjects.EnemyMinions.Where(m => m.IsMinion() && m.IsValidTarget(Vars.E.Range)).ToList();

        /// <summary>
        ///     The main hero target.
        /// </summary>
        public static AIHeroClient Target => TargetSelector.GetTarget(Vars.E.Range, DamageType.Physical);

        #endregion
    }
}