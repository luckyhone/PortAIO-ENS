using EnsoulSharp;
using EnsoulSharp.SDK;

namespace ExorAIO.Champions.Darius
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
            Game.OnUpdate += Darius.OnUpdate;
            AIBaseClient.OnDoCast += Darius.OnSpellCast;
            AntiGapcloser.OnGapcloser += Darius.OnGapCloser;
            Interrupter.OnInterrupterSpell += Darius.OnInterruptableTarget;
        }

        #endregion
    }
}