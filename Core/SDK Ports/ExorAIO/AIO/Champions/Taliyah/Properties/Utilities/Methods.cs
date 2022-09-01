using EnsoulSharp;
using EnsoulSharp.SDK;

namespace ExorAIO.Champions.Taliyah
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
            Game.OnUpdate += Taliyah.OnUpdate;
            GameObject.OnCreate += Taliyah.OnCreate;
            GameObject.OnDelete += Taliyah.OnDelete;
            AIBaseClient.OnProcessSpellCast += Taliyah.OnProcessSpellCast;
            AntiGapcloser.OnGapcloser += Taliyah.OnGapCloser;
            Interrupter.OnInterrupterSpell += Taliyah.OnInterruptableTarget;
        }

        #endregion
    }
}