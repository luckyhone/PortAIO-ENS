using EnsoulSharp;
using EnsoulSharp.SDK;

namespace ExorAIO.Champions.Twitch
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