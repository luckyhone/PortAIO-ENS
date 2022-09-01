using EnsoulSharp;
using EnsoulSharp.SDK;

namespace ExorAIO.Champions.Kalista
{

    /// <summary>
    ///     The methods class.
    /// </summary>
    internal class Methods
    {
        #region Public Methods and Operators

        /// <summary>
        ///     The methods.
        /// </summary>
        public static void Initialize()
        {
            Game.OnUpdate += Kalista.OnUpdate;
            Orbwalker.OnBeforeAttack += Kalista.OnAction;
            Orbwalker.OnNonKillableMinion += Kalista.OnNonKillableMinion;
        }

        #endregion
    }
}