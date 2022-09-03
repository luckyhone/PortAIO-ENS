using System;
using System.Collections.Generic;
using System.Linq;
using EnsoulSharp;
using EnsoulSharp.SDK;
using Entropy.Lib.Render;
using PortAIO.Library_Ports.Entropy.Lib.Extensions;
using SharpDX;
using SharpDX.Direct3D9;
using StormAIO.utilities;

namespace Entropy.AIO.Cassiopeia.Misc
{
    static class Definitions
    {
        public static readonly Font Font = FontFactory.CreateNewFont(17, FontWeight.Bold, "Tahoma");

        internal static Dictionary<uint, int> CachedPoisoned = new Dictionary<uint, int>();
        internal static string[]              poisonStrings  = {"cassiopeiawpoison", "cassiopeiaqdebuff"};

        public static void BuffManagerOnOnLoseBuff(AIBaseClient sender, AIBaseClientBuffRemoveEventArgs args)
        {
            var receiver = args.Buff.Caster;
            if (receiver.IsEnemy || receiver.Team == GameObjectTeam.Neutral)
            {
                if (args.Buff.Type != BuffType.Poison && !poisonStrings.Contains(args.Buff.Name))
                {
                    return;
                }

                if (CachedPoisoned.ContainsKey((uint)receiver.NetworkId) && CachedPoisoned[(uint)receiver.NetworkId] > 1)
                {
                    CachedPoisoned[(uint)receiver.NetworkId] -= 1;
                    return;
                }

                CachedPoisoned.Remove((uint)receiver.NetworkId);
            }
        }

        public static void BuffManagerOnOnGainBuff(AIBaseClient sender, AIBaseClientBuffAddEventArgs args)
        {
            var receiver = args.Buff.Caster;
            if (receiver.IsEnemy || receiver.Team == GameObjectTeam.Neutral)
            {
                if (args.Buff.Type != BuffType.Poison && !poisonStrings.Contains(args.Buff.Name))
                {
                    return;
                }

                if (CachedPoisoned.ContainsKey((uint)receiver.NetworkId))
                {
                    CachedPoisoned[(uint)receiver.NetworkId] += 1;
                    return;
                }

                CachedPoisoned.Add((uint)receiver.NetworkId, 1);
            }
        }

        public static float AngleBetweenEx(this Vector2 p1, Vector2 p2)
        {
            var theta = p1.Polar() - p2.Polar();

            if (theta > 180)
            {
                theta = 360 - theta;
            }

            if (theta < -180)
            {
                theta = 360 + theta;
            }

            return theta;
        }

        public static Vector3 GetMovementPrediction(AIBaseClient target, float time = 1f)
        {
            try
            {
                if (target.Buffs.Any(b => BuffExtensions.IsMovementImpairing(b) && Helper.TimeLeft(b) <= time))
                {
                    return target.Position;
                }

                var distance = target.MoveSpeed * (time + Game.Ping / 1000f);
                if (target.Path[target.Path.Length - 1].Distance(target.Position) <= distance)
                {
                    distance = target.Path[target.Path.Length - 1].Distance(target.Position);
                }

                return target.Position.Extend(target.Path[target.Path.Length - 1], target.IsMoving ? distance : 0);
            }
            catch (Exception e)
            {
                return target.Position;
            }
            
        }
    }
}