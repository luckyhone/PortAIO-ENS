using EnsoulSharp;
using EnsoulSharp.SDK;

namespace ExorAIO.Champions.Jhin
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
            Game.OnUpdate += Jhin.OnUpdate;
            AIBaseClient.OnDoCast += Jhin.OnSpellCast;
            AntiGapcloser.OnGapcloser += Jhin.OnGapCloser;
            AIBaseClient.OnDoCast += Jhin.OnProcessSpellCast;
            Orbwalker.OnBeforeAttack += Jhin.OnAction;
        }

        #endregion
    }
}