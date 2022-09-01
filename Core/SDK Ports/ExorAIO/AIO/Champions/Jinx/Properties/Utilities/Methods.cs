using EnsoulSharp;
using EnsoulSharp.SDK;

namespace ExorAIO.Champions.Jinx
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
            Game.OnUpdate += Jinx.OnUpdate;
            AntiGapcloser.OnGapcloser += Jinx.OnGapCloser;
            Orbwalker.OnBeforeAttack += Jinx.OnAction;
        }

        #endregion
    }
}