using EnsoulSharp;
using EnsoulSharp.SDK;

namespace ExorAIO.Champions.Cassiopeia
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
            Game.OnUpdate += Cassiopeia.OnUpdate;
            AntiGapcloser.OnGapcloser += Cassiopeia.OnGapCloser;
            Spellbook.OnCastSpell += Cassiopeia.OnCastSpell;
            Interrupter.OnInterrupterSpell += Cassiopeia.OnInterruptableTarget;
            Orbwalker.OnBeforeAttack += Cassiopeia.OnAction;
        }

        #endregion
    }
}