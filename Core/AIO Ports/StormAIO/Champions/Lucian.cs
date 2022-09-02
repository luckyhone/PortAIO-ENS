using System;
using System.Collections.Generic;
using System.Linq;
using EnsoulSharp;
using EnsoulSharp.SDK;
using EnsoulSharp.SDK.MenuUI;
using SharpDX;
using StormAIO.utilities;
using Color = System.Drawing.Color;
using Menu = EnsoulSharp.SDK.MenuUI.Menu;

namespace StormAIO.Champions
{
    internal class Lucian
    {
        #region Basics

        private static Spell Q, Q2, W, E, R;

        private static Menu champMenu;
        private static AIHeroClient Player => ObjectManager.Player;

        private static bool HasPassiveBuff =>
            CanLosePassive || Player.Buffs.Any(x =>
                x.Name.Equals("lucianpassivebuff", StringComparison.CurrentCultureIgnoreCase));


        private static bool HasWDebuff(AIBaseClient unit)
        {
            return unit.Buffs.Any(x => x.Name.Equals("lucianwdebuff", StringComparison.CurrentCultureIgnoreCase));
        }


        private static bool IsPreAttack;
        private static bool HasSheenBuff => Player.HasBuff("Sheen");

        private static bool IsCastingQ => Player.Spellbook.IsCastingSpell &&
                                          Q.CooldownTime - Variables.GameTimeTickCount <= 0 &&
                                          Q.State == SpellState.Cooldown;

        private static bool IsCastingR =>
            Player.Buffs.Any(x => x.Name.Equals("lucianr", StringComparison.CurrentCultureIgnoreCase));

        private static bool HasAnyOrbwalkerFlags => Orbwalker.ActiveMode != 0;


        // ReSharper disable once NotAccessedField.Local
        private static float LastETime;

        private static int QMana => Q.State != SpellState.NotLearned ? 50 + 10 * (Q.Level - 1) : 0;
        private static int WMana => W.State != SpellState.NotLearned ? 70 : 0;
        private static int EMana => E.State != SpellState.NotLearned ? 40 - 10 * (E.Level - 1) : 0;
        private static int RMana => R.State != SpellState.NotLearned ? 100 : 0;

        private static int LastSpellCastTime;

        private static bool CanLosePassive
            => Variables.GameTimeTickCount - LastSpellCastTime <=
               Player.AttackCastDelay * 1000 + Math.Min(Game.Ping / 2, 70);

        #endregion


        #region Menu

        private static void CreateMenu()
        {
            champMenu = new Menu("Lucian", "Lucian", true);

            // Q
            var QMenu = new Menu("Q", "Q")
            {
                new MenuBool("QC", "Use Q in Combo"),
                new MenuBool("QH", "Use Q in Harass"),
                new MenuSlider("QMana", "Mana for Q in harass", 40),
                new MenuBool("QE", "Use Extended Q ")
            };
            champMenu.Add(QMenu);

            // W
            var WMenu = new Menu("W", "W")
            {
                new MenuBool("WC", "Use W in Combo")
            };
            champMenu.Add(WMenu);
            // E
            var EMenu = new Menu("E", "E")
            {
                new MenuBool("EC", "Use E in Combo")
            };
            champMenu.Add(EMenu);
            // R
            var RMenu = new Menu("R", "R")
            {
                new MenuBool("RC", "Use R in Combo Only When Target is out of range and killable"),
                new MenuKeyBind("RS", "Rsimikey", Keys.T, KeyBindType.Press)
            };
            champMenu.Add(RMenu);
            // lane clear
            var laneclear = new Menu("laneclear", "Lane Clear")
            {
                new MenuBool("Q", "Use Q for Lane Clear"),
                new MenuBool("W", "Use W for Lane Clear", false),
                new MenuBool("E", "Use E for Lane Clear", false),
                new MenuSlider("Mana", "Mana for LaneClear", 40),
                new MenuSeparator("JGL", "JungleClear"),
                new MenuBool("QJ", "Use Q"),
                new MenuBool("WJ", "Use W"),
                new MenuBool("EJ", "Use E")
            };
            champMenu.Add(laneclear);

            // kill steal
            var killsteal = new Menu("KillSteal", "Kill Steal")
            {
                new MenuBool("Q", "Use Q"),
                new MenuBool("W", "Use W"),
                new MenuBool("R", "Use R", false)
            };
            champMenu.Add(killsteal);

            // Drawing
            var drawMenu = new Menu("Drawing", "Draw")
            {
                new MenuBool("DrawQ", "Draw Q"),
                new MenuBool("DrawW", "Draw W"),
                new MenuBool("DrawE", "Draw E")
            };

            champMenu.Add(drawMenu);
            MainMenu.Main_Menu.Add(champMenu);
        }

        #endregion

        #region Menu Checker

        private static bool UseE => champMenu["E"].GetValue<MenuBool>("EC").Enabled;
        private static bool UseQC => champMenu["Q"].GetValue<MenuBool>("QC").Enabled;
        private static bool UseQH => champMenu["Q"].GetValue<MenuBool>("QH").Enabled;
        private static int UseQHM => champMenu["Q"].GetValue<MenuSlider>("QMana").Value;
        private static bool UseQmaxRange => champMenu["Q"].GetValue<MenuBool>("QE").Enabled;
        private static bool UseWC => champMenu["W"].GetValue<MenuBool>("WC").Enabled;
        private static bool UseR => champMenu["R"].GetValue<MenuBool>("RC").Enabled;
        private static MenuKeyBind RKeyActive => champMenu["R"].GetValue<MenuKeyBind>("RS");
        private static int LaneMana => champMenu["laneclear"].GetValue<MenuSlider>("Mana").Value;
        private static bool QLaneClear => champMenu["laneclear"].GetValue<MenuBool>("Q").Enabled;
        private static bool WLaneClear => champMenu["laneclear"].GetValue<MenuBool>("W").Enabled;
        private static bool ELaneClear => champMenu["laneclear"].GetValue<MenuBool>("E").Enabled;
        private static bool QjglClear => champMenu["laneclear"].GetValue<MenuBool>("QJ").Enabled;
        private static bool WjglClear => champMenu["laneclear"].GetValue<MenuBool>("WJ").Enabled;
        private static bool EjglClear => champMenu["laneclear"].GetValue<MenuBool>("EJ").Enabled;
        private static bool KSQ => champMenu["KillSteal"].GetValue<MenuBool>("Q").Enabled;
        private static bool KSW => champMenu["KillSteal"].GetValue<MenuBool>("W").Enabled;
        private static bool KSR => champMenu["KillSteal"].GetValue<MenuBool>("R").Enabled;

        #endregion

        #region Loader

        public Lucian()
        {
            CreateMenu();
            Q = new Spell(SpellSlot.Q, 500) {Delay = 0.4f};
            Q2 = new Spell(SpellSlot.Q, 1000f);
            Q2.SetSkillshot(0.4f, 25f, int.MaxValue, false, SpellType.Line);
            W = new Spell(SpellSlot.W, 900);
            W.SetSkillshot(0.25f, 320f, 1600f, false, SpellType.Circle);
            E = new Spell(SpellSlot.E, 425);
            E.SetSkillshot(0f, 65f, 2000, false, SpellType.Line);
            R = new Spell(SpellSlot.R, 1200);
            R.SetSkillshot(0.25f, 110f, 2000, false, SpellType.Line);

            AIHeroClient.OnLevelUp += AIHeroClientOnOnLevelUp;
            Orbwalker.OnAfterAttack += OnAfterAttack;
            Orbwalker.OnBeforeAttack += OnBeforeAttack;
            GameObject.OnCreate += GameObject_OnCreate;
            Game.OnUpdate += GameOnOnUpdate;
            Drawing.OnDraw += DrawingOnOnDraw;
            Drawing.OnEndScene += delegate
            {
                var t = TargetSelector.GetTarget(2000f,DamageType.Physical);
                if (!Helper.drawIndicator || t == null) return;
                Helper.Indicator(AllDamage(t));
            };
            AIBaseClient.OnPlayAnimation += (sender, args) =>
            {
                if (sender.IsMe &&
                    (args.Animation == "Spell1" || args.Animation == "Spell2" || args.Animation == "Spell3") &&
                    HasAnyOrbwalkerFlags) Player.IssueOrder(GameObjectOrder.MoveTo, Game.CursorPos, false);
            };

            Spellbook.OnCastSpell += (sender, args) =>
            {
                if (!sender.Owner.IsMe)
                    return;

                if (args.Slot != SpellSlot.Q && args.Slot != SpellSlot.W && args.Slot != SpellSlot.E)
                    return;

                if (HasAnyOrbwalkerFlags)
                {
                    if (IsPreAttack)
                    {
                        args.Process = false;
                        return;
                    }

                    if (args.Slot == SpellSlot.E) LastETime = Variables.GameTimeTickCount;
                }

                LastSpellCastTime = Variables.GameTimeTickCount;
            };
            // ReSharper disable once ObjectCreationAsStatement
            new DrawText("Simi R Key", RKeyActive.Key.ToString(), RKeyActive, Color.GreenYellow, Color.Red, 123, 132);
        }

        private void OnBeforeAttack(object sender, BeforeAttackEventArgs args)
        {
            if (IsCastingR) args.Process = false;
        }

        private void OnAfterAttack(object sender, AfterAttackEventArgs e)
        {
            IsPreAttack = false;
        }

        #endregion

        #region Args

        private void AIHeroClientOnOnLevelUp(AIHeroClient sender, AIHeroClientLevelUpEventArgs args)
        {
            if (sender.IsMe)
            {
                if (Q2.Level > 1)
                    Q.Delay = (float) (0.409 - 0.009 *
                            Player.Level
                        ); // from https://leagueoflegends.fandom.com/wiki/Lucian Q cast Time 
                if (Q.Level > 1)
                    Q.Delay = (float) (0.409 - 0.009 *
                            Player.Level
                        ); // from https://leagueoflegends.fandom.com/wiki/Lucian Q cast Time 
            }
        }

        private void DrawingOnOnDraw(EventArgs args)
        {
            var drawQ = champMenu["Drawing"].GetValue<MenuBool>("DrawQ");
            var drawW = champMenu["Drawing"].GetValue<MenuBool>("DrawW");
            var drawE = champMenu["Drawing"].GetValue<MenuBool>("DrawE");

            var p = Player.Position;

            if (drawQ.Enabled && Q.IsReady())
                Drawing.DrawCircleIndicator(p, Player.GetRealAutoAttackRange(), Color.Purple);
            if (drawE.Enabled && E.IsReady()) Drawing.DrawCircleIndicator(p, E.Range, Color.Red);
            if (drawW.Enabled && W.IsReady()) Drawing.DrawCircleIndicator(p, W.Range, Color.DarkCyan);
        }

        private static void GameOnOnUpdate(EventArgs args)
        {
            switch (Orbwalker.ActiveMode)
            {
                case OrbwalkerMode.Combo:
                    Combo();
                    break;
                case OrbwalkerMode.Harass:
                    Harass();
                    break;
                case OrbwalkerMode.LaneClear:
                    LaneClear();
                    JunglClear();
                    break;
                case OrbwalkerMode.LastHit:
                    break;
                case OrbwalkerMode.None:
                    break;
            }

            KillSteal();
            if (RKeyActive.Active) CastR();
        }

        private static void GameObject_OnCreate(GameObject sender, EventArgs args)
        {
            if (!HasAnyOrbwalkerFlags || !sender.IsMe)
                return;


            if (sender.Name == "LucianWMissile")
            {
                Orbwalker.ResetAutoAttackTimer();
                return;
            }

            if (sender.GetType() != typeof(EffectEmitter))
                return;

            var particle = sender as EffectEmitter;

            if (particle == null || !particle.Name.Contains("Lucian_Base_Q_laser") || particle.Distance(Player) > 200)
                return;

            Orbwalker.ResetAutoAttackTimer();
        }

        #endregion

        private static float GetComboDamage(AIHeroClient unit, int autoAttacks = 1)
        {
            var damage = Player.GetAutoAttackDamage(unit) * autoAttacks;

            if (unit.IsValidTarget(1000) && Q.IsReady())
                damage += Player.GetSpellDamage(unit, SpellSlot.Q);

            if (unit.IsValidTarget(W.Range) && W.IsReady())
                damage += Player.GetSpellDamage(unit, SpellSlot.W);


            return (float) damage;
        }

        #region Orbwalker Events

        private static void Combo()
        {
            var qTarget = TargetSelector.GetTarget(1025, DamageType.Physical);

            if (qTarget != null && PossibleToInterruptQ(qTarget))
            {
                var positionAfterE = Prediction.GetPrediction(qTarget, 300).UnitPosition;
                var pos = Player.Position.Extend(Game.CursorPos,
                    positionAfterE.Distance(Player) + qTarget.BoundingRadius);

                if (!pos.IsUnderEnemyTurret() && UseE)
                {
                    E.Cast(pos);
                    return;
                }
            }

            if (UseE) ELogics();


            if (Q.IsReady() && UseQC && !HasPassiveBuff && !HasSheenBuff)
            {
                var target = TargetSelector.GetTarget(Q.Range, DamageType.Physical);
                var target2 = TargetSelector.GetTarget(1025, DamageType.Physical);

                if (PossibleEqCombo(target) || PossibleEqCombo(target2))
                    return;

                if (Helper.CanAttackAnyHero && target != null && Orbwalker.CanAttack())
                {
                    var predictedPosition = Prediction.GetPrediction(target,
                        (int) ((Player.AttackCastDelay + Player.AttackDelay) * 1000) + Game.Ping / 2).CastPosition;

                    if (IsInRange(Player.Position, predictedPosition, Player.GetRealAutoAttackRange())) goto WRLogc;
                }

                if (target != null && target.IsValidTarget(Q.Range) &&
                    (Player.Mana - QMana > EMana + (R.IsReady() ? RMana : 0) && !Player.IsDashing() ||
                     Player.GetSpellDamage(target, SpellSlot.Q) + Player.GetAutoAttackDamage(target) * 3 >
                     target.Health + target.AllShield))
                {
                    CastQ();
                    return;
                }

                if (target2 != null &&
                    (Player.Mana - QMana > EMana + (R.IsReady() ? RMana : 0) && !Player.IsDashing() ||
                     Player.GetSpellDamage(target2, SpellSlot.Q) +
                     Player.GetAutoAttackDamage(target2) * 3 > target2.Health + target2.AllShield) &&
                    !Player.IsDashing())
                    CastQ();
            }

            WRLogc:

            if (W.IsReady() && !IsCastingR && !HasPassiveBuff && !HasSheenBuff && UseWC)
            {
                var target = TargetSelector.GetTarget(W.Range, DamageType.Physical);

                if (target != null && (Player.Mana - WMana > (R.IsReady() ? RMana : 0) && !Player.IsDashing() ||
                                       Player.GetSpellDamage(target, SpellSlot.W) > target.Health + target.AllShield))

                    W.Cast(target);
            }

            if (!R.IsReady() || Player.IsUnderEnemyTurret() || !UseR)
                return;

            if (Player.CountEnemyHeroesInRange(Player.GetRealAutoAttackRange() + 150) == 0)
            {
                var rTarget = TargetSelector.GetTarget(R.Range - 100, DamageType.Physical);

                if (rTarget == null || rTarget.HasBuffOfType(BuffType.Invulnerability))
                    return;

                var health = rTarget.Health + rTarget.AllShield;

                if (health < 0)
                    return;


                var damage = Rdamage(rTarget);

                if (damage >= health && Player.Spellbook.GetSpell(SpellSlot.R).Name == "LucianR")
                    R.CastIfHitchanceMinimum(rTarget, HitChance.Medium);
            }
            else if (Player.CountEnemyHeroesInRange(Player.GetRealAutoAttackRange() + 300) == 1)
            {
                var target = TargetSelector.GetTarget(Player.GetRealAutoAttackRange(), DamageType.Physical);

                if (target == null || !HasWDebuff(target) || !target.IsFacing(Player) || !target.IsMoving ||
                    target.Distance(Player) < Player.GetRealAutoAttackRange() ||
                    Player.Spellbook.GetSpell(SpellSlot.R).Name != "LucianR")
                    return;

                var health = target.Health + target.AllShield;

                if (health < GetComboDamage(target, 3))
                    return;

                R.CastIfHitchanceMinimum(target, HitChance.High);
            }
        }

        private static void LaneClear()
        {
            if (!MainMenu.SpellFarm.Active || Player.ManaPercent < LaneMana) return;
            var Minions = GameObjects.GetMinions(Player.Position, Q.Range)
                .Where(x => x.IsValidTarget(Player.GetRealAutoAttackRange()) && x.IsEnemy)
                .OrderBy(x => x.DistanceToPlayer())
                .ToList();

            if (!Minions.Any())
                return;

            if (HasPassiveBuff || HasSheenBuff || Minions.Count() <= 1)
                return;

            foreach (var target in Minions)
            {
                if (Q.IsReady() && QLaneClear) Q.CastOnUnit(target);

                if (W.IsReady() && WLaneClear) W.CastOnUnit(target);

                if (!E.IsReady() || !ELaneClear)
                    return;

                var shortEPosition = Player.Position.Extend(Game.CursorPos, 85);
                E.Cast(shortEPosition);
            }
        }

        private static void JunglClear()
        {
            var jungleMinions = GameObjects.GetJungles(Player.Position, Q2.Range)
                .Where(x => x.IsValidTarget(Player.GetRealAutoAttackRange())).OrderBy(x => x.DistanceToPlayer())
                .ToList();

            if (!jungleMinions.Any())
                return;

            if (HasPassiveBuff || HasSheenBuff)
                return;

            var target = Orbwalker.GetTarget() as AIBaseClient;

            if (target == null)
                return;

            if (Q.IsReady() && QjglClear)
            {
                Q.CastOnUnit(target);
                return;
            }

            if (W.IsReady() && WjglClear)
            {
                W.Cast(target);
                return;
            }

            if (!E.IsReady() || !EjglClear)
                return;

            var shortEPosition = Player.Position.Extend(Game.CursorPos, 85);
            E.Cast(shortEPosition);
        }

        private static void Harass()
        {
            if (!UseQH || Player.ManaPercent < UseQHM)
                return;

            CastQ();
        }

        #endregion

        private static void KillSteal()
        {
            var target = TargetSelector.GetTarget(1000,DamageType.Physical);
            if (target == null || target.HasBuffOfType(BuffType.Invulnerability)) return;
            if (Q.GetDamage(target) > target.Health + target.AllShield && KSQ) CastQ();
            if (W.GetDamage(target) > target.Health + target.AllShield && KSW)
                if (W.IsReady() && !IsCastingR)
                {
                    W.Cast(target);
                    return;
                }

            if (KSR)
            {
                if (!R.IsReady() || Player.IsUnderEnemyTurret())
                    return;

                if (Player.CountEnemyHeroesInRange(Player.GetRealAutoAttackRange() + 150) == 0)
                {
                    var rTarget = TargetSelector.GetTarget(R.Range - 100, DamageType.Physical);

                    if (rTarget == null || rTarget.HasBuffOfType(BuffType.Invulnerability))
                        return;

                    var health = rTarget.Health + rTarget.AllShield;

                    if (health < 0)
                        return;


                    var damage = Rdamage(rTarget);

                    if (damage >= health && Player.Spellbook.GetSpell(SpellSlot.R).Name == "LucianR")
                        R.CastIfHitchanceMinimum(rTarget, HitChance.Medium);
                }
                else if (Player.CountEnemyHeroesInRange(Player.GetRealAutoAttackRange() + 300) == 1)
                {
                    if (!target.IsFacing(Player) || !target.IsMoving ||
                        target.Distance(Player) < 200 ||
                        Player.Spellbook.GetSpell(SpellSlot.R).Name != "LucianR")
                        return;

                    R.CastIfHitchanceMinimum(target, HitChance.High);
                }
            }
        }

        private static void CastR()
        {
            var rTarget = TargetSelector.GetTarget(R.Range - 100, DamageType.Physical);

            if (rTarget == null || rTarget.HasBuffOfType(BuffType.Invulnerability))
                return;
            var rPrediciton = R.GetPrediction(rTarget);
            if (rPrediciton.Hitchance >= HitChance.Medium) R.Cast(rPrediciton.UnitPosition);
        }

        private static void ELogics()
        {
            if (!E.IsReady() || IsCastingR || HasPassiveBuff || HasSheenBuff)
                return;

            if (Helper.CanAttackAnyHero)
                return;

            var heroClient = TargetSelector.GetTarget(Player.GetRealAutoAttackRange() + 470, DamageType.Physical);

            if (heroClient == null)
                return;

            if (Helper.CanAttackAnyHero && !Q.IsReady() && heroClient.Health + heroClient.AllShield >=
                Player.GetAutoAttackDamage(heroClient) * 5)
                return;

            if (IsCastingQ && !PossibleToInterruptQ(heroClient))
                return;

            var castTime = Player.Spellbook.CastTime - Variables.GameTimeTickCount;

            if (Helper.CanAttackAnyHero && castTime > 0)
                return;

            var shortEPosition = Player.Position.Extend(Game.CursorPos, 70);

            if (Q.IsReady() && Helper.CanAttackAnyHero && shortEPosition.IsUnderEnemyTurret())
                return;

            if ((
                    GetComboDamage(heroClient, 4) >= heroClient.Health + heroClient.AllShield &&
                    Player.CountEnemyHeroesInRange(1300) <= 2 ||
                    Player.CountEnemyHeroesInRange(1300) <= 1
                ) && IsInRange(Player.Position, heroClient.Position, Player.GetRealAutoAttackRange() - 70) &&
                shortEPosition.Distance(heroClient) > 400)
            {
                E.Cast(shortEPosition);
                Orbwalker.ResetAutoAttackTimer();
                return;
            }

            var damage = GetComboDamage(heroClient, 2);
            var pos = Game.CursorPos.Distance(Player) > 470
                ? Player.Position.Extend(Game.CursorPos, 470)
                : Game.CursorPos;
            var enemiesInPosition = pos.CountEnemyHeroesInRange(335);

            if (Helper.CanAttackAnyHero && (damage < heroClient.Health + heroClient.AllShield ||
                                            !PossibleEqCombo(heroClient) ||
                                            enemiesInPosition <= 0 || enemiesInPosition >= 3))
                return;

            var enemies = Player.CountEnemyHeroesInRange(1300);

            if (!pos.IsUnderEnemyTurret())
            {
                if (enemies == 1)
                {
                    var isInRange = IsInRange(pos, heroClient.Position, heroClient.IsMelee ? 500 : 300);
                    if (!isInRange ||
                        damage >= heroClient.Health + heroClient.AllShield &&
                        EnemiesInDirectionOfTheDash(pos, 2000).Any(x => x.Equals(heroClient)) ||
                        !Player.IsFacing(heroClient))
                    {
                        if (Player.HealthPercent >= heroClient.HealthPercent &&
                            IsInRange(Player.Position, heroClient.Position, Player.GetRealAutoAttackRange()) &&
                            IsInRange(pos, heroClient.Position, Player.GetRealAutoAttackRange() - 50))
                            return;

                        E.Cast(pos);
                        return;
                    }
                }
                else if (enemies == 2 && (Player.CountAllyHeroesInRange(400) > 1 ||
                                          damage >= heroClient.Health + heroClient.AllShield &&
                                          Game.CursorPos.CountEnemyHeroesInRange(Player.GetRealAutoAttackRange()) ==
                                          1 ||
                                          !GameObjects.EnemyHeroes.Any(x => x.IsValidTarget(1200) &&
                                                                            IsInRange(pos,
                                                                                heroClient
                                                                                    .Position,
                                                                                x.IsMelee
                                                                                    ? 500
                                                                                    : x.GetRealAutoAttackRange()))))

                {
                    E.Cast(pos);
                    return;
                }
                else
                {
                    var range = enemies * 150;

                    if (!GameObjects.EnemyHeroes.Any(x => IsInRange(pos,
                        heroClient.Position,
                        range < x.GetRealAutoAttackRange() ? x.GetRealAutoAttackRange() : range)))
                    {
                        E.Cast(pos);
                        return;
                    }
                }
            }

            var closest = GameObjects.EnemyHeroes.Where(x => x.IsValidTarget(1300)).OrderBy(x => x.Distance(Player))
                .FirstOrDefault();
            var paths = GameObjects.EnemyHeroes.Where(x => x.IsValidTarget(1300))
                .Count(x => x.IsFacing(Player));
            var validEscapeDash = pos.Distance(closest) > Player.Distance(closest) && pos.Distance(Player) >= 450;

            if (closest != null && Player.CountEnemyHeroesInRange(350) >= 1 && paths >= 1 && validEscapeDash)
                E.Cast(pos);
        }


        private static bool PossibleToInterruptQ(AIHeroClient target)
        {
            if (target == null)
                return false;

            return IsCastingQ && E.IsReady() && Player.Mana >= EMana &&
                   target.Health + target.AllShield <= GetComboDamage(target, 2) &&
                   target.InAutoAttackRange();
        }

        private static bool PossibleEqCombo(AIHeroClient target)
        {
            if (target == null)
                return false;

            return Q.IsReady() && E.IsReady() && Player.Mana >= QMana + EMana && !HasPassiveBuff;
        }

        private static IEnumerable<AIHeroClient> EnemiesInDirectionOfTheDash(Vector3 dashEndPosition,
            float maxRangeToEnemy)
        {
            return
                from enemy in
                    GameObjects.EnemyHeroes.Where(x => x.IsValidTarget(maxRangeToEnemy))
                let dotProduct = (dashEndPosition - Player.Position).Normalized()
                    .ToVector2()
                    .CrossProduct(enemy.Position.ToVector2().Normalized())
                where dotProduct >= .65
                select enemy;
        }

        private static bool IsInRange(Vector3 pos, Vector3 target, float range)
        {
            return pos.Distance(target) < range;
        }

        private static void CastQ()
        {
            var target = TargetSelector.GetTarget(Q2.Range,DamageType.Physical);
            if (target == null) return;
            if (!Q.IsReady() || target.IsDead || !target.IsValid || target.HasBuffOfType(BuffType.SpellShield)) return;
            if (Q.IsInRange(target))
            {
                Q.CastOnUnit(target);
            }
            else if (Q2.IsInRange(target) && UseQmaxRange)
            {
                var collisions =
                    GameObjects.EnemyMinions.Where(x =>
                            x.IsValidTarget(Q.Range) && !x.IsDead && x.IsValid)
                        .OrderBy(x => x.Health)
                        .ToList();

                if (!collisions.Any()) return;

                foreach (var minion in from minion in collisions
                    let qPred = Q2.GetPrediction(target)
                    let qPloygon =
                        new Geometry.Rectangle(Player.Position,
                            Player.Position.Extend(minion.Position, Q2.Range), Q2.Width)
                    where qPloygon.IsInside(qPred.UnitPosition.ToVector2()) && minion.IsValidTarget(Q.Range)
                    select minion)
                    Q.CastOnUnit(minion);
            }
        }

        private static float AllDamage(AIHeroClient target)
        {
            float Damage = 0;
            if (target == null) return 0;
            if (target.HasBuffOfType(BuffType.Invulnerability)) return 0;
            Damage += (float) Player.GetAutoAttackDamage(target);
            if (Q.IsReady()) Damage += Q.GetDamage(target);
            if (W.IsReady()) Damage += W.GetDamage(target);
            if (R.IsReady()) Damage += Rdamage(target);
            if (Player.GetBuffCount("itemmagicshankcharge") == 100)
                Damage += (float) Player.CalculateMagicDamage(target, 100 + 0.1 * Player.TotalMagicalDamage);
            if (Helper.Ignite) Damage += (float) Player.GetSummonerSpellDamage(target, SummonerSpell.Ignite);
            return Damage;
        }


        private static float Rdamage(AIHeroClient target)
        {
            if (R.Level < 1) return 0;
            var ad = Player.TotalAttackDamage * 0.25;
            var bulletdamage = new[] {20 + ad, 40 + ad, 60 + ad}[R.Level - 1];
            var shots = new[] {22, 28, 34}[R.Level - 1];
            var total = bulletdamage * shots;
            return (float) Player.CalculatePhysicalDamage(target, total);
        }
    }
}