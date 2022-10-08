using System;
using System.Collections.Generic;
using System.Drawing.Text;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using EnsoulSharp;
using EnsoulSharp.SDK;
using EnsoulSharp.SDK.MenuUI;
using EnsoulSharp.SDK.Rendering;
using EnsoulSharp.SDK.Utility;
using HERMES_Kalista.MyLogic.Others;
using SharpDX;
using Color = System.Drawing.Color;
using SharpDX.IO;


 namespace Tyler1
{
    class Program
    {
        private static Menu Menu;
        public static MenuBool AutoCatch;
        public static MenuBool CatchOnlyCloseToMouse;
        public static MenuSlider MaxDistToMouse;
        public static MenuBool OnlyCatchIfSafe;
        public static MenuSlider MinQLaneclearManaPercent;
        public static Menu EMenu;
        public static MenuBool ECombo;
        public static MenuBool EGC;
        public static MenuBool EInterrupt;
        public static Menu RMenu;
        public static MenuBool RKS;
        public static MenuBool RKSOnlyIfCantAA;
        public static MenuSlider RIfHit;
        public static MenuBool WCombo;
        public static MenuBool UseItems;
        public static Menu DrawingMenu;
        public static MenuBool DrawAXELocation;
        public static MenuBool DrawAXECatchRadius;
        public static MenuBool DrawAXELine;
        public static MenuColor ColorMenu;
        public static Color DrawingCololor => ColorMenu.Color.ToSystemColor();
        private static AIHeroClient Player = ObjectManager.Player;
        private static Spell Q, W, E, R;
        static Items.Item BOTRK, Bilgewater, Yomamas, Mercurial, QSS;
        public static Color color = Color.DarkOrange;
        public static float MyRange = 550f;
        //private static int _lastCatchAttempt;
        private static MenuBool R1vs1;

        private static Dictionary<int, GameObject> Reticles;

        private static int AxesCount
        {
            get
            {
                var data = Player.GetBuff("dravenspinningattack");
                if (data == null || data.Count == -1)
                {
                    return 0;
                }
                return data.Count == 0 ? 1 : data.Count;
            }
        }

        private static int TotalAxesCount
        {
            get
            {
                return (ObjectManager.Player.HasBuff("dravenspinning") ? 1 : 0)
                       + (ObjectManager.Player.HasBuff("dravenspinningleft") ? 1 : 0) + Reticles.Count;
            }
        }

        public static void Loads()
        {
            //Bootstrap.Init();
            Load();
        }

        private static void Load()
        {
            DelayAction.Add(1500, () =>
            {
                if (ObjectManager.Player.CharacterName != "Draven") return;
                InitSpells();
                FinishLoading();
                Reticles = new Dictionary<int, GameObject>();
                GameObject.OnCreate += OnCreate;
                GameObject.OnDelete += OnDelete;
            });
        }

        private static void OnDelete(GameObject sender, EventArgs args)
        {
            var itemToDelete = Reticles.FirstOrDefault(ret => ret.Value.NetworkId == sender.NetworkId);
            if (itemToDelete.Key != 0)
            {
                Reticles.Remove(itemToDelete.Key);
            }
        }

        private static void OnCreate(GameObject sender, EventArgs args)
        {
            if (sender.Name.Contains("Draven_") && sender.Name.Contains("_Q_reticle_self") && !sender.IsDead)
            {
                Reticles.Add(Variables.TickCount, sender);
            }
        }

        private static void InitSpells()
        {
            Q = new Spell(SpellSlot.Q);
            W = new Spell(SpellSlot.W);
            E = new Spell(SpellSlot.E, 1050);
            E.SetSkillshot(0.25f, 130, 1400, false, SpellType.Line);
            R = new Spell(SpellSlot.R, 3000);
            R.SetSkillshot(0.25f, 160f, 2000f, false, SpellType.Line);

            BOTRK = new Items.Item(3153, 550);
            Bilgewater = new Items.Item(3144, 550);
            Yomamas = new Items.Item(3142, 400);
            Mercurial = new Items.Item(3139, 22000);
            QSS = new Items.Item(3140, 22000);
        }

        private static void FinishLoading()
        {
            Drawing.OnDraw += Draw;
            Game.OnUpdate += OnUpdate;
            AntiGapcloser.OnGapcloser += OnGapcloser;
            Interrupter.OnInterrupterSpell += OnInterruptableTarget;
            DelayAction.Add(3000, () => MyRange = Player.GetRealAutoAttackRange());
            //Variables.Orbwalker.Enabled = true;
            //DelayAction.Add(1000, () => Variables.Orbwalker.Enabled = true);
            //DelayAction.Add(5000, () => Variables.Orbwalker.Enabled = true);
            //DelayAction.Add(10000, () => Variables.Orbwalker.Enabled = true);
            Menu = new Menu("tyler1", "Tyler1", true);
            AutoCatch = Menu.Add(new MenuBool("tyler1auto", "Auto catch axes?", true));
            CatchOnlyCloseToMouse = Menu.Add(new MenuBool("tyler1onlyclose", "Catch only axes close to mouse?", true));
            MaxDistToMouse = Menu.Add(new MenuSlider("tyler1maxdist", "Max axe distance to mouse", 500, 250, 1250));
            OnlyCatchIfSafe = Menu.Add(new MenuBool("tyler1safeaxes", "Only catch axes if safe (anti melee)", false));
            MinQLaneclearManaPercent =
                Menu.Add(new MenuSlider("tyler1QLCMana", "Min Mana Percent for Q Laneclear", 60, 0, 100));
            EMenu = Menu.Add(new Menu("tyler1E", "E Settings: "));
            ECombo = EMenu.Add(new MenuBool("tyler1ECombo", "Use E in Combo", true));
            EGC = EMenu.Add(new MenuBool("tyler1EGC", "Use E on Gapcloser", true));
            EInterrupt = EMenu.Add(new MenuBool("tyler1EInterrupt", "Use E to Interrupt", true));
            RMenu = Menu.Add(new Menu("tyler1R", "R Settings:"));
            RKS = RMenu.Add(new MenuBool("tyler1RKS", "Use R to steal kills", true));
            RKSOnlyIfCantAA = RMenu.Add(new MenuBool("tyler1RKSOnlyIfCantAA", "Use R KS only if can't AA", true));
            RIfHit = RMenu.Add(new MenuSlider("tyler1RIfHit", "Use R if it will hit X enemies", 2, 1, 5));
            R1vs1 = RMenu.Add(new MenuBool("tyler1R1v1", "Always use R in 1v1", true));

            WCombo = Menu.Add(new MenuBool("tyler1WCombo", "Use W in Combo", true));
            UseItems = Menu.Add(new MenuBool("tyler1Items", "Use Items?", true));

            DrawingMenu = Menu.Add(new Menu("tyler1DrawSettings", "Draw Settings:"));
            DrawAXECatchRadius = DrawingMenu.Add(new MenuBool("tyler1AxeCatchDraw", "Draw Axe Catch Radius", true));
            DrawAXELocation = DrawingMenu.Add(new MenuBool("tyler1AxeLocationDraw", "Draw Axe Location", true));
            DrawAXELine = DrawingMenu.Add(new MenuBool("tyler1AxeLineDraw", "Draw Line to Axe Position", true));
            ColorMenu = DrawingMenu.Add(new MenuColor("tyler1DrawingColor", "Drawing Color", ColorBGRA.FromRgba(Color.Red.ToRgba())));

            Menu.Attach();
        }

        private static void OnUpdate(EventArgs args)
        {
            try
            {
                var target = TargetSelector.GetTarget(E.Range,DamageType.Physical);
                if (Orbwalker.ActiveMode == OrbwalkerMode.LaneClear) Farm();
                if (Orbwalker.ActiveMode == OrbwalkerMode.Combo && target != null)
                {
                    Combo();
                    RCombo();
                }
                CatchAxes();
                KS();
                if (W.IsReady() && Player.HasBuffOfType(BuffType.Slow) &&
                    target.Distance(ObjectManager.Player) <= MyRange) W.Cast();
                R1V1(target);

            }
            catch
            {
            }
        }

        private static void R1V1(AIHeroClient target)
        {
            if (R1vs1.Enabled && target != null && target.IsHPBarRendered && target.Distance(ObjectManager.Player) < 650 && !ShouldntUlt(target))
            {
                if (target.HealthPercent > ObjectManager.Player.HealthPercent &&
                    (target.MaxHealth <= ObjectManager.Player.MaxHealth + 300 ||
                     noobchamps.Contains(target.CharacterName)))
                {
                    var pred = R.GetPrediction(target);
                    if (pred.Hitchance >= HitChance.High)
                    {
                        R.Cast(pred.UnitPosition);
                    }
                }
            }
        }

        private static List<string> noobchamps = new List<string>
        {
            "Ahri",
            "Anivia",
            "Annie",
            "Ashe",
            "Azir",
            "Brand",
            "Caitlyn",
            "Cassiopeia",
            "Corki",
            "Draven",
            "Ezreal",
            "Graves",
            "Jinx",
            "Kalista",
            "Karma",
            "Karthus",
            "Katarina",
            "Kennen",
            "KogMaw",
            "Leblanc",
            "Kindred",
            "Lucian",
            "Lux",
            "Malzahar",
            "MasterYi",
            "MissFortune",
            "Orianna",
            "Quinn",
            "Sivir",
            "Syndra",
            "Talon",
            "Teemo",
            "Tristana",
            "TwistedFate",
            "Twitch",
            "Varus",
            "Vayne",
            "Veigar",
            "Velkoz",
            "Viktor",
            "Xerath",
            "Zed",
            "Ziggs",
            "Soraka",
            "Akali",
            "Diana",
            "Ekko",
            "Fiddlesticks",
            "Fiora",
            "Fizz",
            "Heimerdinger",
            "Illaoi",
            "Jayce",
            "Kassadin",
            "Kayle",
            "KhaZix",
            "Kindred",
            "Lissandra",
            "Mordekaiser",
            "Nidalee",
            "Riven",
            "Shaco",
            "Vladimir",
            "Yasuo",
            "Zilean"
        };

        /// <summary>
        /// Those buffs make the target either unkillable or a pain in the ass to kill, just wait until they end
        /// </summary>
        private static List<string> UndyingBuffs = new List<string>
        {
            "JudicatorIntervention",
            "UndyingRage",
            "FerociousHowl",
            "ChronoRevive",
            "ChronoShift",
            "lissandrarself",
            "kindredrnodeathbuff"
        };

        private static bool ShouldntUlt(AIHeroClient target)
        {
            //Dead or not a hero
            if (target == null || !target.IsHPBarRendered) return true;
            //Undying
            if (UndyingBuffs.Any(buff => target.HasBuff(buff))) return true;
            //Blitzcrank
            if (target.CharacterName == "Blitzcrank" && !target.HasBuff("BlitzcrankManaBarrierCD")
                && !target.HasBuff("ManaBarrier"))
            {
                return true;
            }
            //Sivir
            return target.CharacterName == "Sivir" && target.HasBuffOfType(BuffType.SpellShield) ||
                   target.HasBuffOfType(BuffType.SpellImmunity);
        }

        private static void RCombo()
        {
            var target = TargetSelector.GetTarget(E.Range,DamageType.Physical);
            if (target != null && target.IsHPBarRendered && !target.IsDead && !target.IsZombie())
            {
                var pred = R.GetPrediction(target);
                if (pred.Hitchance > HitChance.High && pred.AoeTargetsHit.Count >= RIfHit.Value)
                {
                    R.Cast(pred.UnitPosition);
                }
            }
        }

        private static void Farm()
        {
            if (ObjectManager.Player.ManaPercent < MinQLaneclearManaPercent.Value) return;
            if (
                ObjectManager.Get<AIMinionClient>()
                    .Any(m => m.IsHPBarRendered && m.IsEnemy && m.Distance(ObjectManager.Player) < MyRange))
            {
                if (TotalAxesCount < 2) Q.Cast();
            }
        }

        private static void Combo()
        {
            var target = TargetSelector.GetTarget(E.Range, DamageType.Physical);
            if (target.Distance(Player) < MyRange + 100)
            {
                if (TotalAxesCount < 2) Q.Cast();
                if (WCombo.Enabled && W.IsReady() && !Player.HasBuff("dravenfurybuff")) W.Cast();
            }
            if (ECombo.Enabled && E.IsReady() && target.IsValidTarget(750))
            {
                var pred = E.GetPrediction(target);
                if (pred.Hitchance >= HitChance.High)
                    E.Cast(pred.UnitPosition);
            }

            if (UseItems.Enabled)
            {
                if (target.IsValidTarget(MyRange))
                {
                    if (Yomamas.IsReady) Yomamas.Cast();
                    //if (Bilgewater.IsReady) Bilgewater.Cast(target);
                    //if (BOTRK.IsReady) BOTRK.Cast(target);
                }
                //QSS
                if (Player.HasBuffOfType(BuffType.Stun) || Player.HasBuffOfType(BuffType.Fear) ||
                    Player.HasBuffOfType(BuffType.Charm) || Player.HasBuffOfType(BuffType.Taunt) ||
                    Player.HasBuffOfType(BuffType.Blind))
                {
                    if (Mercurial.IsReady) DelayAction.Add(100, () => Mercurial.Cast());
                    if (QSS.IsReady) DelayAction.Add(100, () => QSS.Cast());
                }
            }
        }

        private static void CatchAxes()
        {
            Vector3 Mouse = Game.CursorPos;
            if (!ObjectManager
                .Get<GameObject>(
                ).Any(x => x.Name.Contains("Draven_") && x.Name.Contains("_Q_reticle_self") && !x.IsDead) || !AutoCatch.Enabled)
            {
                Orbwalker.MoveEnabled = true;
            }
            if (AutoCatch.Enabled)
            {
                foreach (
                    var reticle in
                        Reticles
                            .Where(
                                x => !x.Value.IsDead &&
                                     (!x.Value.Position.IsUnderEnemyTurret() ||
                                      (Mouse.IsUnderEnemyTurret() && ObjectManager.Player.IsUnderEnemyTurret())))
                            .OrderBy(ret => ret.Key))
                {
                    var AXE = reticle.Value;
                    if (OnlyCatchIfSafe.Enabled &&
                        GameObjects.EnemyHeroes.Count(
                            e => e.IsHPBarRendered && e.IsMelee && e.ServerPosition.Distance(AXE.Position) < 350) >= 1)
                    {
                        break;
                    }
                    if (CatchOnlyCloseToMouse.Enabled && AXE.Distance(Mouse) > MaxDistToMouse.Value)
                    {
                        Orbwalker.MoveEnabled = true;

                        if (GameObjects.EnemyHeroes.Count(
                            e => e.IsHPBarRendered && e.IsMelee && e.ServerPosition.Distance(AXE.Position) < 350) >= 1)
                        {
                            //user probably doesn't want to go there, try the next reticle
                            break;
                        }
                        //maybe user just has potato reaction time
                        return;
                    }
                    if (AXE.Distance(Player.ServerPosition) > 80 && Orbwalker.CanMove())
                    {
                        Orbwalker.Move(AXE.Position.Randomize());
                        Orbwalker.MoveEnabled = false;
                        //DelayAction.Add(300, () => Variables.Orbwalker.SetMovementState(true));
                    }
                    if (AXE.Distance(Player.ServerPosition) <= 80)
                    {
                        Orbwalker.MoveEnabled = true;
                    }
                }
            }
        }


        /// <summary>
        /// Will need to add an actual missile check for the axes in air instead of brosciencing
        /// </summary>
        private static void KS()
        {
            if (!RKS.Enabled) return;
            foreach (
                var enemy in
                    GameObjects.EnemyHeroes.Where(
                        e =>
                            e.IsHPBarRendered && e.Distance(ObjectManager.Player) < 3000 &&
                            (e.Distance(ObjectManager.Player) > MyRange + 150 || !RKSOnlyIfCantAA.Enabled)))
            {
                if (enemy.Health < R.GetDamage(enemy) && !ShouldntUlt(enemy))
                {
                    var pred = R.GetPrediction(enemy);
                    if (pred.Hitchance >= HitChance.High)
                    {
                        R.Cast(pred.UnitPosition);
                    }
                }
            }
        }

        private static void Draw(EventArgs args)
        {
            if (ObjectManager.Player.IsDead) return;
            var reticles =
                ObjectManager.Get<GameObject>()
                    .Where(x => x.Name.Contains("Draven_") && x.Name.Contains("_Q_reticle_self") && !x.IsDead).ToArray();
            if (reticles.Any())
            {
                var PlayerPosToScreen = Drawing.WorldToScreen(ObjectManager.Player.Position);

                if (DrawAXELocation.Enabled)
                    foreach (var AXE in reticles)
                    {
                        var AXEToScreen = Drawing.WorldToScreen(AXE.Position);
                        CircleRender.Draw(AXE.Position, 140, DrawingCololor.ToSharpDxColor(), 8);
                    }

                Drawing.DrawLine(PlayerPosToScreen, Drawing.WorldToScreen(reticles[0].Position), 8, DrawingCololor);

                if (DrawAXELine.Enabled)
                    for (int i = 0; i < reticles.Length; i++)
                    {
                        if (i < reticles.Length - 1)
                        {
                            Drawing.DrawLine(Drawing.WorldToScreen(reticles[i].Position),
                                Drawing.WorldToScreen(reticles[i + 1].Position), 8, DrawingCololor);
                        }
                    }
                if (DrawAXECatchRadius.Enabled)
                    if (CatchOnlyCloseToMouse.Enabled && MaxDistToMouse.Value < 700 &&
                        ObjectManager.Get<GameObject>()
                            .Any(x => x.Name.Contains("Draven_") && x.Name.Contains("_Q_reticle_self") && !x.IsDead))
                    {
                        CircleRender.Draw(Game.CursorPos, MaxDistToMouse.Value, DrawingCololor.ToSharpDxColor(), 8);
                    }
            }
        }

        private static void OnGapcloser(AIHeroClient sender, AntiGapcloser.GapcloserArgs gapcloser)
        {
            if (EGC.Enabled && E.IsReady() && sender.Distance(ObjectManager.Player) < 800)
            {
                var pred = E.GetPrediction(sender);
                if (pred.Hitchance > HitChance.High)
                {
                    E.Cast(pred.UnitPosition);
                }
            }
        }

        private static void OnInterruptableTarget(AIHeroClient sender, Interrupter.InterruptSpellArgs args)
        {
            if (EInterrupt.Enabled && E.IsReady() && args.Sender.Distance(ObjectManager.Player) < 950)
            {
                E.Cast(args.Sender.Position);
            }
        }
    }
}