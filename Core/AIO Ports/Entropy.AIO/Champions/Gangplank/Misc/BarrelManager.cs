using System.Collections.Generic;
using System.Linq;
using EnsoulSharp;
using EnsoulSharp.SDK;
using PortAIO.Library_Ports.Entropy.Lib.Geometry;
using SharpDX;
using static Entropy.AIO.Bases.ChampionBase;

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
            return !CastedBarrels.Any(x => Vector3Extensions.Distance(x, position) <= 100);
        }

        public static bool BarrelWillHit(Barrel barrel, AIBaseClient target)
        {
            return GameObjectExtensions.Distance(barrel.Object, target) <= Definitions.ExplosionRadius;
        }

        public static bool BarrelWillHit(Barrel barrel, Vector3 position)
        {
            return GameObjectExtensions.Distance(barrel.Object, position) <= Definitions.ExplosionRadius;
        }

        public static List<AIHeroClient> GetEnemiesInChainRadius(Barrel barrel, bool outsideExplosionRadius = true)
        {
            if (outsideExplosionRadius)
            {
                return GameObjects.EnemyHeroes.Where(x => x.IsValidTarget() &&
                                                          GameObjectExtensions.Distance(barrel.Object, x) <=
                                                          Definitions.ChainRadius + Definitions.ExplosionRadius    &&
                                                          GameObjectExtensions.Distance(barrel.Object, x) >= Definitions.ExplosionRadius &&
                                                          GameObjectExtensions.Distance(x, Player)        <= E.Range).
                                   ToList();
            }

            return GameObjects.EnemyHeroes.Where(x => x.IsValidTarget() &&
                                                      GameObjectExtensions.Distance(barrel.Object, x) <=
                                                      Definitions.ChainRadius + Definitions.ExplosionRadius &&
                                                      GameObjectExtensions.Distance(x, Player) <= E.Range).
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

            return barrelsWillHit.Distinct().OrderBy(x => GameObjectExtensions.Distance(x.Object, Player)).ToList();
        }

        public static List<Barrel> GetBarrelsThatWillHit(AIBaseClient target)
        {
            return Barrels.Where(x => GameObjectExtensions.Distance(x.Object, target) <= Definitions.ExplosionRadius).
                           OrderBy(x => GameObjectExtensions.Distance(x.Object, Player)).
                           ToList();
        }

        public static List<Barrel> GetChainedBarrels(Barrel barrel)
        {
            var barrels         = new List<Barrel> {barrel};
            var currentBarrelId = barrel.NetworkId;
            while (true)
            {
                var barrelToAdd = Barrels.FirstOrDefault(x => !x.Object.IsDead &&
                                                              GameObjectExtensions.Distance(x.Object, barrel.Object) <=
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
            return barrels.Where(x => !x.Object.IsDead && x.CanQ && x.Object.InRange(Player, Q.Range)).
                           OrderBy(x => x.Created).
                           FirstOrDefault();
        }

        public static Barrel GetNearestBarrel()
        {
            return Barrels.Where(x => !x.Object.IsDead && GameObjectExtensions.Distance(x.Object, Player) <= E.Range).
                           OrderBy(x => GameObjectExtensions.Distance(x.Object, Player)).
                           FirstOrDefault();
        }

        public static Barrel GetNearestBarrel(Vector3 position)
        {
            return Barrels.Where(x => !x.Object.IsDead && GameObjectExtensions.Distance(x.Object, Player) <= E.Range).
                           OrderBy(x => GameObjectExtensions.Distance(x.Object, position)).
                           FirstOrDefault();
        }

        public static Vector3 GetBestChainPosition(AIBaseClient target, Barrel barrel, bool usePred = true)
        {
            //var input = E.GetPredictionInput(target);
            //input.Delay *= 2;
            //var pred = PredictionZ.GetPrediction(input);
            var pred         = E.GetPrediction(target);
            var castPosition = pred.CastPosition;
            if (Components.ComboMenu.EMax.Enabled &&
                DistanceEx.Distance(barrel.Object, castPosition) <= Definitions.ChainRadius + Definitions.ExplosionRadius)
            {
                var bestCastPos = ExtendEx.Extend(barrel.ServerPosition,
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
                var bestCastPos = ExtendEx.Extend(barrel.ServerPosition,
                                                  usePred ? castPosition : target.Position,
                                                  Definitions.ChainRadius - 5);
                return bestCastPos;
            }

            return Vector3.Zero;
        }

        public static Vector3 GetBestChainPosition(Vector3 position, Barrel barrel)
        {
            if (GameObjectExtensions.Distance(barrel.Object, position) <= Definitions.ChainRadius)
            {
                return position;
            }

            if (GameObjectExtensions.Distance(barrel.Object, position) <= Definitions.ChainRadius + Definitions.ExplosionRadius)
            {
                var bestCastPos = ExtendEx.Extend(barrel.ServerPosition, position, Definitions.ChainRadius - 5);
                return bestCastPos;
            }

            return Vector3.Zero;
        }
    }
}