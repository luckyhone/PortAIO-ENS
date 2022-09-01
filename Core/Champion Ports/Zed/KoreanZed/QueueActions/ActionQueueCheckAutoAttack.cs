using System;
using EnsoulSharp;
using EnsoulSharp.SDK;
using EnsoulSharp.SDK.Utility;

namespace KoreanZed.QueueActions
{
    class ActionQueueCheckAutoAttack
    {
        private bool status;

        public bool Status
        {
            get
            {
                if (status)
                {
                    status = false;
                    return true;
                }
                else
                {
                    return false;
                }
            }
        }

        public ActionQueueCheckAutoAttack()
        {
            status = false;
            Game.OnUpdate += Game_OnUpdate;
            Orbwalker.OnAfterAttack += Orbwalking_AfterAttack;
        }

        private void Orbwalking_AfterAttack(object unit, AfterAttackEventArgs e)
        {
            if (!e.Target.IsMe)
            {
                return;
            }

            status = true;
            DelayAction.Add(100, () => status = false);
        }

        private void Game_OnUpdate(EventArgs args)
        {

        }
    }
}