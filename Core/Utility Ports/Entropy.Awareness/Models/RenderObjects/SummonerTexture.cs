using System;
using System.Drawing;
using Entropy.Awareness.Bases;
using Entropy.Awareness.Helpers;
using Entropy.Awareness.Trackers;
using Entropy.Lib.Render;
using LeagueSharpCommon;
using PortAIO.Core.Utility_Ports.Entropy.Awareness.Resources.SummonerSpells;
using SharpDX;
using SharpDX.Direct3D9;
using Color = SharpDX.Color;

namespace Entropy.Awareness.Models.RenderObjects
{
    public class SummonerTexture : RenderObject
    {
        private static Vector2 MinuteTextAlignment;

        private static Vector2 SecondsTextAlignment;

        private static Vector2 SecondTextAlignment;

        private static bool created;

        private static Color LineColor = new Color(164, 215, 71);

        public string CooldownText;

        public Vector2 CooldownTextOffset;

        public SpellInformation RenderInformation;

        public SummonerTexture(Vector2 offset, string sumName, int width, int height) : base("SummonerTexture", offset, width, height)
        {
            var bitmap = (Bitmap) SummonerSpellResource.ResourceManager.GetObject(sumName);

            var cutBitmap = BitmapHelper.ResizeImage(bitmap, new Size(width, height));

            Texture = TrackersCommon.TextureLoader.Load(cutBitmap, out _);

            CreateAlignments();

            CooldownTextOffset = MinuteTextAlignment;
        }

        private static void CreateAlignments()
        {
            if (created)
            {
                return;
            }

            created = true;
            //0:00 Size
            var minuteSize = SpellTracker.Font.MeasureText("0:00");
            MinuteTextAlignment = new Vector2(1, minuteSize.Height / 6f);

            var secondsSize = SpellTracker.Font.MeasureText("00");
            SecondsTextAlignment = new Vector2(secondsSize.Width / 6f, secondsSize.Height / 6f);

            var secondSize = SpellTracker.Font.MeasureText("0");
            SecondTextAlignment = new Vector2(secondSize.Width / 3f, secondSize.Height / 6f);
        }

        public override void BuildRenderObject(Vector2 position)
        {
            try
            {
                if (RenderInformation.IsReady)
                {
                    TextureRendering.Render(position, Texture);
                    return;
                }

                TextureRendering.Render(position, Texture, TrackersCommon.Gray);

                TextRendering.Render(CooldownText, Color.Black, SpellTracker.Font, position + 1 + CooldownTextOffset);
                TextRendering.Render(CooldownText, Color.White, SpellTracker.Font, position + CooldownTextOffset);
            }catch(Exception e){ // inore
            }
        }


        public override void UpdateInformation()
        {
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
        }
    }
}