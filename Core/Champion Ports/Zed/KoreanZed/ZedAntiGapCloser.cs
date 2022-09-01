using System.Linq;
using EnsoulSharp;
using EnsoulSharp.SDK;
using KoreanZed.QueueActions;
using SharpDX;

namespace KoreanZed
{
    class ZedAntiGapCloser
    {
        private readonly ZedMenu zedMenu;

        private readonly ZedSpell w;

        private readonly ZedSpell e;

        private readonly ActionQueue actionQueue;

        private readonly ActionQueueList antiGapCloserList;

        private readonly ZedShadows shadows;

        private readonly AIHeroClient player;

        public ZedAntiGapCloser(ZedMenu menu, ZedSpells spells, ZedShadows shadows)
        {
            zedMenu = menu;
            w = spells.W;
            e = spells.E;
            this.shadows = shadows;
            player = ObjectManager.Player;

            actionQueue = new ActionQueue();
            antiGapCloserList = new ActionQueueList();

            AntiGapcloser.OnGapcloser += AntiGapcloser_OnEnemyGapcloser;
            
        }
        

        private void Game_OnUpdate(System.EventArgs args)
        {
            actionQueue.ExecuteNextAction(antiGapCloserList);
            if (antiGapCloserList.Count == 0)
            {
                Game.OnUpdate -= Game_OnUpdate;
            }
        }

        private void AntiGapcloser_OnEnemyGapcloser(AIHeroClient sender, AntiGapcloser.GapcloserArgs args)
        {
            if (ObjectManager.Player.Distance(sender.Position) > e.Range)
            {
                return;
            }

            if (zedMenu.GetParamBool("koreanzed.miscmenu.useeantigc") && e.IsReady())
            {
                e.Cast();
            }

            if (zedMenu.GetParamBool("koreanzed.miscmenu.usewantigc") && w.IsReady() && antiGapCloserList.Count == 0)
            {
                if (shadows.CanCast)
                {
                    actionQueue.EnqueueAction(
                        antiGapCloserList,
                        () => player.Mana > w.Mana && player.HealthPercent - 10 < sender.HealthPercent,
                        () => shadows.Cast(Vector3.Negate(sender.Position)),
                        () => true);
                    actionQueue.EnqueueAction(
                        antiGapCloserList,
                        () => w.Instance.ToggleState != 0,
                        () => shadows.Switch(),
                        () => !w.IsReady());
                    Game.OnUpdate += Game_OnUpdate;
                    return;
                }
                else if (!shadows.CanCast && shadows.CanSwitch)
                {
                    int champCount =
                        GameObjects.EnemyHeroes.Count(enemy => enemy.Distance(shadows.Instance.Position) < 1500F);

                    if ((player.HealthPercent > 80 && champCount <= 3)
                        || (player.HealthPercent > 40 && champCount <= 2)
                        )
                    {
                        shadows.Switch();
                    }
                   
                }
            }
        }
    }
}