using System;
using System.Linq;
using EnsoulSharp;
using EnsoulSharp.SDK;
using EnsoulSharp.SDK.MenuUI;
using SharpDX;

namespace StormAIO.utilities
{
    public class Rundown
    {
        private static Menu RundownMenu;

        #region MenuHelper

        private static bool rundownTop => RundownMenu["rundownTab"].GetValue<MenuBool>("rundownTop").Enabled;
        private static bool rundownMid => RundownMenu["rundownTab"].GetValue<MenuBool>("rundownMid").Enabled;
        private static bool rundownBot => RundownMenu["rundownTab"].GetValue<MenuBool>("rundownBot").Enabled;
        private static bool followBool => RundownMenu.GetValue<MenuBool>("selector").Enabled;

        #endregion

        #region Main

        public Rundown()
        {
            RundownMenu = new Menu("rundownMenu", "Rundown™")
            {
                new Menu("rundownTab", "Run it down?")
                {
                    new MenuBool("rundownTop", "Top").SetValue(false),
                    new MenuBool("rundownMid", "Mid").SetValue(false),
                    new MenuBool("rundownBot", "Bot").SetValue(false)
                },
                new MenuBool("selector", "Adjust the weight of the champs to follow").SetValue(false)
            };
            var allies = from hero in ObjectManager.Get<AIHeroClient>() where hero.IsAlly select hero;
            foreach (var ally in allies.Where(x => !x.IsMe))
                RundownMenu.Add(new MenuSlider(ally.CharacterName, "Follow weight: " + ally.CharacterName, 1, 1, 5));

            MainMenu.UtilitiesMenu.Add(RundownMenu);

            Game.OnUpdate += GameOnUpdate;
            //AIBaseClient.OnBuffGain += AiHeroClientOnOnBuffGain;
            Inting();
            InitSpell();
        }

        #endregion

        #region Other

        private static Spell Q, W, E;
        private static readonly Vector3 Bot = new Vector3(12743.6f, 2305.73f, 51.5507f);
        private static readonly Vector3 Top = new Vector3(2076, 12356, 52.8381f);

        private static void InitSpell()
        {
            Q = new Spell(SpellSlot.Q, 1000f);
            Q.SetSkillshot(0.25f, 20f, 1500f, false, SpellType.Line);
            W = new Spell(SpellSlot.W, 1000f);
            W.SetSkillshot(0.25f, 20f, 1500f, false, SpellType.Line);
            E = new Spell(SpellSlot.E, 1000f);
            E.SetSkillshot(0.25f, 20f, 1500f, false, SpellType.Circle);
        }

        private static bool TopSpotArrived { get; set; }
        private static bool RunMidSpot { get; set; }
        private static bool BotSpotArrived { get; set; }

        private static Vector3 _runitdownMid;
        private static AIHeroClient Player => ObjectManager.Player;

        #endregion

        #region Args

        private static void GameOnUpdate(EventArgs args)
        {
            Troll();
            Follow();
            if (Game.MapId != GameMapId.SummonersRift) return; // don't try to run to lanes if they Don't exist 
            if (rundownTop)
            {
                if (RunMidSpot || Player.IsDead)
                {
                    TopSpotArrived = false;
                    RunMidSpot = false;
                }

                if (Player.Position.Distance(Top) > 600 && !TopSpotArrived)
                    Player.IssueOrder(GameObjectOrder.MoveTo, Top);
                else
                    TopSpotArrived = true;

                if (TopSpotArrived)
                {
                    if (!RunMidSpot) Player.IssueOrder(GameObjectOrder.MoveTo, _runitdownMid);

                    if (Player.Position.Distance(_runitdownMid) < 400 || Player.IsDead) RunMidSpot = true;
                }
            }
            else
            {
                TopSpotArrived = false;
                RunMidSpot = false;
            }

            if (rundownMid) Player.IssueOrder(GameObjectOrder.MoveTo, _runitdownMid);
            if (rundownBot)
            {
                if (RunMidSpot || Player.IsDead)
                {
                    BotSpotArrived = false;
                    RunMidSpot = false;
                }

                if (Player.Position.Distance(Bot) > 600 && !BotSpotArrived)
                    Player.IssueOrder(GameObjectOrder.MoveTo, Bot);
                else
                    BotSpotArrived = true;

                if (BotSpotArrived)
                {
                    if (!RunMidSpot) Player.IssueOrder(GameObjectOrder.MoveTo, _runitdownMid);

                    if (Player.Position.Distance(_runitdownMid) < 400 || Player.IsDead) RunMidSpot = true;
                }
            }
            else
            {
                BotSpotArrived = false;
                RunMidSpot = false;
            }
        }

        // Use for testing purposes
        private void AiHeroClientOnOnBuffGain(AIBaseClient sender, AIBaseClientBuffAddEventArgs args)
        {
            if (args.Buff.Name.Contains("ASSETS"))
            {
                return;
            }

            if (sender.IsMe)
            {
                Game.Print("My Buff Name: " + args.Buff.Name);
                Game.Print("My Buff Count: " + args.Buff.Count);
                Game.Print("y Buff Type: " + args.Buff.Type);
                return;
            }

            Game.Print("Enemy Buff Name: " + args.Buff.Name);
            Game.Print("Enemy Buff Count: " + args.Buff.Count);
            Game.Print("Enemy Buff Type: " + args.Buff.Type);
        }

        #endregion

        #region Functions

        private static void Follow()
        {
            if (!followBool) return;
            var validAllies = GameObjects.AllyHeroes.Where(x => x != null && x.IsValid && !x.IsMe && !x.IsDead)
                .ToList();
            if (!validAllies.Any()) return;
            foreach (var ally in validAllies.OrderByDescending(x =>
                RundownMenu.GetValue<MenuSlider>(x.CharacterName).Value))
            {
                if (ally.IsRecalling() && (Player.CharacterName != "Anivia" || !Player.CharacterName.Equals("Trundle")))
                {
                    Player.Spellbook.CastSpell(SpellSlot.Recall);
                    return;
                }

                Orbwalker.Move(ally.Position.Extend(Player.Position, 100));
                return;
            }
        }

        private static void Troll()
        {
            var validAllies = GameObjects.AllyHeroes.Where(x => x != null && x.IsValid && !x.IsMe);

            foreach (var ally in validAllies.Where(x => x.HasBuff("recall") || x.HasBuff("SummonerTeleport")))
                if (ally.Position.Distance(Player) < 2000)

                    switch (Player.CharacterName)
                    {
                        case "Anivia":
                        {
                            if (!(ally.Position.Distance(Player) < 2000)) continue;
                            if (W.IsReady()) W.Cast(ally.Position);
                        }
                            break;

                        case "Trundle":
                        {
                            if (!(ally.Position.Distance(Player) < 2000)) continue;
                            if (E.IsReady()) E.Cast(ally.Position);
                        }
                            break;
                    }
        }

        private static void Inting()
        {
            var first = ObjectManager.Get<Obj_SpawnPoint>().FirstOrDefault(x => x.IsEnemy);
            if (first == null) return;
            _runitdownMid = first.Position;
        }

        #endregion
    }
}