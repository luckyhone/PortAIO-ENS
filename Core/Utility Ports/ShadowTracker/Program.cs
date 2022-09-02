using System;
using System.Collections.Generic;
using System.Linq;
using EnsoulSharp;
using EnsoulSharp.SDK;
using EnsoulSharp.SDK.Utility;
using SharpDX;

namespace ShadowTracker
{
    enum InfoType
    {
        MovingSkill,
        StopSkill,
        UsingItem,
        PetStyle
    }

    //무빙 스킬
    class MovingTrackInfomation
    {
        public AIBaseClient Sender;
        public int CastTime;
        public InfoType InfoType;
        public int ExpireTime;
        public Vector3 StartPosition;
        public Vector3 EndPosition;
    }
    class MovingSkillInfomation
    {
        public string SpellName;
        public float MaxRange;
    }

    //스탑 스킬
    class StopTrackInfomation
    {
        public AIBaseClient Sender;
        public Vector3 CastPosition;
        public InfoType InfoType;
        public int ExpireTime;
    }
    class StopSkillInfomation
    {
        public string SpellName;
        public int ExpireTime;
    }

    //아이템
    class UsingTrackInfomation
    {
        public AIBaseClient Sender;
        public int CastTime;
        public InfoType InfoType;
        public int ExpireTime;
    }
    class UsingItemInfomation
    {
        public string ItemName;
        public int ExpireTime;
    }

    //펫스킬
    class PetSkillInfomation
    {
        public string ChampionName;
        public int ExpireTime;
    }

    class Program
    {
        public static AIHeroClient Player => ObjectManager.Player;
        public static int EnemyCnt => GameObjects.EnemyHeroes.Count();
        public static List<MovingTrackInfomation> TrackInfomationList = new List<MovingTrackInfomation>();
        public static List<StopTrackInfomation> StopTrackInfomationList = new List<StopTrackInfomation>();
        public static List<UsingTrackInfomation> UsingItemInfomationList = new List<UsingTrackInfomation>();

        public static MovingSkillInfomation[] MovingSkillInfoList = new MovingSkillInfomation[]
            {
                new MovingSkillInfomation { SpellName = "summonerflash", MaxRange = 450f },
                new MovingSkillInfomation { SpellName = "EzrealArcaneShift", MaxRange = 450f },
                new MovingSkillInfomation { SpellName = "Deceive", MaxRange = 450f },
                new MovingSkillInfomation { SpellName = "Riftwalk", MaxRange = 500f }
            };
        public static StopSkillInfomation[] StopSkillInfoList = new StopSkillInfomation[]
            {
                new StopSkillInfomation { SpellName = "MonkeyKingDecoy", ExpireTime=1500 },
                new StopSkillInfomation { SpellName = "AkaliSmokeBomb", ExpireTime=8000 },
                new StopSkillInfomation { SpellName = "summonerteleport", ExpireTime=3500 }
            };
        public static PetSkillInfomation[] PetSkillInfoList = new PetSkillInfomation[]
            {
                new PetSkillInfomation { ChampionName = "Leblanc", ExpireTime = 8000 },
                new PetSkillInfomation { ChampionName = "Shaco", ExpireTime = 18000 },
                new PetSkillInfomation { ChampionName = "Mordekaiser", ExpireTime = 30000 }
            };
        public static UsingItemInfomation[] UsingItemInfoList = new UsingItemInfomation[]
            {
                new UsingItemInfomation { ItemName = "ZhonyasHourglass", ExpireTime = 2500 }
            };

        public static void Game_OnGameLoad()
        {
            try
            {
                MainMenu.Menu();

                Game.OnUpdate += OnGameUpdate;
                Drawing.OnDraw += OnDraw.Drawing_OnDraw;
                AntiGapcloser.OnGapcloser += AntiGapcloser_OnEnemyGapcloser;
                AIBaseClient.OnDoCast += OnProcessSpell;
                Interrupter.OnInterrupterSpell += Interrupter2_OnInterruptableTarget;
                GameObject.OnCreate += GameObject_OnCreate;
                GameObject.OnDelete += GameObject_OnDelete;
            }
            catch (Exception e)
            {
                Console.Write(e);
                Game.Print("ShadowTracker is not working. plz send message by KorFresh (Code 3)");
            }
        }

        private static void AntiGapcloser_OnEnemyGapcloser(AIHeroClient sender, AntiGapcloser.GapcloserArgs args)
        {
            try
            {

            }
            catch (Exception e)
            {
                Console.WriteLine(e);
                Game.Print("ShadowTracker is not working. plz send message by KorFresh (Code 6)");
            }
        }

        private static void OnGameUpdate(EventArgs args)
        {
            try
            {
                //만료시간 지난 아이템 삭제
                TrackInfomationList.RemoveAll(x => x.ExpireTime <= Environment.TickCount);
                StopTrackInfomationList.RemoveAll(x => x.ExpireTime <= Environment.TickCount);
                UsingItemInfomationList.RemoveAll(x => x.ExpireTime <= Environment.TickCount);
            }
            catch (Exception e)
            {
                Console.Write(e);
                Game.Print("ShadowTracker is not working. plz send message by KorFresh (Code 4)");
            }
        }

        /*private static void AntiGapcloser_OnEnemyGapcloser(ActiveGapcloser gapcloser)
        {
            try
            {

            }
            catch (Exception e)
            {
                Console.Write(e);
                Game.Print("ShadowTracker is not working. plz send message by KorFresh (Code 6)");
            }
        }*/

        private static void OnProcessSpell(AIBaseClient sender, AIBaseClientProcessSpellCastEventArgs args)
        {
            try
            {
                if (sender.IsEnemy)
                {                    
                    var MovingSkillInfo = MovingSkillInfoList.FirstOrDefault(x => x.SpellName == args.SData.Name);
                    var StopSkillInfo = StopSkillInfoList.FirstOrDefault(x => x.SpellName == args.SData.Name);
                    var UsingItemInfo = UsingItemInfoList.FirstOrDefault(x => x.ItemName == args.SData.Name);

                    if (MovingSkillInfo != null)
                    {
                        TrackInfomationList.Add(new MovingTrackInfomation
                        {
                            InfoType = InfoType.MovingSkill,
                            CastTime = Environment.TickCount,
                            Sender = sender,
                            ExpireTime = TickCount(3000),
                            StartPosition = args.Start,
                            EndPosition = args.Start.Extend(args.To, MovingSkillInfo.MaxRange)
                        });
                    }
                    if (StopSkillInfo != null)
                    {
                        //TacticalMap.ShowPing(PingCategory.Danger, sender.Position);
                        
                        StopTrackInfomationList.Add(new StopTrackInfomation
                        {
                            InfoType = InfoType.StopSkill,
                            Sender = sender,
                            CastPosition = args.To,
                            ExpireTime = TickCount(StopSkillInfo.ExpireTime)
                        });
                    }
                    if (UsingItemInfo != null)
                    {
                        UsingItemInfomationList.Add(new UsingTrackInfomation
                        {
                            InfoType = InfoType.UsingItem,
                            Sender = sender,
                            CastTime = Environment.TickCount,
                            ExpireTime = TickCount(UsingItemInfo.ExpireTime)
                        });
                    }
                }
            }
            catch (Exception e)
            {
                Console.Write(e);
                Game.Print("ShadowTracker is not working. plz send message by KorFresh (Code 7)");
            }
        }

        private static void Interrupter2_OnInterruptableTarget(AIHeroClient sender, Interrupter.InterruptSpellArgs args)
        {

        }

        private static int TickCount(int time)
        {
            return Environment.TickCount + time;
        }

        private static void GameObject_OnCreate(GameObject obj, EventArgs args)
        {
            
        }

        private static void GameObject_OnDelete(GameObject obj, EventArgs args)
        {
            
        }
    }
}