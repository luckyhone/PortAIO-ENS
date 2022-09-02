using EnsoulSharp;
using EnsoulSharp.SDK;

namespace ExorAIO.Champions.Jax
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
            Game.OnUpdate += Jax.OnUpdate;
            AIBaseClient.OnDoCast += Jax.OnSpellCast;
            AntiGapcloser.OnGapcloser += Jax.OnGapCloser;
        }

        #endregion
    }
}