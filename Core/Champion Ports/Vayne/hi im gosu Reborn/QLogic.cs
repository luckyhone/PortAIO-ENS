using System.Linq;
using Challenger_Series.Utils;
using EnsoulSharp;
using EnsoulSharp.SDK;
using EnsoulSharp.SDK.MenuUI;
using PortAIO.Library_Ports.Entropy.Lib.Geometry;
using SharpDX;

namespace hi_im_gosu_Reborn
{
    class QLogic
    {
        private void QLogics(AIHeroClient target)
        {
            if (!Vayne.Q.IsReady())
            {
                return;
            }

            var qPosition = ExtendEx.Extend(ObjectManager.Player.ServerPosition, Game.CursorPos, Vayne.Q.Range);
            var targetDisQ = DistanceEx.Distance(target.ServerPosition, qPosition);
            var canQ = false;

            if (Vayne.qmenu["QTurret"].GetValue<MenuBool>().Enabled && qPosition.UnderTurret(true))
            {
                canQ = false;
            }

            if (Vayne.qmenu["QCheck"].GetValue<MenuBool>().Enabled)
            {
                if (Vector3Extensions.CountEnemyHeroesInRange(qPosition,300f) >= 3)
                {
                    canQ = false;
                }

                //Catilyn W
                if (ObjectManager
                        .Get<EffectEmitter>()
                        .FirstOrDefault(
                            x =>
                                x != null && x.IsValid &&
                                x.Name.ToLower().Contains("yordletrap_idle_red.troy") &&
                                Vector3.Distance(x.Position,qPosition) <= 100) != null)
                {
                    canQ = false;
                }

                //Jinx E
                if (ObjectManager.Get<AIMinionClient>()
                        .FirstOrDefault(x => x.IsValid && x.IsEnemy && x.Name == "k" &&
                                             Vector3.Distance(x.Position,qPosition) <= 100) != null)
                {
                    canQ = false;
                }

                //Teemo R
                if (ObjectManager.Get<AIMinionClient>()
                        .FirstOrDefault(x => x.IsValid && x.IsEnemy && x.Name == "Noxious Trap" &&
                                             Vector3.Distance(x.Position,qPosition) <= 100) != null)
                {
                    canQ = false;
                }
            }

            if (targetDisQ >= Vayne.Q.Range && targetDisQ <= Vayne.Q.Range * 2)
            {
                canQ = true;
            }

            if (canQ)
            {
                Vayne.Q.Cast(qPosition);
                canQ = false;
            }
        }
    }
}