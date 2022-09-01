
using EnsoulSharp;
using EnsoulSharp.SDK;
using EnsoulSharp.SDK.MenuUI;
using EnsoulSharp.SDK.Utility;
using ExorAIO.Utilities;

#pragma warning disable 1587

namespace ExorAIO.Champions.Graves
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
            var target = args.Target as AIHeroClient;
            if (target == null || Invulnerable.Check(target))
            {
                return;
            }

            /// <summary>
            ///     The E Combo Weaving Logic.
            /// </summary>
            if (Vars.E.IsReady() && Vars.Menu["spells"]["e"]["combo"].GetValue<MenuBool>().Enabled)
            {
                Vars.E.Cast(GameObjects.Player.ServerPosition.Extend(Game.CursorPos, GameObjects.Player.BoundingRadius));
                return;
            }

            /// <summary>
            ///     The W Combo Weaving Logic.
            /// </summary>
            if (Vars.W.IsReady() && Vars.Menu["spells"]["w"]["combo"].GetValue<MenuBool>().Enabled)
            {
                Vars.W.Cast(Vars.W.GetPrediction(target).CastPosition);
            }
        }

        #endregion
    }
}