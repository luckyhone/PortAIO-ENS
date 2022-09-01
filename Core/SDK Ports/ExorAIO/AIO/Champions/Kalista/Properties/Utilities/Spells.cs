using EnsoulSharp;
using EnsoulSharp.SDK;

namespace ExorAIO.Champions.Kalista
{
    using ExorAIO.Utilities;


    /// <summary>
    ///     The settings class.
    /// </summary>
    internal class Spells
    {
        #region Public Methods and Operators

        /// <summary>
        ///     Sets the spells.
        /// </summary>
        public static void Initialize()
        {
            Vars.Q = new Spell(SpellSlot.Q, 1150f);
            Vars.W = new Spell(SpellSlot.W, 5000f);
            Vars.E = new Spell(SpellSlot.E, 1000f);
            Vars.R = new Spell(SpellSlot.R, 1100f);
            Vars.Q.SetSkillshot(0.25f, 40f, 2400f, true, SpellType.Line);
        }

        #endregion
    }
}