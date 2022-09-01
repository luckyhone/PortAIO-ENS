using EnsoulSharp;
using EnsoulSharp.SDK;

namespace ExorAIO.Champions.Jax
{
    using ExorAIO.Utilities;


    /// <summary>
    ///     The spells class.
    /// </summary>
    internal class Spells
    {
        #region Public Methods and Operators

        /// <summary>
        ///     Sets the spells.
        /// </summary>
        public static void Initialize()
        {
            Vars.Q = new Spell(SpellSlot.Q, 700f);
            Vars.W = new Spell(SpellSlot.W);
            Vars.E = new Spell(SpellSlot.E, GameObjects.Player.BoundingRadius * 2 + 187.5f);
            Vars.R = new Spell(SpellSlot.R);
        }

        #endregion
    }
}