using System;
using System.Collections.Generic;
using System.Linq;
using EnsoulSharp;
using EnsoulSharp.SDK;
using EnsoulSharp.SDK.MenuUI;
using EnsoulSharp.SDK.Utility;
using ExorAIO.Utilities;
using LeagueSharpCommon;
using SharpDX;
using SharpDX.Direct3D9;
using Color = System.Drawing.Color;
using Dash = EnsoulSharp.SDK.Dash;
using EnumerableExtensions = LeagueSharpCommon.EnumerableExtensions;
using GamePath = System.Collections.Generic.List<SharpDX.Vector2>;
namespace Evade
{
    internal class Program
    {
        public static SpellList<Skillshot> DetectedSkillshots = new SpellList<Skillshot>();
        private static bool _evading;
        private static Vector2 _evadePoint;
        public static bool NoSolutionFound = false;
        public static Vector2 EvadeToPoint = new Vector2();
        public static int LastWardJumpAttempt = 0;
        public static Vector2 PreviousTickPosition = new Vector2();
        public static Vector2 PlayerPosition = new Vector2();
        public static string PlayerChampionName;
        private static readonly Random RandomN = new Random();
        public static int LastSentMovePacketT = 0;
        public static int LastSentMovePacketT2 = 0;

        public static bool Evading
        {
            get { return _evading; }
            set
            {
                if (value == true)
                {
                    LastSentMovePacketT = 0;
                    ObjectManager.Player.SendMovePacket(LeagueSharpCommon.Geometry.Geometry.To2D(ObjectManager.Player.GetPath(LeagueSharpCommon.Geometry.Geometry.To3D(EvadePoint)).Last()));
                }

                _evading = value;
            }
        }

        public static Vector2 EvadePoint
        {
            get { return _evadePoint; }
            set
            {
                _evadePoint = value;
            }
        }

        private static Font WarningMsg { get; } = new Font(Drawing.Direct3DDevice9,
        new FontDescription
        {
            FaceName = "Courier New",
            Height = 17,
            OutputPrecision = FontPrecision.Default,
            Quality = FontQuality.Default,
            Weight = FontWeight.Normal
        });

        private static int startT;

        public static void Loads()
        {
            if (Game.State == GameState.Running)
                Game_OnGameStart();
            else
                GameEvent.OnGameLoad += Game_OnGameStart;
        }

        private static void Game_OnGameStart()
        {
            startT = Utils.TickCount;
            PlayerChampionName = ObjectManager.Player.CharacterName;

            //Create the menu to allow the user to change the config.
            Config.CreateMenu();

            //Add the game events.
            Game.OnUpdate += Game_OnOnGameUpdate;
            AIHeroClient.OnIssueOrder += ObjAiHeroOnOnIssueOrder;
            Spellbook.OnCastSpell += Spellbook_OnCastSpell;

            //Set up the OnDetectSkillshot Event.
            SkillshotDetector.OnDetectSkillshot += OnDetectSkillshot;
            SkillshotDetector.OnDeleteMissile += SkillshotDetectorOnOnDeleteMissile;

            //For skillshot drawing.
            Drawing.OnDraw += Drawing_OnDraw;

            //Ondash event.
            Dash.OnDash += UnitOnOnDash;

            DetectedSkillshots.OnAdd += DetectedSkillshots_OnAdd;
            Drawing.OnPreReset += dummyArgs => { WarningMsg.OnLostDevice(); };
            Drawing.OnPostReset += dummyArgs => { WarningMsg.OnResetDevice(); };

            //Initialze the collision
            Collision.Init();

            if (Config.PrintSpellData)
            {
                foreach (var hero in ObjectManager.Get<AIHeroClient>())
                {
                    foreach (var spell in hero.Spellbook.Spells.Where(s => s.SData.Name != "BaseSpell"))
                    {
                        if(Config.Debug) Console.WriteLine("\n\n");
                        if(Config.Debug) Console.WriteLine("SpellSlot: {0} Spell: {1}", spell.Slot, spell.SData.Name);
                        if(Config.Debug) Console.WriteLine("=================================================================");
                        foreach (var prop in spell.SData.GetType().GetProperties())
                        {
                            if (prop.Name != "Entries")
                                if(Config.Debug) Console.WriteLine("\t{0} => '{1}'", prop.Name, prop.GetValue(spell.SData, null));
                        }

                    }
                }
                Console.WriteLine(ObjectManager.Player.Spellbook.GetSpell(SpellSlot.W).Name);
            }

            if (Config.TestOnAllies)
                Benchmarking.Benchmark.Initialize();
        }
        

        private static void DetectedSkillshots_OnAdd(object sender, EventArgs e)
        {
            if(Config.Debug) Console.WriteLine("evading false3 ");
            Evading = false;
        }

        private static void SkillshotDetectorOnOnDeleteMissile(Skillshot skillshot, MissileClient missile)
        {
            if (skillshot.SpellData.SpellName == "VelkozQ")
            {
                var spellData = SpellDatabase.GetByName("VelkozQSplit");
                var direction = skillshot.Direction.Perpendicular();
                if (DetectedSkillshots.Count(s => s.SpellData.SpellName == "VelkozQSplit") == 0)
                {
                    for (var i = -1; i <= 1; i = i + 2)
                    {
                        var pos = skillshot.GetMissilePosition(-25);

                        var skillshotToAdd = new Skillshot(DetectionType.ProcessSpell, spellData, Utils.TickCount, pos,
                            pos + i * direction * spellData.Range, skillshot.Unit);
                        DetectedSkillshots.Add(skillshotToAdd);
                    }
                }
            }
        }

        private static void OnDetectSkillshot(Skillshot skillshot)
        {
            //Check if the skillshot is already added.
            var alreadyAdded = false;

            if (Config.Menu["Misc"]["DisableFow"].GetValue<MenuBool>().Enabled && !skillshot.Unit.IsVisible)
                return;

            foreach (var item in DetectedSkillshots)
            {
                if (item.SpellData.SpellName == skillshot.SpellData.SpellName &&
                    (item.Unit.NetworkId == skillshot.Unit.NetworkId &&
                     LeagueSharpCommon.Geometry.Geometry.AngleBetween((skillshot.Direction), item.Direction) < 5 &&
                     (LeagueSharpCommon.Geometry.Geometry.Distance(skillshot.Start, item.Start) < 100 || skillshot.SpellData.FromObjects.Length == 0)))
                {
                    alreadyAdded = true;
                    break;
                }
            }

            //Check if the skillshot is from an ally.
            if (skillshot.Unit.Team == ObjectManager.Player.Team && !Config.TestOnAllies)
                return;

            //Check if the skillshot is too far away.
            if (LeagueSharpCommon.Geometry.Geometry.Distance(skillshot.Start, PlayerPosition) > (skillshot.SpellData.Range + skillshot.SpellData.Radius + 1000) * 1.5)
                return;

            //Add the skillshot to the detected skillshot list.
            if (!alreadyAdded || skillshot.SpellData.DontCheckForDuplicates)
            {
                //Multiple skillshots like twisted fate Q.
                if (skillshot.DetectionType == DetectionType.ProcessSpell)
                {
                    if (skillshot.SpellData.MultipleNumber != -1)
                    {
                        var originalDirection = skillshot.Direction;

                        for (var i = -(skillshot.SpellData.MultipleNumber - 1) / 2;
                            i <= (skillshot.SpellData.MultipleNumber - 1) / 2;
                            i++)
                        {
                            var end = skillshot.Start +
                                      skillshot.SpellData.Range *
                                      LeagueSharpCommon.Geometry.Geometry.Rotated(originalDirection, skillshot.SpellData.MultipleAngle * i);
                            var skillshotToAdd = new Skillshot(
                                skillshot.DetectionType, skillshot.SpellData, skillshot.StartTick, skillshot.Start, end,
                                skillshot.Unit);

                            DetectedSkillshots.Add(skillshotToAdd);
                        }
                        return;
                    }

                    if (skillshot.SpellData.SpellName == "BardR" && LeagueSharpCommon.Geometry.Geometry.Distance(skillshot.End, skillshot.Start) < 850)
                    {
                        skillshot.StartTick = Utils.TickCount - skillshot.SpellData.Delay + 800;
                    }

                    if (skillshot.SpellData.SpellName == "MordekaiserE")
                    {
                        var end = skillshot.OriginalEnd;
                        if (LeagueSharpCommon.Geometry.Geometry.Distance(skillshot.Start, skillshot.OriginalEnd) > 700)
                        {
                            end = skillshot.Start + skillshot.Direction * 700;
                        }

                        skillshot.End = end + skillshot.Direction * 275;
                        skillshot.Start = skillshot.End;
                        skillshot.End = skillshot.End - skillshot.Direction * 900;
                        skillshot.SpellData.Delay = 200 + 250 + (int)((LeagueSharpCommon.Geometry.Geometry.Distance(skillshot.Start, skillshot.End) / skillshot.SpellData.MissileSpeed) * 1000);

                        var skillshotToAdd = new Skillshot(
                            skillshot.DetectionType, skillshot.SpellData, skillshot.StartTick, skillshot.Start, skillshot.End,
                            skillshot.Unit);

                        DetectedSkillshots.Add(skillshotToAdd);
                        return;
                    }

                    if (skillshot.SpellData.SpellName == "SennaQCast" && skillshot.Unit != null)
                    {
                        skillshot.SpellData.Delay = (int)(skillshot.Unit.AttackCastDelay * 1000);
                    }

                    if (skillshot.SpellData.SpellName == "UFSlash")
                    {
                        skillshot.SpellData.MissileSpeed = 1600 + (int)skillshot.Unit.MoveSpeed;
                    }

                    if (skillshot.SpellData.SpellName == "SionR")
                    {
                        skillshot.SpellData.MissileSpeed = (int)skillshot.Unit.MoveSpeed;
                    }

                    if (skillshot.SpellData.Invert)
                    {
                        var newDirection = -LeagueSharpCommon.Geometry.Geometry.Normalized((skillshot.End - skillshot.Start));
                        var end = skillshot.Start + newDirection * LeagueSharpCommon.Geometry.Geometry.Distance(skillshot.Start, skillshot.End);
                        var skillshotToAdd = new Skillshot(
                            skillshot.DetectionType, skillshot.SpellData, skillshot.StartTick, skillshot.Start, end,
                            skillshot.Unit);
                        DetectedSkillshots.Add(skillshotToAdd);
                        return;
                    }

                    if (skillshot.SpellData.Centered)
                    {
                        var start = skillshot.Start - skillshot.Direction * skillshot.SpellData.Range;
                        var end = skillshot.Start + skillshot.Direction * skillshot.SpellData.Range;
                        var skillshotToAdd = new Skillshot(
                            skillshot.DetectionType, skillshot.SpellData, skillshot.StartTick, start, end,
                            skillshot.Unit);
                        DetectedSkillshots.Add(skillshotToAdd);
                        return;
                    }

                    if (skillshot.SpellData.SpellName == "TaricE" && (skillshot.Unit as AIHeroClient).CharacterName == "Taric")
                    {
                        var target = HeroManager.AllHeroes.FirstOrDefault(h => h.Team == skillshot.Unit.Team && h.IsVisible && h.HasBuff("taricwleashactive"));
                        if (target != null)
                        {
                            var start = target.ServerPosition.To2D();
                            var direction = LeagueSharpCommon.Geometry.Geometry.Normalized((skillshot.OriginalEnd - start));
                            var end = start + direction * skillshot.SpellData.Range;
                            var skillshotToAdd = new Skillshot(
                                    skillshot.DetectionType, skillshot.SpellData, skillshot.StartTick,
                                    start, end, target)
                            {
                                OriginalEnd = skillshot.OriginalEnd
                            };
                            DetectedSkillshots.Add(skillshotToAdd);
                        }
                    }

                    if (skillshot.SpellData.SpellName == "SylasQ")
                    {
                        var sylasQLine = SpellDatabase.GetByName("SylasQLine");

                        if (sylasQLine != null)
                        {
                            var dir = skillshot.Direction.Perpendicular();
                            var leftStart = skillshot.Start + dir * 125;
                            var leftEnd = LeagueSharpCommon.Geometry.Geometry.Extend(leftStart, skillshot.End, sylasQLine.Range);

                            var rightStart = skillshot.Start - dir * 125;
                            var rightEnd = LeagueSharpCommon.Geometry.Geometry.Extend(rightStart, skillshot.End, sylasQLine.Range);

                            DetectedSkillshots.Add(new Skillshot(skillshot.DetectionType, sylasQLine, skillshot.StartTick, leftStart, leftEnd, skillshot.Unit));
                            DetectedSkillshots.Add(new Skillshot(skillshot.DetectionType, sylasQLine, skillshot.StartTick, rightStart, rightEnd, skillshot.Unit));
                        }
                    }

                    if (skillshot.SpellData.SpellName == "PykeR")
                    {
                        var start2 = skillshot.End + new Vector2(250, -250);
                        var end2 = skillshot.End + new Vector2(-250, 250);

                        skillshot.Start = skillshot.End - new Vector2(250, 250);
                        skillshot.End = skillshot.End + new Vector2(250, 250);

                        DetectedSkillshots.Add(new Skillshot(skillshot.DetectionType, skillshot.SpellData, skillshot.StartTick, start2, end2, skillshot.Unit));
                        DetectedSkillshots.Add(new Skillshot(skillshot.DetectionType, skillshot.SpellData, skillshot.StartTick, skillshot.Start, skillshot.End, skillshot.Unit));
                        return;
                    }

                    if (skillshot.SpellData.SpellName == "SyndraE" || skillshot.SpellData.SpellName == "syndrae5")
                    {
                        var angle = 60;
                        var edge1 =
                            LeagueSharpCommon.Geometry.Geometry.Rotated((skillshot.End - LeagueSharpCommon.Geometry.Geometry.To2D(skillshot.Unit.ServerPosition)), -angle / 2 * (float)Math.PI / 180);
                        var edge2 = LeagueSharpCommon.Geometry.Geometry.Rotated(edge1, angle * (float)Math.PI / 180);

                        var positions = new GamePath();

                        var explodingQ = DetectedSkillshots.FirstOrDefault(s => s.SpellData.SpellName == "SyndraQ");

                        if (explodingQ != null)
                        {
                            var position = explodingQ.End;
                            var v = position - LeagueSharpCommon.Geometry.Geometry.To2D(skillshot.Unit.ServerPosition);
                            if (LeagueSharpCommon.Geometry.Geometry.CrossProduct(edge1, v) > 0 && LeagueSharpCommon.Geometry.Geometry.CrossProduct(v, edge2) > 0 &&
                                position.Distance(skillshot.Unit) < 800)
                            {
                                var start = position;
                                var end1 = LeagueSharpCommon.Geometry.Geometry.To2D(skillshot.Unit.ServerPosition);
                                var end =    LeagueSharpCommon.Geometry.Geometry.Extend(end1,position, LeagueSharpCommon.Geometry.Geometry.Distance(skillshot.Unit, position) > 200 ? 1300 : 1000);

                                var startTime = skillshot.StartTick;

                                startTime += (int)(150 + Math.Min(250, explodingQ.StartTick + explodingQ.SpellData.Delay - 150 - Utils.TickCount) + LeagueSharpCommon.Geometry.Geometry.Distance(skillshot.Unit, position) / 2.5f);
                                var skillshotToAdd = new Skillshot(
                                    skillshot.DetectionType, skillshot.SpellData, startTime, start, end,
                                    skillshot.Unit);
                                DetectedSkillshots.Add(skillshotToAdd);
                            }
                        }

                        foreach (var minion in ObjectManager.Get<AIMinionClient>())
                        {
                            if (minion.Name == "Seed" && !minion.IsDead && (minion.Team != ObjectManager.Player.Team || Config.TestOnAllies))
                            {
                                positions.Add(LeagueSharpCommon.Geometry.Geometry.To2D(minion.ServerPosition));
                            }
                        }

                        foreach (var position in positions)
                        {
                            var v = position - LeagueSharpCommon.Geometry.Geometry.To2D(skillshot.Unit.ServerPosition);
                            if (LeagueSharpCommon.Geometry.Geometry.CrossProduct(edge1, v) > 0 && LeagueSharpCommon.Geometry.Geometry.CrossProduct(v, edge2) > 0 &&
                                position.Distance(skillshot.Unit) < 800)
                            {
                                var start = position;
                                //var end = skillshot.Unit.ServerPosition.To2D()
                                 //   .Extend(position, skillshot.Unit.Distance(position) > 200 ? 1300 : 1000);
                                 var end1 = (skillshot.Unit.ServerPosition).ToVector2();
                                 var end = LeagueSharpCommon.Geometry.Geometry.Extend(end1,position, skillshot.Unit.Distance(position) > 200 ? 1300 : 1000);

                                var startTime = skillshot.StartTick;

                                startTime += (int)(150 + LeagueSharpCommon.Geometry.Geometry.Distance(skillshot.Unit, position) / 2.5f);
                                var skillshotToAdd = new Skillshot(
                                    skillshot.DetectionType, skillshot.SpellData, startTime, start, end,
                                    skillshot.Unit);
                                DetectedSkillshots.Add(skillshotToAdd);
                            }
                        }
                        return;
                    }

                    if (skillshot.SpellData.SpellName == "ZoeE")
                    {
                        Vector2 wall_start = Vector2.Zero;
                        float range_left = 0;
                        var range_max = skillshot.SpellData.RawRange + skillshot.SpellData.ExtraRange;

                        for (int i = 0; i < range_max; i += 10)
                        {
                            var curr_pos = skillshot.Start + skillshot.Direction * i;

                            if (Utility.IsWall(curr_pos))
                            {
                                wall_start = curr_pos;
                                range_left = range_max - i;
                                break;
                            }
                        }

                        int max = 70;
                        while (Utility.IsWall(wall_start) && max > 0)
                        {
                            wall_start = wall_start + skillshot.Direction * 35;
                            max--;
                        }

                        for (int i = 0; i < range_left; i += 10)
                        {
                            var curr_pos = wall_start + skillshot.Direction * i;

                            if (Utility.IsWall(curr_pos))
                            {
                                range_left = i;
                                break;
                            }
                        }



                        if (range_left > 0)
                        {
                            skillshot.End = wall_start + skillshot.Direction * range_left;

                            var skillshotToAdd = new Skillshot(
                                skillshot.DetectionType, skillshot.SpellData, skillshot.StartTick, skillshot.Start, skillshot.End,
                                skillshot.Unit);
                            DetectedSkillshots.Add(skillshotToAdd);
                            return;
                        }
                    }

                    if (skillshot.SpellData.SpellName == "MalzaharQ")
                    {
                        var start = skillshot.End - skillshot.Direction.Perpendicular() * 400;
                        var end = skillshot.End + skillshot.Direction.Perpendicular() * 400;
                        var skillshotToAdd = new Skillshot(
                            skillshot.DetectionType, skillshot.SpellData, skillshot.StartTick, start, end,
                            skillshot.Unit);
                        DetectedSkillshots.Add(skillshotToAdd);
                        return;
                    }

                    if (skillshot.SpellData.SpellName == "ZyraQ")
                    {
                        var start = skillshot.End - skillshot.Direction.Perpendicular() * 450;
                        var end = skillshot.End + skillshot.Direction.Perpendicular() * 450;
                        var skillshotToAdd = new Skillshot(
                            skillshot.DetectionType, skillshot.SpellData, skillshot.StartTick, start, end,
                            skillshot.Unit);
                        DetectedSkillshots.Add(skillshotToAdd);
                        return;
                    }

                    if (skillshot.SpellData.SpellName == "DianaQ")
                    {
                        var skillshotToAdd = new Skillshot(
                        skillshot.DetectionType, SpellDatabase.GetByName("DianaArcArc"), skillshot.StartTick, skillshot.Start, skillshot.End, skillshot.Unit);

                        DetectedSkillshots.Add(skillshotToAdd);
                    }

                    if (skillshot.SpellData.SpellName == "ZiggsQ")
                    {
                        var d1 = LeagueSharpCommon.Geometry.Geometry.Distance(skillshot.Start, skillshot.End);
                        var d2 = d1 * 0.4f;
                        var d3 = d2 * 0.69f;


                        var bounce1SpellData = SpellDatabase.GetByName("ZiggsQBounce1");
                        var bounce2SpellData = SpellDatabase.GetByName("ZiggsQBounce2");

                        var bounce1Pos = skillshot.End + skillshot.Direction * d2;
                        var bounce2Pos = bounce1Pos + skillshot.Direction * d3;

                        bounce1SpellData.Delay =
                            (int)(skillshot.SpellData.Delay + d1 * 1000f / skillshot.SpellData.MissileSpeed + 500);
                        bounce2SpellData.Delay =
                            (int)(bounce1SpellData.Delay + d2 * 1000f / bounce1SpellData.MissileSpeed + 500);

                        var bounce1 = new Skillshot(
                            skillshot.DetectionType, bounce1SpellData, skillshot.StartTick, skillshot.End, bounce1Pos,
                            skillshot.Unit);
                        var bounce2 = new Skillshot(
                            skillshot.DetectionType, bounce2SpellData, skillshot.StartTick, bounce1Pos, bounce2Pos,
                            skillshot.Unit);

                        DetectedSkillshots.Add(bounce1);
                        DetectedSkillshots.Add(bounce2);
                    }

                    if (skillshot.SpellData.SpellName == "ZiggsR")
                    {
                        skillshot.SpellData.Delay =
                            (int)(1500 + 1500 * LeagueSharpCommon.Geometry.Geometry.Distance(skillshot.End, skillshot.Start) / skillshot.SpellData.Range);
                    }

                    if (skillshot.SpellData.SpellName == "JarvanIVDragonStrike")
                    {
                        var endPos = new Vector2();

                        foreach (var s in DetectedSkillshots)
                        {
                            if (s.Unit.NetworkId == skillshot.Unit.NetworkId && s.SpellData.Slot == SpellSlot.E)
                            {
                                var extendedE = new Skillshot(
                                    skillshot.DetectionType, skillshot.SpellData, skillshot.StartTick, skillshot.Start,
                                    skillshot.End + skillshot.Direction * 100, skillshot.Unit);
                                if (!extendedE.IsSafe(s.End))
                                {
                                    endPos = s.End;
                                }
                                break;
                            }
                        }

                        foreach (var m in ObjectManager.Get<AIMinionClient>())
                        {
                            if (m.Name == "jarvanivstandard" && m.Team == skillshot.Unit.Team)
                            {

                                var extendedE = new Skillshot(
                                    skillshot.DetectionType, skillshot.SpellData, skillshot.StartTick, skillshot.Start,
                                    skillshot.End + skillshot.Direction * 100, skillshot.Unit);
                                if (!extendedE.IsSafe(LeagueSharpCommon.Geometry.Geometry.To2D(m.Position)))
                                {
                                    endPos = LeagueSharpCommon.Geometry.Geometry.To2D(m.Position);
                                }
                                break;
                            }
                        }

                        if (LeagueSharpCommon.Geometry.Geometry.IsValid(endPos))
                        {
                            skillshot = new Skillshot(DetectionType.ProcessSpell, SpellDatabase.GetByName("JarvanIVEQ"), Utils.TickCount, skillshot.Start, endPos, skillshot.Unit);
                            skillshot.End = endPos + 200 * LeagueSharpCommon.Geometry.Geometry.Normalized((endPos - skillshot.Start));
                            skillshot.Direction = LeagueSharpCommon.Geometry.Geometry.Normalized((skillshot.End - skillshot.Start));
                        }
                    }
                }

                if (skillshot.SpellData.SpellName == "OriannasQ")
                {
                    var skillshotToAdd = new Skillshot(
                        skillshot.DetectionType, SpellDatabase.GetByName("OriannaQend"), skillshot.StartTick, skillshot.Start, skillshot.End,
                        skillshot.Unit);

                    DetectedSkillshots.Add(skillshotToAdd);
                }

                if (skillshot.SpellData.SpellName == "IreliaE2")
                {
                    var reg = new System.Text.RegularExpressions.Regex("Irelia_.+_E_.+_Indicator");
                    var firstE = ObjectManager.Get<EffectEmitter>().Where(x => x.IsValid && reg.IsMatch(x.Name)).
                        OrderByDescending(x => Vector3Extensions.Distance(x.Position, LeagueSharpCommon.Geometry.Geometry.To3D2(skillshot.Start))).FirstOrDefault();

                    if (firstE == null)
                    {
                        var firstEMissile = ObjectManager.Get<MissileClient>().Where(x =>
                            x.IsValid && LeagueSharpCommon.Geometry.Geometry.Distance(LeagueSharpCommon.Geometry.Geometry.To2D(x.EndPosition), skillshot.End) > 5 && x.SData.Name == skillshot.SpellData.MissileSpellName).
                            OrderByDescending(x => LeagueSharpCommon.Geometry.Geometry.Distance(x.Position, LeagueSharpCommon.Geometry.Geometry.To3D2(skillshot.Start))).FirstOrDefault();

                        if (firstEMissile != null)
                        {
                            if(Config.Debug) Console.WriteLine("Adding from missile");

                            var skillshotToAdd = new Skillshot(skillshot.DetectionType, skillshot.SpellData, skillshot.StartTick, LeagueSharpCommon.Geometry.Geometry.To2D(firstEMissile.EndPosition), skillshot.End,
                                skillshot.Unit);

                            DetectedSkillshots.Add(skillshotToAdd);
                        }
                        return;
                    }

                    if (firstE != null)
                    {
                        if(Config.Debug) Console.WriteLine("Adding from particle");

                        var skillshotToAdd = new Skillshot(skillshot.DetectionType, skillshot.SpellData, skillshot.StartTick, LeagueSharpCommon.Geometry.Geometry.To2D(firstE.Position), skillshot.End,
                            skillshot.Unit);

                        DetectedSkillshots.Add(skillshotToAdd);
                    }

                    return;
                }


                //Dont allow fow detection.
                if (skillshot.SpellData.DisableFowDetection && skillshot.DetectionType == DetectionType.RecvPacket)
                {
                    return;
                }
#if DEBUG
                //Console.WriteLine(Utils.TickCount + "Adding new skillshot: " + skillshot.SpellData.SpellName);
#endif

                DetectedSkillshots.Add(skillshot);
            }
        }

        private static void Game_OnOnGameUpdate(EventArgs args)
        {
            PlayerPosition = LeagueSharpCommon.Geometry.Geometry.To2D(ObjectManager.Player.ServerPosition);

            //Set evading to false after blinking
            if (LeagueSharpCommon.Geometry.Geometry.IsValid(PreviousTickPosition) && LeagueSharpCommon.Geometry.Geometry.Distance(PlayerPosition, PreviousTickPosition) > 200)
            {
                Evading = false;
                EvadeToPoint = Vector2.Zero;
            }

            PreviousTickPosition = PlayerPosition;

            //Remove the detected skillshots that have expired.
            DetectedSkillshots.RemoveAll(skillshot => !skillshot.IsActive());

            //Trigger OnGameUpdate on each skillshot.
            foreach (var skillshot in DetectedSkillshots)
            {
                skillshot.Game_OnGameUpdate();
            }

            //Evading disabled
            if (!Config.Menu["Enabled"].GetValue<MenuKeyBind>().Active || Config.Menu["dontDodge"].GetValue<MenuKeyBind>().Active)
            {
                Evading = false;
                return;
            }

            if (PlayerChampionName == "Olaf" && Config.Menu["Misc"]["DisableEvadeForOlafR"].GetValue<MenuBool>().Enabled && ObjectManager.Player.HasBuff("OlafRagnarok"))
            {
                Evading = false;
                return;
            }

            //Avoid sending move/cast packets while dead.
            if (ObjectManager.Player.IsDead)
            {
                Evading = false;
                EvadeToPoint = Vector2.Zero;
                return;
            }

            //Avoid sending move/cast packets while channeling interruptable spells that cause hero not being able to move.
            if (ObjectManager.Player.IsCastingImporantSpell())
            {
                Evading = false;
                EvadeToPoint = Vector2.Zero;
                return;
            }


            //if (ObjectManager.Player.IsWindingUp && !Orbwalking.IsAutoAttack(ObjectManager.Player.LastCastedSpellName()))
            //{
            //    Evading = false;
            //    return;
            //}

            /*Avoid evading while stunned or immobile.*/
            if (Utils.ImmobileTime(ObjectManager.Player) - Utils.TickCount > Game.Ping / 2 + 70)
            {
                Evading = false;
                return;
            }

            /*Avoid evading while dashing.*/
            if (Dash.IsDashing(ObjectManager.Player))
            {
                Evading = false;
                return;
            }

            //Don't evade while casting R as sion
            if (PlayerChampionName == "Sion" && ObjectManager.Player.HasBuff("SionR"))
                return;

            //Shield allies.
            foreach (var ally in ObjectManager.Get<AIHeroClient>())
            {
                if (AttackUnitExtensions.IsValidTarget(ally, 1000, false))
                {
                    var shieldAlly = Config.Menu["Shielding"]["shield" + ally.CharacterName];
                    if (shieldAlly != null && shieldAlly.GetValue<MenuBool>().Enabled)
                    {
                        var allySafeResult = IsSafe(LeagueSharpCommon.Geometry.Geometry.To2D(ally.ServerPosition));

                        if (!allySafeResult.IsSafe)
                        {
                            var dangerLevel = 0;

                            foreach (var skillshot in allySafeResult.SkillshotList)
                            {
                                dangerLevel = Math.Max(dangerLevel, skillshot.GetValue<MenuSlider>("DangerLevel").Value);
                            }

                            foreach (var evadeSpell in EvadeSpellDatabase.Spells)
                            {
                                if (evadeSpell.IsShield && evadeSpell.CanShieldAllies &&
                                    LeagueSharpCommon.Geometry.Geometry.Distance(ally, ObjectManager.Player.ServerPosition) < evadeSpell.MaxRange &&
                                    dangerLevel >= evadeSpell.DangerLevel &&
                                    ObjectManager.Player.Spellbook.CanUseSpell(evadeSpell.Slot) == SpellState.Ready &&
                                    IsAboutToHit(ally, evadeSpell.Delay))
                                {
                                    ObjectManager.Player.Spellbook.CastSpell(evadeSpell.Slot, ally);
                                }
                            }
                        }
                    }
                }
            }

            //Spell Shielded
            if (ObjectManager.Player.IsSpellShielded())
                return;

            var currentPath = Utility.GetWaypoints(ObjectManager.Player);
            var safeResult = IsSafe(PlayerPosition);
            var safePath = IsSafePath(currentPath, 100);

            NoSolutionFound = false;

            //Continue evading
            if (Evading && IsSafe(EvadePoint).IsSafe)
            {
                if (safeResult.IsSafe)
                {
                    if(Config.Debug) Console.WriteLine("evading false1 ");
                    //We are safe, stop evading.
                    Evading = false;
                }
                else
                {
                    if (!Orbwalker.CanMove() && CanAttackInSkillshot())
                    {
                        return;
                    }

                    if (Utils.TickCount - LastSentMovePacketT > 1000 / 3)
                    {
                        LastSentMovePacketT = Utils.TickCount;
                        ObjectManager.Player.SendMovePacket(LeagueSharpCommon.Geometry.Geometry.To2D(ObjectManager.Player.GetPath(LeagueSharpCommon.Geometry.Geometry.To3D2(EvadePoint)).Last()));
                    }
                    return;
                }
            }
            //Stop evading if the point is not safe.
            else if (Evading)
            {
                if(Config.Debug) Console.WriteLine("evading false2 ");
                Evading = false;
            }

           
            //The path is not safe.
            if (!safePath.IsSafe)
            {
                //Inside the danger polygon.
                if (!safeResult.IsSafe)
                {
                    //Search for an evade point:
                    TryToEvade(safeResult.SkillshotList, LeagueSharpCommon.Geometry.Geometry.IsValid(EvadeToPoint) ? EvadeToPoint : LeagueSharpCommon.Geometry.Geometry.To2D(Game.CursorPos));
                }
            }
        }

        static bool CanAttackInSkillshot()
        {
            var canAttack = true;
            foreach (var skillshot in DetectedSkillshots)
            {
                var dangerValue = skillshot.SpellData.DangerValue;
                var dangerSkillMenu = Config.skillShots[skillshot.SpellData.MenuItemName]["DangerLevel" + skillshot.SpellData.MenuItemName];
                if (dangerSkillMenu != null)
                    //Ga
                    dangerValue = dangerSkillMenu.GetValue<MenuSlider>().Value;

                if (skillshot.Evade() && skillshot.IsDanger(PlayerPosition) && dangerValue > Config.misc["AllowAaLevel"].GetValue<MenuSlider>().Value)
                    canAttack = false;
            }
            return canAttack;
        }

        static void Spellbook_OnCastSpell(Spellbook sender, SpellbookCastSpellEventArgs args)
        {
            if (sender.Owner.IsValid && sender.Owner.IsMe)
            {
                //if (args.Slot == SpellSlot.Recall)
                //    EvadeToPoint = new Vector2();
                var pointToCheck = HeroManager.Player.Position - HeroManager.Player.Direction * 100;

                if (Evading || !IsSafe(PlayerPosition).IsSafe || !IsSafe(LeagueSharpCommon.Geometry.Geometry.To2D(pointToCheck)).IsSafe)
                {
                    var blockLevel = Config.Menu["Misc"]["BlockSpells"].GetValue<MenuList>().Index;

                    if (blockLevel == 0)
                        return;

                    var isDangerous = false;
                    foreach (var skillshot in DetectedSkillshots)
                    {
                        if (skillshot.Evade() && skillshot.IsDanger(PlayerPosition) )
                        {
                            isDangerous = skillshot.GetValue<MenuBool>("IsDangerous").Enabled;  
                        }
                    }

                    if (blockLevel == 1 && !isDangerous)
                        return;

                    if(args.Slot == SpellSlot.Q)
                        args.Process = !Config.Menu["SpellBlocker"]["spellBlockerQ"].GetValue<MenuBool>().Enabled;
                    if (args.Slot == SpellSlot.W)
                        args.Process = !Config.Menu["SpellBlocker"]["spellBlockerW"].GetValue<MenuBool>().Enabled;
                    if (args.Slot == SpellSlot.E)
                        args.Process = !Config.Menu["SpellBlocker"]["spellBlockerE"].GetValue<MenuBool>().Enabled;
                    if (args.Slot == SpellSlot.R)
                        args.Process = !Config.Menu["SpellBlocker"]["spellBlockerR"].GetValue<MenuBool>().Enabled;
                }
            }
        }

        /// Used to block the movement to avoid entering in dangerous areas.
        private static void ObjAiHeroOnOnIssueOrder(AIBaseClient sender, AIBaseClientIssueOrderEventArgs args)
        {
            if (!sender.IsMe)
                return;

            if (args.Order == GameObjectOrder.MoveTo || args.Order == GameObjectOrder.AttackUnit)
            {
                EvadeToPoint.X = args.TargetPosition.X;
                EvadeToPoint.Y = args.TargetPosition.Y;
            }
            else
            {
                EvadeToPoint = Vector2.Zero;
            }

            //Don't block the movement packets if cant find an evade point.
            if (NoSolutionFound)
            {
                if(Config.Debug) Console.WriteLine("NoSolutionFound");
                return;
            }

            //Evading disabled
            if (!Config.Menu["Enabled"].GetValue<MenuKeyBind>().Active || Config.Menu["dontDodge"].GetValue<MenuKeyBind>().Active)
                return;

            if (EvadeSpellDatabase.Spells.Any(evadeSpell => evadeSpell.Name == "Walking" && !evadeSpell.Enabled))
                return;

            //Spell Shielded
            if (ObjectManager.Player.IsSpellShielded())
                return;

            if (PlayerChampionName == "Olaf" && Config.Menu["Misc"]["DisableEvadeForOlafR"].GetValue<MenuBool>().Enabled && ObjectManager.Player.HasBuff("OlafRagnarok"))
                return;

            var myPath = ObjectManager.Player.GetPath(new Vector3(args.TargetPosition.X, args.TargetPosition.Y, ObjectManager.Player.ServerPosition.Z)).To2DList();
            var safeResult = IsSafe(PlayerPosition);

            if (args.Order == GameObjectOrder.AttackUnit)
            {
                var target = args.Target;
                if (target != null && target.IsValid && target.IsVisible)
                {
                    if(CanAttackInSkillshot())
                        return;
                }
            }
            //If we are evading:
            if (Evading || !safeResult.IsSafe)
            {
                var rcSafePath = IsSafePath(myPath, Config.EvadingRouteChangeTimeOffset);
                if (args.Order == GameObjectOrder.MoveTo)
                {
                    if(Config.Debug) Console.WriteLine("Evading issue order " + Evading);

                    if (Evading && Utils.TickCount - Config.LastEvadePointChangeT > Config.EvadePointChangeInterval)
                    {
                        if(Config.Debug) Console.WriteLine("update point first ");
                        //Update the evade point to the closest one:
                        var points = Evader.GetEvadePoints(-1, 0, false, true);
                        if (points.Count > 0)
                        {
                            if(Config.Debug) Console.WriteLine("update point");
                            var to = new Vector2(args.TargetPosition.X, args.TargetPosition.Y);
                            EvadePoint = to.Closest(points);
                            Evading = true;
                            Config.LastEvadePointChangeT = Utils.TickCount;
                        }
                    }

                    //If the path is safe let the user follow it.
                    if (rcSafePath.IsSafe && IsSafe(myPath[myPath.Count - 1]).IsSafe && args.Order == GameObjectOrder.MoveTo)
                    {
                        if(Config.Debug) Console.WriteLine("update path");
                        EvadePoint = myPath[myPath.Count - 1];
                        Evading = true;
                    }
                }

                //Block the packets if we are evading or not safe.
                args.Process = false;
                return;
            }

            var safePath = IsSafePath(myPath, Config.CrossingTimeOffset);

            //Not evading, outside the skillshots.
            if (!safePath.IsSafe && args.Order != GameObjectOrder.AttackUnit)
            {
                if(Config.Debug) Console.WriteLine("block move");
                Vector2 pathfinderPoint = GetPathFinderPoint();
                if (LeagueSharpCommon.Geometry.Geometry.IsValid(pathfinderPoint))
                {
                    if(Config.Debug) Console.WriteLine("FOUND POINT");
                    if (Utils.TickCount - LastSentMovePacketT > 1000 / 3)
                    {
                        LastSentMovePacketT = Utils.TickCount;
                        ObjectManager.Player.SendMovePacket(ObjectManager.Player.GetPath(LeagueSharpCommon.Geometry.Geometry.To3D(pathfinderPoint)).Last().To2D(), true);
                    }
                }
                args.Process = false;
                return;
            }

            //AutoAttacks.
            if (!safePath.IsSafe && args.Order == GameObjectOrder.AttackUnit)
            {
                var target = args.Target;
                if (target != null && target.IsValid && target.IsVisible)
                {
                    //Out of attack range.
                    if (Vector2Extensions.Distance(PlayerPosition, ((AIBaseClient)target).ServerPosition) >
                        ObjectManager.Player.AttackRange + ObjectManager.Player.BoundingRadius +
                        target.BoundingRadius)
                    {
                        if (safePath.Intersection.Valid)
                        {
                            ObjectManager.Player.SendMovePacket(ObjectManager.Player.GetPath(LeagueSharpCommon.Geometry.Geometry.To3D(safePath.Intersection.Point)).Last().To2D());
                        }
                        args.Process = false;
                        return;
                    }
                }
            }
            if(Config.Debug) Console.WriteLine("move accept");
        }

        public static Vector2 GetPathFinderPoint()
        {
            var gameCursorVec2 = Game.CursorPos.To2D();
            var Points = Utils.CirclePoints(36, 300, ObjectManager.Player.Position.To2D());
            Points = Points.OrderBy(x => x.Distance(gameCursorVec2)).ToArray();

            foreach (var vector2 in Points)
            {
                var truePosition = Utils.CutVector(PlayerPosition,vector2);

                if (!IsSafe(truePosition).IsSafe)
                    continue;

                if (ObjectManager.Player.Distance(truePosition) < 100 * 100)
                    continue;

                var safeResult = IsSafePath(ObjectManager.Player.GetPath(LeagueSharpCommon.Geometry.Geometry.To3D(truePosition)).To2DList(), Config.CrossingTimeOffset);
                if (!safeResult.IsSafe || safeResult.Intersection.Valid)
                    continue;

                if (LeagueSharpCommon.Geometry.Geometry.AngleBetween(ObjectManager.Player.Direction.To2D(), truePosition - PlayerPosition) < 120)
                    return truePosition;
            }

            return Vector2.Zero;
        }

        private static void UnitOnOnDash(AIBaseClient sender, Dash.DashArgs args)
        {
            if (sender.IsMe)
            {
                if (Config.PrintSpellData)
                {
                    Console.WriteLine(Utils.TickCount + "DASH: Speed: " + args.Speed + " Width:" + Vector2Extensions.Distance(args.EndPos, args.StartPos));
                }

                EvadeToPoint = args.EndPos;
                //Utility.DelayAction.Add(args.Duration, delegate { Evading = false; });
            }
        }

        /// Returns true if the point is not inside the detected skillshots.
        public static IsSafeResult IsSafe(Vector2 point)
        {
            var result = new IsSafeResult();
            result.SkillshotList = new List<Skillshot>();

            foreach (var skillshot in DetectedSkillshots)
            {
                if (skillshot.Evade() && skillshot.IsDanger(point) )
                {
                    result.SkillshotList.Add(skillshot);
                }
            }

            result.IsSafe = (result.SkillshotList.Count == 0);
            return result;
        }

        /// Returns if the unit will get hit by skillshots taking the path.
        public static SafePathResult IsSafePath(GamePath path, int timeOffset, int speed = -1, int delay = 0, AIBaseClient unit = null)
        {
            var IsSafe = true;
            var intersections = new List<FoundIntersection>();
            var intersection = new FoundIntersection();

            foreach (var skillshot in DetectedSkillshots)
            {
                if (skillshot.Evade())
                {
                    var sResult = skillshot.IsSafePath(path, timeOffset, speed, delay, unit);
                    IsSafe = (IsSafe) ? sResult.IsSafe : false;

                    if (sResult.Intersection.Valid)
                        intersections.Add(sResult.Intersection);
                }
            }

            //Return the first intersection
            if (!IsSafe)
            {
                var intersetion = EnsoulSharp.SDK.EnumerableExtensions.MinOrDefault(intersections, o => o.Distance);
                return new SafePathResult(false, intersetion.Valid ? intersetion : intersection);
            }

            return new SafePathResult(true, intersection);
        }

        /// Returns if you can blink to the point without being hit.
        public static bool IsSafeToBlink(Vector2 point, int timeOffset, int delay)
        {
            foreach (var skillshot in DetectedSkillshots)
            {
                if (skillshot.Evade() && !skillshot.IsSafeToBlink(point, timeOffset, delay))
                    return false;
            }
            return true;
        }

        /// Returns true if some detected skillshot is about to hit the unit.
        public static bool IsAboutToHit(AIBaseClient unit, int time)
        {
            time += 150;
            foreach (var skillshot in DetectedSkillshots)
            {
                if (skillshot.Evade() && skillshot.IsAboutToHit(time, unit))
                    return true;
            }
            return false;
        }

        private static void TryToEvade(List<Skillshot> HitBy, Vector2 to)
        {
            var dangerLevel = 0;

            foreach (var skillshot in HitBy)
            {
                dangerLevel = Math.Max(dangerLevel, skillshot.GetValue<MenuSlider>("DangerLevel").Value);
            }

            foreach (var evadeSpell in EvadeSpellDatabase.Spells)
            {
                if (evadeSpell.Enabled && evadeSpell.DangerLevel <= dangerLevel)
                {
                    //Walking
                    if (evadeSpell.Name == "Walking")
                    {
                        var points = Evader.GetEvadePoints();
                        if (points.Count > 0)
                        {
                            EvadePoint = to.Closest(points);
                            var nEvadePoint = LeagueSharpCommon.Geometry.Geometry.Extend(EvadePoint, PlayerPosition, -100);
                            if (Program.IsSafePath(ObjectManager.Player.GetPath(LeagueSharpCommon.Geometry.Geometry.To3D(nEvadePoint)).To2DList(), Config.EvadingSecondTimeOffset, (int)ObjectManager.Player.MoveSpeed, 100).IsSafe)
                            {
                                EvadePoint = nEvadePoint;
                            }

                            Evading = true;
                            return;
                        }
                    }

                    //SpellShields
                    if (evadeSpell.IsSpellShield && ObjectManager.Player.Spellbook.CanUseSpell(evadeSpell.Slot) == SpellState.Ready)
                    {
                        if (evadeSpell.Name == "Samira W")
                            if (!HitBy.Exists(x => x.SpellData.MissileSpeed > 0 && x.SpellData.MissileSpeed != int.MaxValue))
                                continue;

                        if (IsAboutToHit(ObjectManager.Player, evadeSpell.Delay))
                            ObjectManager.Player.Spellbook.CastSpell(evadeSpell.Slot, ObjectManager.Player);

                        //Let the user move freely inside the skillshot.
                        NoSolutionFound = true;
                        return;
                    }

                    //Shields
                    if (evadeSpell.IsShield && ObjectManager.Player.Spellbook.CanUseSpell(evadeSpell.Slot) == SpellState.Ready)
                    {
                        if (IsAboutToHit(ObjectManager.Player, evadeSpell.Delay))
                        {
                            ObjectManager.Player.Spellbook.CastSpell(evadeSpell.Slot, ObjectManager.Player);
                            NoSolutionFound = true;
                            return;
                        }
                        //Let the user move freely inside the skillshot.
                    }

                    if (evadeSpell.IsReady())
                    {
                        //MovementSpeed Buff
                        if (evadeSpell.IsMovementSpeedBuff)
                        {
                            var points = Evader.GetEvadePoints((int)evadeSpell.MoveSpeedTotalAmount());

                            if (points.Count > 0)
                            {
                                EvadePoint = to.Closest(points);
                                Evading = true;

                                if (evadeSpell.IsSummonerSpell)
                                    ObjectManager.Player.Spellbook.CastSpell(evadeSpell.Slot, ObjectManager.Player);
                                else
                                    ObjectManager.Player.Spellbook.CastSpell(evadeSpell.Slot, ObjectManager.Player);

                                return;
                            }
                        }

                        //Dashes
                        if (evadeSpell.IsDash)
                        {
                            //Targetted dashes
                            if (evadeSpell.IsTargetted) //Lesinga W.
                            {
                                var targets = Evader.GetEvadeTargets(
                                    evadeSpell.ValidTargets, evadeSpell.Speed, evadeSpell.Delay, evadeSpell.MaxRange,
                                    false, false);

                                if (targets.Count > 0)
                                {
                                    var closestTarget = Utils.Closest(targets, to);
                                    EvadePoint = closestTarget.ServerPosition.To2D();
                                    Evading = true;

                                    if (evadeSpell.IsSummonerSpell)
                                        ObjectManager.Player.Spellbook.CastSpell(evadeSpell.Slot, closestTarget);
                                    else
                                        ObjectManager.Player.Spellbook.CastSpell(evadeSpell.Slot, closestTarget);

                                    return;
                                }
                                if (Utils.TickCount - LastWardJumpAttempt < 250)
                                {
                                    //Let the user move freely inside the skillshot.
                                    NoSolutionFound = true;
                                    return;
                                }

                                if (evadeSpell.IsTargetted && evadeSpell.ValidTargets.Contains(SpellValidTargets.AllyWards) && Config.Menu["evadeSpells"][evadeSpell.Name]["WardJump" + evadeSpell.Name].GetValue<MenuBool>().Enabled)
                                {
                                    var wardSlot = Items.GetWardSlot(ObjectManager.Player);
                                    if (wardSlot != null)
                                    {
                                        var points = Evader.GetEvadePoints(evadeSpell.Speed, evadeSpell.Delay, false);

                                        // Remove the points out of range
                                        points.RemoveAll(item => LeagueSharpCommon.Geometry.Geometry.Distance(item, ObjectManager.Player.ServerPosition) > 600);

                                        if (points.Count > 0)
                                        {
                                            //Dont dash just to the edge:
                                            for (var i = 0; i < points.Count; i++)
                                            {
                                                var k = (int)(600 - LeagueSharpCommon.Geometry.Geometry.Distance(PlayerPosition, points[i]));
                                                k = k - new Random(Utils.TickCount).Next(k);
                                                var extended = points[i] + k * LeagueSharpCommon.Geometry.Geometry.Normalized((points[i] - PlayerPosition));

                                                if (IsSafe(extended).IsSafe)
                                                    points[i] = extended;
                                            }

                                            var ePoint = to.Closest(points);
                                            ObjectManager.Player.Spellbook.CastSpell(wardSlot.SpellSlot, LeagueSharpCommon.Geometry.Geometry.To3D(ePoint));
                                            LastWardJumpAttempt = Utils.TickCount;
                                            //Let the user move freely inside the skillshot.
                                            NoSolutionFound = true;
                                            return;
                                        }
                                    }
                                }
                            }
                            //Skillshot type dashes.
                            else
                            {
                                var points = Evader.GetEvadePoints(evadeSpell.Speed, evadeSpell.Delay, false);

                                // Remove the points out of range
                                points.RemoveAll(item => LeagueSharpCommon.Geometry.Geometry.Distance(item, ObjectManager.Player.ServerPosition) > evadeSpell.MaxRange);

                                //If the spell has a fixed range (Vaynes Q), calculate the real dashing location. TODO: take into account walls in the future.
                                if (evadeSpell.FixedRange)
                                {
                                    for (var i = 0; i < points.Count; i++)
                                        points[i] = LeagueSharpCommon.Geometry.Geometry.Extend(PlayerPosition, points[i], evadeSpell.MaxRange);

                                    for (var i = points.Count - 1; i > 0; i--)
                                        if (!IsSafe(points[i]).IsSafe || Utility.IsWall(points[i]))
                                            points.RemoveAt(i);
                                }
                               
                                if (points.Count > 0)
                                {
                                    EvadePoint = Game.CursorPos.To2D().Closest(points);
                                    Evading = true;

                                    if (!evadeSpell.Invert)
                                    {
                                        if (evadeSpell.RequiresPreMove)
                                        {
                                            ObjectManager.Player.SendMovePacket(ObjectManager.Player.GetPath(LeagueSharpCommon.Geometry.Geometry.To3D(EvadePoint)).Last().To2D());
                                            var theSpell = evadeSpell;
                                            DelayAction.Add(
                                                Game.Ping / 2 + 100,
                                                delegate
                                                {
                                                    ObjectManager.Player.Spellbook.CastSpell( theSpell.Slot, LeagueSharpCommon.Geometry.Geometry.To3D(EvadePoint));
                                                });
                                        }
                                        else
                                        {
                                            ObjectManager.Player.Spellbook.CastSpell(evadeSpell.Slot, LeagueSharpCommon.Geometry.Geometry.To3D(EvadePoint));
                                        }
                                    }
                                    else
                                    {
                                        var castPoint = PlayerPosition - (EvadePoint - PlayerPosition);
                                        ObjectManager.Player.Spellbook.CastSpell(evadeSpell.Slot, LeagueSharpCommon.Geometry.Geometry.To3D(castPoint));
                                    }
                                    NoSolutionFound = true;
                                    return;
                                }
                            }
                        }

                        //Invulnerabilities, like Fizz's E
                        if (evadeSpell.IsInvulnerability)
                        {
                            if (evadeSpell.IsTargetted)
                            {
                                var targets = Evader.GetEvadeTargets(evadeSpell.ValidTargets, int.MaxValue, 0, evadeSpell.MaxRange, true, false, true);

                                if (targets.Count > 0)
                                {
                                    if (IsAboutToHit(ObjectManager.Player, evadeSpell.Delay))
                                    {
                                        var closestTarget = Utils.Closest(targets, to);
                                        EvadePoint = closestTarget.ServerPosition.To2D();
                                        Evading = true;
                                        ObjectManager.Player.Spellbook.CastSpell(evadeSpell.Slot, closestTarget);
                                    }

                                    //Let the user move freely inside the skillshot.
                                    NoSolutionFound = true;
                                    return;
                                }
                            }
                            else
                            {
                                if (IsAboutToHit(ObjectManager.Player, evadeSpell.Delay))
                                {
                                    if (evadeSpell.SelfCast)
                                        ObjectManager.Player.Spellbook.CastSpell(evadeSpell.Slot);
                                    else
                                        ObjectManager.Player.Spellbook.CastSpell(evadeSpell.Slot, ObjectManager.Player.ServerPosition);
                                }
                            }

                            //Let the user move freely inside the skillshot.
                            NoSolutionFound = true;
                            return;
                        }
                    }

                    //Zhonyas
                    if (evadeSpell.Name == "Zhonyas")
                    {
                        if (Items.CanUseItem(ObjectManager.Player,(int)ItemId.Zhonyas_Hourglass))
                        {
                            if (IsAboutToHit(ObjectManager.Player, 150))
                            {
                                NoSolutionFound = true;
                                Items.UseItem(ObjectManager.Player,(int)ItemId.Zhonyas_Hourglass);
                            }
                            return;
                        }
                        
                        if (Items.CanUseItem(ObjectManager.Player,(int)ItemId.Stopwatch))
                        {
                            if (IsAboutToHit(ObjectManager.Player, 150))
                            {
                                NoSolutionFound = true;
                                Items.UseItem(ObjectManager.Player,(int)ItemId.Stopwatch);
                            }
                            return;
                        }
                        if (Items.CanUseItem(ObjectManager.Player,(int)ItemId.Perfectly_Timed_Stopwatch))
                        {
                            if (IsAboutToHit(ObjectManager.Player, 150))
                            {
                                NoSolutionFound = true;
                                Items.UseItem(ObjectManager.Player,(int)ItemId.Perfectly_Timed_Stopwatch);
                            }
                            return;
                        }
                    }

                    //Blinks
                    if (evadeSpell.IsReady())
                    {
                        if (evadeSpell.IsBlink && IsAboutToHit(ObjectManager.Player, evadeSpell.Delay))
                        {
                            //Targetted blinks
                            if (evadeSpell.IsTargetted)
                            {
                                var targets = Evader.GetEvadeTargets(evadeSpell.ValidTargets, int.MaxValue, evadeSpell.Delay, evadeSpell.MaxRange, true, false);

                                if (targets.Count > 0)
                                {
                                    if (IsAboutToHit(ObjectManager.Player, evadeSpell.Delay))
                                    {
                                        var closestTarget = Utils.Closest(targets, to);
                                        EvadePoint = closestTarget.ServerPosition.To2D();
                                        Evading = true;

                                        if (evadeSpell.IsSummonerSpell)
                                            ObjectManager.Player.Spellbook.CastSpell(evadeSpell.Slot, closestTarget);
                                        else
                                            ObjectManager.Player.Spellbook.CastSpell(evadeSpell.Slot, closestTarget);
                                    }

                                    //Let the user move freely inside the skillshot.
                                    NoSolutionFound = true;
                                    return;
                                }
                                if (Utils.TickCount - LastWardJumpAttempt < 250)
                                {
                                    //Let the user move freely inside the skillshot.
                                    NoSolutionFound = true;
                                    return;
                                }

                                if (evadeSpell.IsTargetted && evadeSpell.ValidTargets.Contains(SpellValidTargets.AllyWards) && Config.Menu["evadeSpells"][evadeSpell.Name]["WardJump" + evadeSpell.Name].GetValue<MenuBool>().Enabled)
                                {
                                    var wardSlot = Items.GetWardSlot(ObjectManager.Player);
                                    if (wardSlot != null)
                                    {
                                        var points = Evader.GetEvadePoints(int.MaxValue, evadeSpell.Delay, true);

                                        // Remove the points out of range
                                        points.RemoveAll(item => LeagueSharpCommon.Geometry.Geometry.Distance(item, ObjectManager.Player.ServerPosition) > 600);

                                        if (points.Count > 0)
                                        {
                                            //Dont blink just to the edge:
                                            for (var i = 0; i < points.Count; i++)
                                            {
                                                var k = (int)(600 - LeagueSharpCommon.Geometry.Geometry.Distance(PlayerPosition, points[i]));
                                                k = k - new Random(Utils.TickCount).Next(k);
                                                var extended = points[i] + k * LeagueSharpCommon.Geometry.Geometry.Normalized((points[i] - PlayerPosition));
                                                if (IsSafe(extended).IsSafe)
                                                    points[i] = extended;
                                            }

                                            var ePoint = to.Closest(points);
                                            ObjectManager.Player.Spellbook.CastSpell(wardSlot.SpellSlot, LeagueSharpCommon.Geometry.Geometry.To3D(ePoint));
                                            LastWardJumpAttempt = Utils.TickCount;
                                            //Let the user move freely inside the skillshot.
                                            NoSolutionFound = true;
                                            return;
                                        }
                                    }
                                }
                            }
                            //Skillshot type blinks.
                            else
                            {
                                var points = Evader.GetEvadePoints(int.MaxValue, evadeSpell.Delay, true);

                                // Remove the points out of range
                                points.RemoveAll(item => LeagueSharpCommon.Geometry.Geometry.Distance(item, ObjectManager.Player.ServerPosition) > evadeSpell.MaxRange || Utility.IsWall(item));

                                //points.OrderBy(x=> x.Distance)
                                if (points.Count > 0)
                                {
                                    foreach (var extraPoint in Utils.CirclePoints(30, evadeSpell.MaxRange, HeroManager.Player.ServerPosition))
                                    {
                                        if(!Utility.IsWall(extraPoint) && IsSafeToBlink(extraPoint.To2D(), Config.EvadingFirstTimeOffset, evadeSpell.Delay))
                                        {
                                            points.Add(extraPoint.To2D());
                                        }
                                    }
                                    EvadePoint = Game.CursorPos.To2D().Closest(points);
                                    Evading = true;
                                    ObjectManager.Player.Spellbook.CastSpell(evadeSpell.Slot, LeagueSharpCommon.Geometry.Geometry.To3D(EvadePoint));
                                    //Let the user move freely inside the skillshot.
                                    NoSolutionFound = true;
                                    return;
                                }
                            }
                        }
                    }
                }
            }
            NoSolutionFound = true;
        }

        private static void Drawing_OnDraw(EventArgs args)
        {
            if (Utils.TickCount < startT + 20000 && Config.Menu["Drawings"]["DrawWarningMsg"].GetValue<MenuBool>().Enabled)
            {
                WarningMsg.DrawText(
                    null,
                    "To use \"Evade#\" you need to unbind Right Mouse Button in Game Settings\n要使用“Evade#”，您需要在游戏设置中取消绑定鼠标右键",
                    75,
                    100,
                    new ColorBGRA(33, 227, 252, 180));
            }

            if (!Config.Menu["Drawings"]["EnableDrawings"].GetValue<MenuBool>().Enabled)
                return;

            if (Config.Menu["Misc"]["ShowEvadeStatus"].GetValue<MenuBool>().Enabled)
            {
                var heropos = Drawing.WorldToScreen(ObjectManager.Player.Position);
                if (Config.Menu["Enabled"].GetValue<MenuKeyBind>().Active)
                    Drawing.DrawText(heropos.X, heropos.Y, Color.Green, "Evade: ON");
            }

            var Border = Config.drawings["Border"].GetValue<MenuSlider>().Value;
            var missileColor = Config.drawings["MissileColor"].GetValue<MenuColor>();

            //Draw the polygon for each skillshot.
            foreach (var skillshot in DetectedSkillshots)
            {
                skillshot.Draw((skillshot.Evade() && (Config.Menu["Enabled"].GetValue<MenuKeyBind>().Active || !Config.Menu["dontDodge"].GetValue<MenuKeyBind>().Active) )
            ? Config.drawings["EnabledColor"].GetValue<MenuColor>().Color.ToSystemColor()
                        : Config.drawings["DisabledColor"].GetValue<MenuColor>().Color.ToSystemColor(), missileColor.Color.ToSystemColor(), Border);
            }

            if (Config.TestOnAllies)
            {
                var myPath = Utility.GetWaypoints(ObjectManager.Player);

                for (var i = 0; i < myPath.Count - 1; i++)
                {
                    var A = myPath[i];
                    var B = myPath[i + 1];
                    var SA = Drawing.WorldToScreen(LeagueSharpCommon.Geometry.Geometry.To3D(A));
                    var SB = Drawing.WorldToScreen(LeagueSharpCommon.Geometry.Geometry.To3D(B));
                    Drawing.DrawLine(SA.X, SA.Y, SB.X, SB.Y, 1, Color.White);
                }

                //var evadePath = Pathfinding.Pathfinding.PathFind(PlayerPosition, Game.CursorPos.To2D());

                //for (var i = 0; i < evadePath.Count - 1; i++)
                //{
                //    var A = evadePath[i];
                //    var B = evadePath[i + 1];
                //    var SA = Drawing.WorldToScreen(A.To3D());
                //    var SB = Drawing.WorldToScreen(B.To3D());
                //    Drawing.DrawLine(SA.X, SA.Y, SB.X, SB.Y, 1, Color.Red);
                //}



                //Drawing.DrawCircle(EvadePoint.To3D(), 300, Color.White);
                //Drawing.DrawCircle(EvadeToPoint.To3D(), 300, Color.Red);
            }
        }

        public struct IsSafeResult
        {
            public bool IsSafe;
            public List<Skillshot> SkillshotList;
        }
    }
}