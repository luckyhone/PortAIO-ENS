using EnsoulSharp;
using EnsoulSharp.SDK;

namespace ExorAIO.Champions.Lucian
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
            Game.OnUpdate += Lucian.OnUpdate;
            AIBaseClient.OnProcessSpellCast += Lucian.OnSpellCast;
            AntiGapcloser.OnGapcloser += Lucian.OnGapCloser;
            AIBaseClient.OnPlayAnimation += Lucian.OnPlayAnimation;
            Orbwalker.OnBeforeAttack += Lucian.OnAction;
        }

        #endregion
    }
}