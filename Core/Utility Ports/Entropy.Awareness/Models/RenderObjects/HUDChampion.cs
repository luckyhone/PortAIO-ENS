using EnsoulSharp;
using Entropy.Lib.Render;
using PortAIO.Core.Utility_Ports.Entropy.Awareness.Resources.SummonerSpells;

namespace Entropy.Awareness.Models.RenderObjects
{
    using System;
    using System.Drawing;
    using System.Linq;
    using Bases;
    using Helpers;
    using SharpDX;
    using SharpDX.Direct3D9;
    using Trackers;
    using Color = SharpDX.Color;
    using Font = SharpDX.Direct3D9.Font;

    public class HUDChampion : RenderObject
    {
        public static readonly Font Font = FontFactory.CreateNewFont(16, FontWeight.SemiBold);

        public readonly Texture ChampionTexture;
        public readonly float   CircleRadius;

        public readonly int     LineThickness;
        public readonly int     LineWidth;
        public readonly Texture Summoner1Texture;
        public readonly Texture Summoner2Texture;
        public          Vector2 CirclePosition;

        public Vector2 CircleTexturePosition;

        public string CooldownText = "9";

        public bool DrawCD = true;

        public Vector2 FontOffset;

        public Color   HPColor = new Color(14, 179, 68);
        public Vector2 HPEndPosition;

        public Vector2 HPStartPosition;
        public Color   MPColor = new Color(0, 57, 215);
        public Vector2 MPEndPosition;
        public Vector2 MPStartPosition;
        public bool    Orange = true;
        public Vector2 Summoner1Position;
        public Vector2 Summoner2Position;

        public bool UltimateReady = true;

        public ChampionInformation ChampionInformation { get; set; }

        public HUDChampion(Vector2 offset, ChampionInformation championInformation) : base("HUDChampion", 
        offset,128,128)
        {
            ChampionInformation = championInformation;
            
            CircleTexturePosition = new Vector2(HUDTracker.FrameSize - HUDTracker.UltimateCircleSize, 0);
            CircleRadius          = HUDTracker.UltimateCircleSize / 2f - 3f;
            CirclePosition = new Vector2(HUDTracker.FrameSize - HUDTracker.UltimateCircleSize + CircleRadius + 3,
                                         CircleRadius                                                        + 3);

            var x   = 8;
            var hpY = 97;
            var mpY = 114;

            LineThickness = 14;
            LineWidth     = 113;

            HPStartPosition = new Vector2(x, hpY);
            HPEndPosition   = new Vector2(x + LineWidth, hpY);

            MPStartPosition = new Vector2(x, mpY);
            MPEndPosition   = new Vector2(x + LineWidth, mpY);

            FontOffset = new Vector2(2, 7);

            //Loading champ Texture
            {
                var bitmap  = new Bitmap($@"{BitmapHelper.GetResourcePath(ChampionInformation.Source.CharacterName)}\Portrait.bmp");
                var resized = BitmapHelper.ResizeImage(bitmap, new Size(69, 72));

                ChampionTexture = TrackersCommon.TextureLoader.Load(resized, out _);
            }
            //Summoner 1
            {
                var sum1 = ChampionInformation.Source.Spellbook.GetSpell(SpellSlot.Summoner1).SData.Name;
                var bitmap = (Bitmap) SummonerSpellResource.ResourceManager.GetObject(sum1);

                var resized = BitmapHelper.ResizeImage(bitmap, new Size(36, 36));

                Summoner1Texture = TrackersCommon.TextureLoader.Load(resized, out _);
            }
            //Summoner 2
            {
                var sum2 = ChampionInformation.Source.Spellbook.GetSpell(SpellSlot.Summoner2).SData.Name;
                var bitmap = (Bitmap) SummonerSpellResource.ResourceManager.GetObject(sum2);

                var resized = BitmapHelper.ResizeImage(bitmap, new Size(36, 36));

                Summoner2Texture = TrackersCommon.TextureLoader.Load(resized, out _);
            }

            Summoner1Position = new Vector2(9, 10);
            Summoner2Position = new Vector2(9, 50);
        }

        public override void BuildRenderObject(Vector2 position)
        {
            //Frame
            TextureRendering.Render(Vector2.Zero, HUDTracker.Frame);

            //Champ
            TextureRendering.Render(new Vector2(49, 10), ChampionTexture);

            //Summoner1
            if (ChampionInformation.Summoner1.IsReady)
            {
                TextureRendering.Render(Summoner1Position, Summoner1Texture);
            }
            else
            {
                TextureRendering.Render(Summoner1Position, Summoner1Texture, TrackersCommon.Gray);
            }

            //Summoner2
            if (ChampionInformation.Summoner2.IsReady)
            {
                TextureRendering.Render(Summoner2Position, Summoner2Texture);
            }
            else
            {
                TextureRendering.Render(Summoner2Position, Summoner2Texture, TrackersCommon.Gray);
            }

            //Circle
            if (UltimateReady)
            {
                TextureRendering.Render(CircleTexturePosition, HUDTracker.UltimateCircle);
            }
            else
            {
                TextureRendering.Render(CircleTexturePosition, HUDTracker.UltimateCircle);
            }


            if (DrawCD)
            {
                TextRendering.Render(CooldownText, Color.Black, Font, CirclePosition              - FontOffset);
                TextRendering.Render(CooldownText, Color.White, Font, CirclePosition - FontOffset - 1);
            }


            //HP Bar
            LineRendering.Render(HPColor, LineThickness, HPStartPosition, HPEndPosition);
            //MP Bar
            LineRendering.Render(MPColor, LineThickness, MPStartPosition, MPEndPosition);
        }

        public override void UpdateInformation()
        {
            HPEndPosition = new Vector2(HPStartPosition.X + LineWidth * (ChampionInformation.Source.HealthPercent / 100f),
                                        HPStartPosition.Y);
        }
    }
}