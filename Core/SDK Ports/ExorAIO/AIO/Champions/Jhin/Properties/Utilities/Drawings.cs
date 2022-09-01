using EnsoulSharp;
using EnsoulSharp.SDK;
using EnsoulSharp.SDK.MenuUI;

#pragma warning disable 1587

namespace ExorAIO.Champions.Jhin
{
    using System.Linq;

    using ExorAIO.Utilities;


    using SharpDX;

    using Color = System.Drawing.Color;

    /// <summary>
    ///     The prediction drawings class.
    /// </summary>
    internal class ConeDrawings
    {
        #region Public Methods and Operators

        /// <summary>
        ///     Loads the range drawings.
        /// </summary>
        public static void Initialize()
        {
            if (GameObjects.Player.IsDead)
            {
                return;
            }

            Drawing.OnDraw += delegate
            {
                /// <summary>
                ///     Loads the R Cone drawing.
                /// </summary>
                if (Jhin.End != Vector3.Zero && Vars.R.Instance.Name.Equals("JhinRShot")
                                             && Vars.Menu["drawings"]["rc"].GetValue<MenuBool>().Enabled)
                {
                    Jhin.Cone.Draw(
                        GameObjects.EnemyHeroes.Any(
                            t => t.IsValidTarget() && !Jhin.Cone.IsOutside((Vector2)t.ServerPosition))
                            ? Color.Green
                            : Color.Red);
                }
            };
        }

        #endregion
    }
}