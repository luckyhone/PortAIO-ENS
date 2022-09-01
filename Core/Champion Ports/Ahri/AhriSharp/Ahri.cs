using System;
using System.Collections.Generic;
using System.Linq;
using EnsoulSharp;
using EnsoulSharp.SDK;
using EnsoulSharp.SDK.MenuUI;
using EnsoulSharp.SDK.Rendering;
using EzAIO.Champions.Jinx.Modes;
using SharpDX;
using SPrediction;
using Prediction = EnsoulSharp.SDK.Prediction;

namespace AhriSharp
{
    internal class Ahri
    {
        private Menu _menu;

        private Spell _spellQ, _spellW, _spellE, _spellR;

        const float _spellQSpeed = 2600;
        const float _spellQSpeedMin = 400;
        const float _spellQFarmSpeed = 1600;
        const float _spellQAcceleration = -3200;

        public Ahri()
        {
            if(ObjectManager.Player.CharacterName != "Ahri") 
                return;
            
            (_menu = new Menu("AhriSharp", "AhriSharp", true)).Attach();
            
            var comboMenu = _menu.Add(new Menu("Combo", "Combo"));
            comboMenu.Add(new MenuBool("comboQ", "Use Q").SetValue(true));
            comboMenu.Add(new MenuBool("comboW", "Use W").SetValue(true));
            comboMenu.Add(new MenuBool("comboE", "Use E").SetValue(true));
            comboMenu.Add(new MenuBool("comboR", "Use R").SetValue(true));
            comboMenu.Add(new MenuBool("comboROnlyUserInitiate", "Use R only if user initiated").SetValue(false));

            var harassMenu = _menu.Add(new Menu("Harass", "Harass"));
            harassMenu.Add(new MenuBool("harassQ", "Use Q").SetValue(true));
            harassMenu.Add(new MenuBool("harassE", "Use E").SetValue(true));
            harassMenu.Add(new MenuSlider("harassPercent", "Skills until Mana %",20));

            var farmMenu = _menu.Add(new Menu("Lane Clear", "LaneClear"));
            farmMenu.Add(new MenuBool("farmQ", "Use Q").SetValue(true));
            farmMenu.Add(new MenuBool("farmW", "Use W").SetValue(false));
            farmMenu.Add(new MenuSlider("farmPercent", "Skills until Mana %",20));
            farmMenu.Add(new MenuSlider("farmStartAtLevel", "Only AA until Level",8,1,18));
            
            var drawMenu = _menu.Add(new Menu("Drawing", "Drawing"));
            drawMenu.Add(new MenuBool("drawQ", "Draw Q range")); // Green
            drawMenu.Add(new MenuBool("drawE", "Draw E range")); // Purple
            drawMenu.Add(new MenuBool("drawW", "Draw W range")); // Blue
            var dmgAfterComboItem = new MenuBool("DamageAfterCombo", "Draw Combo Damage").SetValue(true); //copied from esk0r Syndra
            drawMenu.Add(dmgAfterComboItem);
            
            var miscMenu = _menu.Add(new Menu("Misc", "Misc"));
            miscMenu.Add(new MenuBool("autoE", "Auto E on gapclosing targets").SetValue(true));
            miscMenu.Add(new MenuBool("autoEI", "Auto E to interrupt").SetValue(true));


            _spellQ = new Spell(SpellSlot.Q, 880);
            _spellW = new Spell(SpellSlot.W, 700);
            _spellE = new Spell(SpellSlot.E, 975);
            _spellR = new Spell(SpellSlot.R, 1000 - 100);

            _spellQ.SetSkillshot(0.25f, 50, 1600f, false, SpellType.Line);
            _spellW.SetSkillshot(0.70f, _spellW.Range, float.MaxValue, false, SpellType.Circle);
            _spellE.SetSkillshot(0.25f, 60, 1550f, true, SpellType.Line);

            AntiGapcloser.OnGapcloser += AntiGapcloser_OnEnemyGapcloser;
            Interrupter.OnInterrupterSpell += Interrupter2_OnInterruptableTarget;
            Drawing.OnEndScene += Drawing_OnDraw;
            Game.OnUpdate += Game_OnUpdate;
            
            Game.Print("<font color=\"#1eff00\">AhriSharp by Beaving</font> - <font color=\"#00BFFF\">Loaded</font>");
        }

        private void AntiGapcloser_OnEnemyGapcloser(AIHeroClient sender, AntiGapcloser.GapcloserArgs args)
        {
            if (!_menu.GetValue<MenuBool>("autoE").Enabled) return;
            if (ObjectManager.Player.Distance(sender) < _spellE.Range * _spellE.Range)
            {
                _spellE.Cast(sender);
            }
        }

        private void Interrupter2_OnInterruptableTarget(AIHeroClient sender, Interrupter.InterruptSpellArgs args)
        {
            if (!_menu.GetValue<MenuBool>("autoEI").Enabled) return;

            if (ObjectManager.Player.Distance(sender) < _spellE.Range * _spellE.Range)
            {
                _spellE.Cast(sender);
            }
        }

        private void Game_OnUpdate(EventArgs args)
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
                    break;
                default:
                    break;
            }
        }

        void Harass()
        {
            if (_menu.GetValue<MenuBool>("harassE").Enabled && ObjectManager.Player.ManaPercent >= _menu.GetValue<MenuSlider>("harassPercent").Value)
                CastE();

            if (_menu.GetValue<MenuBool>("harassQ").Enabled && ObjectManager.Player.ManaPercent >= _menu.GetValue<MenuSlider>("harassPercent").Value)
                CastQ();
        }
        private void LaneClear()
        {
            _spellQ.Speed = _spellQFarmSpeed;
            var minions = MinionManager.GetMinions(ObjectManager.Player.Position, _spellQ.Range,
                MinionManager.MinionTypes.All, MinionManager.MinionTeam.NotAlly);

            bool jungleMobs = minions.Any(x => x.Team == GameObjectTeam.Neutral);

            if ((_menu.GetValue<MenuBool>("farmQ").Enabled && ObjectManager.Player.ManaPercent >= _menu.GetValue<MenuSlider>("farmPercent").Value && ObjectManager.Player.Level >= _menu.GetValue<MenuSlider>("farmStartAtLevel").Value) || jungleMobs)
            {
                var farmLocation = _spellQ.GetLineFarmLocation(minions);

                if (farmLocation.Position.IsValid())
                    if (farmLocation.MinionsHit >= 2 || jungleMobs)
                        CastQ(farmLocation.Position);
            }

            minions = MinionManager.GetMinions(ObjectManager.Player.Position, _spellW.Range, MinionManager.MinionTypes.All, MinionManager.MinionTeam.NotAlly);

            if (minions.Count() > 0)
            {
                jungleMobs = minions.Any(x => x.Team == GameObjectTeam.Neutral);

                if ((_menu.GetValue<MenuBool>("farmW").Enabled && ObjectManager.Player.ManaPercent >= _menu.GetValue<MenuSlider>("farmPercent").Value && ObjectManager.Player.Level >= _menu.GetValue<MenuSlider>("farmStartAtLevel").Value) || jungleMobs)
                    CastW(true);
            }
        }

        bool CastE()
        {
            if (!_spellE.IsReady())
            {
                return false;
            }

            var target = TargetSelector.GetTarget(_spellE.Range, DamageType.Magical);

            if (target != null)
            {
                return _spellE.Cast(target) == CastStates.SuccessfullyCasted;
            }

            return false;
        }
        
        void CastQ()
        {
            if (!_spellQ.IsReady())
            {
                return;    
            }    

            var target = TargetSelector.GetTarget(_spellQ.Range, DamageType.Magical);

            if (target != null)
            {
                var predictedPos = Prediction.GetPrediction(target, _spellQ.Delay * 1.5f); //correct pos currently not possible with spell acceleration
                if (predictedPos.Hitchance >= HitChance.High)
                {
                    _spellQ.Speed = GetDynamicQSpeed(ObjectManager.Player.Distance(predictedPos.UnitPosition));
                    if (_spellQ.Speed > 0f)
                    {
                        _spellQ.Cast(target);
                    }  
                }
            }
        }
        void CastQ(Vector2 pos)
        {
            if (!_spellQ.IsReady())
                return;

            _spellQ.Cast(pos);
        }
        void CastW(bool ignoreTargetCheck = false)
        {
            if (!_spellW.IsReady())
            {
                return;    
            }  

            var target = TargetSelector.GetTarget(_spellW.Range, DamageType.Magical);

            if (target != null || ignoreTargetCheck)
            {
                _spellW.CastOnUnit(ObjectManager.Player);    
            }   
        }

        void Combo()
        {
            if (_menu.GetValue<MenuBool>("comboE").Enabled)
            {
                if (CastE())
                {
                    return;
                }
            }
            if (_menu.GetValue<MenuBool>("comboQ").Enabled)
            {
                CastQ();    
            }


            if (_menu.GetValue<MenuBool>("comboW").Enabled)
            {
                CastW();    
            }


            if (_menu.GetValue<MenuBool>("comboR").Enabled && _spellR.IsReady())
            {
                if (OkToUlt())
                {
                    _spellR.Cast(Game.CursorPos);      
                }   
            }
        }
        List<SpellSlot> GetSpellCombo()
        {
            var spellCombo = new List<SpellSlot>();

            if (_spellQ.IsReady())
                spellCombo.Add(SpellSlot.Q);
            if (_spellW.IsReady())
                spellCombo.Add(SpellSlot.W);
            if (_spellE.IsReady())
                spellCombo.Add(SpellSlot.E);
            if (_spellR.IsReady())
                spellCombo.Add(SpellSlot.R);
            return spellCombo;
        }
        float GetComboDamage(AIBaseClient target)
        {
            double comboDamage = 0;
            if (_spellQ.IsReady()) comboDamage += _spellQ.GetDamage(target);
            if (_spellW.IsReady()) comboDamage += _spellW.GetDamage(target);
            if (_spellE.IsReady()) comboDamage += _spellE.GetDamage(target);
            if (_spellR.IsReady()) comboDamage += _spellR.GetDamage(target);
            

            return (float)(comboDamage + ObjectManager.Player.GetAutoAttackDamage(target));
        }

        bool OkToUlt()
        {
            if (Program.Helper.EnemyTeam.Any(x => x.Distance(ObjectManager.Player) < 500)) //any enemies around me?
                return true;
            Vector3 mousePos = Game.CursorPos;

            var enemiesNearMouse = Program.Helper.EnemyTeam.Where(x => x.Distance(ObjectManager.Player) < _spellR.Range && x.Distance(mousePos) < 650);

            if (enemiesNearMouse.Count() > 0)
            {
                if (IsRActive()) //R already active
                    return true;
                bool enoughMana = ObjectManager.Player.Mana >
                                  ObjectManager.Player.Spellbook.GetSpell(SpellSlot.Q).ManaCost +
                                  ObjectManager.Player.Spellbook.GetSpell(SpellSlot.E).ManaCost +
                                  ObjectManager.Player.Spellbook.GetSpell(SpellSlot.R).ManaCost;

                if (_menu.GetValue<MenuBool>("comboROnlyUserInitiate").Enabled ||
                    !(_spellQ.IsReady() && _spellE.IsReady()) ||
                    !enoughMana) //dont initiate if user doesnt want to, also dont initiate if Q and E isnt ready or not enough mana for QER combo
                    return false;
                var friendsNearMouse = Program.Helper.OwnTeam.Where(x => x.IsMe || x.Distance(mousePos) < 650); //me and friends near mouse (already in fight)

                if (enemiesNearMouse.Count() == 1) //x vs 1 enemy
                {
                    AIHeroClient enemy = enemiesNearMouse.FirstOrDefault();

                    bool underTower = enemy.IsUnderEnemyTurret();

                    return GetComboDamage(enemy) / enemy.Health >= (underTower ? 1.25f : 1); //if enemy under tower, only initiate if combo damage is >125% of enemy health
                }
                else //fight if enemies low health or 2 friends vs 3 enemies and 3 friends vs 3 enemies, but not 2vs4
                {
                    int lowHealthEnemies = enemiesNearMouse.Count(x => x.Health / x.MaxHealth <= 0.1); //dont count low health enemies

                    float totalEnemyHealth = enemiesNearMouse.Sum(x => x.Health);

                    return friendsNearMouse.Count() - (enemiesNearMouse.Count() - lowHealthEnemies) >= -1 || ObjectManager.Player.Health / totalEnemyHealth >= 0.8;
                }
            }

            return false;
        }
        
        private void Drawing_OnDraw(EventArgs args)
        {
            if (!ObjectManager.Player.IsDead)
            {
                var drawQ = _menu.GetValue<MenuBool>("drawQ").Enabled;
                var drawW = _menu.GetValue<MenuBool>("drawW").Enabled;
                var drawE = _menu.GetValue<MenuBool>("drawE").Enabled;

                if (drawQ)
                    CircleRender.Draw(ObjectManager.Player.Position, _spellQ.Range, Color.Green);

                if (drawE)
                    CircleRender.Draw(ObjectManager.Player.Position, _spellE.Range, Color.Purple);

                if (drawW)
                    CircleRender.Draw(ObjectManager.Player.Position, _spellW.Range, Color.Blue);
            }
        }
        
        float GetDynamicQSpeed(float distance)
        {
            var a = 0.5f * _spellQAcceleration;
            var b = _spellQSpeed;
            var c = - distance;

            if (b * b - 4 * a * c <= 0f)
            {
                return 0;    
            }

            var t = (float) (-b + Math.Sqrt(b * b - 4 * a * c)) / (2 * a);
            return distance / t;
        }
        bool IsRActive()
        {
            return ObjectManager.Player.HasBuff("AhriTumble");
        }

        int GetRStacks()
        {
            return ObjectManager.Player.GetBuffCount("AhriTumble");
        }
    }
}