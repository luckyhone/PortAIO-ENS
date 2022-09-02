using System;
using System.Collections.Generic;
using System.Linq;
using EnsoulSharp;
using EnsoulSharp.SDK;
using EnsoulSharp.SDK.MenuUI;
using EnsoulSharp.SDK.Rendering;
using SharpDX;
using SPrediction;

namespace zedisback
{
    class Program
    {
        private const string ChampionName = "Zed";
        private static List<Spell> SpellList = new List<Spell>();
        private static Spell _q, _w, _e, _r;
        public static Menu _config;
        public static Menu TargetSelectorMenu;
        private static AIHeroClient _player;
        private static SpellSlot _igniteSlot;
        private static Items.Item _tiamat, _hydra, _blade, _bilge, _rand, _lotis, _youmuu;
        private static Vector3 linepos;
        private static Vector3 castpos;
        private static int clockon;
        private static int countults;
        private static int countdanger;
        private static int ticktock;
        private static Vector3 rpos;
        private static int shadowdelay = 0;
        private static int delayw = 500;

        public static void Loads()
        {
            Game_OnGameLoad();
        }

        static void Game_OnGameLoad()
        {
            try
            {
                _player = ObjectManager.Player;
                if (ObjectManager.Player.CharacterName != ChampionName) return;
                _q = new Spell(SpellSlot.Q, 900f);
                _w = new Spell(SpellSlot.W, 700f);
                _e = new Spell(SpellSlot.E, 270f);
                _r = new Spell(SpellSlot.R, 650f);

                _q.SetSkillshot(0.25f, 50f, 1700f, false, SpellType.Line);
                
                _igniteSlot = _player.GetSpellSlot("SummonerDot");

                var enemy = from hero in ObjectManager.Get<AIHeroClient>()
                    where hero.IsEnemy == true
                    select hero;
                
                // Just menu things test
                _config = new Menu("Zed Is Back", "Zed Is Back", true);
                
                //Combo
                var comboSettings = _config.Add(new Menu("Combo", "Combo"));
                comboSettings.Add(new MenuBool("UseWC", "Use W (also gap close)")).SetValue(true);
                comboSettings.Add(new MenuBool("UseIgnitecombo", "Use Ignite(rush for it)")).SetValue(true);
                comboSettings.Add(new MenuBool("UseUlt", "Use Ultimate")).SetValue(true);
                comboSettings.Add(new MenuKeyBind("ActiveCombo", "Combo!",Keys.Space, KeyBindType.Press));
                comboSettings.Add(new MenuKeyBind("TheLine", "The Line Combo",Keys.T, KeyBindType.Press));
                
                //Harass
                var harassSettings = _config.Add(new Menu("Harass", "Harass"));
                harassSettings.Add(new MenuKeyBind("longhar", "Long Poke (toggle)",Keys.U, KeyBindType.Toggle));
                harassSettings.Add(new MenuBool("UseWH", "Use W")).SetValue(true);
                harassSettings.Add(new MenuKeyBind("ActiveHarass", "Harass!",Keys.C, KeyBindType.Press));

                // Farm
                var farmSettings = _config.Add(new Menu("Farm", "Farm"));
                // LaneClear
                var laneFarmSettings = farmSettings.Add(new Menu("LaneFarm", "LaneFarm"));
                laneFarmSettings.Add(new MenuBool("UseQL", "Q LaneClear")).SetValue(true);
                laneFarmSettings.Add(new MenuBool("UseEL", "E LaneClear")).SetValue(true);
                laneFarmSettings.Add(new MenuSlider("Energylane", "Energy Lane% >",45, 1, 100));
                laneFarmSettings.Add(new MenuKeyBind("Activelane", "Lane clear!",Keys.V, KeyBindType.Press));
                // LastHit
                var lastHitSettings = farmSettings.Add(new Menu("LastHit", "LastHit"));
                lastHitSettings.Add(new MenuBool("UseQLH", "Q LastHit")).SetValue(true);
                lastHitSettings.Add(new MenuBool("UseELH", "E LastHit")).SetValue(true);
                lastHitSettings.Add(new MenuSlider("Energylast", "Energy lasthit% >",85, 1, 100));
                lastHitSettings.Add(new MenuKeyBind("ActiveLast", "LastHit!",Keys.X, KeyBindType.Press));
                var jungleSettings = farmSettings.Add(new Menu("Jungle", "Jungle"));
                jungleSettings.Add(new MenuBool("UseQJ", "Q Jungle")).SetValue(true);
                jungleSettings.Add(new MenuBool("UseWJ", "W Jungle")).SetValue(true);
                jungleSettings.Add(new MenuBool("UseEJ", "E Jungle")).SetValue(true);
                jungleSettings.Add(new MenuSlider("Energyjungle", "Energy Jungle% >",85, 1, 100));
                jungleSettings.Add(new MenuKeyBind("Activejungle", "Jungle!",Keys.V, KeyBindType.Press));
                
                //Misc
                var miscSettings = _config.Add(new Menu("Misc", "Misc"));
                miscSettings.Add(new MenuBool("UseIgnitekill", "Use Ignite KillSteal")).SetValue(true);
                miscSettings.Add(new MenuBool("UseQM", "Use Q KillSteal")).SetValue(true);
                miscSettings.Add(new MenuBool("UseEM", "Use E KillSteal")).SetValue(true);
                miscSettings.Add(new MenuBool("AutoE", "Auto E")).SetValue(true);
                miscSettings.Add(new MenuBool("rdodge", "R Dodge Dangerous")).SetValue(true);
                miscSettings.Add(new MenuSeparator("dafdasf", " "));
                foreach (var e in enemy)
                {
                    SpellDataInst rdata = e.Spellbook.GetSpell(SpellSlot.R);
                    if (DangerDB.DangerousList.Any(spell => spell.Contains(rdata.SData.Name)))
                        miscSettings.Add(new MenuBool("ds" + e.CharacterName, rdata.SData.Name)).SetValue(true);
                }
                
                //Drawings
                var drawingsSettings = _config.Add(new Menu("Drawings", "Drawings"));
                drawingsSettings.Add(new MenuBool("DrawQ", "Draw Q")).SetValue(true);
                drawingsSettings.Add(new MenuBool("DrawE", "Draw E")).SetValue(true);
                drawingsSettings.Add(new MenuBool("DrawQW", "Draw long harras")).SetValue(true);
                drawingsSettings.Add(new MenuBool("DrawR", "Draw R")).SetValue(true);
                drawingsSettings.Add(new MenuBool("DrawHP", "Draw HP bar")).SetValue(true);
                drawingsSettings.Add(new MenuBool("shadowd", "Shadow Position")).SetValue(true);
                drawingsSettings.Add(new MenuBool("damagetest", "Damage Text")).SetValue(true);
                drawingsSettings.Add(new MenuBool("CircleLag", "Lag Free Circles").SetValue(true));
                drawingsSettings.Add(new MenuSlider("CircleQuality", "Circles Quality",100, 10, 100));
                drawingsSettings.Add(new MenuSlider("CircleThickness", "Circles Thickness",1, 1, 10));
                
                _config.Attach();
                new AssassinManager();
                Game.Print("<font color='#881df2'>Zed is Back by jackisback</font> Loaded.");
                Game.Print("<font color='#f2881d'>if you wanna help me to pay my internet bills^^ paypal= bulut@live.co.uk</font>");
                
                Drawing.OnDraw += Drawing_OnDraw;
                Game.OnUpdate += Game_OnUpdate;
                try
                {
                    AIBaseClient.OnProcessSpellCast += OnProcessSpell;
                }
                catch (Exception){}
            }
            catch (Exception e)
            {
                Console.WriteLine(e);
                Game.Print("Error something went wrong");
            }
        }
        
        private static void OnProcessSpell(AIBaseClient unit, AIBaseClientProcessSpellCastEventArgs castedSpell)
        {
            if (unit.Type != GameObjectType.AIHeroClient)
                return;
            if (unit.IsEnemy && _config["Misc"]["ds" + unit.CharacterName] != null)
            {
                
                if (_config["Misc"]["rdodge"].GetValue<MenuBool>().Enabled && _r.IsReady() && UltStage == UltCastStage.First &&
                    _config["Misc"]["ds" + unit.CharacterName].GetValue<MenuBool>().Enabled)
                {
                    
                    if (DangerDB.DangerousList.Any(spell => spell.Contains(castedSpell.SData.Name)) && 
                        (unit.Distance(_player.Position) < 650f || _player.Distance(castedSpell.End) <= 250f))
                    {
                        if (castedSpell.SData.Name == "SyndraR")
                        {
                            clockon = Environment.TickCount + 150;
                            countdanger = countdanger + 1;
                        }
                        else
                        {
                            var target = TargetSelector.GetTarget(640, DamageType.Physical);
                            _r.Cast(target);
                        }
                    }
                }
            }
 
            if (unit.IsMe && castedSpell.SData.Name == "zedult")
            {
                ticktock = Environment.TickCount + 200;

            }
        }
        
        private static void Game_OnUpdate(EventArgs args)
        {
            try
            {
                if (_config["Combo"].GetValue<MenuKeyBind>("ActiveCombo").Active)
                {
                    Combo(GetEnemy);

                }

                if (_config["Combo"].GetValue<MenuKeyBind>("TheLine").Active)
                {
                    TheLine(GetEnemy);
                }

                if (_config["Harass"].GetValue<MenuKeyBind>("ActiveHarass").Active)
                {
                    Harass(GetEnemy);

                }

                if (_config["Farm"]["LaneFarm"].GetValue<MenuKeyBind>("Activelane").Active)
                {
                    Laneclear();
                }

                if (_config["Farm"]["Jungle"].GetValue<MenuKeyBind>("Activejungle").Active)
                {
                    JungleClear();
                }

                if (_config["Farm"]["LastHit"].GetValue<MenuKeyBind>("ActiveLast").Active)
                {
                    LastHit();
                }

                if (_config["Misc"].GetValue<MenuBool>("AutoE").Enabled)
                {
                    CastE();
                }

                if (Environment.TickCount >= clockon && countdanger > countults)
                {
                    _r.Cast(TargetSelector.GetTarget(640, DamageType.Physical));
                    countults = countults + 1;
                }


                if (LastCast.LastCastPacketSent.Slot == SpellSlot.R)
                {
                    AIMinionClient shadow;
                    shadow = ObjectManager.Get<AIMinionClient>()
                        .FirstOrDefault(minion => minion.IsVisible && minion.IsAlly && minion.Name == "Shadow");

                    rpos = shadow.Position;
                }


                _player = ObjectManager.Player;


                KillSteal();
            }
            catch (Exception)
            {
            }
        }
        
        private static void Combo(AIHeroClient t)
        {
            var target = t;
            var overkill= _player.GetSpellDamage(target, SpellSlot.Q)+ _player.GetSpellDamage(target, SpellSlot.E)+_player.GetAutoAttackDamage(target, true) * 2;
            var doubleu = _player.Spellbook.GetSpell(SpellSlot.W);


            if (_config["Combo"].GetValue<MenuBool>("UseUlt").Enabled && UltStage == UltCastStage.First && (overkill < target.Health ||
                (!_w.IsReady()&& doubleu.Cooldown>2f && _player.GetSpellDamage(target, SpellSlot.Q) < target.Health && target.Distance(_player.Position) > 400)))
            {
                if ((target.Distance(_player.Position) > 700 && target.MoveSpeed > _player.MoveSpeed) || target.Distance(_player.Position) > 800)
                {
                    CastW(target);
                    _w.Cast();
                    
                }
                _r.Cast(target);
            }

            else
            {
                if (target != null && _config["Combo"].GetValue<MenuBool>("UseIgnitecombo").Enabled && _igniteSlot != SpellSlot.Unknown &&
                        _player.Spellbook.CanUseSpell(_igniteSlot) == SpellState.Ready)
                {
                    if (ComboDamage(target) > target.Health || target.HasBuff("zedulttargetmark"))
                    {
                        _player.Spellbook.CastSpell(_igniteSlot, target);
                    }
                }
                if (target != null && ShadowStage == ShadowCastStage.First && _config["Combo"].GetValue<MenuBool>("UseWC").Enabled &&
                        target.Distance(_player.Position) > 400 && target.Distance(_player.Position) < 1300)
                {
                    CastW(target);
                }
                if (target != null && ShadowStage == ShadowCastStage.Second && _config["Combo"].GetValue<MenuBool>("UseWC").Enabled &&
                    target.Distance(WShadow.Position) < target.Distance(_player.Position))
                {
                    _w.Cast();
                }
                
                CastE();
                CastQ(target);

            }
            
            
        }
        
        private static void TheLine(AIHeroClient t)
        {
            var target = t;

            if (target == null)
            {
                ObjectManager.Player.IssueOrder(GameObjectOrder.MoveTo, Game.CursorPos);
            }
            else
            {
                ObjectManager.Player.IssueOrder(GameObjectOrder.AttackUnit, target);
            }

            if ( !_r.IsReady() || target.Distance(_player.Position) >= 640)
            {
                return;
            }
            if (UltStage == UltCastStage.First)  
                _r.Cast(target);
            linepos = target.Position.Extend(_player.Position, -500);

            if (target != null && ShadowStage == ShadowCastStage.First &&  UltStage == UltCastStage.Second)
            {
                if (LastCast.LastCastPacketSent.Slot != SpellSlot.W)
                {
                    _w.Cast(linepos);
                    CastE();
                    CastQ(target);
                    
                    
                    if (target != null && _config["Combo"].GetValue<MenuBool>("UseIgnitecombo").Enabled && _igniteSlot != SpellSlot.Unknown &&
                        _player.Spellbook.CanUseSpell(_igniteSlot) == SpellState.Ready)
                    {
                        _player.Spellbook.CastSpell(_igniteSlot, target);
                    }
                
                }
            }

            if (target != null && WShadow != null && UltStage == UltCastStage.Second && target.Distance(_player.Position) > 250 && (target.Distance(WShadow.Position) < target.Distance(_player.Position)))
            {
                _w.Cast();
            }

        }
        
        private static void Harass(AIHeroClient t)
        {
            var target = t;

            if (target.IsValidTarget() && _config["Harass"].GetValue<MenuKeyBind>("longhar").Active && _w.IsReady() && _q.IsReady() && ObjectManager.Player.Mana >
                ObjectManager.Player.Spellbook.GetSpell(SpellSlot.Q).ManaCost +
                ObjectManager.Player.Spellbook.GetSpell(SpellSlot.W).ManaCost && target.Distance(_player.Position) > 850 &&
                target.Distance(_player.Position) < 1400)
            {
                CastW(target);
            }

            if (target.IsValidTarget() && (ShadowStage == ShadowCastStage.Second || ShadowStage == ShadowCastStage.Cooldown || !(_config["Harass"].GetValue<MenuBool>("UseWH").Enabled))
                                       && _q.IsReady() &&
                                       (target.Distance(_player.Position) <= 900 || target.Distance(WShadow.Position) <= 900))
            {
                CastQ(target);
            }

            if (target.IsValidTarget() && _w.IsReady() && _q.IsReady() && _config["Harass"].GetValue<MenuBool>("UseWH").Enabled &&
                ObjectManager.Player.Mana >
                ObjectManager.Player.Spellbook.GetSpell(SpellSlot.Q).ManaCost +
                ObjectManager.Player.Spellbook.GetSpell(SpellSlot.W).ManaCost )
            {
                if (target.Distance(_player.Position) < 750)

                    CastW(target);
            }
            
            CastE();

        }
        
        private static void Laneclear()
        {
            var allMinionsQ = MinionManager.GetMinions(ObjectManager.Player.Position, _q.Range);
            var allMinionsE = MinionManager.GetMinions(ObjectManager.Player.Position, _e.Range);
            var mymana = (_player.Mana >= (_player.MaxMana*_config["Farm"]["LaneFarm"].GetValue<MenuSlider>("Energylane").Value)/100);
            
            var useQl = _config["Farm"]["LaneFarm"].GetValue<MenuBool>("UseQL").Enabled;
            var useEl = _config["Farm"]["LaneFarm"].GetValue<MenuBool>("UseEL").Enabled;
            if (_q.IsReady() && useQl && mymana)
            {
                var fl2 = _q.GetLineFarmLocation(allMinionsQ, _q.Width);

                if (fl2.MinionsHit >= 3)
                {
                    _q.Cast(fl2.Position);
                }
                else
                    foreach (var minion in allMinionsQ)
                        if (!_player.InAutoAttackRange(minion) &&
                            minion.Health < 0.75 * _player.GetSpellDamage(minion, SpellSlot.Q))
                            _q.Cast(minion);
            }

            if (_e.IsReady() && useEl && mymana)
            {
                if (allMinionsE.Count > 2)
                {
                    _e.Cast();
                }
                else
                    foreach (var minion in allMinionsE)
                        if (!_player.InAutoAttackRange(minion) &&
                            minion.Health < 0.75 * _player.GetSpellDamage(minion, SpellSlot.E))
                            _e.Cast();
            }
            
        }
        
        private static void LastHit()
        {
            var allMinions = MinionManager.GetMinions(ObjectManager.Player.Position, _q.Range, MinionManager.MinionTypes.All);
            var mymana = (_player.Mana >=
                          (_player.MaxMana * _config["Farm"]["LastHit"].GetValue<MenuSlider>("Energylast").Value) / 100);
            var useQ = _config["Farm"]["LastHit"].GetValue<MenuBool>("UseQLH").Enabled;
            var useE = _config["Farm"]["LastHit"].GetValue<MenuBool>("UseELH").Enabled;
            foreach (var minion in allMinions)
            {
                if (mymana && useQ && _q.IsReady() && _player.Distance(minion.Position) < _q.Range &&
                    minion.Health < 0.75 * _player.GetSpellDamage(minion, SpellSlot.Q))
                {
                    _q.Cast(minion);
                }

                if (mymana && _e.IsReady() && useE && _player.Distance(minion.Position) < _e.Range &&
                    minion.Health < 0.95 * _player.GetSpellDamage(minion, SpellSlot.E))
                {
                    _e.Cast();
                }
            }
        }
        
        private static void JungleClear()
        {
            var mobs = MinionManager.GetMinions(_player.Position, _q.Range,
                MinionManager.MinionTypes.All,
                MinionManager.MinionTeam.Neutral, MinionManager.MinionOrderTypes.MaxHealth);
            var mymana = (_player.Mana >=
                          (_player.MaxMana * _config["Farm"]["Jungle"].GetValue<MenuSlider>("Energyjungle").Value) / 100);
            var useQ = _config["Farm"]["Jungle"].GetValue<MenuBool>("UseQJ").Enabled;
            var useW = _config["Farm"]["Jungle"].GetValue<MenuBool>("UseWJ").Enabled;
            var useE = _config["Farm"]["Jungle"].GetValue<MenuBool>("UseEJ").Enabled;

            if (mobs.Count > 0)
            {
                var mob = mobs[0];
                if (mymana && _w.IsReady() && useW && _player.Distance(mob.Position) < _q.Range)
                {
                    _w.Cast(mob.Position);
                }
                if (mymana && useQ && _q.IsReady() && _player.Distance(mob.Position) < _q.Range)
                {
                    CastQ(mob);
                }
                if (mymana && _e.IsReady() && useE && _player.Distance(mob.Position) < _e.Range)
                {
                    _e.Cast();
                }
            }

        }
        
        private static void KillSteal()
        {
            var target = TargetSelector.GetTarget(2000, DamageType.Physical);
            var igniteDmg = _player.GetSummonerSpellDamage(target, SummonerSpell.Ignite);
            if (target.IsValidTarget() && _config["Misc"].GetValue<MenuBool>("UseIgnitekill").Enabled && _igniteSlot != SpellSlot.Unknown &&
                _player.Spellbook.CanUseSpell(_igniteSlot) == SpellState.Ready)
            {
                if (igniteDmg > target.Health && _player.Distance(target.Position) <= 600)
                {
                    _player.Spellbook.CastSpell(_igniteSlot, target);
                }
            }
            if (target.IsValidTarget() && _q.IsReady() && _config["Misc"].GetValue<MenuBool>("UseQM").Enabled && _q.GetDamage(target) > target.Health)
            {
                if (_player.Distance(target.Position) <= _q.Range)
                {
                    _q.Cast(target);
                }
                else if (WShadow != null && WShadow.Distance(target.Position) <= _q.Range)
                {
                    _q.UpdateSourcePosition(WShadow.Position, WShadow.Position);
                    _q.Cast(target);
                }
                else if (RShadow != null && RShadow.Distance(target.Position) <= _q.Range)
                {
                    _q.UpdateSourcePosition(RShadow.Position, RShadow.Position);
                    _q.Cast(target);
                }
            }
            
            if (target.IsValidTarget() && _q.IsReady() && _config["Misc"].GetValue<MenuBool>("UseQM").Enabled && _q.GetDamage(target) > target.Health)
            {
                if (_player.Distance(target.Position) <= _q.Range)
                {
                    _q.Cast(target);
                }
                else if (WShadow != null && WShadow.Distance(target.Position) <= _q.Range)
                {
                    _q.UpdateSourcePosition(WShadow.Position, WShadow.Position);
                    _q.Cast(target);
                }
            }
            if (_e.IsReady() && _config["Misc"].GetValue<MenuBool>("UseEM").Enabled)
            {
                var t = TargetSelector.GetTarget(_e.Range, DamageType.Physical);
                if (_e.GetDamage(t) > t.Health && (_player.Distance(t.Position) <= _e.Range || WShadow.Distance(t.Position) <= _e.Range))
                {
                    _e.Cast();
                }
            }
        }

        private static float ComboDamage(AIBaseClient enemy)
        {
            var damage = 0d;
            if (_igniteSlot != SpellSlot.Unknown &&
                _player.Spellbook.CanUseSpell(_igniteSlot) == SpellState.Ready)
                damage += ObjectManager.Player.GetSummonerSpellDamage(enemy, SummonerSpell.Ignite);
            if (_q.IsReady())
                damage += _player.GetSpellDamage(enemy, SpellSlot.Q);
            if (_w.IsReady() && _q.IsReady())
                damage += _player.GetSpellDamage(enemy, SpellSlot.Q)/2;
            if (_e.IsReady())
                damage += _player.GetSpellDamage(enemy, SpellSlot.E);
            if (_r.IsReady())
                damage += _player.GetSpellDamage(enemy, SpellSlot.R);
            damage += (_r.Level*0.15 + 0.05)*
                      (damage - ObjectManager.Player.GetSummonerSpellDamage(enemy, SummonerSpell.Ignite));

            return (float)damage;
        }
        
        private static void CastW(AIBaseClient target)
        {
            if (delayw >= Environment.TickCount - shadowdelay || ShadowStage != ShadowCastStage.First || 
                ( target.HasBuff("zedulttargetmark") && LastCast.LastCastPacketSent.Slot == SpellSlot.R && UltStage == UltCastStage.Cooldown))
                return;

            var herew = target.Position.Extend(ObjectManager.Player.Position, -200);
        
            _w.Cast(herew);
            shadowdelay = Environment.TickCount;

        }
        
        private static void CastQ(AIBaseClient target)
        {
            if (!_q.IsReady()) return;
            
            if (WShadow != null && target.Distance(WShadow.Position) <= 900 && target.Distance(_player.Position)>450)
            {

                var shadowpred = _q.GetPrediction(target);
                _q.UpdateSourcePosition(WShadow.Position, WShadow.Position);
                if (shadowpred.Hitchance >= HitChance.Medium)
                    _q.Cast(target);

              
            }
            else
            {
                
                _q.UpdateSourcePosition(_player.Position, _player.Position);
                var normalpred = _q.GetPrediction(target);

                if (normalpred.CastPosition.Distance(_player.Position) < 900 && normalpred.Hitchance >= HitChance.Medium)
                {
                    _q.Cast(target);
                }
               

            }
        }

        
        private static void CastE()
        {
            if (!_e.IsReady()) return;
            if (ObjectManager.Get<AIHeroClient>()
                    .Count(
                        hero =>
                            hero.IsValidTarget() &&
                            (hero.Distance(ObjectManager.Player.Position) <= _e.Range ||
                             (WShadow != null && hero.Distance(WShadow.Position) <= _e.Range))) > 0)
                _e.Cast();
        }
        
        static AIHeroClient GetEnemy
        {
            get
            {
                var assassinRange = _config["MenuAssassin"].GetValue<MenuSlider>("AssassinSearchRange").Value;

                var vEnemy = ObjectManager.Get<AIHeroClient>()
                    .Where(
                        enemy =>
                            enemy.Team != ObjectManager.Player.Team && !enemy.IsDead && enemy.IsVisible &&
                            _config["MenuAssassin"]["AssassinMode"]["Assassin" + enemy.CharacterName] != null &&
                            _config["MenuAssassin"]["AssassinMode"].GetValue<MenuBool>("Assassin" + enemy.CharacterName).Enabled &&
                            ObjectManager.Player.Distance(enemy.Position) < assassinRange);

                if (_config["MenuAssassin"].GetValue<MenuList>("AssassinSelectOption").Index == 1)
                {
                    vEnemy = (from vEn in vEnemy select vEn).OrderByDescending(vEn => vEn.MaxHealth);
                }

                AIHeroClient[] objAiHeroes = vEnemy as AIHeroClient[] ?? vEnemy.ToArray();

                AIHeroClient t = !objAiHeroes.Any()
                    ? TargetSelector.GetTarget(1400, DamageType.Magical)
                    : objAiHeroes[0];

                return t;

            }

        }
        
        private static AIMinionClient WShadow
        {
            get
            {
                return
                    ObjectManager.Get<AIMinionClient>()
                        .FirstOrDefault(minion => minion.IsVisible && minion.IsAlly && (minion.Position != rpos) && minion.Name == "Shadow");
            }
        }
        private static AIMinionClient RShadow
        {
            get
            {
                return
                    ObjectManager.Get<AIMinionClient>()
                        .FirstOrDefault(minion => minion.IsVisible && minion.IsAlly && (minion.Position == rpos) && minion.Name == "Shadow");
            }
        }

        private static UltCastStage UltStage
        {
            get
            {
                if (!_r.IsReady()) return UltCastStage.Cooldown;

                return (ObjectManager.Player.Spellbook.GetSpell(SpellSlot.R).Name == "ZedR"
                    //return (ObjectManager.Player.Spellbook.GetSpell(SpellSlot.R).Name == "zedult"
                    ? UltCastStage.First
                    : UltCastStage.Second);
            }
        }
        
        private static ShadowCastStage ShadowStage
        {
            get
            {
                if (!_w.IsReady()) return ShadowCastStage.Cooldown;

                return (ObjectManager.Player.Spellbook.GetSpell(SpellSlot.W).Name == "ZedW"
                    //return (ObjectManager.Player.Spellbook.GetSpell(SpellSlot.W).Name == "ZedShadowDash"
                    ? ShadowCastStage.First
                    : ShadowCastStage.Second);
               
            }
        }
        
        internal enum UltCastStage
        {
            First,
            Second,
            Cooldown
        }

        internal enum ShadowCastStage
        {
            First,
            Second,
            Cooldown
        }

        private static void Drawing_OnDraw(EventArgs args)
        {
            if (RShadow != null)
            {
                CircleRender.Draw(RShadow.Position, RShadow.BoundingRadius * 2, Color.Blue);
            }


           
            if (_config["Drawings"].GetValue<MenuBool>("shadowd").Enabled)
            {
                if (WShadow != null)
                {
                    if (ShadowStage == ShadowCastStage.Cooldown)
                    {
                        CircleRender.Draw(WShadow.Position, WShadow.BoundingRadius * 1.5f, Color.Red);
                    }
                    else if (WShadow != null && ShadowStage == ShadowCastStage.Second)
                    {
                        CircleRender.Draw(WShadow.Position, WShadow.BoundingRadius * 1.5f, Color.Yellow);
                    }
                }
            }
            if (_config["Drawings"].GetValue<MenuBool>("damagetest").Enabled)
            {
                foreach (
                    var enemyVisible in
                        ObjectManager.Get<AIHeroClient>().Where(enemyVisible => enemyVisible.IsValidTarget()))
                {

                    if (ComboDamage(enemyVisible) > enemyVisible.Health)
                    {
                        Drawing.DrawText(Drawing.WorldToScreen(enemyVisible.Position)[0] + 50,
                            Drawing.WorldToScreen(enemyVisible.Position)[1] - 40, Color.Red.ToSystemColor(),
                            "Combo=Rekt");
                    }
                    else if (ComboDamage(enemyVisible) + _player.GetAutoAttackDamage(enemyVisible, true) * 2 >
                             enemyVisible.Health)
                    {
                        Drawing.DrawText(Drawing.WorldToScreen(enemyVisible.Position)[0] + 50,
                            Drawing.WorldToScreen(enemyVisible.Position)[1] - 40, Color.Orange.ToSystemColor(),
                            "Combo + 2 AA = Rekt");
                    }
                    else
                        Drawing.DrawText(Drawing.WorldToScreen(enemyVisible.Position)[0] + 50,
                            Drawing.WorldToScreen(enemyVisible.Position)[1] - 40, Color.Green.ToSystemColor(),
                            "Unkillable with combo + 2AA");
                }
            }

            if (_config["Drawings"].GetValue<MenuBool>("CircleLag").Enabled)
            {
                if (_config["Drawings"].GetValue<MenuBool>("DrawQ").Enabled)
                {
                    CircleRender.Draw(ObjectManager.Player.Position, _q.Range, Color.Blue);
                }
                if (_config["Drawings"].GetValue<MenuBool>("DrawE").Enabled)
                {
                    CircleRender.Draw(ObjectManager.Player.Position, _e.Range, Color.White);
                }
                if (_config["Drawings"].GetValue<MenuBool>("DrawQW").Enabled && _config["Harass"].GetValue<MenuKeyBind>("longhar").Active)
                {
                    CircleRender.Draw(ObjectManager.Player.Position, 1400, Color.Yellow);
                }
                if (_config["Drawings"].GetValue<MenuBool>("DrawR").Enabled)
                {
                    CircleRender.Draw(ObjectManager.Player.Position, _r.Range, Color.Blue);
                }
            }
            else
            {
                if (_config["Drawings"].GetValue<MenuBool>("DrawQ").Enabled)
                {
                    CircleRender.Draw(ObjectManager.Player.Position, _q.Range, Color.White);
                }
                if (_config["Drawings"].GetValue<MenuBool>("DrawE").Enabled)
                {
                    CircleRender.Draw(ObjectManager.Player.Position, _e.Range, Color.White);
                }
                if (_config["Drawings"].GetValue<MenuBool>("DrawQW").Enabled && _config["Harass"].GetValue<MenuKeyBind>("longhar").Active)
                {
                    CircleRender.Draw(ObjectManager.Player.Position, 1400, Color.White);
                }
                if (_config["Drawings"].GetValue<MenuBool>("DrawR").Enabled)
                {
                    CircleRender.Draw(ObjectManager.Player.Position, _r.Range, Color.White);
                }
            }
        }
    }
}