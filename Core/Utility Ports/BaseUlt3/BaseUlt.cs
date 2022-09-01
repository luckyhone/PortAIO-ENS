using System;
using System.Collections.Generic;
using System.Linq;
using EnsoulSharp;
using EnsoulSharp.SDK;
using EnsoulSharp.SDK.MenuUI;
using EnsoulSharp.SDK.Utility;
using SharpDX;
using SharpDX.Direct3D9;
using Color = System.Drawing.Color;

namespace BaseUlt3
{
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
    internal class BaseUlt
    {
        Menu Menu;
        Menu TeamUlt;
        Menu DisabledChampions;

        Spell Ultimate;
        int LastUltCastT;

        private MapType Map;
        
        List<AIHeroClient> Heroes;
        List<AIHeroClient> Enemies;
        List<AIHeroClient> Allies;

        public List<EnemyInfo> EnemyInfo = new List<EnemyInfo>();

        public Dictionary<int, int> RecallT = new Dictionary<int, int>();

        Vector3 EnemySpawnPos;

        Font Text;

        System.Drawing.Color NotificationColor = System.Drawing.Color.FromArgb(136, 207, 240);

        static float BarX = Drawing.Width * 0.425f;
        float BarY = Drawing.Height * 0.80f;
        static int BarWidth = (int)(Drawing.Width - 2 * BarX);
        int BarHeight = 6;
        int SeperatorHeight = 5;
        static float Scale = (float)BarWidth / 8000;
        
        public BaseUlt()
        {
            Menu = new Menu("BaseUlt3", "BaseUlt3", true);
            Menu.Add(new MenuBool("showRecalls", "Show Recalls", true));
            Menu.Add(new MenuBool("baseUlt", "Base Ult", true));
            Menu.Add(new MenuBool("checkCollision", "Check Collision", true));
            Menu.Add(new MenuKeyBind("panicKey", "No Ult while SBTW", Keys.Space, KeyBindType.Press));
            Menu.Add(new MenuKeyBind("regardlessKey", "No timelimit (hold)", Keys.Control, KeyBindType.Press));
            
            Heroes = ObjectManager.Get<AIHeroClient>().ToList();
            Enemies = Heroes.Where(x => x.IsEnemy).ToList();
            Allies = Heroes.Where(x => x.IsAlly).ToList();
            
            EnemyInfo = Enemies.Select(x => new EnemyInfo(x)).ToList();
            
            bool compatibleChamp = IsCompatibleChamp(ObjectManager.Player.CharacterName);

            TeamUlt = Menu.Add(new Menu("TeamUlt", "Team Baseult Friends"));
            DisabledChampions = Menu.Add(new Menu("DisabledChampions", "Disabled Champion targets"));

            if (compatibleChamp)
            {
                foreach (AIHeroClient champ in Allies.Where(x => !x.IsMe && IsCompatibleChamp(x.CharacterName)))
                    TeamUlt.Add(new MenuBool(champ.CharacterName, "Ally with baseult " + champ.CharacterName, false));
                foreach (AIHeroClient champ in Enemies)
                    DisabledChampions.Add(new MenuBool(champ.CharacterName, "Don't shoot: " + champ.CharacterName,
                        false));
            }
            var NotificationsMenu = Menu.Add(new Menu("Notifications", "Notifications"));
            NotificationsMenu.Add(new MenuBool("notifRecFinished", "Recall finished", true));
            NotificationsMenu.Add(new MenuBool("notifRecAborted", "Recall aborted", true));
            
            EnemySpawnPos = ObjectManager.Get<Obj_SpawnPoint>().FirstOrDefault(x => x.IsEnemy).Position; //ObjectManager.Get<GameObject>().FirstOrDefault(x => x.Type == GameObjectType.obj_SpawnPoint && x.IsEnemy).Position;

            //Map = LeagueSharp.Common.Utility.Map.GetMap().Type;
            Map = EnsoulSharp.SDK.Utility.Map.GetMap().Type;

            Ultimate = new Spell(SpellSlot.R);

            Text = new Font(Drawing.Direct3DDevice9, new FontDescription { FaceName = "Calibri", Height = 13, Width = 6, OutputPrecision = FontPrecision.Default, Quality = FontQuality.Default });

            Teleport.OnTeleport += Obj_AI_Base_OnTeleport;
            Drawing.OnPreReset += Drawing_OnPreReset;
            Drawing.OnPostReset += Drawing_OnPostReset;
            Drawing.OnDraw += Drawing_OnDraw;
            AppDomain.CurrentDomain.DomainUnload += CurrentDomain_DomainUnload;
            AppDomain.CurrentDomain.ProcessExit += CurrentDomain_DomainUnload;

            if (compatibleChamp)
                Game.OnUpdate += Game_OnUpdate;

            //ShowNotification("BaseUlt3 by Beaving - Loaded", NotificationColor);
            //ShowNotification("BaseUlt3 by Beaving - Loaded");
            Menu.Attach();

        }

        private void Game_OnUpdate(EventArgs args)
        {
            int time = Variables.TickCount;
            foreach (EnemyInfo enemyInfo in EnemyInfo.Where(x => x.Player.IsHPBarRendered))
                enemyInfo.LastSeen = time;
            if(!Menu.GetValue<MenuBool>("baseUlt").Enabled)
                return;
            foreach (EnemyInfo enemyInfo in EnemyInfo.Where(x=>
                x.Player.IsValid &&
                !x.Player.IsDead &&
                !DisabledChampions.GetValue<MenuBool>(x.Player.CharacterName).Enabled &&
                x.RecallInfo.Recall.Status == Teleport.TeleportStatus.Start && x.RecallInfo.Recall.Type == Teleport.TeleportType.Recall).OrderBy(x=>x.RecallInfo.GetRecallCountdown()))
            {
                if(Variables.TickCount - LastUltCastT > 15000)
                    HandleUltTarget(enemyInfo);
            }
        }

        void HandleUltTarget(EnemyInfo enemyInfo)
        {
            bool ultNow = false;
            bool me = false;

            foreach (AIHeroClient champ in Allies.Where(x=> x.IsValid &&
                                                            !x.IsDead &&
                                                            ((x.IsMe && !x.IsStunned)) &&
                                                            CanUseUlt(x))) 
            {
                if (Menu.GetValue<MenuBool>("checkCollision").Enabled && UltSpellData[champ.CharacterName].Collision &&
                    IsCollidingWithChamps(champ, EnemySpawnPos, UltSpellData[champ.CharacterName].Width))
                {
                    enemyInfo.RecallInfo.IncomingDamage[champ.NetworkId] = 0;
                    return;
                }

                var timeneeded = GetUltTravelTime(champ, UltSpellData[champ.CharacterName].Speed,
                    UltSpellData[champ.CharacterName].Delay, EnemySpawnPos) - 65;
                if (enemyInfo.RecallInfo.GetRecallCountdown() >= timeneeded)
                    enemyInfo.RecallInfo.IncomingDamage[champ.NetworkId] =
                        (float) champ.GetSpellDamage(enemyInfo.Player, SpellSlot.R,
                            UltSpellData[champ.CharacterName].SpellStage) *
                        UltSpellData[champ.CharacterName].DamageMultiplicator;
                else if (enemyInfo.RecallInfo.GetRecallCountdown() < timeneeded - (champ.IsMe ? 0 : 125))
                {
                    enemyInfo.RecallInfo.IncomingDamage[champ.NetworkId] = 0;
                    continue;
                }

                if (champ.IsMe)
                {
                    me = true;
                    enemyInfo.RecallInfo.EstimatedShootT = timeneeded;

                    if (enemyInfo.RecallInfo.GetRecallCountdown() - timeneeded < 60)
                        ultNow = true;
                }
                
            }

            if (me)
            {
                if (!IsTargetKillable(enemyInfo))
                {
                    enemyInfo.RecallInfo.LockedTarget = false;
                    return;
                }
                

                enemyInfo.RecallInfo.LockedTarget = true;
                
                if(!ultNow || Menu.GetValue<MenuKeyBind>("panicKey").Active)
                    return;
                
                Ultimate.Cast(EnemySpawnPos);
                LastUltCastT = Variables.TickCount;
            }
            else
            {
                enemyInfo.RecallInfo.LockedTarget = false;
                enemyInfo.RecallInfo.EstimatedShootT = 0;
            }
        }
        
        bool IsTargetKillable(EnemyInfo enemyInfo)
        {
            float totalUltDamage = enemyInfo.RecallInfo.IncomingDamage.Values.Sum();


            float targetHealth = GetTargetHealth(enemyInfo, enemyInfo.RecallInfo.GetRecallCountdown());

            if (Variables.TickCount - enemyInfo.LastSeen > 20000 && !Menu.GetValue<MenuKeyBind>("regardlessKey").Active)
            {
                if (totalUltDamage < enemyInfo.Player.MaxHealth)
                    return false;
            }
            else if (totalUltDamage < targetHealth)
            {
                return false;
            }

            return true;
        }

        float GetTargetHealth(EnemyInfo enemyInfo, int additionalTime)
        {
            if (enemyInfo.Player.IsHPBarRendered)
                return enemyInfo.Player.Health;

            var healthPerSec = 0.45f * enemyInfo.Player.Level;

            float predictedHealth = enemyInfo.Player.Health + (healthPerSec * ((Variables.TickCount - enemyInfo.LastSeen + additionalTime) / 1000f));

            return predictedHealth > enemyInfo.Player.MaxHealth ? enemyInfo.Player.MaxHealth : predictedHealth;
        }
        
        float GetUltTravelTime(AIHeroClient source, float speed, float delay, Vector3 targetpos)
        {
            if (source.CharacterName == "Karthus")
                return delay * 1000;

            float distance = Vector3.Distance(source.Position, targetpos);

            float missilespeed = speed;

            if (source.CharacterName == "Jinx" && distance > 1350)
            {
                const float accelerationrate = 0.3f; //= (1500f - 1350f) / (2200 - speed), 1 unit = 0.3units/second

                var acceldifference = distance - 1350f;

                if (acceldifference > 150f) //it only accelerates 150 units
                    acceldifference = 150f;

                var difference = distance - 1500f;

                missilespeed = (1350f * speed + acceldifference * (speed + accelerationrate * acceldifference) + difference * 2200f) / distance;
            }

            return (distance / missilespeed + delay) * 1000;
        }
        bool CanUseUlt(AIHeroClient hero) //use for allies when fixed: champ.Spellbook.GetSpell(SpellSlot.R) = Ready
        {
            return hero.Spellbook.CanUseSpell(SpellSlot.R) == SpellState.Ready ||
                   (hero.Spellbook.GetSpell(SpellSlot.R).Level > 0 && hero.Spellbook.CanUseSpell(SpellSlot.R) == SpellState.Surpressed && hero.Mana >= hero.Spellbook.GetSpell(SpellSlot.R).ManaCost);
        }
        bool IsCollidingWithChamps(AIHeroClient source, Vector3 targetpos, float width)
        {
            var input = new PredictionInput
            {
                Radius = width,
                Unit = source,
            };

            input.CollisionObjects[0] = CollisionObjects.Heroes;

            return Collisions.GetCollision(new List<Vector3> { targetpos }, input).Any(); //x => x.NetworkId != targetnetid, hard to realize with teamult
        }

        private void CurrentDomain_DomainUnload(object sender, EventArgs e)
        {
            Text.Dispose();
        }

        private void Drawing_OnPreReset(EventArgs args)
        {
            Text.OnLostDevice();
        }

        void Drawing_OnPostReset(EventArgs args)
        {
            Text.OnResetDevice();
        }

        void Obj_AI_Base_OnTeleport(AIBaseClient sender, Teleport.TeleportEventArgs args)
        {
            var unit = sender as AIHeroClient;

            if (unit == null || !unit.IsValid || unit.IsAlly || unit.IsMe)
            {
                return;
            }

            var recall = new RecallInf(unit.NetworkId, args.Status, args.Type, args.Duration, args.Start);
            var enemyInfo = EnemyInfo.Find(x => x.Player.NetworkId == unit.NetworkId).RecallInfo.UpdateRecall(recall);

            if (recall.Type == Teleport.TeleportType.Recall)
            {
                switch (recall.Status)
                {
                    case Teleport.TeleportStatus.Abort:
                        if (Menu["Notifications"].GetValue<MenuBool>("notifRecAborted").Enabled)
                        {
                            //ShowNotification(enemyInfo.Player.CharacterName + ": Recall ABORTED", System.Drawing.Color.Orange);
                            //ShowNotification(enemyInfo.Player.CharacterName + ": Recall ABORTED");
                        }

                        break;
                    case Teleport.TeleportStatus.Finish:
                        if (Menu["Notifications"].GetValue<MenuBool>("notifRecFinished").Enabled)
                        {
                            //ShowNotification(enemyInfo.Player.CharacterName + ": Recall FINISHED", NotificationColor);
                            //ShowNotification(enemyInfo.Player.CharacterName + ": Recall FINISHED");
                        }
                        break;
                }
            }
        }

        void Drawing_OnDraw(EventArgs args)
        {
            if(!Menu.GetValue<MenuBool>("showRecalls").Enabled || Drawing.Direct3DDevice9 == null || Drawing.Direct3DDevice9.IsDisposed)
                return;
            bool indicated = false;

            float fadeout = 1f;
            int count = 0;

            foreach (EnemyInfo enemyInfo in EnemyInfo.Where(x=>
                x.Player.IsValid() &&
                x.RecallInfo.ShouldDraw() &&
                !x.Player.IsDead &&
                x.RecallInfo.GetRecallCountdown()>0).OrderBy(x=>x.RecallInfo.GetRecallCountdown()))
            {
                if (!enemyInfo.RecallInfo.LockedTarget)
                {
                    fadeout = 1f;
                    Color color = System.Drawing.Color.White;

                    if (enemyInfo.RecallInfo.WasAborted())
                    {
                        fadeout = (float) enemyInfo.RecallInfo.GetDrawTime() /
                                  (float) enemyInfo.RecallInfo.FADEOUT_TIME;
                        color = Color.Yellow;
                    }
                    DrawRect(BarX,BarY,(int)(Scale * (float) enemyInfo.RecallInfo.GetRecallCountdown()), BarHeight,1,System.Drawing.Color.FromArgb((int)(100f*fadeout), System.Drawing.Color.White));
                    DrawRect(BarX + Scale * (float)enemyInfo.RecallInfo.GetRecallCountdown() - 1, BarY - SeperatorHeight, 0, SeperatorHeight + 1, 1, System.Drawing.Color.FromArgb((int)(255f * fadeout), color));

                    Text.DrawText(null, enemyInfo.Player.CharacterName, (int)BarX + (int)(Scale * (float)enemyInfo.RecallInfo.GetRecallCountdown() - (float)(enemyInfo.Player.CharacterName.Length * Text.Description.Width) / 2), (int)BarY - SeperatorHeight - Text.Description.Height - 1, new ColorBGRA(color.R, color.G, color.B, (byte)((float)color.A * fadeout)));
                }
                else
                {
                    if (!indicated && enemyInfo.RecallInfo.EstimatedShootT != 0)
                    {
                        indicated = true;
                        DrawRect(BarX + Scale * enemyInfo.RecallInfo.EstimatedShootT, BarY + SeperatorHeight + BarHeight - 3, 0, SeperatorHeight * 2, 2, System.Drawing.Color.Orange);
                    }

                    DrawRect(BarX, BarY, (int)(Scale * (float)enemyInfo.RecallInfo.GetRecallCountdown()), BarHeight, 1, System.Drawing.Color.FromArgb(255, System.Drawing.Color.Red));
                    DrawRect(BarX + Scale * (float)enemyInfo.RecallInfo.GetRecallCountdown() - 1, BarY + SeperatorHeight + BarHeight - 3, 0, SeperatorHeight + 1, 1, System.Drawing.Color.IndianRed);

                    Text.DrawText(null, enemyInfo.Player.CharacterName, (int)BarX + (int)(Scale * (float)enemyInfo.RecallInfo.GetRecallCountdown() - (float)(enemyInfo.Player.CharacterName.Length * Text.Description.Width) / 2), (int)BarY + SeperatorHeight + Text.Description.Height / 2, new ColorBGRA(255, 92, 92, 255));
                }

                count++;
            }
            if (count > 0)
            {
                if (count != 1) //make the whole bar fadeout when its only 1
                    fadeout = 1f;

                DrawRect(BarX, BarY, BarWidth, BarHeight, 1, System.Drawing.Color.FromArgb((int)(40f * fadeout), System.Drawing.Color.White));

                DrawRect(BarX - 1, BarY + 1, 0, BarHeight, 1, System.Drawing.Color.FromArgb((int)(255f * fadeout), System.Drawing.Color.White));
                DrawRect(BarX - 1, BarY - 1, BarWidth + 2, 1, 1, System.Drawing.Color.FromArgb((int)(255f * fadeout), System.Drawing.Color.White));
                DrawRect(BarX - 1, BarY + BarHeight, BarWidth + 2, 1, 1, System.Drawing.Color.FromArgb((int)(255f * fadeout), System.Drawing.Color.White));
                DrawRect(BarX + 1 + BarWidth, BarY + 1, 0, BarHeight, 1, System.Drawing.Color.FromArgb((int)(255f * fadeout), System.Drawing.Color.White));
            }
        }
        public void DrawRect(float x, float y, int width, int height, float thickness, System.Drawing.Color color)
        {
            for (int i = 0; i < height; i++)
                Drawing.DrawLine(x, y + i, x + width, y + i, thickness, color);
        }

        /*public void ShowNotification(string message)
        {
            Notifications.Add(new Notification(null,message,null));
        }*/
        
        public bool IsCompatibleChamp(String championName)
        {
            return UltSpellData.Keys.Any(x => x == championName);
        }
        
        struct UltSpellDataS
        {
            public int SpellStage;
            public float DamageMultiplicator;
            public float Width;
            public float Delay;
            public float Speed;
            public bool Collision;
        }
        
        Dictionary<String, UltSpellDataS> UltSpellData = new Dictionary<string, UltSpellDataS>
        {
            {"Jinx",    new UltSpellDataS { SpellStage = 0, DamageMultiplicator = 1.0f, Width = 140f, Delay = 0600f/1000f, Speed = 1700f, Collision = true}},
            {"Ashe",    new UltSpellDataS { SpellStage = 0, DamageMultiplicator = 1.0f, Width = 130f, Delay = 0250f/1000f, Speed = 1600f, Collision = true}},
            {"Draven",  new UltSpellDataS { SpellStage = 0, DamageMultiplicator = 0.95f, Width = 160f, Delay = 0400f/1000f, Speed = 2000f, Collision = true}},
            {"Ezreal",  new UltSpellDataS { SpellStage = 0, DamageMultiplicator = 1.34f, Width = 160f, Delay = 1000f/1000f, Speed = 2000f, Collision = false}},
            {"Karthus", new UltSpellDataS { SpellStage = 0, DamageMultiplicator = 1.0f, Width = 000f, Delay = 3125f/1000f, Speed = 0000f, Collision = false}},
        };
    }
    
    
    
    
    class EnemyInfo
    {
        public AIHeroClient Player;
        public int LastSeen;

        public RecallInfo RecallInfo;

        public EnemyInfo(AIHeroClient player)
        {
            Player = player;
            RecallInfo = new RecallInfo(this);
        }
    }
    class RecallInfo
    {
        public EnemyInfo EnemyInfo;
        public Dictionary<int, float> IncomingDamage; //from, damage
        public RecallInf Recall;
        public RecallInf AbortedRecall;
        public bool LockedTarget;
        public float EstimatedShootT;
        public int AbortedT;
        public int FADEOUT_TIME = 3000;

        public RecallInfo(EnemyInfo enemyInfo)
        {
            EnemyInfo = enemyInfo;
            Recall = new RecallInf(
                EnemyInfo.Player.NetworkId,
                Teleport.TeleportStatus.Unknown,
                Teleport.TeleportType.Unknown,
                0);
            IncomingDamage = new Dictionary<int, float>();
        }

        public bool ShouldDraw()
        {
            return IsPorting() || (WasAborted() && GetDrawTime() > 0);
        }

        public bool IsPorting()
        {
            return Recall.Type == Teleport.TeleportType.Recall && Recall.Status == Teleport.TeleportStatus.Start;
        }

        public bool WasAborted()
        {
            return Recall.Type == Teleport.TeleportType.Recall && Recall.Status == Teleport.TeleportStatus.Abort;
        }

        public EnemyInfo UpdateRecall(RecallInf newRecall)
        {
            IncomingDamage.Clear();
            LockedTarget = false;
            EstimatedShootT = 0;

            if (newRecall.Type == Teleport.TeleportType.Recall && newRecall.Status == Teleport.TeleportStatus.Abort)
            {
                AbortedRecall = Recall;
                AbortedT = Variables.TickCount;
            }
            else
                AbortedT = 0;

            Recall = newRecall;
            return EnemyInfo;
        }

        public int GetDrawTime()
        {
            int drawtime = 0;

            if (WasAborted())
                drawtime = FADEOUT_TIME - (Variables.TickCount - AbortedT);
            else
                drawtime = GetRecallCountdown();

            return drawtime < 0 ? 0 : drawtime;
        }

        public int GetRecallCountdown()
        {
            int time = Variables.GameTimeTickCount;
            int countdown = 0;

            if (time - AbortedT < FADEOUT_TIME)
                countdown = AbortedRecall.Duration - (AbortedT - AbortedRecall.Start);
            else if (AbortedT > 0)
                countdown = 0; //AbortedT = 0
            else
                countdown = Recall.Start + Recall.Duration - time;

            return countdown < 0 ? 0 : countdown;
        }

        public override string ToString()
        {
            String drawtext = EnemyInfo.Player.CharacterName + ": " + Recall.Status; //change to better string and colored

            float countdown = GetRecallCountdown() / 1000f;

            if (countdown > 0)
                drawtext += " (" + countdown.ToString("0.00", System.Globalization.CultureInfo.InvariantCulture) + "s)";

            return drawtext;
        }
    }
}