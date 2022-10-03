using System;
using System.Runtime.Serialization;
using EnsoulSharp.SDK;
using Entropy.Lib.Render;
using PortAIO.Library_Ports.Entropy.Lib.Geometry;
using ObjectManager = EnsoulSharp.ObjectManager;

namespace Entropy.AIO.Irelia
{
    #region

    using Bases;
    using Misc;
    using SharpDX;
    using static Components;

    #endregion

    class Drawings : DrawingBase
    {
        public Drawings(params Spell[] spells)
        {
            this.Spells = spells;
        }

        public static void OnRender(EventArgs args)
        {
            var pos       = ObjectManager.Player.Position.WorldToScreen();
            var farmText  = "Only Marked: ";
            var farmText2 = ComboMenu.markedKey.Active ? "ON" : "OFF";
            TextRendering.Render(farmText,
                Color.White,
                new Vector2(pos.X - 54, pos.Y + 10));
            TextRendering.Render(farmText2,
                ComboMenu.markedKey.Active ? Color.Lime : Color.Red,
                new Vector2(pos.X + 50, pos.Y + 10));
            if (Drawing.KillableMinion.Enabled)
            {
                foreach (var minion in MinionManager.KillableMinions)
                {
                    if (minion.IsValidTarget())
                    {
                        CircleRendering.Render(Color.Yellow, 50, minion, 2f);
                    }
                }
            }
        }
    }
}