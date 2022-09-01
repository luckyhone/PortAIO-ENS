using EnsoulSharp;
using EnsoulSharp.SDK;
using LeagueSharpCommon.Geometry;
using SPrediction;

namespace ADCCOMMON
{
    using System;
    using System.Collections.Generic;
    using System.Linq;
    using SharpDX;

    public class MinionCache // Credit By Sebby
    {
        public static List<AIBaseClient> AllMinionsObj = new List<AIBaseClient>();
        public static List<AIBaseClient> MinionsListEnemy = new List<AIBaseClient>();
        public static List<AIBaseClient> MinionsListAlly = new List<AIBaseClient>();
        public static List<AIBaseClient> MinionsListNeutral = new List<AIBaseClient>();
        public static List<AITurretClient> TurretList = ObjectManager.Get<AITurretClient>().ToList();
        public static List<HQClient> NexusList = ObjectManager.Get<HQClient>().ToList();
        public static List<BarracksDampenerClient> InhiList = ObjectManager.Get<BarracksDampenerClient>().ToList();

        static MinionCache()
        {
            foreach (var minion in ObjectManager.Get<AIMinionClient>().Where(minion => minion.IsValid))
            {
                AddMinionObject(minion);

                if (!minion.IsAlly)
                {
                    AllMinionsObj.Add(minion);
                }
            }

            GameObject.OnCreate += OnCreate;
            Game.OnUpdate += OnUpdate;
        }

        private static void OnUpdate(EventArgs args)
        {
            MinionsListEnemy.RemoveAll(minion => !IsValidMinion(minion));
            MinionsListNeutral.RemoveAll(minion => !IsValidMinion(minion));
            MinionsListAlly.RemoveAll(minion => !IsValidMinion(minion));
            AllMinionsObj.RemoveAll(minion => !IsValidMinion(minion));
        }

        private static void OnCreate(GameObject sender, EventArgs args)
        {
            var minion = sender as AIMinionClient;

            if (minion != null)
            {
                AddMinionObject(minion);

                if (!minion.IsAlly)
                {
                    AllMinionsObj.Add(minion);
                }
            }
        }

        private static void AddMinionObject(AIBaseClient minion)
        {
            if (minion.MaxHealth >= 225)
            {
                if (minion.Team == GameObjectTeam.Neutral)
                {
                    MinionsListNeutral.Add(minion);
                }
                else if (minion.MaxMana == 0 && minion.MaxHealth >= 300)
                {
                    if (minion.Team == GameObjectTeam.Unknown)
                    {

                    }

                    if (minion.Team != ObjectManager.Player.Team)
                    {
                        MinionsListEnemy.Add(minion);
                    }
                    else if (minion.Team == ObjectManager.Player.Team)
                    {
                        MinionsListAlly.Add(minion);
                    }
                }
            }
        }

        public static List<AIBaseClient> GetMinions(Vector3 from, float range = float.MaxValue,
            MinionManager.MinionTeam team = MinionManager.MinionTeam.Enemy)
        {
            switch (team)
            {
                case MinionManager.MinionTeam.Enemy:
                    {
                        return MinionsListEnemy.FindAll(minion => CanReturn(minion, from, range));
                    }
                case MinionManager.MinionTeam.Ally:
                    {
                        return MinionsListAlly.FindAll(minion => CanReturn(minion, from, range));
                    }
                case MinionManager.MinionTeam.Neutral:
                    {
                        return
                            MinionsListNeutral.Where(minion => CanReturn(minion, from, range))
                                .OrderByDescending(minion => minion.MaxHealth)
                                .ToList();
                    }
                case MinionManager.MinionTeam.NotAlly:
                    {
                        return AllMinionsObj.FindAll(minion => CanReturn(minion, from, range));
                    }
                default:
                    {
                        return AllMinionsObj.FindAll(minion => CanReturn(minion, from, range));
                    }
            }
        }

        private static bool IsValidMinion(GameObject minion)
        {
            return minion != null && minion.IsValid && !minion.IsDead;
        }

        private static bool CanReturn(AttackableUnit minion, Vector3 from, float range)
        {
            if (minion != null && minion.IsValid && !minion.IsDead && minion.IsVisible && minion.IsTargetable)
            {
                if (range == float.MaxValue)
                {
                    return true;
                }

                if (range == 0)
                {
                    return ObjectManager.Player.InAutoAttackRange(minion);
                }

                return Vector2.DistanceSquared(from.To2D(), minion.Position.To2D()) < range * range;
            }

            return false;
        }
    }

}
