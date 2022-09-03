using System;
using EnsoulSharp;
using EnsoulSharp.SDK;
using Entropy.AIO.Bases;
using Entropy.AIO.Cassiopeia.Misc;
using Entropy.Lib.Render;
using PortAIO.Library_Ports.Entropy.Lib.Geometry;
using SharpDX;

namespace Entropy.AIO.Cassiopeia
{
    using static Components;

    class Drawings : DrawingBase
    {
        public Drawings(params Spell[] spells)
        {
            this.Spells = spells;
        }

        protected void OnRender(EventArgs args)
        {
            base.OnRender(args);
            var pos       = ObjectManager.Player.Position.WorldToScreen();
            var farmText  = "Farm Mode: ";
            var farmText2 = LaneClearMenu.farmKey.Active ? "PASSIVE" : "PUSH";

            TextRendering.Render(farmText,
                Color.White,
                Definitions.Font,
                new Vector2(pos.X - 54, pos.Y + 5));

            TextRendering.Render(farmText2,
                Color.LightPink,
                Definitions.Font,
                new Vector2(pos.X + 32, pos.Y + 5));
        }
    }
}