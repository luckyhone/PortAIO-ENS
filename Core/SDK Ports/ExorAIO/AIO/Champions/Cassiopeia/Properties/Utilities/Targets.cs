using EnsoulSharp;
using EnsoulSharp.SDK;

namespace ExorAIO.Champions.Cassiopeia
{
    using System.Collections.Generic;
    using System.Linq;

    using ExorAIO.Utilities;


    /// <summary>
    ///     The targets class.
    /// </summary>
    internal class Targets
    {
        #region Public Properties

        /// <summary>
        ///     The jungle minion targets.
        /// </summary>
        public static List<AIMinionClient> JungleMinions
            =>
                GameObjects.Jungle.Where(
                    m =>
                        m.IsValidTarget(Vars.W.Range)
                        && (!GameObjects.JungleSmall.Contains(m) || m.Name.Equals("Sru_Crab"))).ToList();

        /// <summary>
        ///     The minions target.
        /// </summary>
        public static List<AIMinionClient> Minions
            => GameObjects.EnemyMinions.Where(m => m.IsMinion() && m.IsValidTarget(Vars.W.Range)).ToList();

        /// <summary>
        ///     The ultimate targets.
        /// </summary>
        public static List<AIHeroClient> RTargets
            =>
                GameObjects.EnemyHeroes.Where(
                    t => t.IsValidTarget(Vars.R.Range - 100f) && t.IsFacing(GameObjects.Player)).ToList();

        /// <summary>
        ///     The main hero target.
        /// </summary>
        public static AIHeroClient Target => TargetSelector.GetTarget(Vars.W.Range, DamageType.Magical);

        #endregion
    }
}