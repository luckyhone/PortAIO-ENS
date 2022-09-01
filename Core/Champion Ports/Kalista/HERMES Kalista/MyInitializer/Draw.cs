using System.Drawing;
using System.Linq;
using EnsoulSharp;
using EnsoulSharp.SDK;
using EnsoulSharp.SDK.MenuUI;
using EnsoulSharp.SDK.Rendering;

namespace HERMES_Kalista.MyInitializer
{
    public static partial class HERMESLoader
    {
        public static void Draw()
        {
            Drawing.OnDraw += args =>
            {
                if (!Program.DrawingsMenu.GetValue<MenuBool>("streamingmode").Enabled) return;
                if (Program.DrawingsMenu.GetValue<MenuBool>("EDraw").Enabled)
                {
                    CircleRender.Draw(
                        ObjectManager.Player.Position,
                        1000,
                        SharpDX.Color.LightGreen);
                }

                foreach (var source in
                    GameObjects.EnemyHeroes.Where(x => ObjectManager.Player.Distance(x) <= 2000f && !x.IsDead))
                {
                    var currentPercentage = GetRealDamage(source) * 100 / source.Health;

                    Drawing.DrawText(
                        Drawing.WorldToScreen(source.Position)[0],
                        Drawing.WorldToScreen(source.Position)[1],
                        currentPercentage >= 100 ? Color.DarkRed : Color.White,
                        currentPercentage >= 100 ? "Killable With E" : "Current Damage: " + currentPercentage + "%");
                }
            };
        }
        public static float GetRealDamage(AIHeroClient target)
        {
            if (target.HasBuff("ferocioushowl"))
            {
                return Program.E.GetDamage(target) * 0.7f;
            }

            return Program.E.GetDamage(target);
        }
    }
}