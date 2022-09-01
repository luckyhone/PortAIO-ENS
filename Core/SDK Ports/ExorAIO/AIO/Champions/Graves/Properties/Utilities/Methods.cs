using EnsoulSharp;
using EnsoulSharp.SDK;

namespace ExorAIO.Champions.Graves
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
            Game.OnUpdate += Graves.OnUpdate;
            AIBaseClient.OnProcessSpellCast += Graves.OnSpellCast;
            AntiGapcloser.OnGapcloser += Graves.OnGapCloser;
        }

        #endregion
    }
}