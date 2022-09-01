using EnsoulSharp;
using EnsoulSharp.SDK;
using ExorAIO.Utilities;

namespace ExorAIO.Champions.Graves
{

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
            Vars.Q = new Spell(SpellSlot.Q, 800f);
            Vars.W = new Spell(SpellSlot.W, 900f);
            Vars.E = new Spell(SpellSlot.E, GameObjects.Player.GetRealAutoAttackRange() + 425f);
            Vars.R = new Spell(SpellSlot.R, 1050f);
            Vars.Q.SetSkillshot(0.25f, 40f, 3000f, false, SpellType.Line);
            Vars.W.SetSkillshot(0.25f, 250f, 1000f, false, SpellType.Circle);
            Vars.R.SetSkillshot(0.25f, 100f, 2100f, false, SpellType.Line);
        }

        #endregion
    }
}