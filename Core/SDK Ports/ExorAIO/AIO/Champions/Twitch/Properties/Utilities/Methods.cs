using EnsoulSharp;
using EnsoulSharp.SDK;

namespace ExorAIO.Champions.Twitch
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
            Game.OnUpdate += Twitch.OnUpdate;
            AIBaseClient.OnDoCast += Twitch.OnSpellCast;
            Spellbook.OnCastSpell += Twitch.OnCastSpell;
            Orbwalker.OnBeforeAttack += Twitch.OnAction;
        }

        #endregion
    }
}