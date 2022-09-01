using EnsoulSharp;
using EnsoulSharp.SDK;

namespace ExorAIO.Champions.Taliyah
{
    using ExorAIO.Utilities;


    /// <summary>
    ///     The spell class.
    /// </summary>
    internal class Spells
    {
        #region Public Methods and Operators

        /// <summary>
        ///     Initializes the spells.
        /// </summary>
        public static void Initialize()
        {
            Vars.Q = new Spell(SpellSlot.Q, 1000f);
            Vars.W = new Spell(SpellSlot.W, 900f);
            Vars.E = new Spell(SpellSlot.E, 800f);
            Vars.R = new Spell(SpellSlot.R, 1500 + 1500 * GameObjects.Player.Spellbook.GetSpell(SpellSlot.R).Level);
            Vars.Q.SetSkillshot(0.275f, 100f, 3600f, true, SpellType.Line);
            Vars.W.SetSkillshot(1f, 200f, float.MaxValue, false, SpellType.Circle);
            Vars.E.SetSkillshot(0.30f, 450f, float.MaxValue, false, SpellType.Cone);
        }

        #endregion
    }
}