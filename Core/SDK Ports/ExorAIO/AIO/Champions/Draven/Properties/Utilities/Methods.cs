using EnsoulSharp;
using EnsoulSharp.SDK;

namespace ExorAIO.Champions.Draven
{

    /// <summary>
    ///     The methods class.
    /// </summary>
    internal class Methods
    {
        #region Public Methods and Operators

        /// <summary>
        ///     Sets the methods.
        /// </summary>
        public static void Initialize()
        {
            Game.OnUpdate += Draven.OnUpdate;
            AntiGapcloser.OnGapcloser += Draven.OnGapCloser;
            Interrupter.OnInterrupterSpell += Draven.OnInterruptableTarget;
        }

        #endregion
    }
}