using EnsoulSharp;
using EnsoulSharp.SDK;

namespace ExorAIO.Champions.Twitch
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
            Vars.Q = new Spell(SpellSlot.Q);
            Vars.W = new Spell(SpellSlot.W, 950f);
            Vars.E = new Spell(SpellSlot.E, 1200f);
            Vars.R = new Spell(SpellSlot.R, GameObjects.Player.GetRealAutoAttackRange() + 300f);
            Vars.W.SetSkillshot(0.25f, 120f, 1400f, false, SpellType.Circle);
        }

        #endregion
    }
}