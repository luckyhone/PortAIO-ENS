using EnsoulSharp;
using EnsoulSharp.SDK;

namespace ExorAIO.Champions.Vayne
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
            Game.OnUpdate += Vayne.OnUpdate;
            AIBaseClient.OnDoCast += Vayne.OnSpellCast;
            AntiGapcloser.OnGapcloser += Vayne.OnGapCloser;
            Interrupter.OnInterrupterSpell += Vayne.OnInterruptableTarget;
            Orbwalker.OnBeforeAttack += Vayne.OnAction;
        }

        #endregion
    }
}