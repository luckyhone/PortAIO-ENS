using System;
using EnsoulSharp;
using EnsoulSharp.SDK;
using EnsoulSharp.SDK.MenuUI;
using SharpDX;
using SharpDX.Direct3D9;
using Font = SharpDX.Direct3D9.Font;

using LeagueSharpCommon;
using Render = LeagueSharpCommon.Render;

namespace CS_Counter
{
    class CsCounter
    {
        #region vars

        public const int XOffset = 15;
        public const int YOffset = 35;

        public static Line Line;

        public static readonly Render.Text Text = new Render.Text(
            0, 0, "", 12, new ColorBGRA(red: 255, green: 0, blue: 0, alpha: 255), "Verdana");

        public static Font Textx;

        public static int Minionsgesamt;
        public static int Percent;

        private static Texture CdFrameTexture;
        private static Sprite Sprite;

        public static int X;
        public static int Y;

        public static bool Enabled = true;

        public static long MinCount;

        #endregion
        
        public static void Loads()
        {
            Game_OnGameLoad(new EventArgs());
        }

        private static void Game_OnGameLoad(EventArgs args)
        {
            //if (Game.MapId != GameMapId.CrystalScar) { return; }

            Init.PrepareMenu();

            Minionsgesamt = Game.Time < 60 ? 0 : GameObjects.Player.MinionsKilled;

            Drawing.OnDraw += Drawing_OnDraw;
            GameObject.OnCreate += Obj_AI_Minion_OnCreate;


        }

        private static void Obj_AI_Minion_OnCreate(GameObject sender, EventArgs args)
        {
            if (GameObjects.Player.Team == GameObjectTeam.Chaos && sender.Name.Contains("Minion_T200"))
            {
                MinCount++;

            }
            else if (GameObjects.Player.Team == GameObjectTeam.Order && sender.Name.Contains("Minion_T100"))
            {
                MinCount++;
            }
        }

        private static void Drawing_OnDraw(EventArgs args)
        {
            foreach (var hero in ObjectManager.Get<AIHeroClient>())
            {

                int mingesamt = 0;

                if (Game.MapId == GameMapId.HowlingAbyss)
                {
                    mingesamt = (int)MinCount + Minionsgesamt;
                }
                else if (Game.MapId == GameMapId.TwistedTreeline)
                {
                    mingesamt = (int)MinCount / 2 + Minionsgesamt;
                }
                else if (Game.MapId == GameMapId.SummonersRift)
                {
                    mingesamt = (int)MinCount / 3 + Minionsgesamt;
                }

                if (hero.IsDead | !hero.IsVisible | !Init.Menuenable2.GetValue<MenuBool>().Enabled |
                    (hero.IsAlly && !Init.Menuenable4.GetValue<MenuBool>().Enabled && !hero.IsMe) | (hero.IsMe && !Init.Menuenable3.GetValue<MenuBool>().Enabled))
                {
                    Text.text = "";
                    continue;
                }

                var cs = hero.MinionsKilled + hero.NeutralMinionsKilled + 1;
                var pos = Drawing.WorldToScreen(hero.Position);
                pos.X -= 50 + Init.XPos.GetValue<MenuSlider>().Value;
                pos.Y += 20 + Init.YPos.GetValue<MenuSlider>().Value;

                if (hero.IsMe && Init.Menuenable3.GetValue<MenuBool>().Enabled)
                {

                    Text.X = (int)pos.X;
                    Text.X += 110 / 6;
                    Text.Y = (int)pos.Y;
                    Text.Color = new ColorBGRA(red: 255, green: 255, blue: 255, alpha: 255);

                    Percent = mingesamt != 0 ? (cs * 100 / mingesamt) : 0;

                    if (!Init.Advanced.GetValue<MenuBool>().Enabled)
                    {

                        if (!Init.Advanced_box.GetValue<MenuBool>().Enabled)
                        {
                            DrawBoxes(pos.X, pos.Y);
                        }

                        Text.text = Percent + " %";

                    }
                    else
                    {

                        if (!Init.Advanced_box.GetValue<MenuBool>().Enabled)
                        {
                            DrawBoxes(pos.X, pos.Y);
                        }

                        Text.text = Percent + " %" + " |  " + cs + " / " + mingesamt;
                    }

                    Text.OnEndScene();

                    continue;

                }

                if (!Init.Advanced_box.GetValue<MenuBool>().Enabled)
                {
                    DrawBoxes(pos.X, pos.Y);
                }

                Text.X = (int)pos.X;
                Text.X += 110 / 6;

                Text.Y = (int)pos.Y;
                Text.Color = new ColorBGRA(red: 255, green: 255, blue: 255, alpha: 255);
                Text.text = "CS Count: " + cs;
                Text.OnEndScene();

            }
        }
        
        internal static void DrawBoxes(float x, float y)
        {
            Line.Begin();
            Line.Draw(new[] { new Vector2(x, y - 2), new Vector2(x + 100, y - 2) },
                new ColorBGRA(255, 255, 255, 255));
            Line.End();

            Line.Begin();
            Line.Draw(new[] { new Vector2(x, y + 14), new Vector2(x + 100, y + 14) },
                new ColorBGRA(255, 255, 255, 255));
            Line.End();

            Line.Begin();
            Line.Draw(new[] { new Vector2(x, y - 2), new Vector2(x, y + 14) },
                new ColorBGRA(255, 255, 255, 255));
            Line.End();

            Line.Begin();
            Line.Draw(new[] { new Vector2(x + 100, y - 2), new Vector2(x + 100, y + 14) },
                new ColorBGRA(255, 255, 255, 255));
            Line.End();
        }
    }
}