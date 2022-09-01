using System;
using System.Collections.Generic;
using System.Linq;
using EnsoulSharp;

namespace Flowers_Karthus.Common
{
    public class EnemyTracker
    {
        public static List<AIHeroClient> EnemyList;
        public static List<EnemyInfo> enemyInfo;

        public EnemyTracker()
        {
            EnemyList = ObjectManager.Get<AIHeroClient>().Where(x => x.IsEnemy).ToList();

            enemyInfo = EnemyList.Select(x => new EnemyInfo(x)).ToList();

            Game.OnUpdate += OnUpdate;
        }

        private static void OnUpdate(EventArgs args)
        {
            var time = Environment.TickCount;

            foreach (var enemy in enemyInfo.Where(x => x.target.IsVisible))
                enemy.LastSeen = time;
        }

        public static float GetTargetHealth(EnemyInfo enemyinfo, float additionalTime)
        {
            if (enemyinfo.target.IsVisible)
                return enemyinfo.target.Health;

            var predictedHealth = enemyinfo.target.Health + enemyinfo.target.HPRegenRate * ((Environment.TickCount - enemyinfo.LastSeen + additionalTime * 1000) / 1000f);

            return predictedHealth > enemyinfo.target.MaxHealth ? enemyinfo.target.MaxHealth : predictedHealth;
        }
    }

    public class EnemyInfo
    {
        public AIHeroClient target;
        public int LastSeen;

        public EnemyInfo(AIHeroClient enemy)
        {
            target = enemy;
        }
    }
}