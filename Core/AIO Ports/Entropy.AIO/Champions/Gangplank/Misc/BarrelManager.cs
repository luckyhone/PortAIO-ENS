using System.Collections.Generic;
using System.Linq;
using EnsoulSharp;
using EnsoulSharp.SDK;
using PortAIO.Library_Ports.Entropy.Lib.Geometry;
using SharpDX;
using static Entropy.AIO.Bases.ChampionBase;
using static Entropy.AIO.Gangplank.Components;

namespace Entropy.AIO.Gangplank.Misc
{
    public static class BarrelManager
    {
        public static List<Vector3> CastedBarrels;
        public static List<Barrel>  Barrels;
        public static AIHeroClient  Player => Definitions.Player;

        public static bool CastE(Vector3 position)
        {
            return ShouldCastOnPosition(position) && E.Cast(position);
        }

        public static bool ShouldCastOnPosition(Vector3 position)
        {
            return !CastedBarrels.Any(x => Vector3.Distance(x,position) <= 100);
        }

        public static bool BarrelWillHit(Barrel barrel, AIBaseClient target)
        {
            return Vector3.Distance(barrel.Object.Position,target.Position) <= Definitions.ExplosionRadius;
        }

        public static bool BarrelWillHit(Barrel barrel, Vector3 position)
        {
            return Vector3.Distance(barrel.Object.Position,position) <= Definitions.ExplosionRadius;
        }

        public static List<AIHeroClient> GetEnemiesInChainRadius(Barrel barrel, bool outsideExplosionRadius = true)
        {
            if (outsideExplosionRadius)
            {
                return GameObjects.EnemyHeroes.Where(x => x.IsValidTarget() &&
                                                          Vector3.Distance(barrel.Object.Position,x.Position) <=
                                                          Definitions.ChainRadius + Definitions.ExplosionRadius    &&
                                                          Vector3.Distance(barrel.Object.Position,x.Position) >= Definitions.ExplosionRadius &&
                                                          Vector3.Distance(x.Position,Player.Position)        <= E.Range).
                                   ToList();
            }

            return GameObjects.EnemyHeroes.Where(x => x.IsValidTarget() &&
                                                      Vector3.Distance(barrel.Object.Position,x.Position) <=
                                                      Definitions.ChainRadius + Definitions.ExplosionRadius &&
                                                      Vector3.Distance(x.Position,Player.Position) <= E.Range).
                               ToList();
        }

        public static List<Barrel> GetBarrelsThatWillHit()
        {
            //Returns all barrels that can explode on an enemy.
            var barrelsWillHit = new List<Barrel>();
            foreach (var enemy in Definitions.GetAllEnemiesInRange(float.MaxValue))
            {
                barrelsWillHit.AddRange(GetBarrelsThatWillHit(enemy));
            }

            return barrelsWillHit.Distinct().OrderBy(x => Vector3.Distance(x.Object.Position,Player.Position)).ToList();
        }

        public static List<Barrel> GetBarrelsThatWillHit(AIBaseClient target)
        {
            return Barrels.Where(x => Vector3.Distance(x.Object.Position,target.Position) <= Definitions.ExplosionRadius).
                           OrderBy(x => Vector3.Distance(x.Object.Position,Player.Position)).
                           ToList();
        }

        public static List<Barrel> GetChainedBarrels(Barrel barrel)
        {
            var barrels         = new List<Barrel> {barrel};
            var currentBarrelId = barrel.NetworkId;
            while (true)
            {
                var barrelToAdd = Barrels.FirstOrDefault(x => !x.Object.IsDead &&
                                                              Vector3.Distance(x.Object.Position,barrel.Object.Position) <=
                                                              Definitions.ChainRadius        &&
                                                              x.NetworkId != currentBarrelId &&
                                                              !barrels.Contains(x));
                if (barrelToAdd != null)
                {
                    barrels.Add(barrelToAdd);
                    currentBarrelId = barrelToAdd.NetworkId;
                }
                else
                {
                    break;
                }
            }

            return barrels;
        }

        public static Barrel GetBestBarrelToQ(List<Barrel> barrels)
        {
            return barrels.Where(x => !x.Object.IsDead && x.CanQ && x.Object.IsInRange(Player, Q.Range)).
                           OrderBy(x => x.Created).
                           FirstOrDefault();
        }

        public static Barrel GetNearestBarrel()
        {
            return Barrels.Where(x => !x.Object.IsDead && Vector3.Distance(x.Object.Position,Player.Position) <= E.Range).
                           OrderBy(x => Vector3.Distance(x.Object.Position,Player.Position)).
                           FirstOrDefault();
        }

        public static Barrel GetNearestBarrel(Vector3 position)
        {
            return Barrels.Where(x => !x.Object.IsDead && Vector3.Distance(x.Object.Position,Player.Position) <= E.Range).
                           OrderBy(x => Vector3.Distance(x.Object.Position,position)).
                           FirstOrDefault();
        }

        public static Vector3 GetBestChainPosition(AIBaseClient target, Barrel barrel, bool usePred = true)
        {
            //var input = E.GetPredictionInput(target);
            //input.Delay *= 2;
            //var pred = PredictionZ.GetPrediction(input);
            var pred         = E.GetPrediction(target);
            var castPosition = pred.CastPosition;
            if (ComboMenu.EMax.Enabled &&
                DistanceEx.Distance(barrel.Object, castPosition) <= Definitions.ChainRadius + Definitions.ExplosionRadius)
            {
                var bestCastPos = LeagueSharpCommon.Geometry.Geometry.Extend(barrel.ServerPosition,
                                                  usePred ? castPosition : target.Position,
                                                  Definitions.ChainRadius - 5);
                return bestCastPos;
            }

            if (DistanceEx.Distance(barrel.Object, castPosition) <= Definitions.ChainRadius)
            {
                return usePred ? pred.CastPosition : target.Position;
            }

            if (DistanceEx.Distance(barrel.Object, castPosition) <= Definitions.ChainRadius + Definitions.ExplosionRadius)
            {
                var bestCastPos = LeagueSharpCommon.Geometry.Geometry.Extend(barrel.ServerPosition,
                                                  usePred ? castPosition : target.Position,
                                                  Definitions.ChainRadius - 5);
                return bestCastPos;
            }

            return Vector3.Zero;
        }

        public static Vector3 GetBestChainPosition(Vector3 position, Barrel barrel)
        {
            if (Vector3.Distance(barrel.Object.Position,position) <= Definitions.ChainRadius)
            {
                return position;
            }

            if (Vector3.Distance(barrel.Object.Position,position) <= Definitions.ChainRadius + Definitions.ExplosionRadius)
            {
                var bestCastPos = LeagueSharpCommon.Geometry.Geometry.Extend(barrel.ServerPosition, position, Definitions.ChainRadius - 5);
                return bestCastPos;
            }

            return Vector3.Zero;
        }
    }
}