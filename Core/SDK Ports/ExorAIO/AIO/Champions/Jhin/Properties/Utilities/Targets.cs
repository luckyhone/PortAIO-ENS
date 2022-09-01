using EnsoulSharp;
using EnsoulSharp.SDK;
using EnsoulSharp.SDK.MenuUI;
using EnsoulSharp.SDK.Utility;

namespace ExorAIO.Champions.Jhin
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
                        m.IsValidTarget(Vars.Q.Range)
                        && (!GameObjects.JungleSmall.Contains(m) || m.Name.Equals("Sru_Crab"))).ToList();

        /// <summary>
        ///     The minions target.
        /// </summary>
        public static List<AIMinionClient> Minions
            => GameObjects.EnemyMinions.Where(m => m.IsMinion() && m.IsValidTarget(Vars.W.Range)).ToList();

        /// <summary>
        ///     The R targets.
        /// </summary>
        public static List<AIHeroClient> RTargets
            =>
                GameObjects.EnemyHeroes.Where(
                        t =>
                            t.IsValidTarget(Vars.R.Range) && GameObjects.Player.IsFacing(t)
                                                          && !Invulnerable.Check(t, DamageType.Magical, false)
                                                          && Vars.Menu["spells"]["r"]["whitelist"][t.CharacterName.ToLower()].GetValue<MenuBool>().Enabled)
                    .ToList();

        /// <summary>
        ///     The main hero target.
        /// </summary>
        public static AIHeroClient Target => TargetSelector.GetTarget(Vars.R.Range, DamageType.Physical);

        #endregion
    }
}