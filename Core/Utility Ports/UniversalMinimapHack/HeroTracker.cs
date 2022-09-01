using System;
using System.Drawing;
using System.Linq;
using EnsoulSharp;
using EnsoulSharp.SDK;
using EnsoulSharp.SDK.Utility;
using LeagueSharpCommon;
using SharpDX;
using Color = System.Drawing.Color;
using Render = LeagueSharpCommon.Render;

namespace UniversalMinimapHack
{
    public class HeroTracker
    {
        public HeroTracker(AIHeroClient hero, Bitmap bmp)
        {
            Drawing.OnEndScene += Drawing_OnEndScene;
            Hero = hero;

            RecallStatus = Teleport.TeleportStatus.Unknown;
            Hero = hero;
            var image = new Render.Sprite(bmp, new Vector2(0, 0));
            var image2 = new Render.Sprite(bmp, new Vector2(0, 0));
            image.GrayScale();
            image.Scale = new Vector2(MinimapHack.Instance().Menu.IconScale, MinimapHack.Instance().Menu.IconScale);
            image.VisibleCondition = sender => !hero.IsVisible && !hero.IsDead;
            image.PositionUpdate = delegate
            {
                Vector2 v2 = Drawing.WorldToMinimap(LastLocation);
                v2.X -= image.Width / 2f;
                v2.Y -= image.Height / 2f;
                return v2;
            };
            image.Add(0);
            image2.Scale = new Vector2(50/100f, 50/100f);
            image2.VisibleCondition = sender => !hero.IsVisible && !hero.IsDead;
            image2.PositionUpdate = delegate
            {
                Vector2 v3 = Drawing.WorldToScreen(LastLocation);
                v3.X -= image.Width / 2f;
                v3.Y -= image.Height / 2f;
                return v3;
            };
            image2.Add(0);
            LastSeen = 0;
            LastLocation = hero.ServerPosition;
            PredictedLocation = hero.ServerPosition;
            BeforeRecallLocation = hero.ServerPosition;

            Text = new Render.Text(0, 0, "", MinimapHack.Instance().Menu.SSTimerSize, Color.White.ToSharpDxColor())
            {
                VisibleCondition =
                    sender =>
                        !hero.IsHPBarRendered && !Hero.IsDead && MinimapHack.Instance().Menu.SSTimer && LastSeen > 20f &&
                        MinimapHack.Instance().Menu.SSTimerStart <= Game.Time - LastSeen,
                PositionUpdate = delegate
                {
                    Vector2 v2 = Drawing.WorldToMinimap(LastLocation);
                    v2.Y += MinimapHack.Instance().Menu.SSTimerOffset;
                    return v2;
                },
                TextUpdate = () => Program.Format(Game.Time - LastSeen),
                OutLined = true,
                Centered = true
            };
            Text.Add(0);

            Teleport.OnTeleport += Obj_AI_Base_OnTeleport;
            Game.OnUpdate += Game_OnGameUpdate;
            
        }

        private Render.Text Text { get; set; }
        private AIHeroClient Hero { get; set; }
        private Teleport.TeleportStatus RecallStatus { get; set; }
        private float LastSeen { get; set; }
        private Vector3 LastLocation { get; set; }
        private Vector3 PredictedLocation { get; set; }
        private Vector3 BeforeRecallLocation { get; set; }
        private bool Pinged { get; set; }

        private void Drawing_OnEndScene(EventArgs args)
        {
            if (!Hero.IsHPBarRendered && !Hero.IsDead)
            {
                float radius = Math.Abs(LastLocation.X - PredictedLocation.X);
                if (radius < MinimapHack.Instance().Menu.SSCircleSize && MinimapHack.Instance().Menu.SSCircle)
                {
                    SharpDX.Color c = MinimapHack.Instance().Menu.SSCircleColor;
                    if (RecallStatus == Teleport.TeleportStatus.Start)
                    {
                        c = System.Drawing.Color.LightBlue.ToSharpDxColor();
                    }
                    
                    LeagueSharpCommon.Render.Circle.DrawCircle(LastLocation, radius, c.ToSystemColor(), 1, true);
                    MiniMap.DrawCircle(LastLocation,radius,c.ToSystemColor(),1);
                }
            }
            if (Text.Visible)
            {
                Text.OnEndScene();
            }
        }

        private void Game_OnGameUpdate(EventArgs args)
        {
            if (Hero.ServerPosition != LastLocation && Hero.ServerPosition != BeforeRecallLocation)
            {
                LastLocation = Hero.ServerPosition;
                PredictedLocation = Hero.ServerPosition;
                LastSeen = Game.Time;
            }

            if (!Hero.IsHPBarRendered && RecallStatus != Teleport.TeleportStatus.Start)
            {
                PredictedLocation = new Vector3(
                    LastLocation.X + ((Game.Time - LastSeen) * Hero.MoveSpeed), LastLocation.Y, LastLocation.Z);
            }

            if (Hero.IsHPBarRendered && !Hero.IsDead)
            {
                Pinged = false;
                LastSeen = Game.Time;
            }

            if (LastSeen > 0f && MinimapHack.Instance().Menu.Ping && !Hero.IsHPBarRendered)
            {
                if (Game.Time - LastSeen >= MinimapHack.Instance().Menu.MinPing && !Pinged)
                {
                    Game.ShowPing(PingCategory.EnemyMissing,Hero,true);
                    Pinged = true;
                }
            }
        }

        private void Obj_AI_Base_OnTeleport(GameObject sender, Teleport.TeleportEventArgs args)
        {
            var unit = sender as AIHeroClient;

            if (unit == null || !unit.IsValid || unit.IsAlly)
            {
                return;
            }

            var decoded = new RecallInf(unit.NetworkId, args.Status, args.Type, args.Duration, args.Start);
            if (unit.NetworkId == Hero.NetworkId && decoded.Type == Teleport.TeleportType.Recall)
            {
                RecallStatus = decoded.Status;
                if (decoded.Status == Teleport.TeleportStatus.Finish)
                {
                    BeforeRecallLocation = Hero.ServerPosition;
                    Obj_SpawnPoint enemySpawn = ObjectManager.Get<Obj_SpawnPoint>().FirstOrDefault(x => x.IsEnemy);
                    if (enemySpawn != null)
                    {
                        LastLocation = enemySpawn.Position;
                        PredictedLocation = enemySpawn.Position;
                    }
                    LastSeen = Game.Time;
                }
            }
        }
    }

    public class RecallInf
    {
        public int NetworkID;
        public int Duration;
        public int Start;
        public Teleport.TeleportType Type;
        public Teleport.TeleportStatus Status;

        public RecallInf(int netid, Teleport.TeleportStatus stat, Teleport.TeleportType tpe, int dura, int star = 0)
        {
            NetworkID = netid;
            Status = stat;
            Type = tpe;
            Duration = dura;
            Start = star;
        }
    }
}