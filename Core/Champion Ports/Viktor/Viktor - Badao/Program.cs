using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using EnsoulSharp;
using EnsoulSharp.SDK;
using EnsoulSharp.SDK.MenuUI;
using PortAIO;
using SharpDX;

 namespace ViktorBadao
{
    static class Program
    {
        private static AIHeroClient Player { get { return ObjectManager.Player; } }
        private static Spell _q, _w, _e, _r;
        private static Menu _menu,spellMenu,Harass,Combo,Focus,KS;
        private static GameObject ViktorR = null;

        public static void Game_OnGameLoad()
        {
            if (Player.CharacterName != "Viktor")
                return;

            _q = new Spell(SpellSlot.Q , 650);
            _w = new Spell(SpellSlot.W,700);
            _e = new Spell(SpellSlot.E,700);
            _r = new Spell(SpellSlot.R,700);
            _r.SetSkillshot(0.25f, 325,float.MaxValue,false,SpellType.Circle);
            _w.SetSkillshot(0.25f, 325, float.MaxValue, false, SpellType.Circle);
            _e.SetSkillshot(0.25f, 80, 1050, false, SpellType.Line);
            _e.MinHitChance = HitChance.Medium;
            //R = new Spells(SpellSlot.R, SkillshotType.SkillshotCircle, 700, 0.25f, 325 / 2, false);

            _menu = new Menu(Player.CharacterName, Player.CharacterName, true);

            spellMenu = _menu.Add(new Menu("Spells", "Spells"));
            Harass = spellMenu.Add(new Menu("Harass", "Harass"));
            Combo = spellMenu.Add(new Menu("Combo", "Combo"));
            Focus = spellMenu.Add(new Menu("Focus Selected", "Focus Selected"));
            KS = spellMenu.Add(new Menu("KillSteal", "KillSteal"));
            Harass.Add(new MenuBool("Use Q Harass", "Use Q Harass").SetValue(true));
            Harass.Add(new MenuBool("Use E Harass", "Use E Harass").SetValue(true));
            Combo.Add(new MenuBool("Use Q Combo", "Use Q Combo").SetValue(true));
            Combo.Add(new MenuBool("Use E Combo", "Use E Combo").SetValue(true));
            Combo.Add(new MenuBool("Use W Combo", "Use W Combo").SetValue(true));
            Combo.Add(new MenuBool("Use R Burst Selected", "Use R Combo").SetValue(true));
            Focus.Add(new MenuBool("force focus selected", "force focus selected").SetValue(false));
            Focus.Add(new MenuSlider("if selected in :", "if selected in :",1000, 1000, 1500));
            KS.Add(new MenuBool("Use Q KillSteal", "Use Q KillSteal").SetValue(true));
            KS.Add(new MenuBool("Use E KillSteal", "Use E KillSteal").SetValue(true));
            KS.Add(new MenuBool("Use R KillSteal", "Use R KillSteal").SetValue(true));
            spellMenu.Add(new MenuBool("Use R Follow", "Use R Follow").SetValue(true));
            spellMenu.Add(new MenuBool("Use W GapCloser", "Use W anti gap").SetValue(true));

            _menu.Attach();

            Game.OnUpdate += Game_OnGameUpdate;
            GameObject.OnCreate += Create;
            GameObject.OnDelete += Delete;
            AntiGapcloser.OnGapcloser += AntiGapcloser_OnEnemyGapcloser;
            Game.Print("Welcome to ViktorWorld");
        }
        

        private static void AntiGapcloser_OnEnemyGapcloser(AIHeroClient sender, AntiGapcloser.GapcloserArgs args)
        {
            if (spellMenu["Use W GapCloser"].GetValue<MenuBool>().Enabled && _w.IsReady() && sender.IsValidTarget(_w.Range))
            {
                var pos = args.EndPosition;
                if (Player.Distance(pos) <= _w.Range)
                    _w.Cast(pos);
            }
        }
        private static void Create(GameObject sender, EventArgs args)
        {
            if (sender.Name.Contains("Viktor_Base_R_Droid.troy"))
            {
                ViktorR = sender;
            }
        }
        private static void Delete(GameObject sender, EventArgs args)
        {
            if (sender.Name.Contains("Viktor_Base_R_Droid.troy"))
            {
                ViktorR = null;
            }
        }
        private static void Game_OnGameUpdate(EventArgs args)
        {
            if (Player.IsDead)
                return;
            //Chat.Print(Player.Position.Distance(Game.CursorPos).ToString());
            if (Orbwalker.ActiveMode == OrbwalkerMode.Combo || (Orbwalker.ActiveMode == OrbwalkerMode.Harass && Selected()))
            {
                if (_q.IsReady())
                {
                    Orbwalker.AttackEnabled =  false;
                }
                else
                    Orbwalker.AttackEnabled =  (true);
            }
            else
            {
                Orbwalker.AttackEnabled =  (true);
            }

            if (Orbwalker.ActiveMode == OrbwalkerMode.Combo)
            {
                if (Combo["Use Q Combo"].GetValue<MenuBool>().Enabled)
                {
                    UseQ();
                }
                if (Combo["Use E Combo"].GetValue<MenuBool>().Enabled)
                {
                    UseE();
                }
                if (Combo["Use W Combo"].GetValue<MenuBool>().Enabled)
                {
                    UseW();
                }
                if (Combo["Use R Burst Selected"].GetValue<MenuBool>().Enabled)
                {
                    UseR();
                }
            }
            if (Orbwalker.ActiveMode == OrbwalkerMode.Harass)
            {
                if (Harass["Use Q Harass"].GetValue<MenuBool>().Enabled)
                    UseQ();
                if (Harass["Use E Harass"].GetValue<MenuBool>().Enabled)
                    UseE();
            }
            ViktorRMove();
            killsteal();
        }

        private static void UseW()
        {
            var target = TargetSelector.GetTarget(_w.Range, DamageType.Magical);
            if ( target.IsValidTarget() && !target.IsZombie() && _w.IsReady())
            {
                var pos = Prediction.GetPrediction(target, 0.25f).UnitPosition;
                if (Player.Distance(pos) <= _w.Range)
                {
                    _w.Cast(pos);
                }
            }
        }
        private static void ViktorRMove()
        {
            if (spellMenu["Use R Follow"].GetValue<MenuBool>().Enabled && ViktorR != null && _r.IsReady())
            {
                var target = ViktorR.Position.GetEnemiesInRange(2000).Where(t => t.IsValidTarget() && !t.IsZombie()).OrderByDescending(t => 1 - t.Distance(ViktorR.Position)).FirstOrDefault();
                if (target.Distance(ViktorR.Position) >= 50)
                {
                    Vector3 x = Prediction.GetPrediction(target,0.5f).UnitPosition;
                    _r.Cast(x);
                }
            }
        }
        private static bool Selected()
        {
            if (!Focus["force focus selected"].GetValue<MenuBool>().Enabled)
            {
                return false;
            }
            else
            {
                var target = TargetSelector.SelectedTarget;
                float a = Focus["if selected in :"].GetValue<MenuSlider>().Value;
                if (target == null || target.IsDead || target.IsZombie())
                {
                    return false;
                }
                return !(Player.Distance(target.Position) > a);
            }
        }

        private static AIBaseClient Gettarget(float range)
        {
            return Selected() ? TargetSelector.SelectedTarget : TargetSelector.GetTarget(range, DamageType.Magical);
        }

        private static void UseQ()
        {
            if (!_q.IsReady())
                return;
            var target = Gettarget(650);
            if (target != null && target.IsValidTarget(650)  && _q.IsReady())
                _q.Cast(target);
        }

        private static void UseR()
        {
            if (!_r.IsReady())
                return;
            if (_r.IsReady() && _r.Instance.Name == "ViktorChaosStorm")
            {
                {
                    var target = TargetSelector.SelectedTarget;
                    if (target != null && Player.Distance(target.Position) <= 1000 && target.IsValidTarget() && !target.IsZombie() && _r.IsReady() && _r.Instance.Name == "ViktorChaosStorm")
                    {
                        CastR(target);
                    }
                }
                {
                    var target = TargetSelector.GetTarget(1000, DamageType.Magical);
                    if (target != null && target.IsValidTarget() && !target.IsZombie() && _r.IsReady() && _r.Instance.Name == "ViktorChaosStorm" )
                    {
                        if (target.Health <= _r.GetDamage(target)*1.7)
                        {
                            CastR(target);
                        }
                    }
                    foreach(var hero in GameObjects.EnemyHeroes.Where(x=> x.IsValidTarget(1000) && !x.IsZombie()))
                    {

                    }
                }
            }
        }

        private static void CastR(AIBaseClient target)
        {
            if (!target.IsValidTarget() || target.IsDead)
                return;
            var predpos = Prediction.GetPrediction(target, 0.25f).UnitPosition.To2D();
            if (predpos.Distance(Player.Position.To2D()) <= 1000 )
            {
                var castpos = predpos.Distance(Player.Position.To2D()) > 700 ?
                    Player.Position.To2D().Extend(predpos, 700) :
                    predpos;
                _r.Cast(predpos);
            }
        }

        private static void UseE(AIBaseClient  ForceTarget = null)
        {
            if (!_e.IsReady())
                return;
            var target = Gettarget(525 + 700);
            if (ForceTarget != null)
                target = ForceTarget;
            if (target != null && target.IsValidTarget(1025) && !target.IsDead && _e.IsReady())
            {
                AIHeroClient startHeroPos = GameObjects.EnemyHeroes.Where(x => x.IsValidTarget(525) && x.NetworkId != target.NetworkId && x.Distance(target) <= 700).MinOrDefault(x => x.Health);
                AIHeroClient startHeroExtend = GameObjects.EnemyHeroes.Where(x => x.IsValidTarget() && x.NetworkId != target.NetworkId && x.Distance (target) <= 700
                    && target.Position.To2D().Extend(x.Position.To2D(), 700).Distance(Player.Position) <= 525).MinOrDefault(x => x.Health);
                AIHeroClient endHeroPos = GameObjects.EnemyHeroes.Where(x => x.IsValidTarget(525 + 700) && x.NetworkId != target.NetworkId && target.IsValidTarget(525)
                    && x.Distance(target) <= 700).MinOrDefault(x => x.Health);
                AIHeroClient endHeroExtend = GameObjects.EnemyHeroes.Where(x => x.IsValidTarget(1025) && x.NetworkId != target.NetworkId
                    && x.Distance(target) <= 700 && x.Position.To2D().Extend(target.Position.To2D(),700).Distance(Player.Position) <= 525).MinOrDefault(x => x.Health);
                Vector3 DefaultPos = Player.Distance(target.Position) >= 525 ? Player.Position.To2D().Extend(target.Position.To2D(), 525).To3D() : target.Position;
                if (startHeroPos != null)
                {
                    _e.SetSkillshot(0.25f, 80, 1050, false, SpellType.Line, HitChance.High,startHeroPos.Position, startHeroPos.Position);
                    CastE(target);
                }
                else if (startHeroExtend != null)
                {
                    //float r = 525;
                    //float d = target.Distance(Player);
                    //float h = Geometry.Distance(Player.Position.To2D(), target.Position.To2D(), startHeroExtend.Position.To2D());
                    //float a = (float)Math.Sqrt(d * d - h * h);
                    //float b = (float)Math.Sqrt(r * r - h * h);
                    //float c = a - b;
                    _e.SetSkillshot(0.25f, 80, 1050, false, SpellType.Line, HitChance.High,target.Position.To2D().Extend(startHeroExtend.Position.To2D(), 700).To3D(), target.Position.To2D().Extend(startHeroExtend.Position.To2D(), 700).To3D());
                    CastE(target);
                }
                else if (endHeroPos != null)
                {
                    _e.SetSkillshot(0.25f, 80, 1050, false, SpellType.Line, HitChance.High,target.Position, target.Position);
                    CastE(endHeroPos);
                }
                else if(endHeroExtend != null)
                {
                    //float r = 525;
                    //float d = endHeroExtend.Distance(Player);
                    //float h = Geometry.Distance(Player.Position.To2D(), target.Position.To2D(), endHeroExtend.Position.To2D());
                    //float a = (float)Math.Sqrt(d * d - h * h);
                    //float b = (float)Math.Sqrt(r * r - h * h);
                    //float c = a - b;
                    _e.SetSkillshot(0.25f, 80, 1050, false, SpellType.Line, HitChance.High,endHeroExtend.Position.To2D().Extend(target.Position.To2D(), 700).To3D(), endHeroExtend.Position.To2D().Extend(target.Position.To2D(), 700).To3D());
                    CastE(endHeroExtend);
                }
                else
                {
                    _e.SetSkillshot(0.25f, 80, 1050, false, SpellType.Line, HitChance.High,DefaultPos, DefaultPos);
                    CastE(target);
                }
            }
        }
        public static void CastE(AIBaseClient target)
        {
            if (target == null)
                return;
            var pred = _e.GetPrediction(target);
            if (pred.Hitchance >= HitChance.Medium)
            {
                _e.Cast(_e.RangeCheckFrom, pred.CastPosition);
            }
        }
        public static void killsteal()
        {
            if (_q.IsReady() && KS["Use Q KillSteal"].GetValue<MenuBool>().Enabled && !Player.Spellbook.IsAutoAttack)
            {
                foreach (var hero in ObjectManager.Get<AIHeroClient>().Where(hero => hero.IsEnemy && hero.IsValidTarget(650)))
                {
                    var dmg = Dame(hero, SpellSlot.Q);
                    if (hero != null && hero.IsValidTarget() && !hero.IsZombie() && dmg > hero.Health) { _q.Cast(hero); }
                }
            }
            if (_e.IsReady() && KS["Use E KillSteal"].GetValue<MenuBool>().Enabled && !Player.Spellbook.IsAutoAttack)
            {
                foreach (var hero in ObjectManager.Get<AIHeroClient>().Where(hero => hero.IsEnemy && hero.IsValidTarget(1025)))
                {
                    var dmg = Dame(hero, SpellSlot.E);
                    if (hero != null && hero.IsValidTarget() && !hero.IsZombie() && dmg > hero.Health)
                    {
                        UseE(hero);
                    }
                }
            }

            if (_r.IsReady() && KS["Use R KillSteal"].GetValue<MenuBool>().Enabled && !Player.Spellbook.IsAutoAttack)
            {
                foreach (var hero in ObjectManager.Get<AIHeroClient>().Where(hero => hero.IsEnemy && hero.IsValidTarget(860)))
                {
                    var dmgR = Dame(hero, SpellSlot.R);
                    var dmgE = Dame(hero, SpellSlot.E);
                    var dmgQ = Dame(hero, SpellSlot.Q);
                    if (hero != null && hero.IsValidTarget() && !hero.IsZombie())
                    {
                        if (dmgE > hero.Health && dmgR > hero.Health)
                        {
                            if (!_e.IsReady())
                                CastR(hero);
                        }
                        else if (dmgQ > hero.Health && dmgR > hero.Health && Player.Distance(hero.Position) <= 600)
                        {
                            if (!_q.IsReady() && !_e.IsReady())
                                CastR(hero);
                        }
                        else if (dmgR > hero.Health) { _r.Cast(hero); }
                    }
                }
            }

        }
        public static double Dame(AIBaseClient target, SpellSlot x)
        {
            if (target != null) { return Player.GetSpellDamage(target, x); } else return 0;
        }

    }
}
