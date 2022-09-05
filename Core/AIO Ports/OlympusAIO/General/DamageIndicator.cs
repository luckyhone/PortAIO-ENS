using System;
using System.Linq;
using SharpDX;
using EnsoulSharp;
using EnsoulSharp.SDK;
using EnsoulSharp.SDK.MenuUI;
using OlympusAIO.Helpers;
using MenuManager = OlympusAIO.Helpers.MenuManager;

namespace OlympusAIO.General
{
    class DamageIndicator
    {
        public static void OnEndScene(EventArgs args)
        {
            if (MenuManager.DrawingsMenu["Disable"].GetValue<MenuBool>().Enabled || !MenuManager.DamageIndicatorMenu["Enable"].GetValue<MenuBool>().Enabled)
                return;

            foreach (var target in GameObjects.EnemyHeroes.Where(x => x.IsValidTarget(2000) && !x.IsDead && x.IsHPBarRendered))
            {
                Vector2 pos = Drawing.WorldToScreen(target.Position);

                if (!pos.IsOnScreen())
                    return;

                float damage = DamageManager.GetDamageByChampion(target);

                var hpBar = target.HPBarPosition;

                if (damage > target.Health)
                {
                    Drawing.DrawText(hpBar.X + 69, hpBar.Y - 45, System.Drawing.Color.White, "KILLABLE");
                }

                var damagePercentage = ((target.Health - damage) > 0 ? (target.Health - damage) : 0) / target.MaxHealth;
                var currentHealthPercentage = target.Health / target.MaxHealth;

                var startPoint = new Vector2(hpBar.X - 45 + damagePercentage * 104, hpBar.Y - 18);
                var endPoint = new Vector2(hpBar.X - 45 + currentHealthPercentage * 104, hpBar.Y - 18);

                Drawing.DrawLine(startPoint, endPoint, 12, System.Drawing.Color.Gold);
            }
        }
    }
}
