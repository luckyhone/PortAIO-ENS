using System;
using System.Collections.Generic;
using System.Linq;
using EnsoulSharp;
using EnsoulSharp.SDK;
using EnsoulSharp.SDK.MenuUI;
using LeagueSharpCommon;
using SPrediction;

namespace ElSinged
{
    internal enum Spells
    {
        Q,

        W,

        E,

        R
    }

    internal class Singed
    {
        #region Static Fields


        public static float poisonTime;

        public static Dictionary<Spells, Spell> spells = new Dictionary<Spells, Spell>()
                                                             {
                                                                 { Spells.Q, new Spell(SpellSlot.Q, 0) },
                                                                 { Spells.W, new Spell(SpellSlot.W, 1000) },
                                                                 { Spells.E, new Spell(SpellSlot.E, 125) },
                                                                 //Orbwalking.GetRealAutoAttackRange(Player) + 100)
                                                                 { Spells.R, new Spell(SpellSlot.R, 0) }
                                                             };

        private static SpellSlot ignite;

        private static bool posionActivation = false;

        private static bool useQAgain;

        #endregion

        #region Public Properties

        public static AIHeroClient Player
        {
            get
            {
                return ObjectManager.Player;
            }
        }

        #endregion

        #region Public Methods and Operators

        public static void Game_OnGameLoad()
        {
            if (ObjectManager.Player.CharacterName != "Singed")
            {
                return;
            }

            ignite = Player.GetSpellSlot("summonerdot");

            spells[Spells.W].SetSkillshot(0.5f, 350, 700, false, SpellType.Circle);

            useQAgain = true;

            ElSingedMenu.Initialize();
            Game.OnUpdate += OnGameUpdate;
            Orbwalker.OnBeforeAttack += OrbwalkingBeforeAttack;
            Drawing.OnDraw += Drawings.Drawing_OnDraw;
        }
        
        #endregion

        #region Methods

        private static void CastQ()
        {
            if (spells[Spells.Q].Instance.ToggleState == (SpellToggleState)1 && Environment.TickCount - poisonTime > 1200)
            {
                poisonTime = Environment.TickCount + 1200;
                spells[Spells.Q].Cast();
            }
            if (spells[Spells.Q].Instance.ToggleState == (SpellToggleState)2)
            {
                poisonTime = Environment.TickCount + 1200;
            }
        }

        private static void Combo()
        {
            var target = TargetSelector.GetTarget(spells[Spells.W].Range, DamageType.Magical);
            if (target == null)
            {
                return;
            }

            var comboCount = ElSingedMenu.Menu["ElSinged.Combo.R.Count"].GetValue<MenuSlider>().Value;

            var qTarget =
                HeroManager.Enemies.FirstOrDefault(
                    enemy => enemy.IsValidTarget() && enemy.Distance(Player) < 200 && Player.IsMoving && enemy.IsMoving);

            if (MenuReady("ElSinged.Combo.Q") && spells[Spells.Q].IsReady()
                && (qTarget != null || target.HasBuff("poisontrailtarget") || Player.Distance(target) <= 500))
            {
                CastQ();
            }

            if (MenuReady("ElSinged.Combo.W") && target.IsValidTarget(spells[Spells.W].Range)
                && spells[Spells.W].IsReady())
            {
                var pred = spells[Spells.W].GetPrediction(target);
                if (spells[Spells.W].Range - 80 > pred.CastPosition.Distance(Player.Position)
                    && pred.Hitchance >= HitChance.High)
                {
                    spells[Spells.W].Cast(pred.CastPosition);
                }
            }

            if (MenuReady("ElSinged.Combo.E") && spells[Spells.E].IsReady())
            {
                spells[Spells.E].CastOnUnit(target);
            }

            if (MenuReady("ElSinged.Combo.R") && Player.CountEnemyHeroesInRange(spells[Spells.W].Range) >= comboCount)
            {
                spells[Spells.R].Cast();
            }

            if (MenuReady("ElSinged.Combo.Ignite") && Player.Distance(target) <= 600
                && IgniteDamage(target) >= target.Health)
            {
                Player.Spellbook.CastSpell(ignite, target);
            }
        }

        private static void Harass()
        {
            var target = TargetSelector.GetTarget(spells[Spells.W].Range, DamageType.Magical);
            if (target == null)
            {
                return;
            }

            var qTarget =
                HeroManager.Enemies.FirstOrDefault(
                    enemy => enemy.IsValidTarget() && enemy.Distance(Player) < 200 && Player.IsMoving && enemy.IsMoving);

            if (MenuReady("ElSinged.Harass.Q") && spells[Spells.Q].IsReady()
                && (qTarget != null || target.HasBuff("poisontrailtarget") || Player.Distance(target) <= 500))
            {
                CastQ();
            }

      
                if (MenuReady("ElSinged.Harass.W") && target.IsValidTarget(spells[Spells.W].Range)
                    && spells[Spells.W].IsReady())
                {
                    var pred = spells[Spells.W].GetPrediction(target);
                    if (spells[Spells.W].Range - 80 > pred.CastPosition.Distance(Player.Position)
                        && pred.Hitchance >= HitChance.High)
                    {
                        spells[Spells.W].Cast(pred.CastPosition);
                    }
                }

                if (MenuReady("ElSinged.Harass.E") && spells[Spells.E].IsReady())
                {
                    spells[Spells.E].CastOnUnit(target);
                }

                if (MenuReady("ElSinged.Harass.W"))
                {
                    var pred = spells[Spells.W].GetPrediction(target);
                    if (spells[Spells.W].Range - 80 > pred.CastPosition.Distance(Player.Position)
                        && pred.Hitchance >= HitChance.High)
                    {
                        spells[Spells.W].Cast(pred.CastPosition);
                    }
                }
            
        }

        private static float IgniteDamage(AIHeroClient target)
        {
            if (ignite == SpellSlot.Unknown || Player.Spellbook.CanUseSpell(ignite) != SpellState.Ready)
            {
                return 0f;
            }
            return (float)Player.GetSummonerSpellDamage(target, SummonerSpell.Ignite);
        }

        private static void LaneClear()
        {

            var minion = MinionManager.GetMinions(ObjectManager.Player.Position, 400).FirstOrDefault();
            if (minion == null)
            {
                return;
            }

            if (MenuReady("ElSinged.Laneclear.E") && spells[Spells.E].GetDamage(minion) > minion.Health && minion.IsValidTarget(spells[Spells.E].Range))
            {
                spells[Spells.E].CastOnUnit(minion);
            }

            if (MenuReady("ElSinged.Laneclear.Q") && spells[Spells.Q].IsReady())
            {
                spells[Spells.Q].Cast();
            }
        }

        private static bool MenuReady(string menuName)
        {
            return ElSingedMenu.Menu[menuName].GetValue<MenuBool>().Enabled;
        }

        private static void OnGameUpdate(EventArgs args)
        {
            switch (Orbwalker.ActiveMode)
            {
                case OrbwalkerMode.Combo:
                    //TurnOffQ();
                    Combo();
                    break;
                case OrbwalkerMode.LaneClear:
                    //TurnOffQ();
                    LaneClear();
                    break;
                case OrbwalkerMode.Harass:
                    TurnOffQ();
                    Harass();
                    break;

                case OrbwalkerMode.None:
                    TurnOffQ();
                    break;
            }
        }

        private static void OrbwalkingBeforeAttack(object sender, BeforeAttackEventArgs args)
        {
            if (Orbwalker.ActiveMode == OrbwalkerMode.Combo)
            {
                if (spells[Spells.E].IsReady() && args.Target.IsValidTarget(spells[Spells.E].Range))
                {
                    args.Process = false;
                    spells[Spells.E].Cast();
                }
            }
        }

        private static void TurnOffQ()
        {
            if (spells[Spells.Q].Instance.ToggleState == (SpellToggleState)2 && Environment.TickCount - poisonTime > 1200)
            {
                spells[Spells.Q].Cast();
            }
        }

        #endregion
    }
}