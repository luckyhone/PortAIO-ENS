using EnsoulSharp;
using EnsoulSharp.SDK;

namespace ExorAIO.Champions.Draven
{
    using ExorAIO.Utilities;


    /// <summary>
    ///     The spell class.
    /// </summary>
    internal class Spells
    {
        #region Public Methods and Operators

        /// <summary>
        ///     Sets the spells.
        /// </summary>
        public static void Initialize()
        {
            Vars.Q = new Spell(SpellSlot.Q);
            Vars.W = new Spell(SpellSlot.W);
            Vars.E = new Spell(SpellSlot.E, 1050f);
            Vars.R = new Spell(SpellSlot.R, 1500f);
            Vars.E.SetSkillshot(0.25f, 130f, 1400f, false, SpellType.Line);
            Vars.R.SetSkillshot(0.4f, 160f, 2000f, false, SpellType.Line);
        }

        #endregion
    }
}