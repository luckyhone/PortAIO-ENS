using EnsoulSharp;
using EnsoulSharp.SDK;

namespace ExorAIO.Champions.Jinx
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
            Vars.PowPow = new Spell(SpellSlot.Q, 525f + GameObjects.Player.BoundingRadius);
            Vars.Q = new Spell(
                SpellSlot.Q,
                Vars.PowPow.Range + 50f + 25f * GameObjects.Player.Spellbook.GetSpell(SpellSlot.Q).Level);
            Vars.W = new Spell(SpellSlot.W, 1450f);
            Vars.E = new Spell(SpellSlot.E, 900f);
            Vars.R = new Spell(SpellSlot.R, 1500f);
            Vars.W.SetSkillshot(0.6f, 85f, 3200f, true, SpellType.Line);
            Vars.E.SetSkillshot(1f, 100f, 1000f, false, SpellType.Circle);
            Vars.R.SetSkillshot(0.6f, 140f, 1700f, false, SpellType.Line);
        }

        #endregion
    }
}