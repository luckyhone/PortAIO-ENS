using System;
using EnsoulSharp;
using EnsoulSharp.SDK;
using static e.Motion_Gangplank.Program;


namespace e.Motion_Gangplank
{
    public class Barrel
    {
        public int BarrelAttackTick;
        public int BarrelObjectNetworkID;
        private static AIHeroClient Player = ObjectManager.Player;
        public int GetNetworkID()
        {
            return BarrelObjectNetworkID;
        }
        public AIMinionClient GetBarrel()
        {
            return ObjectManager.GetUnitByNetworkId<AIMinionClient>(BarrelObjectNetworkID);
        }
        public bool CanAANow()
        {
            //Console.WriteLine();
            return Environment.TickCount >= BarrelAttackTick - Player.AttackCastDelay * 1000;
        }

        public bool CanQNow(int delay = 0)
        {
            if (Player.Distance(GetBarrel().Position) <= 625 &&
                Helper.GetQTime(GetBarrel().Position) + delay + Environment.TickCount >= BarrelAttackTick +
                additionalServerTick.Value) ;
            {
                return true;
            }
            return false;
        }
        public Barrel(AIMinionClient barrel)
        {
            BarrelObjectNetworkID = barrel.NetworkId;
            BarrelAttackTick = GetBarrelAttackTick();
        }

        public void ReduceBarrelAttackTick()
        {
            if (Player.Level < 7) BarrelAttackTick -= 2000;
            else if (Player.Level < 13) BarrelAttackTick -= 1000;
            else BarrelAttackTick -= 500;
        }
        private static int GetBarrelAttackTick()
        {
            if (Player.Level < 7) return Environment.TickCount + 4000;
            if (Player.Level < 13) return Environment.TickCount + 2000;
            return Environment.TickCount + 1000;
        }
    }
}