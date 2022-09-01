using EnsoulSharp;
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
    ///     The lane clear.
    /// </summary>
    internal class LaneClear
    {
        #region Public Methods and Operators

        /// <summary>
        ///     The execute.
        /// </summary>
        public static void Execute()
        {
            if ((ObjectManager.Player.Mana > LaneMana.Value) && LaneMana.Enabled)
                if (RangeForm())
                {
                    if (LaneCannonQ.Enabled) CastQRange();
                    CastQERange();
                }
                else
                {
                    if (LaneHammerW.Enabled) CastWMelee();
                }
            else if (!LaneMana.Enabled)
                if (RangeForm())
                {
                    if (LaneCannonQ.Enabled) CastQRange();
                    CastQERange();
                }
                else
                {
                    if (LaneHammerW.Enabled) CastWMelee();
                }
        }

        #endregion

        #region Methods

        /// <summary>
        ///     The cast Q range.
        /// </summary>
        private static void CastQERange()
        {
            var Minions = GameObjects.EnemyMinions.Where(x => x.IsValidTarget(QE.Range)).ToList();
            var FarmPos = QE.GetCircularFarmLocation(Minions, QE.Width);
            var MinHit = FarmPos.MinionsHit;

            if (QE.IsReady() && (ObjectManager.Player.Mana > Q.Instance.ManaCost + E.Instance.ManaCost)) if (MinHit >= LaneCannonQHit.Value) QE.Cast(FarmPos.Position);
        }

        /// <summary>
        ///     The cast Q range.
        /// </summary>
        private static void CastQRange()
        {
            var Minions = GameObjects.EnemyMinions.Where(x => x.IsValidTarget(Q.Range)).ToList();
            var FarmPos = Q.GetCircularFarmLocation(Minions, Q.Width);
            var MinHit = FarmPos.MinionsHit;

            if (Q.IsReady() && (!E.IsReady() || !LaneCannonE.Enabled)) if (MinHit >= LaneCannonQHit.Value) Q.Cast(FarmPos.Position);
        }

        /// <summary>
        ///     The cast W melee.
        /// </summary>
        private static void CastWMelee()
        {
            var Minions = GameObjects.EnemyMinions.Where(x => x.IsValidTarget(W1.Range)).ToList();
            var FarmPos = W1.GetCircularFarmLocation(Minions, 300);
            var MinHit = FarmPos.MinionsHit;

            if (W1.IsReady()) if (MinHit >= LaneHammerWHit.Value) W1.Cast();
        }

        #endregion
    }
}