using System;
using System.Linq;
using EnsoulSharp;
using SharpDX;

namespace Perplexed_Gangplank
{
    public class Barrel
    {
        public AIBaseClient Object;
        public int Created;
        public float Health => Object.Health;
        public Vector3 Position => Object.Position;
        public Vector3 ServerPosition => Object.ServerPosition;
        public float DecayRate;
        public float TimeAt1HP;
        public float CanQTime => TimeAt1HP - 250f - ((Utility.DistanceFrom(Object) / 2000f) * 1000) + Game.Ping + 10;
        public bool CanQ => Environment.TickCount >= CanQTime || Health == 1;
        public float CanChainTime => TimeAt1HP - (250f * 2);
        public bool CanChain => Environment.TickCount >= CanChainTime || Health == 1;
        public Barrel(AIBaseClient barrel, int created)
        {
            Object = barrel;
            Created = created;
            DecayRate = Utility.GetDecayRate();
            TimeAt1HP = Created + ((1000 * DecayRate) * 2);
        }
        public static implicit operator Barrel(GameObject barrel)
        {
            return BarrelManager.Barrels.First(x => x.ServerPosition == barrel.Position);
        }
    }
}