﻿using EnsoulSharp;
using EnsoulSharp.SDK;

namespace ExorAIO.Champions.Vayne
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
            Vars.Q = new Spell(SpellSlot.Q, GameObjects.Player.GetRealAutoAttackRange() + 300f);
            Vars.W = new Spell(SpellSlot.W);
            Vars.E = new Spell(SpellSlot.E, 550f + GameObjects.Player.BoundingRadius);
            Vars.E2 = new Spell(SpellSlot.E, 550f + GameObjects.Player.BoundingRadius);
            Vars.R = new Spell(SpellSlot.R);
            Vars.E.SetSkillshot(0.45f, 50f, 1000f, false, SpellType.Line);
            Vars.E2.SetSkillshot(0.65f, 50f, 1000f, false, SpellType.Line);
        }

        #endregion
    }
}