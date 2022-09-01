using System;
using System.Collections.Generic;
using System.Linq;
using EnsoulSharp;
using EnsoulSharp.SDK;
using SharpDX;

namespace OneKeyToWin_AIO_Sebby.Core
{
    class ChampionInfo
    {
        public AIHeroClient Hero { get; set; }

        public Vector3 LastVisablePos { get; set; }
        public float LastVisableTime { get; set; }
        public Vector3 PredictedPos { get; set; }
        public Vector3 LastWayPoint { get; set; }

        public float StartRecallTime { get; set; }
        public float AbortRecallTime { get; set; }
        public float FinishRecallTime { get; set; }
        public bool IsJungler { get; set; }
        public double IncomingDamage = 0;
        public LeagueSharpCommon.Render.Sprite NormalSprite;
        public LeagueSharpCommon.Render.Sprite HudSprite;
        public LeagueSharpCommon.Render.Sprite MinimapSprite;
        public LeagueSharpCommon.Render.Sprite SquareSprite;

        public ChampionInfo(AIHeroClient hero)
        {
            Hero = hero;
            if (hero.IsEnemy)
                NormalSprite = ImageLoader.CreateRadrarIcon(hero.CharacterName, System.Drawing.Color.Red);
            else
                NormalSprite = ImageLoader.CreateRadrarIcon(hero.CharacterName, System.Drawing.Color.GreenYellow);
            SquareSprite = ImageLoader.GetSprite(hero.CharacterName);
            HudSprite = ImageLoader.CreateRadrarIcon(hero.CharacterName, System.Drawing.Color.DarkGoldenrod, 100);
            MinimapSprite = ImageLoader.CreateMinimapSprite(hero.CharacterName);
            LastVisableTime = Game.Time;
            LastVisablePos = hero.Position;
            PredictedPos = hero.Position;
            IsJungler = hero.Spellbook.Spells.Any(spell => spell.Name.ToLower().Contains("smite"));

            StartRecallTime = 0;
            AbortRecallTime = 0;
            FinishRecallTime = 0;

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
    class OKTWtracker
    {
        public static HashSet<ChampionInfo> ChampionInfoList = new HashSet<ChampionInfo>();

        private Vector3 EnemySpawn;

        public void LoadOKTW()
        {
            EnemySpawn = ObjectManager.Get<Obj_SpawnPoint>().FirstOrDefault(x => x.IsEnemy).Position;
            foreach (var hero in GameObjects.Heroes)
            {
                ChampionInfoList.Add(new ChampionInfo(hero));
            }

            Game.OnUpdate += OnUpdate;
            Teleport.OnTeleport += Obj_AI_Base_OnTeleport;
        }

        private static void Obj_AI_Base_OnTeleport(GameObject sender, Teleport.TeleportEventArgs args)
        {
            var unit = sender as AIHeroClient;

            if (unit == null || !unit.IsValid || unit.IsAlly)
                return;
            
            var ChampionInfoOne = ChampionInfoList.Find(x => x.Hero.NetworkId == sender.NetworkId);

            var recall = new RecallInf(unit.NetworkId, args.Status, args.Type, args.Duration, args.Start);

            if (recall.Type == Teleport.TeleportType.Recall)
            {
                switch (recall.Status)
                {
                    case Teleport.TeleportStatus.Start:
                        ChampionInfoOne.StartRecallTime = Game.Time;
                        break;
                    case Teleport.TeleportStatus.Abort:
                        ChampionInfoOne.AbortRecallTime = Game.Time;
                        break;
                    case Teleport.TeleportStatus.Finish:
                        ChampionInfoOne.FinishRecallTime = Game.Time;
                        var spawnPos = ObjectManager.Get<Obj_SpawnPoint>().FirstOrDefault(x => x.IsEnemy).Position;
                        ChampionInfoOne.LastVisablePos = spawnPos;
                        ChampionInfoOne.PredictedPos = spawnPos;
                        ChampionInfoOne.LastWayPoint = spawnPos;
                        ChampionInfoOne.LastVisableTime = Game.Time;
                        break;
                }
            }
        }

        private void OnUpdate(EventArgs args)
        {
            try
            {
                if (Program.LagFree(0))
                {
                    foreach (var extra in ChampionInfoList.Where(x => x.Hero.IsEnemy))
                    {
                        var enemy = extra.Hero;
                        if (enemy == null)
                        {
                            return;
                        }

                        if (enemy.IsDead)
                        {
                            extra.LastVisablePos = EnemySpawn;
                            extra.LastVisableTime = Game.Time;
                            extra.PredictedPos = EnemySpawn;
                            extra.LastWayPoint = EnemySpawn;
                        }
                        else if (enemy.IsHPBarRendered && enemy.IsVisible && !enemy.IsDead)
                        {
                            extra.IncomingDamage = SebbyLib.OktwCommon.GetIncomingDamage(extra.Hero, 0.5f);
                            extra.LastWayPoint = extra.Hero.Path.Last();
                            extra.PredictedPos = enemy.Position.Extend(extra.LastWayPoint, 125);
                            extra.LastVisablePos = enemy.Position;
                            extra.LastVisableTime = Game.Time;
                        }
                    }
                }

                if (Program.LagFree(3))
                {
                    foreach (var extra in ChampionInfoList)
                    {
                        extra.NormalSprite.VisibleCondition = sender => !extra.Hero.IsDead;
                        extra.HudSprite.VisibleCondition = sender => !extra.Hero.IsDead;
                        if (extra.Hero.IsDead || !extra.Hero.IsHPBarRendered || !extra.Hero.IsVisible ||
                            extra.Hero == null)
                        {
                            return;
                        }

                        extra.IncomingDamage = SebbyLib.OktwCommon.GetIncomingDamage(extra.Hero, 0.5f);
                    }
                }
            }
            catch (Exception)
            {
            }
        }
    }
}