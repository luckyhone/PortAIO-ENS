using EnsoulSharp;
using EnsoulSharp.SDK;

namespace ExorAIO.Champions.Caitlyn
{

    /// <summary>
    ///     The methods class.
    /// </summary>
    internal class Methods
    {
        #region Public Methods and Operators

        /// <summary>
        ///     Initializes the methods.
        /// </summary>
        public static void Initialize()
        {
            Game.OnUpdate += Caitlyn.OnUpdate;
            Spellbook.OnCastSpell += Caitlyn.OnCastSpell;
            AntiGapcloser.OnGapcloser += Caitlyn.OnGapCloser;
            Interrupter.OnInterrupterSpell += Caitlyn.OnInterruptableTarget;
            AIBaseClient.OnProcessSpellCast += Caitlyn.OnSpellCast;
        }

        #endregion
    }
}