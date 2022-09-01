using EnsoulSharp;
using Entropy.Lib.Render;
using LeagueSharpCommon;

namespace Entropy.Awareness.Models.RenderObjects
{
    using System;
    using Bases;
    using SharpDX;
    using Trackers;

    public class SpellLine : RenderObject
    {
        private static readonly Vector2 MinuteTextAlignment =
            new Vector2(2, 8);

        private static readonly Vector2 SecondsTextAlignment =
            new Vector2(SpellTracker.Font.MeasureText("00").Width / 4f, 8);

        private static readonly Vector2 SecondTextAlignment =
            new Vector2(SpellTracker.Font.MeasureText("0").Width / 2f, 8);

        private readonly Color  Green = new Color(164, 215, 71);
        public           string CooldownText;

        public Vector2 CooldownTextOffset;
        public bool    DrawCooldown;

        private Color LineColor;

        public SpellInformation RenderInformation;

        public int SpellLevelWidth;

        private float BackLineWidth;
        private float BackLineHeight;

        public SpellLine(Vector2 offset, SpellSlot slot, int width, int height) : base("SpellLine", offset, width,height)
        {
            if (slot == SpellSlot.R)
            {
                SpellLevelWidth = Width / 3;
            }
            else
            {
                SpellLevelWidth = Width / 5;
            }
            BackLineHeight = Height + 2;
            BackLineWidth = Width + 2;
        }

        private float LineWidth;

        public override void BuildRenderObject(Vector2 position)
        {
            RectangleRendering.Render(position - 1, BackLineWidth, BackLineHeight, Color.Black);
            RectangleRendering.Render(position, LineWidth, Height, LineColor);

            if (DrawCooldown)
            {
                TextRendering.Render(CooldownText, Color.Black, SpellTracker.Font, position + 1 + CooldownTextOffset);
                TextRendering.Render(CooldownText, Color.White, SpellTracker.Font, position     + CooldownTextOffset);
            }

            if (!MenuComponents.SpellTracker.SpellLevels.Enabled)
            {
                return;
            }

            for (var i = 0; i < RenderInformation.Level; i++)
            {
                var spellOffset = new Vector2(SpellLevelWidth * i + 3, Height / 2f);
                CircleRendering2D.Render(position + spellOffset, 1f, Color.Black, 0f, true);
                //CircleRendering2D.Render(position + spellOffset, 1f, Color.White, 0f, true);
            }
        }

        public override void UpdateInformation()
        {
            if (!RenderInformation.Learned)
            {
                LineColor = Color.Black;
                return;
            }

            if (RenderInformation.IsReady)
            {
                LineColor = Green;
                LineWidth = Width;
                return;
            }

            //Not ready so calculating render offsets
            if (RenderInformation.TimeUntilReady >= 60)
            {
                CooldownText       = TimeSpan.FromSeconds(RenderInformation.TimeUntilReady).ToString(@"m\:ss");
                CooldownTextOffset = MinuteTextAlignment;
            }

            else if (RenderInformation.TimeUntilReady >= 10)
            {
                CooldownText       = ((int) RenderInformation.TimeUntilReady).ToString();
                CooldownTextOffset = SecondsTextAlignment;
            }
            else
            {
                CooldownText       = ((int) RenderInformation.TimeUntilReady).ToString();
                CooldownTextOffset = SecondTextAlignment;
            }
            

            //LineColor = ColorConversion.HSL2RGB(mappedHueValue, 1D, 0.5d);
            LineWidth = Width * RenderInformation.Progress;
        }
    }
}