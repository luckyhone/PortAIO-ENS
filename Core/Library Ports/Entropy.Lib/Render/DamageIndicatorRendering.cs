using EnsoulSharp;
using EnsoulSharp.SDK;
using PortAIO.Library_Ports.Entropy.Lib.Constants;

namespace Entropy.Lib.Render
{
    using System;
    using System.Collections.Generic;
    using System.Linq;
    using SharpDX;

    public static class DamageIndicatorRendering
    {
        #region Fields

        private static readonly List<JungleHpBarOffset> JungleHpBarOffsetList = new List<JungleHpBarOffset>
        {
            new JungleHpBarOffset {UnitSkinName = "SRU_Dragon_Air", Width   = 142, Height = 12, XOffset = -73, YOffset = -17},
            new JungleHpBarOffset {UnitSkinName = "SRU_Dragon_Fire", Width  = 142, Height = 12, XOffset = -73, YOffset = -17},
            new JungleHpBarOffset {UnitSkinName = "SRU_Dragon_Water", Width = 142, Height = 12, XOffset = -73, YOffset = -17},
            new JungleHpBarOffset {UnitSkinName = "SRU_Dragon_Earth", Width = 142, Height = 12, XOffset = -73, YOffset = -17},
            new JungleHpBarOffset {UnitSkinName = "SRU_Dragon_Elder", Width = 142, Height = 12, XOffset = -73, YOffset = -17},

            new JungleHpBarOffset {UnitSkinName = "SRU_Baron", Width = 165, Height = 13, XOffset = -80, YOffset = -22},

            new JungleHpBarOffset {UnitSkinName = "SLIME_RiftHerald", Width = 142, Height = 10, XOffset = -73, YOffset = -17},

            new JungleHpBarOffset {UnitSkinName = "SRU_RiftHerald", Width = 142, Height = 10, XOffset = -73, YOffset = -17},

            new JungleHpBarOffset {UnitSkinName = "SLIME_Red", Width  = 142, Height = 10, XOffset = -73, YOffset = -15},
            new JungleHpBarOffset {UnitSkinName = "SLIME_Blue", Width = 142, Height = 10, XOffset = -73, YOffset = -15},

            new JungleHpBarOffset {UnitSkinName = "SRU_Red", Width  = 142, Height = 10, XOffset = -73, YOffset = -15},
            new JungleHpBarOffset {UnitSkinName = "SRU_Blue", Width = 142, Height = 10, XOffset = -73, YOffset = -15},

            new JungleHpBarOffset {UnitSkinName = "SLIME_Gromp", Width     = 90, Height  = 5, XOffset  = -46, YOffset = -5},
            new JungleHpBarOffset {UnitSkinName = "SLIME_Crab", Width      = 142, Height = 12, XOffset = -73, YOffset = -17},
            new JungleHpBarOffset {UnitSkinName = "SLIME_Razorbeak", Width = 90, Height  = 5, XOffset  = -46, YOffset = -5},
            new JungleHpBarOffset {UnitSkinName = "SLIME_Murkwolf", Width  = 90, Height  = 5, XOffset  = -46, YOffset = -5},

            new JungleHpBarOffset {UnitSkinName = "SRU_Gromp", Width     = 90, Height  = 5, XOffset  = -46, YOffset = -5},
            new JungleHpBarOffset {UnitSkinName = "Sru_Crab", Width      = 142, Height = 12, XOffset = -73, YOffset = -17},
            new JungleHpBarOffset {UnitSkinName = "SRU_Krug", Width      = 90, Height  = 5, XOffset  = -46, YOffset = -5},
            new JungleHpBarOffset {UnitSkinName = "SRU_Razorbeak", Width = 90, Height  = 5, XOffset  = -46, YOffset = -5},
            new JungleHpBarOffset {UnitSkinName = "SRU_Murkwolf", Width  = 90, Height  = 5, XOffset  = -46, YOffset = -5},

            new JungleHpBarOffset {UnitSkinName = "SLIME_MurkwolfMini", Width  = 90, Height = 5, XOffset = -46, YOffset = -5},
            new JungleHpBarOffset {UnitSkinName = "SLIME_RazorbeakMini", Width = 60, Height = 5, XOffset = -31, YOffset = -5},

            new JungleHpBarOffset {UnitSkinName = "SRU_MurkwolfMini", Width  = 90, Height = 5, XOffset = -46, YOffset = -5},
            new JungleHpBarOffset {UnitSkinName = "SRU_RazorbeakMini", Width = 60, Height = 5, XOffset = -31, YOffset = -5},
            new JungleHpBarOffset {UnitSkinName = "SRU_KrugMini", Width      = 60, Height = 5, XOffset = -31, YOffset = -5}
        };

        #endregion

        #region Offsets

        private static float Height = 12;
        private static float Width = 105;
        private static int XOffset = -46;
        private static int YOffset = -18;
        private static bool AdjustedHealthbars = false;

        #endregion

        #region Methods and Operators

        private static readonly string[] DefaultMinionsToRender =
                    ObjectNames.AllJungleMinionsNames;

        private static Vector2 GetSize(AIBaseClient target)
        {
            if (!AdjustedHealthbars)
            {
                AdjustedHealthbars = true;
                if (Drawing.Height > 1100)
                {
                    var Y = Drawing.Height;
                    Width = 0.0875f * Y;
                    Height = 0.00875f * Y + 1f;
                    XOffset = (int)(-0.0375f * Y);
                    YOffset = (int)(-0.019375f * Y + Height * 0.5f - 1f);
                }
            }
            var width = Width;
            var height = Height;

            if (DefaultMinionsToRender.Contains(target.Name.ToLower()))
            {
                var mobOffset = JungleHpBarOffsetList.FirstOrDefault(x => x.UnitSkinName.Equals(target.Name));
                if (mobOffset == null)
                {
                    return new Vector2(width, height);
                }

                width = mobOffset.Width;
                height = mobOffset.Height;
            }
            else if (target.Name.Contains("Minion"))
            {
                width = Math.Max(60f, Drawing.Height * 0.05f + 1f);

                if (Drawing.Height >= 1900)
                    height = 8;
                else if (Drawing.Height >= 1700)
                    height = 7;
                else if (Drawing.Height >= 1500)
                    height = 6;
                else
                    height = 5;
            }

            return new Vector2(width, height);
        }

        private static Vector2 GetOffset(AIBaseClient target)
        {
            var xOffset = XOffset;
            var yOffset = YOffset;

            if (DefaultMinionsToRender.Contains(target.Name.ToLower()))
            {
                var mobOffset = JungleHpBarOffsetList.FirstOrDefault(x => x.UnitSkinName.Equals(target.Name));
                if (mobOffset == null)
                {
                    return new Vector2(xOffset, yOffset);
                }

                xOffset = mobOffset.XOffset;
                yOffset = mobOffset.YOffset;
            }
            else if (target.Name.Contains("Minion"))
            {
                xOffset = (int)-Math.Max(31f, Drawing.Height * 0.0255f + 1f);

                if (Drawing.Height > 2000)
                    yOffset = -7;
                else if (Drawing.Height >= 1800)
                    yOffset = -6;
                else if (Drawing.Height >= 1600)
                    yOffset = -5;
                else
                    yOffset = -4;
            }

            return new Vector2(xOffset, yOffset);
        }

        private static Vector2 EndPosition(AIBaseClient target, float dmg)
        {
            var size = GetSize(target);
            var length = GetHealthPercent(target, dmg) * size.X;
            var infoBarPosition = target.HPBarPosition;
            var offset = GetOffset(target);

            return new Vector2(infoBarPosition.X + length + offset.X, infoBarPosition.Y + offset.Y);
        }

        private static float GetHealthPercent(AttackableUnit target, float dmg)
        {
            return (target.Health - dmg > 0 ? target.Health - dmg : 0) / target.MaxHealth;
        }

        private static readonly Color Green = new Color(51, 255, 153, 180);
        private static readonly Color Orange = new Color(255, 141, 35, 180);


        private static Color aaa = new ColorBGRA(0, 0, 0, 255);
        public static void Render(AIBaseClient target, float damage, DamageType type, Color forcedColor , bool heal = false)
        {
            try
            {
                if (target == null || !target.IsValid || damage <= 0)
                {
                    return;
                }

                var from = EndPosition(target, 0);
                var to = heal ? EndPosition(target, -damage) : EndPosition(target, damage);
                if (heal && target.Health + damage >= target.MaxHealth)
                {
                    to = EndPosition(target, 0);
                }

                var size = GetSize(target);

                if (from.IsZero || to.IsZero)
                {
                    return;
                }

                var healthToRender = target.AllShield;
                switch (type)
                {
                    case DamageType.Physical:
                        healthToRender += target.PhysicalShield;
                        break;
                    case DamageType.Magical:
                        healthToRender += target.MagicalShield;
                        break;
                    case DamageType.Mixed:
                        healthToRender += target.AllShield;
                        break;
                }


                Color color;
                if (forcedColor.A != 0)
                {
                    color = forcedColor;
                }
                else
                {
                    color = damage > healthToRender
                        ? Green   // Green
                        : Orange; // Orange
                }


                LineRendering.Render(color, size.Y, from, to);

                // USE THIS TO DEBUG HEALTHBAR LENGTH.
                //var startOffset = GetOffset(target) + target.InfoBarPosition.X;
                //LineRendering.Render(new Color(255, 0, 255, 255), size.Y, new Vector2(startOffset.X, from.Y), new Vector2(startOffset.X + size.X, to.Y));
            }
            catch (Exception e)
            {
                Console.WriteLine("DamageIndicatorRendering.cs Failed To Run Render");
            }
        }

        #endregion
    }

    class JungleHpBarOffset
    {
        internal float Height;

        internal string UnitSkinName;

        internal int Width;

        internal int XOffset;

        internal int YOffset;
    }
}