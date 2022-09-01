#region

using System;
using System.Collections.Generic;
using System.Linq;
using EnsoulSharp;
using EnsoulSharp.SDK;
using EnsoulSharp.SDK.MenuUI;
using LeagueSharpCommon.Geometry;
using SharpDX;
using SPrediction;

#endregion

namespace Evade
{
    public enum CollisionObjectTypes
    {
        Minion,
        Champions,
        YasuoWall,
    }

    internal class FastPredResult
    {
        public Vector2 CurrentPos;
        public bool IsMoving;
        public Vector2 PredictedPos;
    }

    internal class DetectedCollision
    {
        public float Diff;
        public float Distance;
        public Vector2 Position;
        public CollisionObjectTypes Type;
        public AIBaseClient Unit;
    }

    internal static class Collision
    {
        private static int WallCastT;
        private static Vector2 YasuoWallCastedPos;

        public static void Init()
        {
            AIBaseClient.OnProcessSpellCast += Obj_AI_Hero_OnProcessSpellCast;
        }


        private static void Obj_AI_Hero_OnProcessSpellCast(AIBaseClient sender, AIBaseClientProcessSpellCastEventArgs args)
        {
            if (sender.IsValid && sender.Team == ObjectManager.Player.Team && args.SData.Name == "YasuoWMovingWall")

            {
                WallCastT = Utils.TickCount;
                YasuoWallCastedPos = sender.ServerPosition.To2D();
            }
        }

        public static FastPredResult FastPrediction(Vector2 from, AIBaseClient unit, int delay, int speed)
        {
            var tDelay = delay / 1000f + (from.Distance(unit) / speed);
            var d = tDelay * unit.MoveSpeed;
            var path = Vector2Extensions.GetWaypoints(unit);

            if (LeagueSharpCommon.Geometry.Geometry.PathLength(path) > d)
            {
                return new FastPredResult
                {
                    IsMoving = true,
                    CurrentPos = unit.ServerPosition.To2D(),
                    PredictedPos = path.CutPath((int) d)[0],
                };
            }
            return new FastPredResult
            {
                IsMoving = false,
                CurrentPos = path[path.Count - 1],
                PredictedPos = path[path.Count - 1],
            };
        }

        public static Vector2 GetCollisionPoint(Skillshot skillshot)
        {
            var collisions = new List<DetectedCollision>();
            var from = skillshot.GetMissilePosition(0);
            skillshot.ForceDisabled = false;
            foreach (var cObject in skillshot.SpellData.CollisionObjects)
            {
                switch (cObject)
                {
                    case CollisionObjectTypes.Minion:

                        if (!Config.Menu["Collision"]["MinionCollision"].GetValue<MenuBool>().Enabled)
                        {
                            break;
                        }
                        foreach (var minion in
                            MinionManager.GetMinions(
                                from.To3D(), 1200, MinionManager.MinionTypes.All,
                                skillshot.Unit.Team == ObjectManager.Player.Team
                                    ? MinionManager.MinionTeam.NotAlly
                                    : MinionManager.MinionTeam.NotAllyForEnemy))
                        {
                            var pred = FastPrediction(
                                from, minion,
                                Math.Max(0, skillshot.SpellData.Delay - (Utils.TickCount - skillshot.StartTick)),
                                skillshot.SpellData.MissileSpeed);
                            var pos = pred.PredictedPos;
                            var w = skillshot.SpellData.RawRadius + (!pred.IsMoving ? (minion.BoundingRadius - 15) : 0) -
                                    LeagueSharpCommon.Geometry.Geometry.Distance(pos, from, skillshot.End, true);
                            if (w > 0)
                            {
                                collisions.Add(
                                    new DetectedCollision
                                    {
                                        Position =
                                            LeagueSharpCommon.Geometry.Geometry.ProjectOn(pos, skillshot.End, skillshot.Start).LinePoint +
                                            skillshot.Direction * 30,
                                        Unit = minion,
                                        Type = CollisionObjectTypes.Minion,
                                        Distance = LeagueSharpCommon.Geometry.Geometry.Distance(pos, from),
                                        Diff = w,
                                    });
                            }
                        }

                        break;

                    case CollisionObjectTypes.Champions:
                        if (!Config.Menu["Collision"]["HeroCollision"].GetValue<MenuBool>().Enabled)
                        {
                            break;
                        }
                        foreach (var hero in
                            ObjectManager.Get<AIHeroClient>()
                                .Where(
                                    h =>
                                        (h.IsValidTarget(1200, false) && h.Team == ObjectManager.Player.Team && !h.IsMe ||
                                         Config.TestOnAllies && h.Team != ObjectManager.Player.Team)))
                        {
                            var pred = FastPrediction(
                                from, hero,
                                Math.Max(0, skillshot.SpellData.Delay - (Utils.TickCount - skillshot.StartTick)),
                                skillshot.SpellData.MissileSpeed);
                            var pos = pred.PredictedPos;

                            var w = skillshot.SpellData.RawRadius + 30 - LeagueSharpCommon.Geometry.Geometry.Distance(pos, from, skillshot.End, true);
                            if (w > 0)
                            {
                                collisions.Add(
                                    new DetectedCollision
                                    {
                                        Position =
                                            LeagueSharpCommon.Geometry.Geometry.ProjectOn(pos, skillshot.End, skillshot.Start).LinePoint +
                                            skillshot.Direction * 30,
                                        Unit = hero,
                                        Type = CollisionObjectTypes.Minion,
                                        Distance = LeagueSharpCommon.Geometry.Geometry.Distance(pos, from),
                                        Diff = w,
                                    });
                            }
                        }
                        break;

                    case CollisionObjectTypes.YasuoWall:
                        if (!Config.Menu["Collision"]["YasuoCollision"].GetValue<MenuBool>().Enabled)
                        {
                            break;
                        }
                        if (
                            !ObjectManager.Get<AIHeroClient>()
                                .Any(
                                    hero =>
                                        hero.IsValidTarget(float.MaxValue, false) &&
                                        hero.Team == ObjectManager.Player.Team && hero.CharacterName == "Yasuo"))
                        {
                            break;
                        }
                        GameObject wall = null;
                        foreach (var gameObject in ObjectManager.Get<GameObject>())
                        {
                            if (gameObject.IsValid &&
                                System.Text.RegularExpressions.Regex.IsMatch(
                                    gameObject.Name, "_w_windwall.\\.troy",
                                    System.Text.RegularExpressions.RegexOptions.IgnoreCase))
                            {
                                wall = gameObject;
                            }
                        }
                        if (wall == null)
                        {
                            break;
                        }
                        var level = wall.Name.Substring(wall.Name.Length - 6, 1);
                        var wallWidth = (300 + 50 * Convert.ToInt32(level));


                        var wallDirection = LeagueSharpCommon.Geometry.Geometry.Normalized((wall.Position.To2D() - YasuoWallCastedPos)).Perpendicular();
                        var wallStart = wall.Position.To2D() + wallWidth / 2 * wallDirection;
                        var wallEnd = wallStart - wallWidth * wallDirection;
                        var wallPolygon = new Geometry.Rectangle(wallStart, wallEnd, 75).ToPolygon();
                        var intersection = new Vector2();
                        var intersections = new List<Vector2>();

                        for (var i = 0; i < wallPolygon.Points.Count; i++)
                        {
                            var inter =
                                LeagueSharpCommon.Geometry.Geometry.Intersection(wallPolygon.Points[i], wallPolygon.Points[i != wallPolygon.Points.Count - 1 ? i + 1 : 0], from,
                                    skillshot.End);
                            if (inter.Intersects)
                            {
                                intersections.Add(inter.Point);
                            }
                        }

                        if (intersections.Count > 0)
                        {
                            intersection = intersections.OrderBy(item => LeagueSharpCommon.Geometry.Geometry.Distance(item, from)).ToList()[0];
                            var collisionT = Utils.TickCount +
                                             Math.Max(
                                                 0,
                                                 skillshot.SpellData.Delay -
                                                 (Utils.TickCount - skillshot.StartTick)) + 100 +
                                             (1000 * LeagueSharpCommon.Geometry.Geometry.Distance(intersection, from)) / skillshot.SpellData.MissileSpeed;
                            if (collisionT - WallCastT < 4000)
                            {
                                if (skillshot.SpellData.Type != SkillShotType.SkillshotMissileLine)
                                {
                                    skillshot.ForceDisabled = true;
                                }
                                return intersection;
                            }
                        }

                        break;
                }
            }

            Vector2 result;
            if (collisions.Count > 0)
            {
                result = collisions.OrderBy(c => c.Distance).ToList()[0].Position;
            }
            else
            {
                result = new Vector2();
            }

            return result;
        }
    }
}