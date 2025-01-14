﻿using EnsoulSharp;
using EnsoulSharp.SDK;

namespace Jayce.Modes
{
    #region

    using System.Linq;

    using static Extensions.Config;
    using static Extensions.Other;
    using static Extensions.Spells;


    #endregion

    /// <summary>
    ///     The kill steal.
    /// </summary>
    internal class KillSteal
    {
        #region Public Methods and Operators

        /// <summary>
        ///     The execute.
        /// </summary>
        public static void Execute()
        {
            if (HammerEKS.Enabled && HammerQKS.Enabled) CastQEHammer();
            if (RangeForm())
            {
                if (CannonQKS.Enabled) CastQRange();
                if (CannonEKS.Enabled && CannonQKS.Enabled) CastQERange();
            }
            else
            {
                if (HammerQKS.Enabled) CastQHammer();
                if (HammerEKS.Enabled) CastEHammer();
            }
        }

        #endregion

        #region Methods

        /// <summary>
        ///     The cast e hammer.
        /// </summary>
        private static void CastEHammer()
        {
            if (E1.IsReady())
            {
                var Enemies = GameObjects.EnemyHeroes.Where(x => (x != null) && x.IsValidTarget());

                foreach (var Enemy in Enemies.Where(x => x.IsValidTarget(E1.Range) && (HammerEDmg(x) > x.Health))) E1.Cast(Enemy);
            }
        }

        /// <summary>
        ///     The cast qe hammer.
        /// </summary>
        private static void CastQEHammer()
        {
            var Enemies = GameObjects.EnemyHeroes.Where(x => (x != null) && x.IsValidTarget());

            foreach (var Enemy in
                Enemies.Where(x => x.IsValidTarget(E1.Range) && (Q1.GetDamage(x) + HammerEDmg(x) > x.Health)))
                if (RangeForm())
                {
                    if (R.IsReady()) R.Cast();
                }
                else
                {
                    if (Q1.IsReady() && E1.IsReady())
                    {
                        Q1.Cast(Enemy);
                        if (E1.IsReady()) E1.Cast();
                    }
                }
        }

        /// <summary>
        ///     The cast QE range.
        /// </summary>
        private static void CastQERange()
        {
            if (QE.IsReady() && E.IsReady() && (ObjectManager.Player.Mana > Q.Instance.ManaCost + E.Instance.ManaCost))
            {
                var Enemies = GameObjects.EnemyHeroes.Where(x => (x != null) && x.IsValidTarget());

                foreach (var Enemy in Enemies.Where(x => x.IsValidTarget(QE.Range) && (CannonQEDmg(x) > x.Health)))
                {
                    var Predinction = Q.GetPrediction(Enemy);
                    if (Predinction.Hitchance >= HitChance.VeryHigh) Q.Cast(Predinction.CastPosition);
                }
            }
        }

        /// <summary>
        ///     The cast q hammer.
        /// </summary>
        private static void CastQHammer()
        {
            if (Q1.IsReady())
            {
                var Enemies = GameObjects.EnemyHeroes.Where(x => (x != null) && x.IsValidTarget());

                foreach (var Enemy in Enemies.Where(x => x.IsValidTarget(Q1.Range) && (Q1.GetDamage(x) > x.Health))) Q1.Cast(Enemy);
            }
        }

        /// <summary>
        ///     The cast Q range.
        /// </summary>
        private static void CastQRange()
        {
            if (Q.IsReady() && (!E.IsReady() || !CannonEKS.Enabled))
            {
                var Enemies = GameObjects.EnemyHeroes.Where(x => (x != null) && x.IsValidTarget());

                foreach (var Enemy in Enemies.Where(x => x.IsValidTarget(Q.Range) && (CannonQDmg(x) > x.Health)))
                {
                    var Predinction = Q.GetPrediction(Enemy);
                    if (Predinction.Hitchance >= HitChance.VeryHigh) Q.Cast(Predinction.CastPosition);
                }
            }
        }

        #endregion
    }
}