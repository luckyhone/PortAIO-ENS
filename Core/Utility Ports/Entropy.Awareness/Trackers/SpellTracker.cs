using Entropy.Awareness.Bases;
using Entropy.Awareness.Managers;
using Entropy.Awareness.Models.RenderObjects;
using Entropy.Lib.Render;
using SharpDX;
using SharpDX.Direct3D9;

namespace Entropy.Awareness.Trackers
{
    public class SpellTracker : TrackerBase
    {
        public static readonly Font Font = FontFactory.CreateNewFont(14, FontWeight.Normal);

        public override void Initialize()
        {
            //Calculating offsets,width and etc that is needed to draw

            var lineWidth = (int)((TrackersCommon.LineOffset.Width - 5f) / 4f);

            foreach (var information in InformationManager.ChampionInformations.Values)
            {
                var i = 0;
                foreach (var spellInformation in information.SpellInformations)
                {
                    var xOffset = i * (lineWidth + 3f) + TrackersCommon.LineOffset.Offset.X;
                    var yOffset = TrackersCommon.LineOffset.Offset.Y;
                    var offset  = new Vector2(xOffset, yOffset);

                    spellInformation.SpellRenderObject =
                        new SpellLine(offset, spellInformation.Slot, lineWidth, TrackersCommon.LineOffset.Height);
                    i++;
                }

                var j = 0;
                foreach (var spellInformation in information.SummonerInformations)
                {
                    var xOffset = TrackersCommon.SummonerOffset.Offset.X;
                    var yOffset = j * 1.1f * TrackersCommon.SummonerOffset.Height + TrackersCommon.SummonerOffset.Offset.Y;

                    var offset = new Vector2(xOffset, yOffset);

                    spellInformation.SummonerTexture = new SummonerTexture(offset,
                                                                           information.Source.Spellbook.GetSpell(spellInformation.Slot).SData.Name,
                                                                           TrackersCommon.SummonerOffset.Width,
                                                                           TrackersCommon.SummonerOffset.Height);
                    j++;
                }
            }

            //Loading summoner textures
        }

        public override void WorldRender()
        {
            foreach (var championInformation in InformationManager.VisibleChampionInformations)
            {
                var infoBarPos = championInformation.Source.HPBarPosition;

                RectangleRendering.Render(infoBarPos + TrackersCommon.BackgroundOffset.Offset,
                                          TrackersCommon.BackgroundOffset.Width,
                                          TrackersCommon.BackgroundOffset.Height,
                                          1f,
                                          new Color(0xff4f4f4f),
                                          Color.Black);
            }
        }

        private static readonly Color ColorExperience = new Color(0xffab08d4);

        public override void Render()
        {
            foreach (var championInformation in InformationManager.VisibleChampionInformations)
            {
                var infoBarPos = championInformation.Source.HPBarPosition;

                RectangleRendering.Render(infoBarPos + TrackersCommon.BackgroundOffset2.Offset,
                                          TrackersCommon.BackgroundOffset2.Width,
                                          TrackersCommon.BackgroundOffset2.Height,
                                          new Color(0xff4f4f4f));

                RectangleRendering.Render(infoBarPos + TrackersCommon.BackgroundOffset.Offset,
                                          TrackersCommon.BackgroundOffset.Height * 0.075f,
                                          TrackersCommon.BackgroundOffset.Height,
                                          new Color(0xff4f4f4f));

                RectangleRendering.Render(infoBarPos + TrackersCommon.BackgroundOffset3.Offset,
                                          TrackersCommon.BackgroundOffset3.Width,
                                          TrackersCommon.BackgroundOffset3.Height,
                                          new Color(0xff4f4f4f));

                RectangleRendering.Render(infoBarPos + TrackersCommon.BackgroundOffset4.Offset,
                                          TrackersCommon.BackgroundOffset4.Width,
                                          TrackersCommon.BackgroundOffset4.Height,
                                          new Color(0xff4f4f4f));

                var expPos = infoBarPos + TrackersCommon.ExperienceOffset.Offset;
                RectangleRendering.Render(expPos, 
                                          TrackersCommon.ExperienceOffset.Width,
                                          TrackersCommon.ExperienceOffset.Height, 
                                          1f, 
                                          Color.Black, 
                                          Color.Black);
                RectangleRendering.Render(expPos,
                                          TrackersCommon.ExperienceOffset.Width * championInformation.EXPProgress,
                                          TrackersCommon.ExperienceOffset.Height,
                                          1f,
                                          ColorExperience,
                                          Color.Black);

                //Spells rendering
                foreach (var spellInformation in championInformation.SpellInformations)
                {
                    spellInformation.SpellRenderObject.Render(infoBarPos);
                }

                foreach (var spellInformation in championInformation.SummonerInformations)
                {
                    spellInformation.SummonerTexture.Render(infoBarPos);
                }
            }
        }

        public SpellTracker() : base("SpellTracker")
        {
        }
    }
}