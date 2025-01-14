using System;
using System.Collections.Generic;
using System.Linq;
using EnsoulSharp;
using EnsoulSharp.SDK;
using EnsoulSharp.SDK.MenuUI;
using ExorAIO.Utilities;
using LeagueSharpCommon;
using SharpDX;
using Dash = EnsoulSharp.SDK.Dash;
using Geometry = LeagueSharpCommon.Geometry.Geometry;

namespace hi_im_gosu_Reborn
{
    class Condemn
    {
        public static void Run()
            {
            foreach (var enemy in GameObjects.EnemyHeroes.Where(x =>x.IsValidTarget(Vayne.E.Range) && !x.HasBuffOfType(BuffType.SpellShield) && !x.HasBuffOfType(BuffType.SpellImmunity) && Condemn.canBeCondemned(x)))

                Vayne.E.CastOnUnit(enemy);

        }
       

        public static long Check;
        public static List<Vector2> Points = new List<Vector2>();
        private static SpellSlot FlashSlot;

        public static bool canBeCondemned(AIHeroClient unit, Vector2 pos = new Vector2())
        {
            if (unit.HasBuffOfType(BuffType.SpellImmunity) || unit.HasBuffOfType(BuffType.SpellShield) || Check + 50 > Environment.TickCount || Dash.IsDashing(ObjectManager.Player)) return false;        
            var pred = pos.IsValid() ? new List<Vector3>() { pos.ToVector3() } : new List<Vector3>
                        {
                            unit.ServerPosition,
                            unit.Position,
                          Vayne.E.GetPrediction(unit).CastPosition,
                          Vayne.E.GetPrediction(unit).UnitPosition
                        };

            var walls = 0;
            Points = new List<Vector2>();
            foreach (var position in pred)
            {
                for (var i = 0; i < Vayne.emenu["PushDistance"].GetValue<MenuSlider>().Value; i += (int)unit.BoundingRadius)
                {
                    var Pos = ObjectManager.Player.Position.Extend(position, ObjectManager.Player.Distance(position) + i).ToVector2();
                    Points.Add(Pos);
                    if (NavMesh.GetCollisionFlags(Pos.ToVector3()).
                        HasFlag(CollisionFlags.Wall) || NavMesh.GetCollisionFlags(Pos.ToVector3()).HasFlag(CollisionFlags.Building))
                    {
                        walls++;
                        break;
                    }
                }
            }
            if ((walls / pred.Count) >= 33 / 100f)
            {
                return true;
            }
            return false;
        }
        public static void FlashE()
        {
            if (!Vayne.emenu["FlashE"].GetValue<MenuKeyBind>().Active) return;

            var positions = GetRotatedFlashPositions();

            foreach (var p in positions)
            {
                var condemnUnit = CondemnCheck(p);
                if (condemnUnit != null)
                {
                    Vayne.E.Cast(condemnUnit);

                    ObjectManager.Player.Spellbook.CastSpell(FlashSlot, p);

                }
            }
        }

        public static int CountHerosInRange(AIHeroClient target, bool checkteam, float range = 1200f)
        {
            var objListTeam =
                ObjectManager.Get<AIHeroClient>()
                    .Where(
                        x => x.IsValidTarget(range));

            return objListTeam.Count(hero => checkteam ? hero.Team != target.Team : hero.Team == target.Team);
        }

        public static Vector3 GetFirstNonWallPos(Vector3 startPos, Vector3 endPos)
        {
            int distance = 0;
            for (int i = 0; i < Vayne.emenu["PushDistance"].GetValue<MenuSlider>().Value; i += 20)
            {
                var cell = startPos.Extend(endPos, endPos.Distance(startPos) + i);
                if (NavMesh.GetCollisionFlags(cell).HasFlag(CollisionFlags.Wall) ||
                    NavMesh.GetCollisionFlags(cell).HasFlag(CollisionFlags.Building))
                {
                    distance = i - 20;
                }
            }
            return startPos.Extend(endPos, distance + endPos.Distance(startPos));
        }

        public static List<Vector3> GetRotatedFlashPositions()
        {
            const int currentStep = 30;
            var direction = ObjectManager.Player.Direction.To2D().Perpendicular();

            var list = new List<Vector3>();
            for (var i = -90; i <= 90; i += currentStep)
            {
                var angleRad = Geometry.DegreeToRadian(i);
                var rotatedPosition = ObjectManager.Player.Position.To2D() + (425f * direction.Rotated(angleRad));
                list.Add(rotatedPosition.ToVector3());
            }
            return list;
        }
        public static void LoadFlash()
        {
            var testSlot = ObjectManager.Player.GetSpellSlot("SummonerFlash");
            if (testSlot != SpellSlot.Unknown)
            {
                Console.WriteLine("Flash Slot: {0}", testSlot);
                FlashSlot = testSlot;
            }
            else
            {
                Console.WriteLine("Error loading Flash! Not found!");
            }
        }

        public static Vector3 GetFlashPos(AIHeroClient target, bool serverPos, int distance = 150)
        {
            var enemyPos = serverPos ? target.ServerPosition : target.Position;
            var myPos = serverPos ? ObjectManager.Player.ServerPosition : ObjectManager.Player.Position;

            return enemyPos + Vector3.Normalize(enemyPos - myPos) * distance;
        }

        public static AIHeroClient CondemnCheck(Vector3 fromPosition)
        {
            var HeroList = GameObjects.EnemyHeroes.Where(
                h =>
                    h.IsValidTarget(Vayne.E.Range) &&
                    !h.HasBuffOfType(BuffType.SpellShield) &&
                    !h.HasBuffOfType(BuffType.SpellImmunity));
            foreach (var Hero in HeroList)
            {
                var ePred = Vayne.E2.GetPrediction(Hero);
                int pushDist = Vayne.emenu["PushDistance"].GetValue<MenuSlider>().Value; 
                for (int i = 0; i < pushDist; i += (int)Hero.BoundingRadius)
                {
                    Vector3 loc3 = ePred.UnitPosition.To2D().Extend(fromPosition.To2D(), -i).ToVector3();
                    var collFlags = NavMesh.GetCollisionFlags(loc3);
                    if (collFlags.HasFlag(CollisionFlags.Wall) || collFlags.HasFlag(CollisionFlags.Building))
                    {
                        return Hero;
                    }
                }
            }
            return null;
        }

    }
}