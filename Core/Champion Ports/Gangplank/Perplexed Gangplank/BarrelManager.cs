using System.Collections.Generic;
using System.Linq;
using EnsoulSharp;
using EnsoulSharp.SDK;
using SharpDX;

namespace Perplexed_Gangplank
{
    public static class BarrelManager
    {
        public static AIHeroClient Player => ObjectManager.Player;
        public static List<Barrel> Barrels;
        public static bool BarrelWillHit(Barrel barrel, AIBaseClient target)
        {
            return barrel.Object.Distance(target) <= SpellManager.ExplosionRadius;
        }
        public static bool BarrelWillHit(Barrel barrel, Vector3 position)
        {
            return barrel.Object.Distance(position) <= SpellManager.ExplosionRadius;
        }
        public static List<AIHeroClient> GetEnemiesInChainRadius(Barrel barrel)
        {
            //Returns enemies within the barrel's chain radius, but outside of its explosion radius.
            return GameObjects.EnemyHeroes.Where(x => x.IsValidTarget() && barrel.Object.Distance(x) <= SpellManager.ChainRadius + SpellManager.ExplosionRadius && barrel.Object.Distance(x) >= SpellManager.ExplosionRadius).ToList();
        }
        public static List<Barrel> GetBarrelsThatWillHit()
        {
            //Returns all barrels that can explode on an enemy.
            var barrelsWillHit = new List<Barrel>();
            foreach(var enemy in Utility.GetAllEnemiesInRange(float.MaxValue))
                barrelsWillHit.AddRange(GetBarrelsThatWillHit(enemy));
            return barrelsWillHit.Distinct().OrderBy(x => x.Object.Distance(Utility.Player)).ToList();
        }
        public static List<Barrel> GetBarrelsThatWillHit(AIBaseClient target)
        {
            return Barrels.Where(x => target.Distance(x.Object) <= SpellManager.ExplosionRadius).OrderBy(x => x.Object.Distance(Utility.Player)).ToList();
        }
        public static List<Barrel> GetChainedBarrels(Barrel barrel)
        {
            return Barrels.Where(x => !x.Object.IsDead && x.Object.Distance(barrel.Object) <= SpellManager.ChainRadius).ToList();
        }
        public static Barrel GetBestBarrelToQ(List<Barrel> barrels)
        {
            return barrels.Where(x => !x.Object.IsDead && x.CanQ && x.Object.InRange(SpellManager.Q.Range)).OrderBy(x => x.Object.Distance(Utility.Player)).FirstOrDefault();
        }
        public static Barrel GetNearestBarrel()
        {
            return Barrels.Where(x => !x.Object.IsDead && x.Object.Distance(Player) <= SpellManager.E.Range).OrderBy(x => x.Object.Distance(Utility.Player)).FirstOrDefault();
        }
        public static Barrel GetNearestBarrel(Vector3 position)
        {
            return Barrels.Where(x => !x.Object.IsDead && x.Object.Distance(Player) <= SpellManager.E.Range).OrderBy(x => x.Object.Distance(position)).FirstOrDefault();
        }
        public static Vector3 GetBestChainPosition(AIBaseClient target, Barrel barrel)
        {
            if (barrel.Object.Distance(target) <= SpellManager.ChainRadius)
                    return target.ServerPosition;
            if (barrel.Object.Distance(target) <= SpellManager.ChainRadius + SpellManager.ExplosionRadius)
            {
                var bestCastPos = barrel.ServerPosition.Extend(target.ServerPosition, SpellManager.ChainRadius - 5);
                return bestCastPos;
            }
            return Vector3.Zero;
        }
        public static Vector3 GetBestChainPosition(Vector3 position, Barrel barrel)
        {
            if (barrel.Object.Distance(position) <= SpellManager.ChainRadius)
                return position;
            if (barrel.Object.Distance(position) <= SpellManager.ChainRadius + SpellManager.ExplosionRadius)
            {
                var bestCastPos = barrel.ServerPosition.Extend(position, SpellManager.ChainRadius - 5);
                return bestCastPos;
            }
            return Vector3.Zero;
        }
    }
}