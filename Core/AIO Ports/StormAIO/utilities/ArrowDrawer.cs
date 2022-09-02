using System;
using System.Linq;
using EnsoulSharp;
using EnsoulSharp.SDK;
using EnsoulSharp.SDK.MenuUI;
using SharpDX;
using StormAIO.Champions;
using Color = System.Drawing.Color;

namespace StormAIO.utilities
{
    public class ArrowDrawer
    {
        private static AIHeroClient Player => ObjectManager.Player;


        #region HerosArrow

        private static Menu ArrowMenu;
        private static int SP1 => ArrowMenu["Heros"].GetValue<MenuSlider>("startp1").Value;
        private static int SP2 => ArrowMenu["Heros"].GetValue<MenuSlider>("startp2").Value;
        private static int EP1 => ArrowMenu["Heros"].GetValue<MenuSlider>("EndPoint1").Value;
        private static int EP2 => ArrowMenu["Heros"].GetValue<MenuSlider>("EndPoint2").Value;

        #endregion

        #region minionArrow

        private static int MinionSP1 => ArrowMenu["minions"].GetValue<MenuSlider>("startp1").Value;
        private static int MinionSP2 => ArrowMenu["minions"].GetValue<MenuSlider>("startp2").Value;
        private static int MinionEP1 => ArrowMenu["minions"].GetValue<MenuSlider>("EndPoint1").Value;
        private static int MinionEP2 => ArrowMenu["minions"].GetValue<MenuSlider>("EndPoint2").Value;
        private static bool DrawOnMinion => ArrowMenu["minions"].GetValue<MenuBool>("DrawMinion").Enabled;
        private static MenuColor MinionColor => ArrowMenu["minions"].GetValue<MenuColor>("MinionColor");
        private static int MinionLine => ArrowMenu["minions"].GetValue<MenuSlider>("thinkness").Value;

        #endregion

        #region MenuHelper

        private static bool DrawOnPlayer => ArrowMenu["Heros"].GetValue<MenuBool>("DrawPlayer").Enabled;
        private static MenuColor PlayerColor => ArrowMenu["Heros"].GetValue<MenuColor>("PlayerColor");
        private static bool DrawOnAlly => ArrowMenu["Heros"].GetValue<MenuBool>("DrawAlly").Enabled;
        private static MenuColor AllyColor => ArrowMenu["Heros"].GetValue<MenuColor>("AllyColor");
        private static bool DrawOnEnemy => ArrowMenu["Heros"].GetValue<MenuBool>("DrawEnemy").Enabled;
        private static MenuColor EnemyColor => ArrowMenu["Heros"].GetValue<MenuColor>("EnemyColor");
        private static bool DrawOnTarget => ArrowMenu["Heros"].GetValue<MenuBool>("DrawTarget").Enabled;
        private static MenuColor TargetColor => ArrowMenu["Heros"].GetValue<MenuColor>("TargetColor");
        private static int HeroLine => ArrowMenu["Heros"].GetValue<MenuSlider>("thinkness").Value;

        #endregion

        public ArrowDrawer()
        {
            ArrowMenu = new Menu("ArrowHelper", "ArrowHelper")
            {
                new Menu("Heros", "Heros")
                {
                    new MenuBool("DrawPlayer", "Draw On Me", false),
                    new MenuColor("PlayerColor", "My Arrow Color", new ColorBGRA(255, 255, 255, 198)),
                    new MenuBool("DrawAlly", "Draw On Ally", false),
                    new MenuColor("AllyColor", "Ally Arrow Color", new ColorBGRA(255, 255, 255, 198)),
                    new MenuBool("DrawEnemy", "Draw On Enemy", false),
                    new MenuColor("EnemyColor", "Enemy Arrow Color", new ColorBGRA(255, 255, 255, 198)),
                    new MenuBool("DrawTarget", "Draw On Selected Target"),
                    new MenuColor("TargetColor", "Target Arrow Color", new ColorBGRA(255, 255, 255, 198)),
                    new MenuSlider("thinkness", "Line thinkness", 13, 1, 30),
                    new MenuSlider("startp1", "HeaderRotate", -35, -200, 200),
                    new MenuSlider("startp2", "HeaderHeight", -94, -200, 200),
                    new MenuSlider("EndPoint1", "BottomRotate", 3, -200, 200),
                    new MenuSlider("EndPoint2", "BottomHeight", -54, -200, 200)
                },
                new Menu("minions", "minions")
                {
                    new MenuBool("DrawMinion", "Draw On Minion"),
                    new MenuColor("MinionColor", "Minion Arrow Color", new ColorBGRA(255, 255, 255, 198)),
                    new MenuSlider("thinkness", "Line thinkness", 6, 1, 30),
                    new MenuSlider("startp1", "HeaderRotate", -19, -200, 200),
                    new MenuSlider("startp2", "HeaderHeight", -47, -200, 200),
                    new MenuSlider("EndPoint1", "BottomRotate", 2, -200, 200),
                    new MenuSlider("EndPoint2", "BottomHeight", -26, -200, 200)
                }
            };


            MainMenu.UtilitiesMenu.Add(ArrowMenu);

            Drawing.OnDraw += DrawingOnOnEndScene;
        }

        private void DrawingOnOnEndScene(EventArgs args)
        {
            if (DrawOnPlayer) DrawPlayerArrow();
            if (DrawOnEnemy) DrawEnemyArrow();
            if (DrawOnAlly) DrawAllyArrow();
            if (DrawOnTarget) DrawSelectedtargetArrow();
            if (DrawOnMinion) DrawMinionArrow();
        }

        private static void DrawPlayerArrow()
        {
            if (Player.IsDead) return;
            var hpBar = Player.HPBarPosition;
            var startPoint = new Vector2(hpBar.X + SP1, hpBar.Y + SP2);
            var endPoint = new Vector2(hpBar.X + EP1, hpBar.Y + EP2);
            var startPoint12 = new Vector2(hpBar.X - SP1, hpBar.Y + SP2);
            var endPoint2 = new Vector2(hpBar.X - EP1, hpBar.Y + EP2);
            Drawing.DrawLine(startPoint, endPoint, HeroLine, Color.FromArgb(PlayerColor.Color.A,
                PlayerColor.Color.R, PlayerColor.Color.G, PlayerColor.Color.B));
            Drawing.DrawLine(startPoint12, endPoint2, HeroLine, Color.FromArgb(PlayerColor.Color.A,
                PlayerColor.Color.R, PlayerColor.Color.G, PlayerColor.Color.B));
        }

        private static void DrawSelectedtargetArrow()
        {
            var target = TargetSelector.SelectedTarget;
            if (target == null || !target.IsVisibleOnScreen) return;
            var hpBar = target.HPBarPosition;
            var startPoint = new Vector2(hpBar.X + SP1, hpBar.Y + SP2);
            var endPoint = new Vector2(hpBar.X + EP1, hpBar.Y + EP2);
            var startPoint12 = new Vector2(hpBar.X - SP1, hpBar.Y + SP2);
            var endPoint2 = new Vector2(hpBar.X - EP1, hpBar.Y + EP2);
            Drawing.DrawLine(startPoint, endPoint, HeroLine, Color.FromArgb(TargetColor.Color.A,
                TargetColor.Color.R, TargetColor.Color.G, TargetColor.Color.B));
            Drawing.DrawLine(startPoint12, endPoint2, HeroLine, Color.FromArgb(TargetColor.Color.A,
                TargetColor.Color.R, TargetColor.Color.G, TargetColor.Color.B));
        }

        private static void DrawAllyArrow()
        {
            var hpBars = GameObjects.AllyHeroes
                .Where(x => !x.IsMe && !x.IsDead && x.IsVisibleOnScreen && x.DistanceToPlayer() < 2000).ToList();
            foreach (var hpBar in hpBars)
            {
                var startPoint = new Vector2(hpBar.HPBarPosition.X + SP1, hpBar.HPBarPosition.Y + SP2);
                var endPoint = new Vector2(hpBar.HPBarPosition.X + EP1, hpBar.HPBarPosition.Y + EP2);
                var startPoint12 = new Vector2(hpBar.HPBarPosition.X - SP1, hpBar.HPBarPosition.Y + SP2);
                var endPoint2 = new Vector2(hpBar.HPBarPosition.X - EP1, hpBar.HPBarPosition.Y + EP2);
                Drawing.DrawLine(startPoint, endPoint, HeroLine, Color.FromArgb(AllyColor.Color.A,
                    AllyColor.Color.R, AllyColor.Color.G, AllyColor.Color.B));
                Drawing.DrawLine(startPoint12, endPoint2, HeroLine, Color.FromArgb(AllyColor.Color.A,
                    AllyColor.Color.R, AllyColor.Color.G, AllyColor.Color.B));
            }
        }

        private static void DrawEnemyArrow()
        {
            var hpBars = TargetSelector.GetTargets(2000,DamageType.Physical);

            foreach (var hpBar in hpBars)
            {
                if (!hpBar.IsVisibleOnScreen || hpBar.IsDead) break;
                var startPoint = new Vector2(hpBar.HPBarPosition.X + SP1, hpBar.HPBarPosition.Y + SP2);
                var endPoint = new Vector2(hpBar.HPBarPosition.X + EP1, hpBar.HPBarPosition.Y + EP2);
                var startPoint12 = new Vector2(hpBar.HPBarPosition.X - SP1, hpBar.HPBarPosition.Y + SP2);
                var endPoint2 = new Vector2(hpBar.HPBarPosition.X - EP1, hpBar.HPBarPosition.Y + EP2);
                Drawing.DrawLine(startPoint, endPoint, HeroLine, Color.FromArgb(EnemyColor.Color.A,
                    EnemyColor.Color.R, EnemyColor.Color.G, EnemyColor.Color.B));
                Drawing.DrawLine(startPoint12, endPoint2, HeroLine, Color.FromArgb(EnemyColor.Color.A,
                    EnemyColor.Color.R, EnemyColor.Color.G, EnemyColor.Color.B));
            }
        }

        private static void DrawMinionArrow()
        {
            if (Player.CharacterName.Equals("Zed"))
            {
                var lane = GameObjects.GetMinions(Player.Position, 2000)
                    .OrderBy(x => x.DistanceToPlayer())
                    .FirstOrDefault();
                if (lane == null) return;
                var dmg = Zed.passivedmg(lane);
                if (lane.IsVisibleOnScreen && dmg + Player.GetAutoAttackDamage(lane) > lane.Health)
                {
                    var hpBar1 = lane.HPBarPosition;


                    var mstartPoint = new Vector2(hpBar1.X + MinionSP1, hpBar1.Y + MinionSP2);
                    var mendPoint = new Vector2(hpBar1.X + MinionEP1, hpBar1.Y + MinionEP2);
                    var mstartPoint12 = new Vector2(hpBar1.X - MinionSP1, hpBar1.Y + MinionSP2);
                    var mendPoint2 = new Vector2(hpBar1.X - MinionEP1, hpBar1.Y + MinionEP2);
                    Drawing.DrawLine(mstartPoint, mendPoint, MinionLine, Color.FromArgb(MinionColor.Color.A,
                        MinionColor.Color.R, MinionColor.Color.G, MinionColor.Color.B));
                    Drawing.DrawLine(mstartPoint12, mendPoint2, MinionLine, Color.FromArgb(MinionColor.Color.A,
                        MinionColor.Color.R, MinionColor.Color.G, MinionColor.Color.B));
                }
            }

            var minioms = GameObjects.GetMinions(Player.Position, 2000)
                .Where(x => Player.GetAutoAttackDamage(x) >= x.Health).OrderBy(x => x.DistanceToPlayer())
                .FirstOrDefault();
            if (minioms != null)
                if (minioms.IsVisibleOnScreen)
                {
                    var hpBar1 = minioms.HPBarPosition;


                    var mstartPoint = new Vector2(hpBar1.X + MinionSP1, hpBar1.Y + MinionSP2);
                    var mendPoint = new Vector2(hpBar1.X + MinionEP1, hpBar1.Y + MinionEP2);
                    var mstartPoint12 = new Vector2(hpBar1.X - MinionSP1, hpBar1.Y + MinionSP2);
                    var mendPoint2 = new Vector2(hpBar1.X - MinionEP1, hpBar1.Y + MinionEP2);
                    Drawing.DrawLine(mstartPoint, mendPoint, MinionLine, Color.FromArgb(MinionColor.Color.A,
                        MinionColor.Color.R, MinionColor.Color.G, MinionColor.Color.B));
                    Drawing.DrawLine(mstartPoint12, mendPoint2, MinionLine, Color.FromArgb(MinionColor.Color.A,
                        MinionColor.Color.R, MinionColor.Color.G, MinionColor.Color.B));
                }
        }
    }
}