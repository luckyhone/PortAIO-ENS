﻿using System;
using EnsoulSharp;
using SharpDX;
using SharpDX.Direct3D9;

namespace Flowers_Series.Common
{
    public class HpBarDraw
    {
        public static Device DxDevice = Drawing.Direct3DDevice9;
        public static Line DxLine;
        public static float Hight = 9;
        public static float Width = 104;

        public static AIHeroClient Unit { get; set; }

        private static Vector2 Offset
        {
            get
            {
                if (Unit != null)
                {
                    return new Vector2(-45, -19);
                }

                return new Vector2();
            }
        }

        public static Vector2 StartPosition
        {
            get { return new Vector2(Unit.HPBarPosition.X + Offset.X, Unit.HPBarPosition.Y + Offset.Y); }
        }

        public HpBarDraw()
        {
            DxLine = new Line(DxDevice) { Width = 11 };

            Drawing.OnPreReset += OnPreReset;
            Drawing.OnPostReset += OnPostReset;
            AppDomain.CurrentDomain.DomainUnload += Unload;
            AppDomain.CurrentDomain.ProcessExit += Unload;
        }

        private static void Unload(object sender, EventArgs eventArgs)
        {
            DxLine.Dispose();
        }

        private static void OnPostReset(EventArgs args)
        {
            DxLine.OnResetDevice();
        }

        private static void OnPreReset(EventArgs args)
        {
            DxLine.OnLostDevice();
        }


        private static float GetHpProc(float dmg = 0)
        {
            float Health = ((Unit.Health - dmg) > 0) ? (Unit.Health - dmg) : 0;
            return (Health / Unit.MaxHealth);
        }

        private static Vector2 GetHpPosAfterDmg(float dmg)
        {
            float w = GetHpProc(dmg) * Width;
            return new Vector2(StartPosition.X + w, StartPosition.Y);
        }

        public static void DrawDmg(float dmg, ColorBGRA color)
        {
            Vector2 hpPosNow = GetHpPosAfterDmg(0);
            Vector2 hpPosAfter = GetHpPosAfterDmg(dmg);

            FullHPBar(hpPosNow, hpPosAfter, color);
        }

        private void FullHPBar(int to, int from, System.Drawing.Color color)
        {
            var sPos = StartPosition;

            for (var i = from; i < to; i++)
            {
                Drawing.DrawLine(sPos.X + i, sPos.Y, sPos.X + i, sPos.Y + 9, 1, color);
            }
        }

        private static void FullHPBar(Vector2 from, Vector2 to, ColorBGRA color)
        {
            DxLine.Begin();

            DxLine.Draw(new[]
            {
                new Vector2((int) from.X, (int) from.Y + 1f),
                new Vector2((int) to.X, (int) to.Y + 1f)
            }, color);

            DxLine.End();
        }
    }
}