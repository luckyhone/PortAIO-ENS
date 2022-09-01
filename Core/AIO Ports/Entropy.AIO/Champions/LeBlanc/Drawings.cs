using System;
using EnsoulSharp;
using EnsoulSharp.SDK;
using Entropy.Lib.Render;
using PortAIO.Library_Ports.Entropy.Lib.Geometry;
using SharpDX;

namespace Entropy.AIO.LeBlanc
{
    using static Components;
    using Bases;
    using Misc;
    class Drawings : DrawingBase
    {
        public Drawings(params Spell[] spells)
        {
            this.Spells = spells;
        }

        public static void OnRender(EventArgs args)
        {

            if (DrawsMenu.RModeBool.Enabled)
            {
                var pos = ObjectManager.Player.Position.WorldToScreen();
                TextRendering.Render($"R Mode: {Definitions.GetRMode.ToString()}",
                    Color.White,
                    new Vector2(pos.X - 40, pos.Y + 70));
                TextRendering.Render($"R State: {Definitions.GetRState().ToString()}",
                    Color.White,
                    new Vector2(pos.X - 40, pos.Y + 90));
            }
        }
    }
}