using System;
using System.Linq;
using EnsoulSharp;
using EnsoulSharp.SDK;

namespace KoreanZed
{
    class ZedAutoE
    {
        private readonly ZedMenu zedMenu;

        private readonly ZedShadows zedShadows;

        private readonly ZedSpell e;

        public ZedAutoE(ZedMenu zedMenu, ZedShadows zedShadows, ZedSpells zedSpells)
        {
            this.zedMenu = zedMenu;
            this.zedShadows = zedShadows;
            e = zedSpells.E;

            Game.OnUpdate += Game_OnUpdate;
        }

        private void Game_OnUpdate(EventArgs args)
        {
            if (!e.IsReady() || ObjectManager.Player.Mana < e.Mana || !zedMenu.GetParamBool("koreanzed.miscmenu.autoe"))
            {
                return;
            }

            if (
                GameObjects.EnemyHeroes.Any(
                    enemy =>
                        !enemy.IsDead && !enemy.IsZombie() && enemy.Distance(ObjectManager.Player) < e.Range
                        && enemy.IsValidTarget())
                || zedShadows.GetShadows()
                    .Any(
                        shadow =>
                            GameObjects.EnemyHeroes.Any(
                                enemy =>
                                    !enemy.IsDead && !enemy.IsZombie() && enemy.Distance(shadow) < e.Range
                                    && enemy.IsValidTarget())))
            {
                e.Cast();
            }
        }
    }
}