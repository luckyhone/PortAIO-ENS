using System;
using EnsoulSharp;
using EnsoulSharp.SDK;
using Entropy.Lib.Render;

namespace Entropy.AIO.Camille
{
    #region

    using Bases;
    using Misc;
    using SharpDX;
    using static Components;
    using static Bases.ChampionBase;

    #endregion

    class Drawings : DrawingBase
    {
        public Drawings(params Spell[] spells)
        {
            this.Spells = spells;
        }

        public static void OnRender(EventArgs args)
        {
            //if (Definitions.WDirection != Vector3.Zero)
            //{
            //	target = TargetSelector.GetBestTarget(ChampionBase.W.Range);
            //	if (target == null)
            //	{
            //		return;
            //	}

            //	var actualEnd = LocalPlayer.Instance.Position + Definitions.WDirection * ChampionBase.W.Range;
            //	var movePos = target.Position.Extend(actualEnd, -490);
            //	new Line(LocalPlayer.Instance.Position, actualEnd).Render(Color.Red, 2f);
            //	new Line(LocalPlayer.Instance.Position, movePos).Render(Color.Green, 2f);
            //}
            //TextRendering.Render($"Hooked: {Definitions.HasHookedWall} || OnWall: {Definitions.IsOnWall}", Color.Red, LocalPlayer.Instance.Position);
            if (E.IsReady())
            {
                var target = TargetSelector.GetTarget(E.Range + 800f,DamageType.Physical);
                if (target == null)
                {
                    return;
                }

                if ((target.Position.IsUnderEnemyTurret()) &&
                    ComboMenu.EUnderTurretBool.Enabled)
                {
                    return;
                }

                var walls    = ObjectManager.Player.Position.RotateAround(E.Range, 60).GetWalls();
                var bestWall = walls.GetBestWallToTarget(target, E.Range);
                if (bestWall.IsZero)
                {
                    return;
                }

                new Line(ObjectManager.Player.Position, bestWall).Render(Color.Red, 2f);
                new Line(bestWall, target.Position).Render(Color.Red, 2f);
            }

            //TextRendering.Render($"{Definitions.QState}", Color.Black, new []{LocalPlayer.Instance.Position});
            //      var walls = LocalPlayer.Instance.Position.RotateAround(1100, 60).GetWalls().ToArray();
            //TextRendering.Render("x", Color.Red, walls);
        }
    }
}