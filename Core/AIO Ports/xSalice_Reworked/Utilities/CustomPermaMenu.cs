using System.Linq;
using EnsoulSharp;
using EnsoulSharp.SDK.MenuUI;

namespace xSalice_Reworked.Utilities
{
    using Base;
    using System;
    using System.Collections.Generic;
    using LeagueSharpCommon;
    using SharpDX;
    using PortAIO.Properties;
    using Menu = EnsoulSharp.SDK.MenuUI;
    public class CustomPermaMenu
    {
        private static int _menuX = (int)(Drawing.Width * 0.90f);
        private static int _menuY = (int)(Drawing.Height * 0.58f);

        private static readonly HashSet<PermaMenu> MyPermaMenus = new HashSet<PermaMenu>();

        private static readonly Render.Sprite MySprite = new Render.Sprite(Resources.banner, Vector2.Zero);
        private static readonly Render.Line MyLine = new Render.Line(Vector2.Zero, Vector2.Zero, 1, new ColorBGRA(209, 179, 40, 255));
        private static readonly Render.Line MyLine2 = new Render.Line(Vector2.Zero, Vector2.Zero, 1, new ColorBGRA(209, 179, 40, 255));
        private static readonly Render.Line MyLine3 = new Render.Line(Vector2.Zero, Vector2.Zero, 1, new ColorBGRA(209, 179, 40, 255));
        private static readonly Render.Line MyLine4 = new Render.Line(Vector2.Zero, Vector2.Zero, 1, new ColorBGRA(209, 179, 40, 255));

        public CustomPermaMenu()
        {
            Drawing.OnDraw += Drawing_OnDraw;
        }

        public Menu.MenuItem AddToMenu(string text, string source)
        {
            if (text == null)
                throw new ArgumentNullException(nameof(text));

            if (source == null)
                throw new ArgumentNullException(nameof(source));

            var newMenu = new PermaMenu(text, source);
            MyPermaMenus.Add(newMenu);
            return newMenu.MenuItem;
        }

        protected virtual void Drawing_OnDraw(EventArgs arg)
        {
            if (!Champion.Menu["enableCustMenu"].GetValue<MenuBool>().Enabled)
                return;

            if (Champion.Menu["custMenu"].GetValue<MenuKeyBind>().Active)
            {
                _menuX = (int) Drawing.WorldToScreen(Game.CursorPos).X;
                _menuY = (int) Drawing.WorldToScreen(Game.CursorPos).Y;
            }

            var yOffset = 0;

            foreach (var obj in MyPermaMenus.Where(obj => obj != null).Where(obj => obj.MenuItem.GetValue<MenuBool>().Enabled))
            {
                obj.RenderTxt.X = _menuX;
                obj.RenderTxt.Y = _menuY + yOffset;

                try
                {
                    if (Champion.Menu[obj.Source].GetValue<MenuKeyBind>().Active)
                    {
                        obj.RenderTxt.Color = new ColorBGRA(209, 179, 40, 255);
                        obj.RenderTxt.text = obj.Text + "On";
                    }
                    else
                    {
                        obj.RenderTxt.Color = new ColorBGRA(255, 0, 0, 255);
                        obj.RenderTxt.text = obj.Text + "Off";
                    }
                }
                catch (Exception e)
                {
                    // ignored
                }

                obj.RenderTxt.OnEndScene();
                yOffset += 20;
            }

            //sprite
            MySprite.X = _menuX - 12;
            MySprite.Y = _menuY - 35;
            MySprite.OnEndScene();

            //line 1
            MyLine.Start = new Vector2(_menuX - 10, _menuY - 10);
            MyLine.End = new Vector2(_menuX - 10, _menuY + yOffset + 10);
            MyLine.OnEndScene();

            //line 2
            MyLine2.Start = new Vector2(_menuX + 135, _menuY - 10);
            MyLine2.End = new Vector2(_menuX + 135, _menuY + yOffset + 10);
            MyLine2.OnEndScene();

            //line 3
            MyLine3.Start = new Vector2(_menuX + 135, _menuY - 10);
            MyLine3.End = new Vector2(_menuX - 10, _menuY - 10);
            MyLine3.OnEndScene();

            //line 3
            MyLine4.Start = new Vector2(_menuX + 135, _menuY + yOffset + 10);
            MyLine4.End = new Vector2(_menuX - 10, _menuY + yOffset + 10);
            MyLine4.OnEndScene();
        }
    }

    internal class PermaMenu : IDisposable
    {
        internal readonly Render.Text RenderTxt;
        internal readonly string Text;
        internal readonly string Source;
        internal readonly Menu.MenuItem MenuItem;
            

        public PermaMenu(string text,  string source)
        {
            Source = source;
            Text = text;
            RenderTxt = new Render.Text(0, 0, "text", 16, new ColorBGRA(209, 179, 40, 255), "monospace");
            MenuItem = new MenuBool(text, text, true).SetValue(true);
        }

        public void Dispose()
        {
            RenderTxt.Dispose();
        }
    }
}