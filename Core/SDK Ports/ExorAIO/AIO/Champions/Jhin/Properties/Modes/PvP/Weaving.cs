﻿using EnsoulSharp;
using EnsoulSharp.SDK;
using EnsoulSharp.SDK.MenuUI;
using EnsoulSharp.SDK.Utility;

#pragma warning disable 1587

namespace ExorAIO.Champions.Jhin
{
    using ExorAIO.Utilities;


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
                return;
            }

            /// <summary>
            ///     The Q Weaving Logic.
            /// </summary>
            if (Vars.Q.IsReady() && ((AIHeroClient)args.Target).IsValidTarget(Vars.Q.Range)
                                 && Vars.Menu["spells"]["q"]["combo"].GetValue<MenuBool>().Enabled
                                 && !Vars.Menu["spells"]["q"]["reloadcombo"].GetValue<MenuBool>().Enabled)
            {
                Vars.Q.CastOnUnit((AIHeroClient)args.Target);
            }
        }

        #endregion
    }
}