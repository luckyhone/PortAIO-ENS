using EnsoulSharp.SDK;

namespace Entropy.AIO.Aatrox.Logics
{
    using System.Linq;
    using static Components;
    using static Bases.ChampionBase;

    public class JungleClear
    {
        public static void ExecuteW()
        {
            if (JungleClearMenu.WBool.Enabled && W.IsReady())
            {
                var target = GameObjects.Jungle.FirstOrDefault(x => x.IsValidTarget(W.Range));
                if (target != null)
                {
                    W.Cast(target);
                }
            }
        }

        public static void ExecuteE()
        {
            if (JungleClearMenu.EBool.Enabled && E.IsReady())
            {
                var target = GameObjects.Jungle.FirstOrDefault(x => x.IsValidTarget(E.Range));
                if (target != null)
                {
                    E.Cast(target.Position);
                }
            }
        }
    }
}