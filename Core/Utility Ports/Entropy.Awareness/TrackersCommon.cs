using System.Collections.Generic;
using System.Linq;
using EnsoulSharp;
using EnsoulSharp.SDK;
using Entropy.Awareness.Models;
using Entropy.Lib.Render;
using SharpDX;
using SharpDX.Direct3D9;

namespace Entropy.Awareness
{
    public static class TrackersCommon
    {
        
        public static readonly TextureLoader TextureLoader = new TextureLoader();
        
        //Common Colors
        public static readonly Color Gray = new Color(75, 75, 75);

        public static void Initialize()
        {
            float Width = 105f, Height = 12f, XOffset = -46f, YOffset = -24f;
            var Y = Drawing.Height;
            if (Y > 1100)
            {
                Width = 0.0875f * Y;
                Height = 0.00875f * Y + 1f;
                XOffset = -0.0375f * Y - 1f;
                YOffset = -0.019375f * Y;
            }

            BackgroundOffset = new ResolutionOffset
            {
                Offset = new Vector2(XOffset - Width * 0.25f, YOffset - Height * 1.3f),
                Width = (int)(Width * 1.385f + Height * 0.45f),
                Height = (int)(Height * 3f)
            };

            BackgroundOffset2 = new ResolutionOffset
            {
                Offset = new Vector2(XOffset - Width * 0.25f, YOffset - Height * .5f - 1f),
                Width = (int)(Width * 0.275f),
                Height = (int)(Height * 0.25f)
            };

            BackgroundOffset3 = new ResolutionOffset
            {
                Offset = new Vector2(XOffset - Width * 0.025f, YOffset - Height * .35f),
                Width = (int)(Width * 1.06f - 1f),
                Height = (int)(Height * 0.25f)
            };

            BackgroundOffset4 = new ResolutionOffset
            {
                Offset = new Vector2(XOffset + Width * 1.025f, YOffset - Height),
                Width = (int)(Width * 0.1),
                Height = (int)(Height * 2.65)
            };

            SummonerOffset = new ResolutionOffset
            {
                Offset = new Vector2(XOffset + Width * 1.03f, YOffset - Height * 1.1f),
                Width = (int)(Height * 1.25f),
                Height = (int)(Height * 1.25f)
            };

            LineOffset = new ResolutionOffset
            {
                Offset = new Vector2(XOffset - Width * 0.23f, YOffset - Height * 1.1f),
                Width = (int)(Width * 1.225f),
                Height = (int)(Height * 0.4f)
            };

            ExperienceOffset = new ResolutionOffset
            {
                Offset = new Vector2(XOffset, YOffset - Height * 0.5f),
                Width = (int)(Width),
                Height = (int)(Height * 0.25f)
            };

            Heroes  = GameObjects.Heroes.Where(x => !x.CharacterName.Equals("PracticeTool_TargetDummy")).ToList();
            Enemies = GameObjects.EnemyHeroes.Where(x => !x.CharacterName.Equals("PracticeTool_TargetDummy")).ToList();
            Allies = GameObjects.AllyHeroes.Where(x => !x.IsMe && !x.CharacterName.Equals("PracticeTool_TargetDummy")).ToList();

        }

        public static List<AIHeroClient> Heroes = new List<AIHeroClient>();
        public static List<AIHeroClient> Enemies = new List<AIHeroClient>();
        public static List<AIHeroClient> Allies = new List<AIHeroClient>();
        
        public static readonly Surface DefaultSurface = Drawing.Direct3DDevice9.GetRenderTarget(0);

        public static ResolutionOffset BackgroundOffset;
        public static ResolutionOffset BackgroundOffset2;
        public static ResolutionOffset BackgroundOffset3;
        public static ResolutionOffset BackgroundOffset4;
        public static ResolutionOffset LineOffset;
        public static ResolutionOffset ExperienceOffset;
        public static ResolutionOffset SummonerOffset;
    }
}