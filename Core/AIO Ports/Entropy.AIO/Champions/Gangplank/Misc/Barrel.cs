using System;
using System.Linq;
using EnsoulSharp;
using EnsoulSharp.SDK.Utility;
using Entropy.AIO.Ahri.Misc;
using GangplankBuddy;
using SharpDX;

namespace Entropy.AIO.Gangplank.Misc
{
    public class Barrel
    {
        public int          Created;
        public float        DecayRate;
        public AIBaseClient Object;
        public float        TimeAt1HP;

        public Barrel(AIBaseClient barrel, int created)
        {
            Object    = barrel;
            Created   = created;
            DecayRate = Definitions.GetDecayRate();
            TimeAt1HP = Created + 1000 * DecayRate * 2;
        }

        public float   Health         => Object.Health;
        public Vector3 Position       => Object.Position;
        public Vector3 ServerPosition => Object.Position;
        public uint    NetworkId      => (uint)Object.NetworkId;

        public float CanQTime =>
            TimeAt1HP - 250f - Definitions.DistanceFrom(Object) / 2000f * 1000 + Game.Ping * 1.5f;

        public float CanAATime => TimeAt1HP -
                                  Definitions.Player.AttackDelay * 1000;

        public bool  CanQ         => Environment.TickCount >= this.CanQTime  || this.Health == 1;
        public bool  CanAA        => Environment.TickCount >= this.CanAATime || this.Health == 1;
        public float CanChainTime => this.CanQTime - 250f;
        public bool  CanChain     => Environment.TickCount >= this.CanChainTime || this.Health == 1;

        public void Decay()
        {
            TimeAt1HP -= 1000 * DecayRate;
        }

        public void Decay(int delay)
        {
            DelayAction.Add(delay, Decay);
        }

        public static implicit operator Barrel(GameObject barrel)
        {
            return BarrelManager.Barrels.FirstOrDefault(x => x.ServerPosition == barrel.Position);
        }
    }
}