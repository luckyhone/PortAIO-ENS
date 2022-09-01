
using System.Drawing;
using System.Linq;
using EnsoulSharp;
using EnsoulSharp.SDK.MenuUI;
using ExorAIO.Utilities;
using AttackUnitExtensions = EnsoulSharp.SDK.AttackUnitExtensions;
using Damage = EnsoulSharp.SDK.Damage;
using GameObjects = EnsoulSharp.SDK.GameObjects;

#pragma warning disable 1587

namespace ExorAIO.Champions.Kalista
{

    /// <summary>
    ///     The drawings class.
    /// </summary>
    internal class Healthbars
    {
        #region Public Methods and Operators

        /// <summary>
        ///     Loads the drawings.
        /// </summary>
        public static void Initialize()
        {
            Drawing.OnEndScene += delegate
                {
                    if (!Vars.E.IsReady() || !Vars.Menu["drawings"]["edmg"].GetValue<MenuBool>().Enabled)
                    {
                        return;
                    }

                    ObjectManager.Get<AIBaseClient>()
                        .Where(
                            h =>
                            AttackUnitExtensions.IsValidTarget(h) && Kalista.IsPerfectRendTarget(h)
                                                                  && (h is AIHeroClient || Vars.JungleList.Contains(h.Name)))
                        .ToList()
                        .ForEach(
                            unit =>
                                {
                                    var heroUnit = (AIHeroClient)unit;
                                    /// <summary>
                                    ///     Defines what HPBar Offsets it should display.
                                    /// </summary>
                                    var mobOffset =
                                        Drawings.JungleHpBarOffsetList.FirstOrDefault(
                                            x => x.BaseSkinName.Equals(unit.Name));

                                    var width = Vars.JungleList.Contains(unit.Name)
                                                    ? mobOffset?.Width ?? Drawings.SWidth
                                                    : Drawings.SWidth;
                                    var height = Vars.JungleList.Contains(unit.Name)
                                                     ? mobOffset?.Height ?? Drawings.SHeight
                                                     : Drawings.SHeight;
                                    var xOffset = Vars.JungleList.Contains(unit.Name)
                                                      ? mobOffset?.XOffset ?? Drawings.SxOffset(heroUnit)
                                                      : Drawings.SxOffset(heroUnit);
                                    var yOffset = Vars.JungleList.Contains(unit.Name)
                                                      ? mobOffset?.YOffset ?? Drawings.SyOffset(heroUnit)
                                                      : Drawings.SyOffset(heroUnit);
                                    var barPos = unit.HPBarPosition;
                                    {
                                        barPos.X += xOffset;
                                        barPos.Y += yOffset;
                                    }
                                    var drawEndXPos = barPos.X + width * (unit.HealthPercent / 100);
                                    var drawStartXPos = barPos.X
                                                        + (Vars.GetRealHealth(unit)
                                                           > (float) Damage.GetSpellDamage(GameObjects.Player, unit,
                                                               SpellSlot.E)
                                                           + (float)
                                                           Damage.GetSpellDamage(GameObjects.Player, unit,
                                                               SpellSlot.E)
                                                            ? width
                                                              * ((Vars.GetRealHealth(unit)
                                                                  - ((float)
                                                                      Damage.GetSpellDamage(GameObjects.Player,
                                                                          unit,
                                                                          SpellSlot.E))
                                                                  + (float)
                                                                  Damage.GetSpellDamage(GameObjects.Player,
                                                                      unit,
                                                                      SpellSlot.E))) : 0);
                                    Drawing.DrawLine(
                                        drawStartXPos,
                                        barPos.Y,
                                        drawEndXPos,
                                        barPos.Y,
                                        height,
                                        Vars.GetRealHealth(unit)
                                        < (float)Damage.GetSpellDamage(GameObjects.Player,unit, SpellSlot.E)
                                        + (float)Damage.GetSpellDamage(GameObjects.Player,unit, SpellSlot.E)
                                            ? Color.Blue
                                            : Color.Orange);
                                    Drawing.DrawLine(
                                        drawStartXPos,
                                        barPos.Y,
                                        drawStartXPos,
                                        barPos.Y + height + 1,
                                        1,
                                        Color.Lime);
                                });
                };
        }

        #endregion
    }
}