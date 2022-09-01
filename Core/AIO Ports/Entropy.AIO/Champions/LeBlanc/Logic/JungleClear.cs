using System.Linq;
using EnsoulSharp;
using EnsoulSharp.SDK;
using Entropy.AIO.Bases;
using PortAIO.Library_Ports.Entropy.Lib.Geometry;

namespace Entropy.AIO.LeBlanc.Logic
{
    using static ChampionBase;
    using static Components;
    class JungleClear
    {
        internal static void ExecuteQ()
        {
            if (Q.IsReady() && JungleClearMenu.QBool.Enabled)
            {
                var target = GameObjects.Jungle.FirstOrDefault(x => x.IsInRange(ObjectManager.Player, Q.Range));

                if (target.IsValidTarget(Q.Range))
                {
                    Q.Cast(target);
                }
            }
        }
    }
}