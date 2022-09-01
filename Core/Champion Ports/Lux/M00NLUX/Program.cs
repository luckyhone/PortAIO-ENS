using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using EnsoulSharp;
using EnsoulSharp.SDK;
using EnsoulSharp.SDK.MenuUI;
using EnsoulSharp.SDK.Rendering;
using PortAIO;
using SharpDX;
using SPrediction;

namespace MoonLux
{
    internal static class Program
    {
        #region Properties

        /// <summary>
        ///     Gets or sets the e.
        /// </summary>
        /// <value>
        ///     The e.
        /// </value>
        private static Spell E { get; set; }

        /// <summary>
        ///     Gets a value indicating whether the E spell was casted.
        /// </summary>
        /// <value>
        ///     <c>true</c> if the E spell was casted; otherwise, <c>false</c>.
        /// </value>
        private static bool ECasted => Player.HasBuff("LuxLightStrikeKugel");

        /// <summary>
        ///     Gets or sets the e object.
        /// </summary>
        /// <value>
        ///     The e object.
        /// </value>
        private static GameObject EObject { get; set; }

        /// <summary>
        ///     Gets or sets the menu.
        /// </summary>
        /// <value>
        ///     The menu.
        /// </value>
        private static Menu Menu { get; set; }
        private static Menu shieldMenu { get; set; }

        /// <summary>
        ///     Gets the player.
        /// </summary>
        /// <value>
        ///     The player.
        /// </value>
        private static AIHeroClient Player => ObjectManager.Player;

        /// <summary>
        ///     Gets or sets the q.
        /// </summary>
        /// <value>
        ///     The q.
        /// </value>
        private static Spell Q { get; set; }

        /// <summary>
        ///     Gets or sets the r.
        /// </summary>
        /// <value>
        ///     The r.
        /// </value>
        private static Spell R { get; set; }

        /// <summary>
        ///     Gets or sets the w.
        /// </summary>
        /// <value>
        ///     The w.
        /// </value>
        private static Spell W { get; set; }

        #endregion
        
        /// <summary>
        ///     Automaticly shields allies with low health.
        /// </summary>
        private static void AutoShield()
        {
            foreach (
                var hero in
                    GameObjects.AllyHeroes.Where(
                        x => !x.IsEnemy && (x.IsMe || W.IsInRange(x)) && shieldMenu.GetValue<MenuBool>("Shield"+x.CharacterName).Enabled))
            {
                if (hero.HealthPercent < shieldMenu.GetValue<MenuSlider>("ASHealthPercent").Value)
                {
                    W.Cast(W.GetPrediction(hero).CastPosition);
                }
            }
        }

        /// <summary>
        ///     Casts the e.
        /// </summary>
        /// <param name="target">The target.</param>
        private static void CastE(AIHeroClient target)
        {
            if (Environment.TickCount - E.LastCastAttemptTime < E.Delay * 1000)
            {
                return;
            }

            if (ECasted)
            {
                if (EObject.Position.CountEnemyHeroesInRange(350) >= 1
                    && ObjectManager.Get<AIHeroClient>()
                           .Count(x => x.IsValidTarget(350, true, EObject.Position) && !x.HasPassive()) >= 1)
                {
                    E.Cast();
                }
            }
            else if (!target.HasPassive())
            {
                E.Cast(target);
            }
        }

        /// <summary>
        ///     Casts the q.
        /// </summary>
        /// <param name="target">The target.</param>
        private static void CastQ(AIBaseClient target)
        {
            if (Menu.GetValue<MenuBool>("QThroughMinions").Enabled)
            {
                var prediction = Q.GetPrediction(target);
                var objects = Q.GetCollision(
                    Player.Position.To2D(), 
                    new List<Vector2> { prediction.CastPosition.To2D() });

                if (objects.Count == 1 || (objects.Count == 1 && objects.ElementAt(0).IsEnemy)
                    || objects.Count <= 1
                    || (objects.Count == 2 && (objects.ElementAt(0).IsEnemy || objects.ElementAt(1).IsEnemy)))
                {
                    Q.Cast(prediction.CastPosition);
                }
            }
            else
            {
                Q.Cast(target);
            }
        }
        
        /// <summary>
        ///     Creates the menu.
        /// </summary>
        private static void CreateMenu()
        {
            Menu = new Menu("ChewyLUXFF","MoonLux", true);

            var comboMenu = new Menu("ComboSettings","Combo Settings" );
            comboMenu.Add(new MenuBool("UseQCombo", "Use Q").SetValue(true));
            comboMenu.Add(new MenuBool("UseQSlowedCombo", "Use Q only if Slowed by E").SetValue(false));
            comboMenu.Add(new MenuBool("UseWCombo", "Use W").SetValue(false));
            comboMenu.Add(new MenuBool("UseECombo", "Use E").SetValue(true));
            comboMenu.Add(new MenuBool("UseRCombo", "Use R").SetValue(true));
            comboMenu.Add(
                new MenuList("UseRComboMode", "R Mode",new[] { "Always", "If Killable", "Too far" }, 1));
            Menu.Add(comboMenu);

            var harassMenu = new Menu("HarassSettings","Harass Settings" );
            harassMenu.Add(new MenuBool("UseQHarass", "Use Q").SetValue(true));
            harassMenu.Add(new MenuBool("UseWHarass", "Use W").SetValue(false));
            harassMenu.Add(new MenuBool("UseEHarass", "Use E").SetValue(true));
            harassMenu.Add(new MenuSlider("HarassMinMana", "Harass Min Mana",50));
            harassMenu.Add(
                new MenuKeyBind("HarassKeybind", "Harass! (toggle)",Keys.T, KeyBindType.Toggle));
            Menu.Add(harassMenu);

            var waveClearMenu = new Menu("WaveClearSettings","Waveclear Settings" );
            waveClearMenu.Add(new MenuBool("UseQWaveClear", "Use Q").SetValue(false));
            waveClearMenu.Add(new MenuBool("UseEWaveClear", "Use E").SetValue(false));
            waveClearMenu.Add(new MenuBool("UseRWaveClear", "Use R").SetValue(false));
            waveClearMenu.Add(new MenuSlider("WaveClearMinMana", "Wave Clear Min Mana",75));
            Menu.Add(waveClearMenu);

            var ksMenu = new Menu("KSSettings","Kill Steal Settings");
            ksMenu.Add(new MenuBool("UseQKS", "Use Q").SetValue(true));
            ksMenu.Add(new MenuBool("UseEKS", "Use E").SetValue(false));
            ksMenu.Add(new MenuBool("UseRKS", "Use R").SetValue(true));
            Menu.Add(ksMenu);

            shieldMenu = new Menu("ASSettings","Auto Shield Settings");
            var shieldOptionsMenu = new Menu("Options", "ShieldOptions");
            shieldOptionsMenu.Add(new MenuSlider("ASHealthPercent", "Health Percent",25));
            shieldOptionsMenu.Add(new MenuSlider("ASDamagePercent", "Damage Percent",20));
            shieldMenu.Add(shieldOptionsMenu);
            GameObjects.AllyHeroes.ForEach(
                x =>
                shieldMenu.Add(new MenuBool("Shield" + x.CharacterName, "Shield " + x.CharacterName).SetValue(true)));
            Menu.Add(shieldMenu);

            var jungleKsMenu = new Menu("JungleKS","Jungle Steal Settings");
            jungleKsMenu.Add(new MenuBool("StealBaron", "Steal Baron").SetValue(true));
            jungleKsMenu.Add(new MenuBool("StealDragon", "Steal Dragon").SetValue(true));
            jungleKsMenu.Add(new MenuBool("StealBlueBuff", "Steal Blue Buff").SetValue(true));
            jungleKsMenu.Add(new MenuBool("StealRedBuff", "Steal Red Buff").SetValue(true));

            jungleKsMenu.Add(
                new MenuList("StealBuffMode", "Buff Stealer Mode",new[] { "Only Enemy", "Both", "Only Ally" }));
            Menu.Add(jungleKsMenu);

            var miscMenu = new Menu("MiscSettings","Miscellaneous Settings" );
            miscMenu.Add(
                new MenuBool("SpellWeaveCombo", "Spell Weave").SetValue(true)
                    .SetTooltip(
                        "Casts a spell, then auto attacks, and then casts a second spell after proc'ing the passive."));
            miscMenu.Add(new MenuBool("QThroughMinions", "Cast Q through minions").SetValue(true));
            miscMenu.Add(new MenuBool("QGapcloser", "Use Q on a Gapcloser").SetValue(true));
            Menu.Add(miscMenu);

            var drawMenu = new Menu("DrawSettings","Drawing Settings" );
            drawMenu.Add(new MenuBool("DrawQ", "Draw Q").SetValue(true));
            drawMenu.Add(new MenuBool("DrawW", "Draw W").SetValue(false));
            drawMenu.Add(new MenuBool("DrawE", "Draw E").SetValue(true));
            drawMenu.Add(new MenuBool("DrawERad", "Draw E Radius").SetValue(true));
            drawMenu.Add(new MenuBool("DrawR", "Draw R").SetValue(true));
            Menu.Add(drawMenu);

            Menu.Add(new MenuSeparator("Seperator1", " "));
            Menu.Add(new MenuSeparator("madeby", "Made by ChewyMoon"));
            Menu.Add(new MenuSeparator("Version", "Version: " + Assembly.GetExecutingAssembly().GetName().Version));

            Menu.Attach();

            Menu.GetValue<MenuKeyBind>("HarassKeybind").AddPermashow();
        }
        
        /// <summary>
        ///     Gets the damage done to a unit.
        /// </summary>
        /// <param name="hero">The hero.</param>
        /// <returns></returns>
        private static float DamageToUnit(AIHeroClient hero)
        {
            var damage = 0f;

            if (Q.IsReady())
            {
                damage += Q.GetDamage(hero) + hero.GetPassiveDamage();
            }

            if (E.IsReady())
            {
                damage += E.GetDamage(hero) + hero.GetPassiveDamage();
            }

            if (R.IsReady())
            {
                damage += R.GetDamage(hero) + hero.GetPassiveDamage() * 2;
            }

            return damage;
        }
        
        /// <summary>
        ///     Does the combo.
        /// </summary>
        private static void DoCombo()
        {
            var useQCombo = Menu.GetValue<MenuBool>("UseQCombo").Enabled;
            var useQSlowedCombo = Menu.GetValue<MenuBool>("UseQSlowedCombo").Enabled;
            var useWCombo = Menu.GetValue<MenuBool>("UseWCombo").Enabled;
            var useECombo = Menu.GetValue<MenuBool>("UseECombo").Enabled;
            var useRCombo = Menu.GetValue<MenuBool>("UseRCombo").Enabled;
            var useRComboMode = Menu.GetValue<MenuList>("UseRComboMode").Index;
            var spellWeaveCombo = Menu.GetValue<MenuBool>("SpellWeaveCombo").Enabled;

            var target = TargetSelector.GetTarget(Q.Range, DamageType.Magical);

            if (!target.IsValidTarget())
            {
                if (GameObjects.EnemyHeroes.Any(x => R.IsInRange(x)) && useRComboMode == 2 && R.IsReady())
                {
                    R.Cast(target);
                }

                return;
            }

            if (useQCombo && Q.IsReady())
            {
                if (spellWeaveCombo)
                {
                    if (!target.HasPassive())
                    {
                        if (useQSlowedCombo && target.HasBuffOfType(BuffType.Slow))
                        {
                            CastQ(target);
                        }
                        else if (!useQSlowedCombo)
                        {
                            CastQ(target);
                        }
                    }
                }
                else
                {
                    CastQ(target);
                }
            }

            if (useWCombo && W.IsReady())
            {
                W.Cast(Game.CursorPos);
            }

            if (useECombo && E.IsReady())
            {
                CastE(target);
            }

            if (!useRCombo || !R.IsReady())
            {
                return;
            }

            // ReSharper disable once SwitchStatementMissingSomeCases
            switch (useRComboMode)
            {
                case 0:
                    R.Cast(target);
                    break;
                case 1:
                    if (R.IsKillable(target))
                    {
                        R.Cast(target);
                    }

                    break;
            }
        }

        internal static bool IsKillable(this Spell Spell, AIBaseClient target)
        {
            return R.GetDamage(target) >= target.Health;
        }
        
        /// <summary>
        ///     Does the harass.
        /// </summary>
        private static void DoHarass()
        {
            var useQHarass = Menu.GetValue<MenuBool>("UseQHarass").Enabled;
            var useWHarass = Menu.GetValue<MenuBool>("UseWHarass").Enabled;
            var useEHarass = Menu.GetValue<MenuBool>("UseEHarass").Enabled;
            var spellWeaveCombo = Menu.GetValue<MenuBool>("SpellWeaveCombo").Enabled;

            var target = TargetSelector.GetTarget(Q.Range, DamageType.Magical);

            if (!target.IsValidTarget() || Player.ManaPercent < Menu.GetValue<MenuSlider>("HarassMinMana").Value)
            {
                return;
            }

            if (useQHarass && Q.IsReady())
            {
                if (spellWeaveCombo)
                {
                    if (!target.HasPassive())
                    {
                        CastQ(target);
                    }
                }
                else
                {
                    CastQ(target);
                }
            }

            if (useWHarass && W.IsReady())
            {
                W.Cast(Game.CursorPos);
            }

            if (useEHarass && E.IsReady())
            {
                CastE(target);
            }
        }
        
        /// <summary>
        ///     Does the lane clear.
        /// </summary>
        private static void DoLaneClear()
        {
            var useQWaveClear = Menu.GetValue<MenuBool>("UseQWaveClear").Enabled;
            var useEWaveClear = Menu.GetValue<MenuBool>("UseEWaveClear").Enabled;
            var useRWaveClear = Menu.GetValue<MenuBool>("UseRWaveClear").Enabled;
            var waveClearMana = Menu.GetValue<MenuSlider>("WaveClearMinMana").Value;

            if (Player.ManaPercent < waveClearMana)
            {
                return;
            }

            if (useQWaveClear && Q.IsReady())
            {
                var farmLoc = Q.GetLineFarmLocation(MinionManager.GetMinions(Q.Range));

                if (farmLoc.MinionsHit >= 2)
                {
                    Q.Cast(farmLoc.Position);
                }
            }

            if (useEWaveClear && E.IsReady())
            {
                var farmLoc = E.GetCircularFarmLocation(MinionManager.GetMinions(E.Range));

                if (farmLoc.MinionsHit >= 3)
                {
                    E.Cast(farmLoc.Position);
                }
            }

            if (!useRWaveClear || !R.IsReady())
            {
                return;
            }
            {
                var farmLoc = R.GetLineFarmLocation(MinionManager.GetMinions(R.Range));

                if (farmLoc.MinionsHit >= 10)
                {
                    R.Cast(farmLoc.Position);
                }
            }
        }
        
        /// <summary>
        ///     Fired when the game is Drawn.
        /// </summary>
        /// <param name="args">The <see cref="EventArgs" /> instance containing the event data.</param>
        private static void Drawing_OnDraw(EventArgs args)
        {
            var drawQ = Menu.GetValue<MenuBool>("DrawQ").Enabled;
            var drawW = Menu.GetValue<MenuBool>("DrawW").Enabled;
            var drawE = Menu.GetValue<MenuBool>("DrawE").Enabled;
            var drawErad = Menu.GetValue<MenuBool>("DrawERad").Enabled;

            if (drawQ)
            {
                CircleRender.Draw(Player.Position, Q.Range, Q.IsReady() ? Color.Aqua : Color.Red);
            }

            if (drawW)
            {
                CircleRender.Draw(Player.Position, W.Range, W.IsReady() ? Color.Aqua : Color.Red);
            }

            if (drawE)
            {
                CircleRender.Draw(Player.Position, E.Range, E.IsReady() ? Color.Aqua : Color.Red);
            }

            if (drawErad && EObject != null)
            {
                CircleRender.Draw(EObject.Position, 350, Color.CornflowerBlue);
            }
        }
        
        /// <summary>
        ///     Fired when the scene has been fully drawn.
        /// </summary>
        /// <param name="args">The <see cref="EventArgs" /> instance containing the event data.</param>
        private static void Drawing_OnEndScene(EventArgs args)
        {
            if (!Menu.GetValue<MenuBool>("DrawR").Enabled || !R.IsReady())
            {
                return;
            }

            var pointList = new List<Vector3>();

            for (var i = 0; i < 30; i++)
            {
                var angle = i * Math.PI * 2 / 30;
                pointList.Add(
                    new Vector3(
                        Player.Position.X + R.Range * (float)Math.Cos(angle), 
                        Player.Position.Y + R.Range * (float)Math.Sin(angle), 
                        Player.Position.Z));
            }

            for (var i = 0; i < pointList.Count; i++)
            {
                var a = pointList[i];
                var b = pointList[i == pointList.Count - 1 ? 0 : i + 1];

                var aonScreen = Drawing.WorldToMinimap(a);
                var bonScreen = Drawing.WorldToMinimap(b);

                Drawing.DrawLine(aonScreen.X, aonScreen.Y, bonScreen.X, bonScreen.Y, 1, System.Drawing.Color.Aqua);
            }
        }
        
        /// <summary>
        ///     Fired when the game is loaded.
        /// </summary>
        /// <param name="args">The <see cref="EventArgs" /> instance containing the event data.</param>
        private static void Game_OnUpdate(EventArgs args)
        {
            try
            {
                switch (Orbwalker.ActiveMode)
                {
                    case OrbwalkerMode.Harass:
                        DoHarass();
                        break;
                    case OrbwalkerMode.LaneClear:
                        DoLaneClear();
                        break;
                    case OrbwalkerMode.Combo:
                        DoCombo();
                        break;
                    case OrbwalkerMode.LastHit:
                        break;
                    case OrbwalkerMode.None:
                        break;
                    default:
                        throw new ArgumentOutOfRangeException();
                }

                if (Menu.GetValue<MenuKeyBind>("HarassKeybind").Active && Orbwalker.ActiveMode != OrbwalkerMode.Harass)
                {
                    DoHarass();
                }

                if (ECasted && EObject.Position.CountEnemyHeroesInRange(350) >= 1
                            && ObjectManager.Get<AIHeroClient>()
                                .Count(x => x.IsValidTarget(350, true, EObject.Position) && !x.HasPassive()) >= 1)
                {
                    E.Cast();
                }

                KillSteal();
                JungleKillSteal();
                AutoShield();
            }catch(Exception){}
        }
        
        /// <summary>
        ///     Fired when the game is loaded.
        /// </summary>
        /// <param name="args">The <see cref="EventArgs" /> instance containing the event data.</param>
        private static void GameOnOnGameLoad()
        {
            if (Player.CharacterName != "Lux")
            {
                return;
            }

            Q = new Spell(SpellSlot.Q, 1175);
            W = new Spell(SpellSlot.W, 1075);
            E = new Spell(SpellSlot.E, 1075);
            R = new Spell(SpellSlot.R, 3000);

            Q.SetSkillshot(0.25f, 80f, 1200f, true, SpellType.Line);
            W.SetSkillshot(0.25f, 110f, 1200f, false, SpellType.Line);
            E.SetSkillshot(0.3f, 250f, 1050f, false, SpellType.Circle);
            R.SetSkillshot(1f, 110f, float.MaxValue, false, SpellType.Line);

            CreateMenu();

            //DamageIndicator.DamageToUnit = DamageToUnit;
            //DamageIndicator.Enabled = true;

            Game.OnUpdate += Game_OnUpdate;
            AntiGapcloser.OnGapcloser += AntiGapcloserOnOnEnemyGapcloser;
            Drawing.OnDraw += Drawing_OnDraw;
            Drawing.OnEndScene += Drawing_OnEndScene;
        }

        /// <summary>
        ///     Fired on an incoming gapcloser.
        /// </summary>
        /// <param name="gapcloser">The gapcloser.</param>
        private static void AntiGapcloserOnOnEnemyGapcloser(AIHeroClient sender, AntiGapcloser.GapcloserArgs args)
        {
            if (!sender.IsValidTarget(Q.Range) || !Menu.GetValue<MenuBool>("QGapcloser").Enabled)
            {
                return;
            }

            Q.Cast(sender);
        }
        
        /// <summary>
        ///     Last hits jungle mobs with a spell.
        /// </summary>
        private static void JungleKillSteal()
        {
            if (!R.IsReady())
            {
                return;
            }

            var stealBlue = Menu.GetValue<MenuBool>("StealBlueBuff").Enabled;
            var stealRed = Menu.GetValue<MenuBool>("StealRedBuff").Enabled;
            var stealDragon = Menu.GetValue<MenuBool>("StealDragon").Enabled;
            var stealBaron = Menu.GetValue<MenuBool>("StealBaron").Enabled;
            var stealBuffMode = Menu.GetValue<MenuList>("StealBuffMode").Index;

            if (stealBaron)
            {
                var baron =
                    ObjectManager.Get<AIMinionClient>().FirstOrDefault(x => x.Name.Equals("SRU_Baron"));

                if (baron != null)
                {
                    var healthPred = SebbyLib.HealthPrediction.GetHealthPrediction(baron, (int)(R.Delay * 1000) + Game.Ping / 2);

                    if (R.GetDamage(baron) >= healthPred)
                    {
                        R.Cast(baron);
                    }
                }
            }

            if (stealDragon)
            {
                var dragon =
                    ObjectManager.Get<AIMinionClient>().FirstOrDefault(x => x.Name.Equals("SRU_Dragon"));

                if (dragon != null)
                {
                    var healthPred = SebbyLib.HealthPrediction.GetHealthPrediction(dragon, (int)(R.Delay * 1000) + Game.Ping / 2);

                    if (R.GetDamage(dragon) >= healthPred)
                    {
                        R.Cast(dragon);
                    }
                }
            }

            if (stealBlue)
            {
                var blueBuffs =
                    ObjectManager.Get<AIMinionClient>().Where(x => x.Name.Equals("SRU_Blue")).ToList();

                if (blueBuffs.Any())
                {
                    var blueBuff =
                        blueBuffs.Where(
                            x =>
                            R.GetDamage(x)
                            > SebbyLib.HealthPrediction.GetHealthPrediction(x, (int)(R.Delay * 1000) + Game.Ping / 2))
                            .FirstOrDefault(
                                x =>
                                (x.CountAllyHeroesInRange(1000) == 0 && stealBuffMode == 0)
                                || (x.CountAllyHeroesInRange(1000) > 0 && stealBuffMode == 2) || stealBuffMode == 3);

                    if (blueBuff != null)
                    {
                        R.Cast(blueBuff);
                    }
                }
            }

            if (!stealRed)
            {
                return;
            }

            var redBuffs =
                ObjectManager.Get<AIMinionClient>().Where(x => x.Name.Equals("SRU_Red")).ToList();

            if (!redBuffs.Any())
            {
                return;
            }

            var redBuff =
                redBuffs.Where(
                    x => R.GetDamage(x) > SebbyLib.HealthPrediction.GetHealthPrediction(x, (int)(R.Delay * 1000) + Game.Ping / 2))
                    .FirstOrDefault(
                        x =>
                        (x.CountAllyHeroesInRange(1000) == 0 && stealBuffMode == 0)
                        || (x.CountAllyHeroesInRange(1000) > 0 && stealBuffMode == 2) || stealBuffMode == 3);

            if (redBuff != null)
            {
                R.Cast(redBuff);
            }
        }
        
        /// <summary>
        ///     Last hits champions with spells.
        /// </summary>
        private static void KillSteal()
        {
            var spellsToUse =
                new List<Spell>(
                    new[] { Q, E, R }.Where(
                        x => x.IsReady() && Menu.GetValue<MenuBool>("Use" + Enum.GetName(typeof(SpellSlot), x.Slot) + "KS").Enabled));

            foreach (var enemy in GameObjects.EnemyHeroes)
            {
                var spell =
                    spellsToUse.Where(x => x.GetDamage(enemy) > enemy.Health && enemy.IsValidTarget(x.Range))
                        .MinOrDefault(x => x.GetDamage(enemy));

                if (spell == null)
                {
                    continue;
                }

                spell.Cast(enemy);

                return;
            }
        }
        
        /// <summary>
        ///     The entry point of the application.
        /// </summary>
        /// <param name="args">The arguments.</param>
        // ReSharper disable once UnusedParameter.Local
        public static void Loads()
        {
            GameOnOnGameLoad();
        }
    }
    
    public static class LuxExtensions
    {
        #region Public Methods and Operators

        /// <summary>
        ///     Gets the passive damage.
        /// </summary>
        /// <param name="target">The target.</param>
        /// <returns></returns>
        public static float GetPassiveDamage(this AIHeroClient target)
        {
            return
                (float)
                ObjectManager.Player.CalculateDamage(
                    target, 
                    DamageType.Magical, 
                    10 + (8 * ObjectManager.Player.Level) + (0.2 * ObjectManager.Player.TotalMagicalDamage));
        }

        /// <summary>
        ///     Determines whether this instance has passive.
        /// </summary>
        /// <param name="target">The target.</param>
        /// <returns></returns>
        public static bool HasPassive(this AIHeroClient target)
        {
            return target.HasBuff("luxilluminatingfraulein");
        }

        #endregion
    }
}