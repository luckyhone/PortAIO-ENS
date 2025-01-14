using System;
using System.Collections.Generic;
using System.Linq;
using EnsoulSharp;
using EnsoulSharp.SDK;
using EnsoulSharp.SDK.MenuUI;
using EnsoulSharp.SDK.Rendering;
using ExorAIO.Utilities;
using SharpDX;
using Geometry = LeagueSharpCommon.Geometry.Geometry;
using Color = System.Drawing.Color;

namespace HikiCarry.Core.Utilities
{
    public static class Utilities
    {
        public static string[] HighChamps =
            {
                "Ahri", "Anivia", "Annie", "Ashe", "Azir", "Brand", "Caitlyn", "Cassiopeia", "Corki", "Draven",
                "Ezreal", "Graves", "Jinx", "Kalista", "Karma", "Karthus", "Katarina", "Kennen", "KogMaw", "Leblanc",
                "Lucian", "Lux", "Malzahar", "MasterYi", "MissFortune", "Orianna", "Quinn", "Sivir", "Syndra", "Talon",
                "Teemo", "Tristana", "TwistedFate", "Twitch", "Varus", "Vayne", "Veigar", "VelKoz", "Viktor", "Xerath",
                "Zed", "Ziggs","Kindred","Jhin"
            };

        public static string[] HitchanceNameArray = { "Low", "Medium", "High", "Very High", "Only Immobile" };
        public static HitChance[] HitchanceArray = { HitChance.Low, HitChance.Medium, HitChance.High, HitChance.VeryHigh, HitChance.Immobile };

        public static HitChance HikiChance(string menuName)
        {
            return HitchanceArray[Initializer.Config[menuName].GetValue<MenuList>().Index];
        }

        public static bool Enabled(string menuName)
        {
            return Initializer.Config[menuName].GetValue<MenuBool>().Enabled;
        }

        public static int Slider(string menuName)
        {
            return Initializer.Config[menuName].GetValue<MenuSlider>().Value;
        }

        public static void ECast(AIHeroClient enemy, Spell blinkspell)
        {
            var range = ObjectManager.Player.GetRealAutoAttackRange(enemy);
            var path = Geometry.CircleCircleIntersection(ObjectManager.Player.ServerPosition.To2D(),
                Prediction.GetPrediction(enemy, 0.25f).UnitPosition.To2D(), blinkspell.Range, range);

            if (path.Count() > 0)
            {
                var epos = path.MinOrDefault(x => x.Distance(Game.CursorPos));
                if (epos.ToVector3().IsUnderEnemyTurret() || epos.ToVector3().IsWall())
                {
                    return;
                }

                if (epos.ToVector3().CountEnemyHeroesInRange(blinkspell.Range - 100) > 0)
                {
                    return;
                }
                blinkspell.Cast(epos);
            }
            if (path.Count() == 0)
            {
                var epos = ObjectManager.Player.ServerPosition.Extend(enemy.ServerPosition, -blinkspell.Range);
                if (epos.IsUnderEnemyTurret() || epos.IsWall())
                {
                    return;
                }

                // no intersection or target to close
                blinkspell.Cast(ObjectManager.Player.ServerPosition.Extend(enemy.ServerPosition, -blinkspell.Range));
            }
        }

        public static void DrawCircle(
           Vector3 center,
           float radius,
           Color color,
           int thickness = 5,
           int quality = 30,
           bool onMinimap = false)
        {
            if (!onMinimap)
            {
                CircleRender.Draw(center, radius, color.ToSharpDxColor(), thickness);
                return;
            }

            var pointList = new List<Vector3>();
            for (var i = 0; i < quality; i++)
            {
                var angle = i * Math.PI * 2 / quality;
                pointList.Add(
                    new Vector3(
                        center.X + radius * (float)Math.Cos(angle),
                        center.Y + radius * (float)Math.Sin(angle),
                        center.Z));
            }

            for (var i = 0; i < pointList.Count; i++)
            {
                var a = pointList[i];
                var b = pointList[i == pointList.Count - 1 ? 0 : i + 1];

                var aonScreen = Drawing.WorldToMinimap(a);
                var bonScreen = Drawing.WorldToMinimap(b);

                Drawing.DrawLine(aonScreen.X, aonScreen.Y, bonScreen.X, bonScreen.Y, thickness, color);
            }
        }

        public static bool IsImmobile(AIBaseClient target)
        {
            return target.HasBuffOfType(BuffType.Slow)
                   || target.HasBuffOfType(BuffType.Charm)
                   || target.HasBuffOfType(BuffType.Fear) || target.HasBuffOfType(BuffType.Stun) || target.HasBuffOfType(BuffType.Taunt)
                   || target.HasBuffOfType(BuffType.Snare);
        }
        public static bool IsActive(this AIHeroClient unit, Spell spell)
        {
            return spell.Instance.Name == "JhinRShot";
        }
    }
}