using System;
using System.Collections.Generic;
using System.Linq;
using EnsoulSharp;
using EnsoulSharp.SDK;
using EnsoulSharp.SDK.MenuUI;
using MoonLux;
using SPrediction;

namespace ElVarus
{
    
    internal enum Spells
    {
        Q,

        W,

        E,

        R
    }
    internal class Varus
    {
        #region Static Fields

        public static Dictionary<Spells, Spell> spells = new Dictionary<Spells, Spell>
        {
            { Spells.Q, new Spell(SpellSlot.Q, 925) },
            { Spells.W, new Spell(SpellSlot.W, 0) },
            { Spells.E, new Spell(SpellSlot.E, 925) },
            { Spells.R, new Spell(SpellSlot.R, 1100) }
        };

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
            if (ObjectManager.Player.CharacterName != "Varus")
            {
                return;
            }

            spells[Spells.Q].SetSkillshot(.25f, 70f, 1650f, false, SpellType.Line);
            spells[Spells.E].SetSkillshot(0.35f, 120, 1500, false, SpellType.Circle);
            spells[Spells.R].SetSkillshot(.25f, 120f, 1950f, false, SpellType.Line);

            spells[Spells.Q].SetCharged("VarusQ","VarusQ",250, 1600, 1.2f);

            ElVarusMenu.Initialize();
            Game.OnUpdate += OnGameUpdate;
            Drawing.OnDraw += Drawings.Drawing_OnDraw;
        }
        
        private static void Combo()
        {
            var target = TargetSelector.GetTarget(
                (spells[Spells.Q].ChargedMaxRange + spells[Spells.Q].Width) * 1.1f,
                DamageType.Magical);
            if (target == null)
            {
                return;
            }

            Items(target);

            if (spells[Spells.E].IsReady() && !spells[Spells.Q].IsCharging
                && ElVarusMenu.Menu["ElVarus.Combo.E"].GetValue<MenuBool>().Enabled)
            {
                if (spells[Spells.E].GetDamage(target) >= target.Health || GetWStacks(target) >= 1)
                {
                    var prediction = spells[Spells.E].GetPrediction(target);
                    if (prediction.Hitchance >= HitChance.VeryHigh)
                    {
                        spells[Spells.E].Cast(prediction.CastPosition);
                    }
                }
            }

            if (spells[Spells.Q].IsReady() && ElVarusMenu.Menu["ElVarus.Combo.Q"].GetValue<MenuBool>().Enabled)
            {
                if (spells[Spells.Q].IsCharging || ElVarusMenu.Menu["ElVarus.combo.always.Q"].GetValue<MenuBool>().Enabled
                    || target.Distance(Player) > Player.GetRealAutoAttackRange(target) * 1.2f
                    || GetWStacks(target) >= ElVarusMenu.Menu["ElVarus.Combo.Stack.Count"].GetValue<MenuSlider>().Value
                    || spells[Spells.Q].GetDamage(target) >= target.Health)
                {
                    if (!spells[Spells.Q].IsCharging)
                    {
                        spells[Spells.Q].StartCharging();
                    }

                    if (spells[Spells.Q].IsCharging)
                    {
                        var prediction = spells[Spells.Q].GetPrediction(target);
                        if (prediction.Hitchance >= HitChance.VeryHigh)
                        {
                            spells[Spells.Q].Cast(prediction.CastPosition);
                        }
                    }
                }
            }

            if (spells[Spells.R].IsReady() && !spells[Spells.Q].IsCharging
                && target.IsValidTarget(spells[Spells.R].Range) && ElVarusMenu.Menu["ElVarus.Combo.R"].GetValue<MenuBool>().Enabled)
            {
                var pred = spells[Spells.R].GetPrediction(target);
                if (pred.Hitchance >= HitChance.VeryHigh)
                {
                    var ultimateHits = GameObjects.EnemyHeroes.Where(x => x.Distance(target) <= 450f).ToList();
                    if (ultimateHits.Count >= ElVarusMenu.Menu["ElVarus.Combo.R.Count"].GetValue<MenuSlider>().Value)
                    {
                        spells[Spells.R].Cast(pred.CastPosition);
                    }
                }
            }
        }
        
        private static int GetWStacks(AIBaseClient target)
        {
            return target.GetBuffCount("varuswdebuff");
        }

        private static void Harass()
        {
            var target = TargetSelector.GetTarget(spells[Spells.Q].ChargedMaxRange, DamageType.Physical);
            if (target == null || !target.IsValidTarget())
            {
                return;
            }

            if (Player.ManaPercent > ElVarusMenu.Menu["minmanaharass"].GetValue<MenuSlider>().Value)
            {
                if (ElVarusMenu.Menu["ElVarus.Harass.E"].GetValue<MenuBool>().Enabled && spells[Spells.E].IsReady() && GetWStacks(target) >= 1)
                {
                    var prediction = spells[Spells.E].GetPrediction(target);
                    if (prediction.Hitchance >= HitChance.VeryHigh)
                    {
                        spells[Spells.E].Cast(prediction.CastPosition);
                    }
                }

                if (ElVarusMenu.Menu["ElVarus.Harass.Q"].GetValue<MenuBool>().Enabled && spells[Spells.Q].IsReady())
                {
                    if (!spells[Spells.Q].IsCharging)
                    {
                        spells[Spells.Q].StartCharging();
                    }

                    if (spells[Spells.Q].IsCharging)
                    {
                        var prediction = spells[Spells.Q].GetPrediction(target);
                        if (prediction.Hitchance >= HitChance.VeryHigh)
                        {
                            spells[Spells.Q].Cast(prediction.CastPosition);
                        }
                    }
                }
            }
        }
        
        private static void Items(AIBaseClient target)
        {
            var ghost = ItemId.Youmuus_Ghostblade;

            var useYoumuu = ElVarusMenu.Menu["ElVarus.Items.Youmuu"].GetValue<MenuBool>().Enabled;
            
            if (EnsoulSharp.SDK.Items.HasItem(Player,ghost) && target.IsValidTarget(spells[Spells.Q].Range) && useYoumuu)
            {
                EnsoulSharp.SDK.Items.UseItem(Player, (int) ghost);
            }
        }
        
        private static void JungleClear()
        {
            var useQ = ElVarusMenu.Menu["useQFarmJungle"].GetValue<MenuBool>().Enabled;
            var useE = ElVarusMenu.Menu["useEFarmJungle"].GetValue<MenuBool>().Enabled;
            var minmana = ElVarusMenu.Menu["minmanaclear"].GetValue<MenuSlider>().Value;
            var minions = MinionManager.GetMinions(
                Player.ServerPosition,
                700,
                MinionManager.MinionTypes.All,
                MinionManager.MinionTeam.Neutral,
                MinionManager.MinionOrderTypes.MaxHealth);

            if (Player.ManaPercent >= minmana)
            {
                foreach (var minion in minions)
                {
                    if (spells[Spells.Q].IsReady() && useQ)
                    {
                        if (!spells[Spells.Q].IsCharging)
                        {
                            spells[Spells.Q].StartCharging();
                        }

                        if (spells[Spells.Q].IsCharging && spells[Spells.Q].Range >= spells[Spells.Q].ChargedMaxRange)
                        {
                            spells[Spells.Q].Cast(minion);
                        }
                    }

                    if (spells[Spells.E].IsReady() && useE)
                    {
                        spells[Spells.E].CastOnUnit(minion);
                    }
                }
            }
        }
        
        //Credits to God :cat_lazy:
        private static void Killsteal()
        {
            if (ElVarusMenu.Menu["ElVarus.KSSS"].GetValue<MenuBool>().Enabled && spells[Spells.Q].IsReady())
            {
                foreach (var target in
                         GameObjects.EnemyHeroes.Where(
                             enemy =>
                                 enemy.IsValidTarget() && spells[Spells.Q].GetDamage(enemy) >= enemy.Health
                                                       && Player.Distance(enemy.Position) <= spells[Spells.Q].ChargedMaxRange))
                {
                    if (!spells[Spells.Q].IsCharging)
                    {
                        spells[Spells.Q].StartCharging();
                    }

                    if (spells[Spells.Q].IsCharging)
                    {
                        var prediction = spells[Spells.Q].GetPrediction(target);
                        if (prediction.Hitchance >= HitChance.VeryHigh)
                        {
                            spells[Spells.Q].Cast(prediction.CastPosition);
                        }
                    }
                }
            }
        }
        
        private static void LaneClear()
        {
            if (Player.ManaPercent < ElVarusMenu.Menu["minmanaclear"].GetValue<MenuSlider>().Value)
            {
                return;
            }

            var minions = MinionManager.GetMinions(Player.ServerPosition, spells[Spells.Q].Range);
            if (minions.Count <= 0)
            {
                return;
            }

            if (spells[Spells.Q].IsReady() && ElVarusMenu.Menu["useQFarm"].GetValue<MenuBool>().Enabled)
            {
                var allMinions = MinionManager.GetMinions(Player.ServerPosition, spells[Spells.Q].Range);
                {
                    foreach (var minion in
                        allMinions.Where(minion => minion.Health <= Player.GetSpellDamage(minion, SpellSlot.Q)))
                    {
                        var killcount = 0;

                        foreach (var colminion in minions)
                        {
                            if (colminion.Health <= spells[Spells.Q].GetDamage(colminion))
                            {
                                killcount++;
                            }
                            else
                            {
                                break;
                            }
                        }

                        if (killcount >= ElVarusMenu.Menu["ElVarus.Count.Minions"].GetValue<MenuSlider>().Value)
                        {
                            if (minion.IsValidTarget())
                            {
                                spells[Spells.Q].Cast(minion);
                                return;
                            }
                        }
                    }
                }
            }

            if (!ElVarusMenu.Menu["useQFarm"].GetValue<MenuBool>().Enabled || !spells[Spells.E].IsReady())
            {
                return;
            }

            var minionkillcount =
                minions.Count(x => spells[Spells.E].CanCast(x) && x.Health <= spells[Spells.E].GetDamage(x));

            if (minionkillcount >= ElVarusMenu.Menu["ElVarus.Count.Minions.E"].GetValue<MenuSlider>().Value)
            {
                foreach (var minion in minions.Where(x => x.Health <= spells[Spells.E].GetDamage(x)))
                {
                    spells[Spells.E].Cast(minion);
                }
            }
        }

        private static void OnGameUpdate(EventArgs args)
        {
            switch (Orbwalker.ActiveMode)
            {
                case OrbwalkerMode.Combo:
                    try{
                        Combo();
                    }catch(Exception) {}
                    break;
                case OrbwalkerMode.LaneClear:
                    try{
                        LaneClear();
                        JungleClear();
                    }catch(Exception) {}
                    break;
                case OrbwalkerMode.Harass:
                    try{
                        Harass();
                    }catch(Exception) {}
                    break;
            }

            Killsteal();

            var target = TargetSelector.GetTarget(spells[Spells.R].Range, DamageType.Physical);
            if (spells[Spells.R].IsReady() && target.IsValidTarget()
                                           && ElVarusMenu.Menu["ElVarus.SemiR"].GetValue<MenuKeyBind>().Active)
            {
                spells[Spells.R].CastOnUnit(target);
            }
        }

        #endregion
    }
}