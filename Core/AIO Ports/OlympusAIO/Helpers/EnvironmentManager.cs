using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

using EnsoulSharp;
using EnsoulSharp.SDK;
using EnsoulSharp.SDK.MenuUI;
using SharpDX;

namespace OlympusAIO.Helpers
{
    class EnvironmentManager
    {
        public static AIHeroClient objPlayer = ObjectManager.Player;

        public class Map
        {
            public static bool WallCollision(Vector3 from, Vector3 target)
            {
                var distance = from.Distance(target);

                for (int i = 1; i < 6; i++)
                {
                    if (from.Extend(target, distance + 80 * i).IsWall())
                    {
                        return true;
                    }
                }
                return false;
            }
            public static Vector3 ClosestWall(Vector3 startPos, Vector3 endPos)
            {
                var distance = startPos.Distance(endPos);

                for (int i = 1; i < 6; i++)
                {
                    if (startPos.Extend(endPos, distance + 70 * i).IsWall())
                    {
                        return startPos.Extend(endPos, distance + 70 * i);
                    }
                }
                return endPos;
            }
            public static float GetPath(AIHeroClient heroClient, Vector3 pos)
            {
                var path = heroClient.GetPath(pos);

                var lastPoint = path[0];
                var distance = 0f;

                foreach (var point in path.Where(x => x.Equals(lastPoint)))
                {
                    distance += lastPoint.Distance(point);
                    lastPoint = point;
                }
                return distance;
            }
        }
    }
}
