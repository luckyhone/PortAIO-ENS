using EnsoulSharp;
using EnsoulSharp.SDK.Utility;

namespace ExorAIO.Champions.Cassiopeia
{

    /// <summary>
    ///     The logics class.
    /// </summary>
    internal partial class Logics
    {
        #region Public Methods and Operators

        /// <summary>
        ///     Called on do-cast.
        /// </summary>
        /// <param name="sender">The sender.</param>
        /// <param name="args">The <see cref="GameObjectProcessSpellCastEventArgs" /> instance containing the event data.</param>
        public static void Weaving(AIBaseClient sender, AIBaseClientProcessSpellCastEventArgs args)
        {
            if (!(args.Target is AIHeroClient) || Invulnerable.Check((AIHeroClient)args.Target))
            {
            }
        }

        #endregion
    }
}