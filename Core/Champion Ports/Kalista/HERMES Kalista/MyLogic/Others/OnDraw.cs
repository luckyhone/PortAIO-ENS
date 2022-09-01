using System;
using System.Drawing;
using System.Linq;
using EnsoulSharp;
using EnsoulSharp.SDK;
using EnsoulSharp.SDK.MenuUI;

namespace HERMES_Kalista.MyLogic.Others
{
    public static partial class Events
    {
        public static void OnDraw(EventArgs args)
        {
            foreach (var hero in GameObjects.EnemyHeroes.Where(h => h.IsValidTarget() && h.Distance(ObjectManager.Player) < 1400))
            {
                var WDMG = Program.W.GetDamage(hero);
                var AADMG = ObjectManager.Player.GetAutoAttackDamage(hero);
                var AAOnly = (int)(hero.Health / AADMG);
                var Combined = (int)((hero.Health - ((AAOnly/3)*WDMG))/AADMG);
                Drawing.DrawText(hero.HPBarPosition.X + 80, hero.HPBarPosition.Y - 30,
                    Combined <= 3 ? Color.Gold : Color.White,
                    "AAs to kill: " + Combined);
            }
        }
    }
}