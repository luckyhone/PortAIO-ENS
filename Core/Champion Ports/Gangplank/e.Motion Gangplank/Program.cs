using System;
using System.Collections.Generic;
using System.Linq;
using EnsoulSharp;
using EnsoulSharp.SDK;
using EnsoulSharp.SDK.MenuUI;
using EnsoulSharp.SDK.Rendering;
using EnsoulSharp.SDK.Utility;
using PortAIO;
using SharpDX;
using SPrediction;
using Color = System.Drawing.Color;
using Prediction = SPrediction.Prediction;

namespace e.Motion_Gangplank
{
    public class Program
    {
        private static Menu gangplankMenu;
        private static Spell Q, W, E, R;
        private static bool BarrelAAForced;
        private static Random Rand = new Random();
        private static DelayManager QDelay;
        private static AIHeroClient UltimateTarget;
        private static bool UltimateToBeUsed;

        private static Dictionary<string, BuffType> Buffs = new Dictionary<string, BuffType>()
        {
            {"charm", BuffType.Charm},
            {"slow", BuffType.Slow},
            {"poison", BuffType.Poison},
            {"blind", BuffType.Blind},
            {"silence", BuffType.Silence},
            {"stun", BuffType.Stun},
            {"fear", BuffType.Flee},
            {"polymorph", BuffType.Polymorph},
            {"snare", BuffType.Snare},
            {"taunt", BuffType.Taunt},
            {"suppression", BuffType.Suppression}
        };

        private static readonly List<Vector2> BarrelPositions = new List<Vector2>()
        {
            new Vector2(1205, 12097),
            new Vector2(1335, 12468),
            new Vector2(1577, 12820),
            new Vector2(1872, 13011),
            new Vector2(2252, 13299),
            new Vector2(2632, 13520)
        };

        public static List<Barrel> AllBarrel = new List<Barrel>();
        public static Vector3 EnemyPosition;
        public static AIHeroClient Player => ObjectManager.Player;

        public static void OnLoad()
        {
            #region Menu Initialization

            gangplankMenu = new Menu("e.Motion Gangplank mainMenu","e.Motion Gangplank" , true);

            var keyMenu = new Menu("key", "Key")
            {
                keyQ,
                keyMode,
                keyE,
                keyR
            };
            gangplankMenu.Add(keyMenu);

            var comboMenu = new Menu("combo", "Combo")
            {
                comboQ,
                comboQe,
                comboAae,
                comboE,
                comboEx,
                comboDouble,
                comboTriple,
                comboR,
                comboRMin
            };
            gangplankMenu.Add(comboMenu);

            var harassMenu = new Menu("harass", "Harass")
            {
                harassQ
            };
            gangplankMenu.Add(harassMenu);

            var lastHitMenu = new Menu("lasthit", "Lasthit")
            {
                LastHitQ,
                LastHitQe,
                lastHitMana
            };
            gangplankMenu.Add(lastHitMenu);

            var killStealMenu = new Menu("killSteal", "Kill Steal")
            {
                ksQ,
                ksR,
                ksMinWave
            };
            gangplankMenu.Add(killStealMenu);

            var drawMenu = new Menu("draw", "Draw")
            {
                qRange,
                eRange,
                eERange,
            };
            gangplankMenu.Add(drawMenu);

            var cleanseMenu = new Menu("cleanse", "Cleanse")
            {
                cleanseW,
                new Menu("bufftypes", "Enable cleanse for:")
                {
                    slow,
                    poison,
                    blind,
                    silence,
                    stun,
                    fear,
                    polymorph,
                    snare,
                    taunt,
                    suppression,
                    charm
                }
            };
            gangplankMenu.Add(cleanseMenu);

            var miscMenu = new Menu("misc", "Miscellanious")
            {
                additionalServerTick,
                enemyReactionTime,
                additionalReactionTime,
                trye,
                autoE
            };
            gangplankMenu.Add(miscMenu);
            gangplankMenu.Attach();

            #endregion
            
            SetUpSpells();
            RegisterEvents();

            QDelay = new DelayManager(Q, 1500);
        }

        #region Menu

        // Key 
        private static readonly MenuKeyBind keyQ = new MenuKeyBind("keyQ", "Semi-Automatic Q", Keys.Q,
            KeyBindType.Press);

        private static readonly MenuList keyMode = new MenuList("keyMode", "Semi Automatic E Mode", new[]
        {
            "Never use",
            "Place Connecting Barrel",
            "Place Connecting Barrel + Explode"
        });

        private static MenuKeyBind keyE = new MenuKeyBind("keyE", "Semi-Automatic E", Keys.E, KeyBindType.Press);
        private static MenuKeyBind keyR = new MenuKeyBind("keyR", "Semi-Automatic R", Keys.R, KeyBindType.Press);

        // Combo
        private static MenuBool comboQ = new MenuBool("comboQ", "Use Q");
        private static MenuBool comboQe = new MenuBool("comboQe", "Use Q on Barrel");
        private static MenuBool comboAae = new MenuBool("comboAae", "Use Autoattack on Barrel");
        private static MenuBool comboE = new MenuBool("comboE", "Use E");
        private static MenuBool comboEx = new MenuBool("comboEx", "Use E to Extend");

        private static MenuBool comboDouble =
            new MenuBool("comboDouble", "Use Double E Combo", false);

        private static MenuBool comboTriple = new MenuBool("comboTriple", "Use Triple E Combo");
        private static MenuBool comboR = new MenuBool("comboR", "Use R");
        private static MenuSlider comboRMin = new MenuSlider("comboRMin", "Minimum enemies for R", 3, 2, 6);

        // Harass
        private static MenuBool harassQ = new MenuBool("harassQ", "Use Q");

        // LastHit
        private static MenuBool LastHitQ = new MenuBool("lastHitQ", "Use Q");
        private static MenuBool LastHitQe = new MenuBool("lastHitQe", "Use Q on Barrels");
        private static MenuSlider lastHitMana = new MenuSlider("lastHitMana", "Minimum Mana %", 30);

        // KillSteal
        private static MenuBool warning = new MenuBool("warning", "Warning");
        private static MenuBool ksQ = new MenuBool("ksQ", "Use Q");
        private static MenuBool ksR = new MenuBool("ksR", "Semi-Automatic R");
        private static MenuSlider ksMinWave = new MenuSlider("minWave", "Minimum Waves for R", 6, 1, 18);

        // Drawings 
        private static MenuBool qRange = new MenuBool("qRange", "Draw Q Range", false);
        private static MenuBool eRange = new MenuBool("eRange", "Draw E Range", false);
        private static MenuBool eERange = new MenuBool("eERange", "Draw E Extended Range", false);


        // Cleanse
        public static MenuBool cleanseW = new MenuBool("cleanseW", "Use W to Cleanse");
        private static MenuBool slow = new MenuBool("cleanse.bufftypes.slow", "Slow");
        private static MenuBool poison = new MenuBool("cleanse.bufftypes.poison", "Poison");
        private static MenuBool blind = new MenuBool("cleanse.bufftypes.blind", "Blind");
        private static MenuBool silence = new MenuBool("cleanse.bufftypes.silence", "Silence");
        private static MenuBool stun = new MenuBool("cleanse.bufftypes.stun", "Stun");
        private static MenuBool fear = new MenuBool("cleanse.bufftypes.fear", "Fear");
        private static MenuBool polymorph = new MenuBool("cleanse.bufftypes.polymorph", "Polymorph");
        private static MenuBool snare = new MenuBool("cleanse.bufftypes.snare", "Snare");
        private static MenuBool taunt = new MenuBool("cleanse.bufftypes.taunt", "Taunt");
        private static MenuBool suppression = new MenuBool("cleanse.bufftypes.suppression", "Suppression");
        private static MenuBool charm = new MenuBool("cleanse.bufftypes.charm", "Charm");

        // Misc
        public static MenuSlider additionalServerTick =
            new MenuSlider("additionalServerTick", "Additional Server Tick", 30);

        public static MenuSlider enemyReactionTime =
            new MenuSlider("enemyReactionTime", "Enemy Reaction Time", 150, 0, 500);

        public static MenuSliderButton additionalReactionTime = new MenuSliderButton("additionalReactionTime",
            "Additional Reaction Time on Direction Change", 50, 0, 200);

        public static MenuBool trye = new MenuBool("trye", "Always extend with E");
        public static MenuBool autoE = new MenuBool("autoE", "Place Barrels automatically");

        #endregion

        public static void RegisterEvents()
        {
            Drawing.OnDraw += OnDraw;
            Game.OnUpdate += OnUpdate;
            GameObject.OnCreate += OnCreate;
            Spellbook.OnCastSpell += CheckForBarrel;
            AIBaseClient.OnProcessSpellCast += OnProcessSpellCast;
        }

        private static void OnProcessSpellCast(AIBaseClient sender, AIBaseClientProcessSpellCastEventArgs args)
        {
            if (sender.IsMe && args.Slot == SpellSlot.Q && E.IsReady(200) && args.Target.Name == "Barrel")
            {
                Barrel attackedBarrel = AllBarrel.Find(b => b.GetNetworkID() == args.Target.NetworkId);
                List<Barrel> barrelsInRange = GetBarrelsInRange(attackedBarrel).ToList();
                if (comboTriple.Enabled && barrelsInRange.Any())
                {
                    foreach (var barrel in barrelsInRange)
                    {
                        DelayAction.Add(Helper.GetQTime(args.Target.Position) - 100 - Game.Ping / 2,
                            () => InvokeTriplePlacement(barrel, AllBarrel));
                    }
                }

                if (comboDouble.Enabled && attackedBarrel.GetBarrel().Distance(Player) >= 610)
                {
                    foreach (var enemy in GameObjects.EnemyHeroes)
                    {
                        if (args.Target.Position.Distance(enemy.Position) >= 350 &&
                            args.Target.Position.Distance(enemy.Position) <= 850)
                        {
                            DelayAction.Add(200 + Game.Ping / 2, () => ForceCast(enemy, args.Target.Position));
                        }
                    }
                }
            }
        }

        private static void ForceCast(AIHeroClient target, Vector3 barrelPosition)
        {
            E.Cast(barrelPosition.ExtendToMaxRange(Player.Position.ExtendToMaxRange(target.Position, 980), 685));
        }

        private static void InvokeTriplePlacement(Barrel connectingBarrel, IEnumerable<Barrel> hitTest)
        {
            if (!E.IsReady())
            {
                return;
            }

            IEnumerable<AIHeroClient> invokedEnemies = GameObjects.EnemyHeroes.Where(e =>
                e.Position.Distance(connectingBarrel.GetBarrel().Position) < 1370 &&
                !hitTest.Any(b => b.GetBarrel().Position.Distance(e.Position) < 340));
            foreach (AIHeroClient enemy in invokedEnemies)
            {
                //Nice Algorithm with Bad Coding Style following
                //DRY - Do Repeat Yourself
                Vector3 tryPosition = enemy.Position;
                if (tryPosition.Distance(connectingBarrel.GetBarrel().Position) <= 685 &&
                    tryPosition.Distance(Player.Position) <= 1000)
                {
                    TriplePlacement(enemy, tryPosition);
                    return;
                }

                tryPosition = Player.Position.ExtendToMaxRange(enemy.Position, 1000);
                if (tryPosition.Distance(connectingBarrel.GetBarrel().Position) <= 685)
                {
                    TriplePlacement(enemy, tryPosition);
                    return;
                }

                tryPosition = connectingBarrel.GetBarrel().Position.ExtendToMaxRange(enemy.Position, 685);
                if (tryPosition.Distance(Player.Position) <= 1000)
                {
                    TriplePlacement(enemy, tryPosition);
                    return;
                }

                List<Vector2> optimalPositions = Helper.IntersectCircles(Player.Position.To2D(), 995,
                    connectingBarrel.GetBarrel().Position.To2D(), 680);
                if (optimalPositions.Count == 2)
                {
                    TriplePlacement(enemy,
                        optimalPositions[0].To3D().Distance(enemy.Position)
                        < optimalPositions[1].To3D().Distance(enemy.Position)
                            ? optimalPositions[0].To3D()
                            : optimalPositions[1].To3D());
                }
            }
        }

        private static void TriplePlacement(AIHeroClient enemy, Vector3 position)
        {
            if (position.Distance(enemy.Position) <= 340)
            {
                E.Cast(position);
            }
        }

        private static void CheckForBarrel(Spellbook sender, SpellbookCastSpellEventArgs args)
        {
            if (args.Target != null && args.Target.Name == "Barrel")
            {
                for (int i = 0; i < AllBarrel.Count; i++)
                {

                    if (AllBarrel.ElementAt(i).GetBarrel().NetworkId == args.Target.NetworkId)
                    {
                        if (sender.Owner.IsMelee)
                        {
                            AllBarrel.ElementAt(i).ReduceBarrelAttackTick();
                        }
                        else
                        {
                            int i1 = i;
                            //DelayAction.Add((int)(args.Start.Distance(args.End)/args.SData.MissileSpeed), () => { AllBarrel.ElementAt(i1).ReduceBarrelAttackTick(); });
                            DelayAction.Add((int) args.StartPosition.Distance(args.EndPosition),
                                () => { AllBarrel.ElementAt(i1).ReduceBarrelAttackTick(); });
                        }
                    }
                }
            }
        }

        private static void OnCreate(GameObject sender, EventArgs args)
        {
            if (sender.Name == "Barrel")
            {
                AllBarrel.Add(new Barrel((AIMinionClient) sender));
            }
        }

        private static void OnUpdate(EventArgs args)
        {
            KillSteal();
            Harass();
            QDelay.CheckEachTick();
            AutoE();
            CleanBarrel();
            Combo();
            Lasthit();
            //Cleanse();
            SemiAutomaticE();
        }

        private static void SemiAutomaticE()
        {
            if (E.IsReady() && keyE.Active)
            {
                if (keyMode.Index == 1)
                {
                    float lowest = 1600;
                    Vector3 bPos = Vector3.Zero;
                    foreach (Barrel barrel in AllBarrel)
                    {
                        if (barrel.GetBarrel().Distance(Game.CursorPos) < lowest)
                        {
                            bPos = barrel.GetBarrel().Position;
                            lowest = barrel.GetBarrel().Distance(Game.CursorPos);
                        }
                    }

                    if (lowest != 1600f)
                    {
                        E.Cast(bPos.Extend(Game.CursorPos, Math.Min(685, lowest)));
                    }
                }
                else if (keyMode.Index == 2 && Q.IsReady())
                {
                    IEnumerable<Barrel> toExplode =
                        AllBarrel.Where(b => b.CanQNow() && b.GetBarrel().Distance(Player) <= Q.Range);
                    if (toExplode.Any())
                    {
                        float lowest = 1600;
                        Barrel bar = null;
                        foreach (Barrel barrel in AllBarrel)
                        {
                            if (barrel.GetBarrel().Distance(Game.CursorPos) < lowest)
                            {
                                bar = barrel;
                                lowest = barrel.GetBarrel().Distance(Game.CursorPos);
                            }
                        }

                        if (bar != null)
                        {
                            E.Cast(bar.GetBarrel().Position.Extend(Game.CursorPos, Math.Min(685, lowest)));
                            QDelay.Delay(bar.GetBarrel());
                        }
                    }
                }
            }
        }

        private static void Cleanse()
        {
            if (W.IsReady() && cleanseW.Enabled)
            {
                if (Buffs.Any(entry =>
                        gangplankMenu.GetValue<MenuBool>("cleanse.bufftypes." + entry.Key).Enabled &&
                        Player.HasBuffOfType(entry.Value)))
                {
                    W.Cast();
                }
            }
        }

        private static void Lasthit()
        {
            if (Orbwalker.ActiveMode != OrbwalkerMode.LastHit || Player.ManaPercent <= lastHitMana.Value)
            {
                return;
            }

            if (Q.IsReady())
            {
                foreach (var barrel in AllBarrel)
                {
                    if (barrel.CanQNow() && MinionManager.GetMinions(barrel.GetBarrel().Position, 650)
                            .Any(m => m.Health < Q.GetDamage(m) && m.Distance(barrel.GetBarrel()) <= 380))
                    {
                        Q.Cast(barrel.GetBarrel());
                    }
                }

                if (LastHitQ.Enabled && (!AllBarrel.Any(b => b.GetBarrel().Position.Distance(Player.Position) < 1200) ||
                                         keyQ.Active))
                {
                    var lowHealthMinion = MinionManager.GetMinions(Player.Position, Q.Range).FirstOrDefault();
                    if (lowHealthMinion != null && lowHealthMinion.Health <= Q.GetDamage(lowHealthMinion))
                        Q.Cast(lowHealthMinion);
                }
            }
        }

        private static void Combo(bool extended = false, AIHeroClient sender = null)
        {
            if (Orbwalker.ActiveMode != OrbwalkerMode.Combo)
            {
                return;
            }

            if (comboAae.Enabled && Orbwalker.CanAttack())
            {
                List<Barrel> barrelsInAutoAttackRange = AllBarrel.Where(b =>
                    b.GetBarrel().Distance(Player) <= Player.GetRealAutoAttackRange(Player) && b.CanAANow()).ToList();
                if (barrelsInAutoAttackRange.Any() &&
                    (Player.CountEnemyHeroesInRange(Player.GetRealAutoAttackRange(Player)) == 0 ||
                     Player.Buffs.All(buff => buff.Name != "gangplankpassiveattack")))
                {
                    BarrelAAForced = false;
                    foreach (Barrel b in barrelsInAutoAttackRange)
                    {
                        if (GameObjects.EnemyHeroes.Any(enemy =>
                                b.GetBarrel().Position.CannotEscapeFromAA(enemy) || GetBarrelsInRange(b)
                                    .Any(bar => bar.GetBarrel().Position.CannotEscapeFromAA(enemy))))
                        {
                            Orbwalker.ForceTarget = b.GetBarrel();
                            BarrelAAForced = true;
                        }
                    }
                }
            }

            if (comboQe.Enabled && Q.IsReady() && !BarrelAAForced)
            {
                AIHeroClient target = TargetSelector.GetTarget(1200, DamageType.Physical);
                if (target != null)
                {
                    EnemyPosition = target.Position;
                    Helper.GetPredPos(target);
                    if (extended && target != sender)
                    {
                        extended = false;
                    }

                    foreach (var b in AllBarrel)
                    {
                        if (b.CanQNow() && (b.GetBarrel().Position.CannotEscape(target, extended) ||
                                            GetBarrelsInRange(b).Any(bb =>
                                                bb.GetBarrel().Position.CannotEscape(target, extended, true))))
                        {
                            QDelay.Delay(b.GetBarrel());
                            break;
                        }
                    }

                    if (E.IsReady() && !QDelay.Active())
                    {
                        if (comboDouble.Enabled)
                        {
                            foreach (var b in AllBarrel)
                            {
                                if (b.CanQNow() && b.GetBarrel().Distance(Player) > 615 &&
                                    b.GetBarrel().Distance(target) < 850)
                                {
                                    Q.Cast(b.GetBarrel());
                                    break;
                                }
                            }
                        }

                        if (comboEx.Enabled)
                        {
                            foreach (var b in AllBarrel)
                            {
                                var castPos = b.GetBarrel().Position.ExtendToMaxRange(Helper.PredPos.To3D(), 685);

                                if (b.CanQNow() && castPos.Distance(Player.Position) < 1000 &&
                                    castPos.CannotEscape(target, extended, true))
                                {
                                    E.Cast(castPos);
                                    QDelay.Delay(b.GetBarrel());
                                    break;
                                }
                            }
                        }
                    }
                }
            }

            if (Q.IsReady() && E.IsReady() && comboTriple.Enabled)
            {
                IEnumerable<Barrel> validBarrels =
                    AllBarrel.Where(b => b.CanQNow() && b.GetBarrel().Distance(Player) <= 625);
                foreach (Barrel validBarrel in validBarrels)
                {
                    IEnumerable<Barrel> inRange = GetBarrelsInRange(validBarrel);
                    if (
                        inRange.Any(
                            b =>
                                GameObjects.EnemyMinions.Any(
                                    e => b.GetBarrel().Distance(e.Position) < 1100 &&
                                         e.Distance(Player.Position) < 1000)))
                    {
                        Q.Cast(validBarrel.GetBarrel());
                    }
                }
            }

            if (comboQ.Enabled && Q.IsReady())
            {
                AIHeroClient target = TargetSelector.GetTarget(Q.Range, DamageType.Physical);
                if (target != null && (keyQ.Active || (!E.IsReady() &&
                                                       !AllBarrel.Any(b =>
                                                           b.GetBarrel().Position.Distance(target.Position) < 600))))
                {
                    Q.Cast(target);
                }
            }

            if (E.IsReady() && E.Instance.Ammo > 1 && comboE.Enabled &&
                !AllBarrel.Any(b => b.GetBarrel().Position.Distance(Player.Position) <= 1200))
            {
                AIHeroClient target = TargetSelector.GetTarget(1000, DamageType.Physical);
                if (target == null) return;
                Helper.GetPredPos(target);
                Vector2 castPos = target.Position.Extend(Helper.PredPos.To3D(), 200).To2D();
                if (Player.Distance(castPos) <= E.Range)
                {
                    E.Cast(castPos);
                }
                else
                {
                    E.Cast(Player.Position.Extend(castPos.To3D(), 1000));
                }
            }
        }


        private static void CleanBarrel()
        {
            for (int i = AllBarrel.Count - 1; i >= 0; i--)
            {
                //Console.WriteLine("Looped");
                // ReSharper disable once CompareOfFloatsByEqualityOperator
                if (AllBarrel.ElementAt(i).GetBarrel() == null || AllBarrel.ElementAt(i).GetBarrel().Health == 0)
                {
                    AllBarrel.RemoveAt(i);
                    break;
                }
            }
        }

        private static void AutoE()
        {
            if (autoE.Enabled && E.IsReady() && E.Ammo > 1 &&
                !AllBarrel.Any(b => b.GetBarrel().Distance(Player) <= 1200))
            {
                AIHeroClient target = TargetSelector.GetTarget(1400, DamageType.Physical);
                List<Vector2> possiblePositions =
                    BarrelPositions.Where(pos => pos.Distance(Player) <= E.Range).ToList();
                if (target != null && possiblePositions.Count != 0)
                {
                    float minDist = 2000;
                    Vector2 castPos = Vector2.Zero;
                    foreach (var pos in possiblePositions.Where(pos => pos.Distance(target) < minDist))
                    {
                        castPos = new Vector2(pos.X + Rand.Next(0, 21) - 10, pos.Y + Rand.Next(0, 21) - 10);
                        minDist = pos.Distance(target);
                    }

                    E.Cast(castPos);

                }
            }
        }

        private static void Harass()
        {
            if (Q.IsReady() && harassQ.Enabled && Orbwalker.ActiveMode == OrbwalkerMode.Harass)
            {
                AIHeroClient target = TargetSelector.GetTarget(Q.Range, DamageType.Physical);
                if (target != null)
                {
                    Q.Cast(target);
                }
            }
        }

        private static void KillSteal()
        {
            if (ksQ.Enabled && Q.IsReady())
            {
                foreach (var enemy in GameObjects.EnemyHeroes)
                {
                    if (enemy.Health <= Q.GetDamage(enemy) && Player.Distance(enemy) <= Q.Range)
                    {
                        Q.Cast(enemy);
                    }
                }
            }

            if (ksR.Enabled && R.IsReady() && UltimateToBeUsed && UltimateTarget != null)
            {
                R.Cast(Prediction.GetFastUnitPosition(UltimateTarget, 150));
            }
        }

        private static void OnDraw(EventArgs args)
        {
            DrawRanges();
            KillstealDrawings();
            Warning();
            DrawE();
        }

        private static void DrawE()
        {
            if (E.IsReady() && eERange.Enabled)
            {
                float lowest = 1600;
                Vector3 bPos = Vector3.Zero;
                foreach (Barrel barrel in AllBarrel)
                {
                    if (barrel.GetBarrel().Distance(Game.CursorPos) < lowest)
                    {
                        bPos = barrel.GetBarrel().Position;
                        lowest = barrel.GetBarrel().Distance(Game.CursorPos);
                    }
                }

                if (lowest != 1600f)
                {
                    Drawing.DrawCircleIndicator(bPos.ExtendToMaxRange(Game.CursorPos, 685), 350, Color.ForestGreen);
                    Drawing.DrawLine(Drawing.WorldToScreen(bPos),
                        Drawing.WorldToScreen(bPos.ExtendToMaxRange(Game.CursorPos, 685)), 5, Color.ForestGreen);
                }
            }
        }

        private static void Warning()
        {
            if ((Player.Position.Distance(new Vector3(394, 461, 171)) <= 1000 ||
                 Player.Position.Distance(new Vector3(14340, 14391, 170)) <= 1000) &&
                Player.GetBuffCount("gangplankbilgewatertoken") >= 500 && warning.Enabled)
            {
                Drawing.DrawText(200, 200, Color.Red, "Don't forget to buy Ultimate Upgrade with Silver Serpents");
            }
        }

        private static void KillstealDrawings()
        {

            if (ksR.Enabled && R.IsReady())
            {
                int minKillWave = 20;
                UltimateTarget = null;
                foreach (var enemy in GameObjects.EnemyHeroes)
                {
                    if (enemy.IsTargetable && !enemy.IsZombie() && enemy.IsVisible && !enemy.IsDead)
                    {
                        int killWave = 1 +
                                       (int) ((enemy.Health - (Player.HasBuff("GangplankRUpgrade2")
                                           ? (R.Instance.Level + 20 + Player.TotalMagicalDamage * 0.1) * 3
                                           : 0)) / R.GetDamage(enemy));
                        if (killWave < minKillWave)
                        {
                            minKillWave = killWave;
                            UltimateTarget = enemy;
                        }
                    }
                }

                if (UltimateTarget != null && minKillWave <= (Player.HasBuff("GangplankRUpgrade1") ? 18 : 12) &&
                    minKillWave <= ksMinWave.Value)
                {
                    UltimateToBeUsed = true;
                    Drawing.DrawText(200, 260, Color.Tomato,
                        UltimateTarget.CharacterName + " is killable " +
                        (minKillWave < 1
                            ? "only with Death Daughter [R] Upgrade"
                            : "with " + minKillWave + " R Waves"));
                }
                else
                {
                    UltimateToBeUsed = false;
                }
            }
        }

        private static void DrawRanges()
        {
            if (qRange.Enabled && Q.IsReady())
            {
                CircleRender.Draw(Player.Position,Q.Range,SharpDX.Color.IndianRed);
            }
            if (eRange.Enabled && E.IsReady())
            {
                CircleRender.Draw(Player.Position,E.Range,SharpDX.Color.IndianRed);
            }
        }

        private static IEnumerable<Barrel> GetBarrelsInRange(Barrel initalBarrel)
        {
            return AllBarrel.Where(b =>
                b.GetBarrel().Position.Distance(initalBarrel.GetBarrel().Position) <= 685 && b != initalBarrel);
        }

        public static void SetUpSpells()
        {
            Q = new Spell(SpellSlot.Q, 625f);
            W = new Spell(SpellSlot.W, 0);
            E = new Spell(SpellSlot.E, 1000);
            R = new Spell(SpellSlot.R);
        }
    }
}