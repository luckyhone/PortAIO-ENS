using System;
using System.Collections.Generic;
using System.Linq;
using EnsoulSharp;
using EnsoulSharp.SDK;
using EnsoulSharp.SDK.MenuUI;
using EnsoulSharp.SDK.Rendering;
using SharpDX;
using SPrediction;

namespace DZAhri
{
    class DZAhri
    {
        public static Menu Menu;
        public static readonly Dictionary<SpellSlot, Spell> _spells = new Dictionary<SpellSlot, Spell>
        {
            { SpellSlot.Q, new Spell(SpellSlot.Q, 925f) },
            { SpellSlot.W, new Spell(SpellSlot.W, 700f) },
            { SpellSlot.E, new Spell(SpellSlot.E, 875f) },
            { SpellSlot.R, new Spell(SpellSlot.R, 400f) }
        };
        private delegate void OnOrbwalkingMode();
        private static Dictionary<OrbwalkerMode, OnOrbwalkingMode> _orbwalkingModesDictionary;

        public static void OnLoad()
        {
            if (ObjectManager.Player.CharacterName != "Ahri")
            {
                return;
            }
            _orbwalkingModesDictionary = new Dictionary<OrbwalkerMode, OnOrbwalkingMode>
            {
                { OrbwalkerMode.Combo, Combo },
                { OrbwalkerMode.Harass, Harass },
                { OrbwalkerMode.LastHit, LastHit },
                { OrbwalkerMode.LaneClear, Laneclear },
                { OrbwalkerMode.None, () => { } }
            };
            SetUpMenu();
            SetUpSpells();
            SetUpEvents();
        }
        
        #region Mode Menu

        private static void Combo()
        {
            if (ObjectManager.Player.ManaPercent < Menu.GetValue<MenuSlider>("dz191.ahri.combo.mana").Value || ObjectManager.Player.IsDead)
            {
                return;
            }
            var comboTarget = TargetSelector.GetTarget(_spells[SpellSlot.E].Range, DamageType.Magical);
            var charmedUnit = GameObjects.EnemyHeroes.Find(h => h.HasBuffOfType(BuffType.Charm) && h.IsValidTarget(_spells[SpellSlot.Q].Range));
            AIHeroClient target = comboTarget;
            
            if (charmedUnit != null)
            {
                target = charmedUnit;
            }

            if (target.IsValidTarget())
            {
                switch (Menu.GetValue<MenuList>("dz191.ahri.combo.mode").Index)
                {
                    case 0:
                        if (!target.IsCharmed() && Helpers.IsMenuEnabled("dz191.ahri.combo.usee") && _spells[SpellSlot.E].IsReady() && _spells[SpellSlot.Q].IsReady())
                        {
                            _spells[SpellSlot.E].CastIfHitchanceEquals(target, HitChance.High);
                        }
                        if (Helpers.IsMenuEnabled("dz191.ahri.combo.useq") && _spells[SpellSlot.Q].IsReady() && (!_spells[SpellSlot.E].IsReady() || ObjectManager.Player.ManaPercent <= 25))
                        {
                            _spells[SpellSlot.Q].CastIfHitchanceEquals(target, HitChance.High);
                        }
                        if (Helpers.IsMenuEnabled("dz191.ahri.combo.usew") && _spells[SpellSlot.W].IsReady() && ObjectManager.Player.Distance(target) <= _spells[SpellSlot.W].Range && (target.IsCharmed() || (_spells[SpellSlot.W].GetDamage(target) + _spells[SpellSlot.Q].GetDamage(target) > target.Health + 25)))
                        {
                            _spells[SpellSlot.W].Cast();
                        }
                        break;
                    case 1:
                        if (!target.IsCharmed() && Helpers.IsMenuEnabled("dz191.ahri.combo.usee") && _spells[SpellSlot.E].IsReady() && _spells[SpellSlot.Q].IsReady())
                        {
                            _spells[SpellSlot.E].CastIfHitchanceEquals(target, HitChance.High);
                        }
                        if (Helpers.IsMenuEnabled("dz191.ahri.combo.useq") && _spells[SpellSlot.Q].IsReady())
                        {
                            _spells[SpellSlot.Q].CastIfHitchanceEquals(target, HitChance.High);
                        }
                        if (Helpers.IsMenuEnabled("dz191.ahri.combo.usew") && _spells[SpellSlot.W].IsReady() && ObjectManager.Player.Distance(target) <= _spells[SpellSlot.W].Range && ((_spells[SpellSlot.W].GetDamage(target) + _spells[SpellSlot.Q].GetDamage(target) > target.Health + 25)))
                        {
                            _spells[SpellSlot.W].Cast();
                        }
                        break;
                }
                HandleRCombo(target);
            }
        }
        
        private static void Harass()
        {
            if (ObjectManager.Player.ManaPercent < Menu.GetValue<MenuSlider>("dz191.ahri.harass.mana").Value || ObjectManager.Player.IsDead)
            {
                return;
            }
            var comboTarget = TargetSelector.GetTarget(_spells[SpellSlot.E].Range, DamageType.Magical);
            var charmedUnit = GameObjects.EnemyHeroes.Find(h => h.IsCharmed() && h.IsValidTarget(_spells[SpellSlot.Q].Range));
            AIHeroClient target = comboTarget;
            if (charmedUnit != null)
            {
                target = charmedUnit;
            }
            if (target.IsValidTarget())
            {
                if (!target.IsCharmed() && Helpers.IsMenuEnabled("dz191.ahri.harass.usee") && _spells[SpellSlot.E].IsReady() && _spells[SpellSlot.Q].IsReady())
                {
                    _spells[SpellSlot.E].CastIfHitchanceEquals(target, HitChance.High);
                }
                if (Helpers.IsMenuEnabled("dz191.ahri.harass.useq") && _spells[SpellSlot.Q].IsReady())
                {
                    if (Helpers.IsMenuEnabled("dz191.ahri.harass.onlyqcharm") && !target.IsCharmed())
                    {
                        return;
                    }
                    _spells[SpellSlot.Q].CastIfHitchanceEquals(target, HitChance.High);
                }
                if (Helpers.IsMenuEnabled("dz191.ahri.harass.usew") && _spells[SpellSlot.W].IsReady() && ObjectManager.Player.Distance(target) <= _spells[SpellSlot.W].Range)
                {
                    _spells[SpellSlot.W].Cast();
                }
            }
        }
        
        private static void LastHit()
        {
            if (ObjectManager.Player.ManaPercent < Menu.GetValue<MenuSlider>("dz191.ahri.farm.mana").Value)
            {
                return;
            }
            if (Helpers.IsMenuEnabled("dz191.ahri.farm.qlh"))
            {
                var minionInQ = MinionManager.GetMinions(ObjectManager.Player.Position, _spells[SpellSlot.Q].Range).FindAll(m => _spells[SpellSlot.Q].GetDamage(m) >= m.Health).ToList();
                var killableMinions = _spells[SpellSlot.Q].GetLineFarmLocation(minionInQ);
                if (killableMinions.MinionsHit > 0)
                {
                    _spells[SpellSlot.Q].Cast(killableMinions.Position);
                }
            }
        }

        private static void Laneclear()
        {
            if (ObjectManager.Player.ManaPercent < Menu.GetValue<MenuSlider>("dz191.ahri.farm.mana").Value)
            {
                return;
            }
            if (Helpers.IsMenuEnabled("dz191.ahri.farm.qlc"))
            {
                var minionInQ = MinionManager.GetMinions(ObjectManager.Player.Position, _spells[SpellSlot.Q].Range);
                var killableMinions = _spells[SpellSlot.Q].GetLineFarmLocation(minionInQ);
                if (killableMinions.MinionsHit >= 3)
                {
                    _spells[SpellSlot.Q].Cast(killableMinions.Position);
                }
            }
            if (Helpers.IsMenuEnabled("dz191.ahri.farm.wlc"))
            {
                var minionInW = MinionManager.GetMinions(
                    ObjectManager.Player.Position, _spells[SpellSlot.W].Range);
                if (minionInW.Count > 0)
                {
                    _spells[SpellSlot.W].Cast();
                }
            }
        }
        
        private static void HandleRCombo(AIHeroClient target)
        {
            if (_spells[SpellSlot.R].IsReady() && Helpers.IsMenuEnabled("dz191.ahri.combo.user"))
            {
                //User chose not to initiate with R.
                if (Helpers.IsMenuEnabled("dz191.ahri.combo.initr"))
                {
                    return;
                }
                //Neither Q or E are ready in <= 2 seconds and we can't kill the enemy with 1 R stack. Don't use R
                if ((!_spells[SpellSlot.Q].IsReady(2) && !_spells[SpellSlot.E].IsReady(2)) || !(Helpers.GetComboDamage(target) >= target.Health + 20))
                {
                    return;
                }
                //Set the test position to the Cursor Position
                var testPosition = Game.CursorPos;
                //Extend from out position towards there
                var extendedPosition = ObjectManager.Player.Position.Extend(testPosition, 500f);
                //Safety checks
                if (extendedPosition.IsSafe())
                {
                    _spells[SpellSlot.R].Cast(extendedPosition);
                }
            }
        }
        
        #endregion

         #region Event delegates
         private static void Game_OnUpdate(EventArgs args)
         {
            _orbwalkingModesDictionary[Orbwalker.ActiveMode]();
            if (Helpers.IsMenuEnabled("dz191.ahri.misc.userexpire"))
            {
                var rBuff = ObjectManager.Player.Buffs.Find(buff => buff.Name == "AhriTumble");
                if (rBuff != null)
                {
                    //This tryhard tho
                    if (rBuff.EndTime - Game.Time <= 1.0f + (Game.Ping / (2f * 1000f)))
                    {
                        var extendedPosition = ObjectManager.Player.Position.Extend(Game.CursorPos, _spells[SpellSlot.R].Range);
                        if (extendedPosition.IsSafe())
                        {
                            _spells[SpellSlot.R].Cast(extendedPosition);
                        }
                    }
                }
            }
            if (Helpers.IsMenuEnabled("dz191.ahri.misc.autoq"))
            {
                var charmedUnit = GameObjects.EnemyHeroes.Find(h => h.IsCharmed() && h.IsValidTarget(_spells[SpellSlot.Q].Range));
                if (charmedUnit != null)
                {
                    _spells[SpellSlot.Q].Cast(charmedUnit);
                }
            }
            if (Helpers.IsMenuEnabled("dz191.ahri.misc.autoq2"))
            {
                var qMana = Menu.GetValue<MenuSlider>("dz191.ahri.misc.autoq2mana").Value;
                if (ObjectManager.Player.ManaPercent >= qMana && _spells[SpellSlot.Q].IsReady())
                {
                    var target = TargetSelector.GetTarget(_spells[SpellSlot.Q].Range, DamageType.Magical);
                    if (target != null && ObjectManager.Player.Distance(target) >= _spells[SpellSlot.Q].Range * 0.7f)
                    {
                        _spells[SpellSlot.Q].CastIfHitchanceEquals(target, HitChance.High);
                    }
                }
            }
            if (Menu.GetValue<MenuKeyBind>("dz191.ahri.misc.instacharm").Active && _spells[SpellSlot.E].IsReady())
            {
                var target = TargetSelector.GetTarget(_spells[SpellSlot.E].Range, DamageType.Magical);
                if (target != null)
                {
                    var prediction = _spells[SpellSlot.E].GetPrediction(target);
                    _spells[SpellSlot.E].Cast(prediction.CastPosition);
                }
            }

        }
         private static void AntiGapcloser_OnEnemyGapcloser(AIHeroClient sender, AntiGapcloser.GapcloserArgs args)

        {
            if (Helpers.IsMenuEnabled("dz191.ahri.misc.egp") && sender.IsValidTarget(_spells[SpellSlot.E].Range) && _spells[SpellSlot.E].IsReady())
            {
                _spells[SpellSlot.E].Cast(sender);
            }
            if (Helpers.IsMenuEnabled("dz191.ahri.misc.rgap") && !_spells[SpellSlot.E].IsReady() &&
                _spells[SpellSlot.R].IsReady())
            {
                _spells[SpellSlot.R].Cast(ObjectManager.Player.Position.Extend(args.EndPosition, -400f));
            }
        }
         private static void Interrupter2_OnInterruptableTarget(AIHeroClient sender, Interrupter.InterruptSpellArgs args)
         {
            if (Helpers.IsMenuEnabled("dz191.ahri.misc.eint") && args.DangerLevel >= Interrupter.DangerLevel.Medium && _spells[SpellSlot.E].IsReady())
            {
                _spells[SpellSlot.E].Cast(sender.Position);
            }
        }
        static void Drawing_OnDraw(EventArgs args)
        {
            var qItem = Menu.GetValue<MenuBool>("dz191.ahri.drawings.q").Enabled;
            var eItem = Menu.GetValue<MenuBool>("dz191.ahri.drawings.e").Enabled;
            if (qItem)
            {
                CircleRender.Draw(ObjectManager.Player.Position,_spells[SpellSlot.Q].Range,Color.Aqua);
            }
            if (eItem)
            {
                CircleRender.Draw(ObjectManager.Player.Position, _spells[SpellSlot.E].Range, Color.Aqua);
            }
        }

        #endregion

        #region Events, Spells, Menu Init
        private static void SetUpEvents()
        {
            Game.OnUpdate += Game_OnUpdate;
            AntiGapcloser.OnAllGapcloser += AntiGapcloser_OnEnemyGapcloser;
            Interrupter.OnInterrupterSpell += Interrupter2_OnInterruptableTarget;
            Drawing.OnDraw += Drawing_OnDraw;
        }

        private static void SetUpSpells()
        {
            _spells[SpellSlot.Q].SetSkillshot(0.25f, 100, 1600, false, SpellType.Line);
            _spells[SpellSlot.E].SetSkillshot(0.25f, 60, 1200, true, SpellType.Line);
        }
        private static void SetUpMenu()
        {
            Menu = new Menu("dz191.ahri","DZAhri", true);
            
            var comboMenu = new Menu("dz191.ahri.combo","[Ahri] Combo"); //Done
            {
                comboMenu.Add(new MenuBool("dz191.ahri.combo.useq", "Use Q Combo").SetValue(true));
                comboMenu.Add(new MenuBool("dz191.ahri.combo.usew", "Use W Combo").SetValue(true));
                comboMenu.Add(new MenuBool("dz191.ahri.combo.usee", "Use E Combo").SetValue(true));
                comboMenu.Add(new MenuBool("dz191.ahri.combo.user", "Use R Combo").SetValue(true));
                comboMenu.Add(new MenuBool("dz191.ahri.combo.initr", "Don't Initiate with R").SetValue(false)); //Done
                comboMenu.Add(new MenuSlider("dz191.ahri.combo.mana", "Min Combo Mana",20));
                comboMenu.Add(new MenuList("dz191.ahri.combo.mode", "Combo Mode",new []{"Wait for Charm","Don't Wait for Charm"}));
            }
            Menu.Add(comboMenu);
            
            var harassMenu = new Menu("dz191.ahri.harass","[Ahri] Harass");
            {
                harassMenu.Add(new MenuBool("dz191.ahri.harass.useq", "Use Q Harass").SetValue(true));
                harassMenu.Add(new MenuBool("dz191.ahri.harass.usew", "Use W Harass").SetValue(true));
                harassMenu.Add(new MenuBool("dz191.ahri.harass.usee", "Use E Harass").SetValue(true));
                harassMenu.Add(new MenuBool("dz191.ahri.harass.onlyqcharm", "Use Q Only when charmed").SetValue(true));
                harassMenu.Add(new MenuSlider("dz191.ahri.harass.mana", "Min Harass Mana",20));
            }
            Menu.Add(harassMenu);

            var miscMenu = new Menu("dz191.ahri.misc","[Ahri] Misc");
            {
                miscMenu.Add(new MenuBool("dz191.ahri.misc.egp", "Auto E Gapclosers").SetValue(true)); //Done
                miscMenu.Add(new MenuBool("dz191.ahri.misc.eint", "Auto E Interrupter").SetValue(true)); //Done
                miscMenu.Add(new MenuBool("dz191.ahri.misc.rgap", "R away gapclosers if E on CD").SetValue(false)); //Done
                miscMenu.Add(new MenuBool("dz191.ahri.misc.autoq", "Auto Q Charmed targets").SetValue(false)); //Done
                miscMenu.Add(new MenuBool("dz191.ahri.misc.userexpire", "Use R when about to expire").SetValue(false)); //Done
                miscMenu.Add(new MenuBool("dz191.ahri.misc.autoq2", "Auto Q poke (Long range)").SetValue(false)); //Done
                miscMenu.Add(new MenuSlider("dz191.ahri.misc.autoq2mana", "Auto Q mana",25)); //Done
                miscMenu.Add(new MenuKeyBind("dz191.ahri.misc.instacharm", "Instacharm",Keys.T,KeyBindType.Press)); //Done
            }
            Menu.Add(miscMenu);

            var farmMenu = new Menu("dz191.ahri.farm","[Ahri] Farm");
            {
                farmMenu.Add(new MenuBool("dz191.ahri.farm.qlh", "Use Q LastHit").SetValue(false));
                farmMenu.Add(new MenuBool("dz191.ahri.farm.qlc", "Use Q Laneclear").SetValue(false));
                farmMenu.Add(new MenuBool("dz191.ahri.farm.wlc", "Use W Laneclear").SetValue(false));
                farmMenu.Add(new MenuSlider("dz191.ahri.farm.mana", "Min Farm Mana",20));
            }
            Menu.Add(farmMenu);

            var drawMenu = new Menu("dz191.ahri.drawings","[Ahri] Drawing");
            {
                drawMenu.Add(new MenuBool("dz191.ahri.drawings.q", "Draw Q")); // Aqua
                drawMenu.Add(new MenuBool("dz191.ahri.drawings.e", "Draw E")); // Aqua
            }
            Menu.Add(drawMenu);

            Menu.Attach();
        }
        #endregion
    }
}