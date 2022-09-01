using System;
using EnsoulSharp;
using EnsoulSharp.SDK;
using EnsoulSharp.SDK.MenuUI;
using PortAIO;
using SharpDX;

namespace DZAhri
{
    static class Helpers
    {
        public static bool IsMenuEnabled(String menu)
        {
            return DZAhri.Menu.GetValue<MenuBool>(menu).Enabled;
        }

        public static bool IsCharmed(this AIHeroClient target)
        {
            return target.HasBuff("AhriSeduce");
        }
        
        public static bool IsSafe(this Vector3 myVector)
        {
            var killableEnemy = myVector.GetEnemiesInRange(600f).Find(h => GetComboDamage(h) >= h.Health);
            var killableEnemyNumber = killableEnemy != null ? 1 : 0;
            var killableEnemyPlayer = ObjectManager.Player.GetEnemiesInRange(600f).Find(h => GetComboDamage(h) >= h.Health);
            var killableEnemyPlayerNumber = killableEnemyPlayer != null ? 1 : 0;

            if ((ObjectManager.Player.IsUnderEnemyTurret() && killableEnemyPlayerNumber == 0) || (myVector.IsUnderEnemyTurret() && killableEnemyNumber == 0))
            {
                return false;
            }
            if (myVector.CountEnemyHeroesInRange(600f) == 1 || ObjectManager.Player.CountEnemyHeroesInRange(600f) >= 1)
            {
                return true;
            }
            return myVector.CountEnemyHeroesInRange(600f) - killableEnemyNumber - myVector.CountAllyHeroesInRange(600f) + 1 >= 0;
        }
        
        public static float GetComboDamage(AIHeroClient enemy)
        {
            float totalDamage = 0;
            totalDamage += DZAhri._spells[SpellSlot.Q].IsReady() ? DZAhri._spells[SpellSlot.Q].GetDamage(enemy) : 0;
            totalDamage += DZAhri._spells[SpellSlot.W].IsReady() ? DZAhri._spells[SpellSlot.W].GetDamage(enemy) : 0;
            totalDamage += DZAhri._spells[SpellSlot.E].IsReady() ? DZAhri._spells[SpellSlot.E].GetDamage(enemy) : 0;
            totalDamage += (DZAhri._spells[SpellSlot.R].IsReady() || (RStacks() != 0)) ? DZAhri._spells[SpellSlot.R].GetDamage(enemy) : 0;
            return totalDamage;
        }
        public static bool IsRCasted()
        {
            return ObjectManager.Player.HasBuff("AhriTumble");
        }
        public static int RStacks()
        {
            var rBuff = ObjectManager.Player.Buffs.Find(buff => buff.Name == "AhriTumble");
            return rBuff != null ? rBuff.Count : 0;
        }
    }
}
