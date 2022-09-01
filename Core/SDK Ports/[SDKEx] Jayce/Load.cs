using EnsoulSharp;
using ExorAIO.Core;

namespace Jayce
{
    #region

    using System;



    #endregion

    /// <summary>
    ///     The Load.
    /// </summary>
    internal class Load
    {
        #region Methods

        /// <summary>
        /// The main.
        /// </summary>
        /// <param name="args">
        /// The args.
        /// </param>
        public static void Loads()
        {
            //Bootstrap(null);
            OnLoad();
        }

        /// <summary>
        /// The on load.
        /// </summary>
        /// <param name="sender">
        /// The sender.
        /// </param>
        /// <param name="eventArgs">
        /// The event args.
        /// </param>
        private static void OnLoad()
        {
            if (ObjectManager.Player.CharacterName == "Jayce")
            {
                Extensions.Events.Initialize();
                Extensions.Spells.Initialize();
                Extensions.Config.Initialize();
            }
        }

        #endregion
    }
}