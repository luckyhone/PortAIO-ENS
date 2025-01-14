﻿using EnsoulSharp;
using EnsoulSharp.SDK;

namespace ExorAIO.Champions.Jhin
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
            Vars.Q = new Spell(SpellSlot.Q, 600f);
            Vars.W = new Spell(SpellSlot.W, 2500f);
            Vars.E = new Spell(SpellSlot.E, 750f);
            Vars.R = new Spell(SpellSlot.R, 3500f);
            Vars.W.SetSkillshot(0.75f, 40f, 5000f, false, SpellType.Line);
            Vars.E.SetSkillshot(1f, 260f, 1000f, false, SpellType.Circle);
            Vars.R.SetSkillshot(0.25f, 80f, 5000f, false, SpellType.Line);
        }

        #endregion
    }
}