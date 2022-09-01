using System;
using System.Collections.Generic;
using System.Drawing;
using EnsoulSharp;
using EnsoulSharp.SDK;
using EnsoulSharp.SDK.MenuUI;
using Entropy.Awareness.Bases;
using Entropy.Awareness.Helpers;
using Entropy.Awareness.Managers;
using Entropy.Awareness.Models.RenderObjects;
using EzAIO.Extras;
using LeagueSharpCommon;
using PortAIO.Core.Utility_Ports.Entropy.Awareness.Resources.HUDTracker;
using SharpDX;
using SharpDX.Direct3D9;
using RectangleF = System.Drawing.RectangleF;

namespace Entropy.Awareness.Trackers
{
    public class HUDTracker : TextureTrackerBase
    {
        public HUDTracker() : base("HUDTracker")
        {
        }

        public static Texture Frame;
        public static Texture UltimateCircle;

        public const int FrameSize = 128;
        public const int UltimateCircleSize = 24;

        public override void Initialize()
        {
            //Loading Frame Texture
            {
                var bitmap = (Bitmap)HudTrackerResources.ResourceManager.GetObject("Frame");

                var cutBitmap = BitmapHelper.ResizeImage(bitmap, new Size(128, 128));
            
                Frame = TrackersCommon.TextureLoader.Load( cutBitmap, out _);
            }
            //Loading Utltimate Circle Texture
            {
                var bitmap = (Bitmap)HudTrackerResources.ResourceManager.GetObject("UltCircle");

                var cutBitmap = BitmapHelper.ResizeImage(bitmap, new Size(24, 24));
            
                UltimateCircle = TrackersCommon.TextureLoader.Load( cutBitmap, out _);
            }
            
            HUDChampions.Add(new HUDChampion(Vector2.Zero, InformationManager.ChampionInformations[(uint)ObjectManager.Player.NetworkId]));
            
            //TODO
            CreateTexture(128, 128);
            UpdatePosition(new Vector2(500,500));
            
            Game.OnWndProc += GameOnOnWndProc;
            
            GameEvent.OnGameTick += OnUpdateTick;
        }
        

        private void OnUpdateTick(EventArgs args)
        {
            foreach (var hudChampion in HUDChampions)
            {
                hudChampion.UpdateInformation();
            }
            mouse = new RectangleF(Game.CursorPos.X, Game.CursorPos.Y, 5, 5);

        }


        private void GameOnOnWndProc(GameWndEventArgs args)
        {
            if (Dragging)
            {
                UpdatePosition(Game.CursorPos.To2D() + DragOffset);
            }

            if (args.Msg == (uint)WindowsMessages.WM_LBUTTONDOWN && MouseOverArea)
            {
                DragOffset = TexturePosition - Game.CursorPos.To2D();
                Dragging           = true;
            }

            else if (Dragging && args.Msg == (uint)WindowsMessages.WM_LBUTTONUP)
            {
                Dragging = false;
            }
        }

        private static Vector2 DragOffset;
        
        private readonly List<HUDChampion> HUDChampions = new List<HUDChampion>();

        public override void BuildTexture()
        {
            foreach (var hudChampion in HUDChampions)
            {
                hudChampion.Render(Vector2.Zero);
            }
        }

        private void UpdatePosition(Vector2 position)
        {
            //Clipping

            var screenX = Drawing.Width;
            var screenY = Drawing.Height;

            if (screenY - Game.CursorPos.Y < Height)
            {
                position.Y = screenY - Height;
            }
            
            if (Game.CursorPos.Y < Height)
            {
                position.Y = 0;
            }
            
            if (screenX - Game.CursorPos.X < Width)
            {
                position.X = screenX - Width;
            }
            
            if (Game.CursorPos.X < Width)
            {
                position.X = 0;
            }

            TexturePosition = position;

            Rectangle = new RectangleF(
                position.X,
                position.Y,
                Width,
                Height);

            foreach (var championHud in HUDChampions)
            {
                championHud.Offset = new Vector2();
            }

        }

        private bool Dragging;
        private static RectangleF Rectangle;
        private static RectangleF mouse;
        
        
        private static bool MouseOverArea => Rectangle.Contains(mouse);
        
        private static class MenuComponents
        {
            public static readonly MenuSlider Size =
                new MenuSlider("size", "Size %", 100);

            public static readonly MenuList HUDStyle =
                new MenuList("style", "HUD Style", new[] {"Compact", "Squared"});

            public static readonly MenuSlider XSlider =
                new MenuSlider("xSlider",
                               "X Position",
                               (int) (Drawing.Width * XMultiplier),
                               0,
                               (int) Drawing.Width);

            public static readonly MenuSlider YSlider =
                new MenuSlider("ySlider",
                               "Y Position",
                               (int) (Drawing.Height * YMultiplier),
                               0,
                               (int) Drawing.Height);
        }

        private const float XMultiplier = 0.6145833334f;
        private const float YMultiplier = 0.7407407407f;
    }
}