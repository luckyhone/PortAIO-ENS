using System;
using System.Linq;
using EnsoulSharp;
using EnsoulSharp.SDK;
using EnsoulSharp.SDK.MenuUI;
using LeagueSharpCommon;
using SharpDX;
using SPrediction;
using xSalice_Reworked.Base;
using xSalice_Reworked.Managers;
using xSalice_Reworked.Utilities;
using KeyBindType = EnsoulSharp.SDK.MenuUI.KeyBindType;
using Keys = EnsoulSharp.SDK.MenuUI.Keys;
using Menu = EnsoulSharp.SDK.MenuUI.Menu;
using MenuItem = EnsoulSharp.SDK.MenuUI.MenuItem;
using Render = LeagueSharpCommon.Render;

namespace xSalice_Reworked.Pluging
{
    internal class Ahri : Champion
    {
        private static bool _rOn;
        private static int _rTimer;
        private static int _rTimeLeft;

        public Ahri()
        {
            SpellManager.Q = new Spell(SpellSlot.Q, 900f);
            SpellManager.W = new Spell(SpellSlot.W, 600f);
            SpellManager.E = new Spell(SpellSlot.E, 950f);
            SpellManager.R = new Spell(SpellSlot.R, 850f);

            SpellManager.Q.SetSkillshot(0.25f, 90f, 1550f, false, SpellType.Line);
            SpellManager.E.SetSkillshot(0.25f, 60f, 1550f, true, SpellType.Line);

            SpellManager.SpellList.Add(Q);
            SpellManager.SpellList.Add(W);
            SpellManager.SpellList.Add(E);
            SpellManager.SpellList.Add(R);

            var combo = new Menu("Combo", "Combo");
            {
                combo.Add(new MenuBool("UseQCombo", "Use Q", true).SetValue(true));
                combo.Add(new MenuBool("UseWCombo", "Use W", true).SetValue(true));
                combo.Add(new MenuBool("UseECombo", "Use E", true).SetValue(true));
                combo.Add(new MenuBool("UseRCombo", "Use R", true).SetValue(true));
                combo.Add(new MenuBool("rSpeed", "Use All R fast Duel", true).SetValue(true));
                combo.Add(new MenuKeyBind("charmCombo", "Q if Charmed in Combo",Keys.I,KeyBindType.Toggle));
                Menu.Add(combo);
            }
            var harass = new Menu("Harass", "Harass");
            {
                harass.Add(new MenuBool("UseQHarass", "Use Q", true).SetValue(true));
                harass.Add(new MenuBool("UseWHarass", "Use W", true).SetValue(false));
                harass.Add(new MenuBool("UseEHarass", "Use E", true).SetValue(true));
                harass.Add(new MenuBool("longQ", "Cast Long range Q", true).SetValue(true));
                harass.Add(new MenuKeyBind("FarmT", "Harass (toggle)!", Keys.N,KeyBindType.Toggle));
                ManaManager.AddManaManagertoMenu(harass, "Harass", 60);
                Menu.Add(harass);
            }

            var farm = new Menu("Farm", "Farm");
            {
                farm.Add(new MenuBool("UseQFarm", "Use Q", true).SetValue(false));
                farm.Add(new MenuBool("UseWFarm", "Use W", true).SetValue(false));
                ManaManager.AddManaManagertoMenu(farm, "Farm", 50);
                Menu.Add(farm);
            }

            var misc = new Menu("Misc", "Misc");
            {
                misc.Add(AOESpellManager.AddHitChanceMenuCombo(true, false, false, false));
                misc.Add(new MenuBool("UseInt", "Use E to Interrupt", true).SetValue(true));
                misc.Add(new MenuBool("UseGap", "Use E for GapCloser", true).SetValue(true));
                misc.Add(new MenuBool("EQ", "Use Q onTop of E", true).SetValue(true));
                misc.Add(new MenuBool("smartKS", "Smart KS", true).SetValue(true));
                Menu.Add(misc);
            }

            var drawing = new Menu("Drawings", "Drawings");
            {
                drawing.Add(
                        new MenuBool("QRange", "Q range"));
                drawing.Add(
                        new MenuBool("WRange", "W range"));
                drawing.Add(
                        new MenuBool("ERange", "E range"));
                drawing.Add(
                        new MenuBool("RRange", "R range"));
                drawing.Add(
                        new MenuBool("cursor", "Draw R Dash Range"));

                var drawComboDamageMenu = new MenuBool("Draw_ComboDamage", "Draw Combo Damage", true).SetValue(true);
                var drawFill = new MenuColor("Draw_Fill", "Draw Combo Damage Fill", new ColorBGRA(90, 255, 169, 4));
                drawing.Add(drawComboDamageMenu);
                drawing.Add(drawFill);
                //DamageIndicator.DamageToUnit = GetComboDamage;
                //DamageIndicator.Enabled = drawComboDamageMenu.GetValue<bool>();
                //DamageIndicator.Fill = drawFill.GetValue<Circle>().Active;
                //DamageIndicator.FillColor = drawFill.GetValue<Circle>().Color;
                Menu.Add(drawing);
            }

            var customMenu = new Menu("Custom Perma Show", "Custom Perma Show");
            {
                var myCust = new CustomPermaMenu();
                customMenu.Add(new MenuKeyBind("custMenu", "Move Menu", Keys.L,KeyBindType.Press));
                customMenu.Add(new MenuBool("enableCustMenu", "Enabled", true).SetValue(true));
                //customMenu.Add(myCust.AddToMenu("Combo Active: ", "Orbwalk"));
                //customMenu.Add(myCust.AddToMenu("Harass Active: ", "Farm"));
                customMenu.Add(myCust.AddToMenu("Harass(T) Active: ", "FarmT"));
                //customMenu.Add(myCust.AddToMenu("Laneclear Active: ", "LaneClear"));
                customMenu.Add(myCust.AddToMenu("Require Charm: ", "charmCombo"));
                Menu.Add(customMenu);
            }
        }
        
        private float GetComboDamage(AIBaseClient enemy)
        {
            if (enemy == null)
                return 0;

            var damage = 0d;

            if (Q.IsReady())
            {
                damage += Player.GetSpellDamage(enemy, SpellSlot.Q);
                damage += Player.GetSpellDamage(enemy, SpellSlot.Q, 1);
            }
            if (W.IsReady())
                damage += Player.GetSpellDamage(enemy, SpellSlot.W);

            if (_rOn)
                damage += Player.GetSpellDamage(enemy, SpellSlot.R) * RCount();
            else if (R.IsReady())
                damage += Player.GetSpellDamage(enemy, SpellSlot.R) * 3;
            

            if (E.IsReady())
                damage += Player.GetSpellDamage(enemy, SpellSlot.E);

            return (float)damage;
        }
        private void Combo()
        {
            var eTarget = TargetSelector.GetTarget(E.Range, DamageType.Magical);
            var rETarget = TargetSelector.GetTarget(E.Range, DamageType.Magical);

            var dmg = GetComboDamage(eTarget);

            if (eTarget == null)
                return;
            

            //E
            if (Menu["UseECombo"].GetValue<MenuBool>().Enabled && E.IsReady() && Player.Distance(eTarget.Position) < E.Range)
            {
                SpellCastManager.CastBasicSkillShot(E, E.Range, DamageType.Magical, HitChance.VeryHigh);
                if (Menu["EQ"].GetValue<MenuBool>().Enabled && Q.IsReady() && !E.IsReady())
                {
                    SpellCastManager.CastBasicSkillShot(Q, Q.Range, DamageType.Magical, HitChance.VeryHigh);
                }
            }

            //W
            if (Menu["UseWCombo"].GetValue<MenuBool>().Enabled && W.IsReady() && Player.Distance(eTarget.Position) <= W.Range - 100 &&
                ShouldW(eTarget))
            {
                W.Cast();
            }

            if (Menu["UseQCombo"].GetValue<MenuBool>().Enabled && Q.IsReady() && Player.Distance(eTarget.Position) <= Q.Range &&
                     ShouldQ(eTarget))
            {
                SpellCastManager.CastBasicSkillShot(Q, Q.Range, DamageType.Magical, HitChance.VeryHigh);
            }

            //R
            if (Menu["UseRCombo"].GetValue<MenuBool>().Enabled && R.IsReady() && Player.Distance(eTarget.Position) < R.Range)
            {
                if (E.IsReady())
                {
                    if (CheckReq(rETarget))
                        SpellCastManager.CastBasicSkillShot(E, E.Range, DamageType.Magical, HitChance.VeryHigh);
                }
                if (ShouldR(eTarget, dmg) && R.IsReady())
                {
                    R.Cast(Game.CursorPos);
                    _rTimer = Utils.TickCount - 250;
                }
                if (_rTimeLeft > 9500 && _rOn && R.IsReady())
                {
                    R.Cast(Game.CursorPos);
                    _rTimer = Utils.TickCount - 250;
                }
            }
        }
        private bool ShouldQ(AIHeroClient target)
        {
            if (Player.GetSpellDamage(target, SpellSlot.Q) + Player.GetSpellDamage(target, SpellSlot.Q, 1) >
                target.Health)
                return true;

            if (_rOn)
                return true;

            if (!Menu["charmCombo"].GetValue<MenuKeyBind>().Active)
                return true;

            return target.HasBuffOfType(BuffType.Charm) || target.HasBuffOfType(BuffType.Taunt);
        }

        private bool ShouldW(AIHeroClient target)
        {
            if (Player.GetSpellDamage(target, SpellSlot.W) > target.Health)
                return true;

            if (_rOn)
                return true;

            if (Player.Mana > ESpell.ManaCost + QSpell.ManaCost)
                return true;

            if (!Menu["charmCombo"].GetValue<MenuKeyBind>().Active)
                return true;

            return target.HasBuffOfType(BuffType.Charm) || target.HasBuffOfType(BuffType.Taunt);
        }

        private bool ShouldR(AIHeroClient target, float dmg)
        {
            var dashVector = Player.ServerPosition + Vector3.Normalize(Game.CursorPos - Player.ServerPosition) * 425;

            if (Player.Distance(Game.CursorPos) < 475)
                dashVector = Game.CursorPos;

            if (target.Distance(dashVector) > 525)
                return false;

            if (Menu["rSpeed"].GetValue<MenuBool>().Enabled && Game.CursorPos.CountEnemyHeroesInRange(1500) < 3 && dmg > target.Health - 100)
                return true;

            if (Player.GetSpellDamage(target, SpellSlot.R) * RCount() > target.Health)
                return true;

            return _rOn && _rTimeLeft > 9500;
        }

        private bool CheckReq(AIHeroClient target)
        {
            if (Player.Distance(Game.CursorPos) < 75)
                return false;

            if (GetComboDamage(target) > target.Health && !_rOn && Game.CursorPos.CountEnemyHeroesInRange(1500) < 3)
            {
                if (target.Distance(Game.CursorPos) <= E.Range && E.IsReady())
                {
                    var dashVector = Player.Position + Vector3.Normalize(Game.CursorPos - Player.Position) * 425;
                    var addedDelay = Player.Distance(dashVector) / 2200;
                    var pred = CommonPredEx.GetP(Game.CursorPos, E, target, addedDelay, false);

                    if (pred.Hitchance >= HitChance.Medium && R.IsReady())
                    {
                        R.Cast(Game.CursorPos);
                        _rTimer = Utils.TickCount - 250;
                        return true;
                    }
                }
            }

            return false;
        }

        private bool IsRActive()
        {
            return Player.HasBuff("AhriTumble");
        }

        private int RCount()
        {
            var buff = Player.Buffs.FirstOrDefault(x => x.Name == "AhriTumble");

            return buff?.Count ?? 0;
        }
        
        private void Farm()
        {
            if (!ManaManager.HasMana("Farm"))
                return;

            var allMinionsQ = MinionManager.GetMinions(ObjectManager.Player.ServerPosition, Q.Range,
                MinionManager.MinionTypes.All, MinionManager.MinionTeam.NotAlly);
            var allMinionsW = MinionManager.GetMinions(ObjectManager.Player.ServerPosition, W.Range,
                MinionManager.MinionTypes.All, MinionManager.MinionTeam.NotAlly);

            var useQ = Menu["UseQFarm"].GetValue<MenuBool>().Enabled;
            var useW = Menu["UseWFarm"].GetValue<MenuBool>().Enabled;

            if (useQ && Q.IsReady())
            {
                var qPos = Q.GetLineFarmLocation(allMinionsQ);
                if (qPos.MinionsHit >= 3)
                {
                    Q.Cast(qPos.Position);
                }
            }

            if (useW && allMinionsW.Count > 0 && W.IsReady())
                W.Cast();
        }

        protected override void Game_OnGameUpdate(EventArgs args)
        {
            _rOn = IsRActive();

            if (_rOn)
                _rTimeLeft = Utils.TickCount - _rTimer;

            if (Menu["smartKS"].GetValue<MenuBool>().Enabled)
                CheckKs();

            if (Menu["FarmT"].GetValue<MenuKeyBind>().Active)
                Harass();

            switch (Orbwalker.ActiveMode)
            {
                case OrbwalkerMode.Combo:
                    Combo();
                    break;
                case OrbwalkerMode.Harass:
                    Harass();
                    break;
                case OrbwalkerMode.LaneClear:
                    Farm();
                    break;
                case OrbwalkerMode.LastHit:
                    break;
                case OrbwalkerMode.None:
                    break;
                case OrbwalkerMode.Flee:
                    break;
                default:
                    throw new ArgumentOutOfRangeException();
            }
        }
        private void Harass()
        {
            if (!ManaManager.HasMana("Harass"))
                return;

            var eTarget = TargetSelector.GetTarget(E.Range, DamageType.Magical);
            var rETarget = TargetSelector.GetTarget(E.Range, DamageType.Magical);

            var dmg = GetComboDamage(eTarget);

            if (eTarget == null)
                return;

            //E
            if (Menu["UseEHarass"].GetValue<MenuBool>().Enabled && E.IsReady() && Player.Distance(eTarget.Position) < E.Range)
            {
                SpellCastManager.CastBasicSkillShot(E, E.Range, DamageType.Magical, HitChance.VeryHigh);
                if (Menu["EQ"].GetValue<MenuBool>().Enabled && Q.IsReady() && !E.IsReady())
                {
                    SpellCastManager.CastBasicSkillShot(Q, Q.Range, DamageType.Magical, HitChance.VeryHigh);
                }
            }

            //W
            if (Menu["UseWHarass"].GetValue<MenuBool>().Enabled && W.IsReady() && Player.Distance(eTarget.Position) <= W.Range - 100)
            {
                W.Cast();
            }

            if (Menu["longQ"].GetValue<MenuBool>().Enabled)
            {
                if (Menu["UseQHarass"].GetValue<MenuBool>().Enabled && Q.IsReady() && Player.Distance(eTarget.Position) <= Q.Range
                    && Player.Distance(eTarget.Position) > 600)
                {
                    SpellCastManager.CastBasicSkillShot(Q, Q.Range,DamageType.Magical, HitChance.VeryHigh);
                }
            }
        }
        
        private void CheckKs()
        {
            foreach (var target in HeroManager.Enemies.Where(x => x.IsValidTarget(1300) && !x.IsDead).OrderByDescending(GetComboDamage))
            {
                if (target != null)
                {
                    if (Player.Distance(target.ServerPosition) <= W.Range &&
                        Player.GetSpellDamage(target, SpellSlot.Q) + Player.GetSpellDamage(target, SpellSlot.Q, 1) +
                        Player.GetSpellDamage(target, SpellSlot.W) > target.Health && Q.IsReady() && Q.IsReady())
                    {
                        Q.Cast(target);
                        return;
                    }

                    if (Player.Distance(target.ServerPosition) <= Q.Range &&
                        Player.GetSpellDamage(target, SpellSlot.Q) + Player.GetSpellDamage(target, SpellSlot.Q, 1) >
                        target.Health && Q.IsReady())
                    {
                        Q.Cast(target);
                        return;
                    }

                    if (Player.Distance(target.ServerPosition) <= E.Range &&
                        Player.GetSpellDamage(target, SpellSlot.E) > target.Health & E.IsReady())
                    {
                        E.Cast(target);
                        return;
                    }

                    if (Player.Distance(target.ServerPosition) <= W.Range &&
                        Player.GetSpellDamage(target, SpellSlot.W) > target.Health && W.IsReady())
                    {
                        W.Cast();
                        return;
                    }

                    //var dashVector = Player.Position +
                    //                     Vector3.Normalize(target.ServerPosition - Player.Position) * 425;
                    //if (Player.Distance(target.ServerPosition) <= R.Range &&
                    //    Player.GetSpellDamage(target, SpellSlot.R) > target.Health && R.IsReady() && _rOn &&
                    //    target.Distance(dashVector) < 425 && R.IsReady())
                    //{
                    //    R.Cast(dashVector);
                    //}
                }
            }
        }
        
        protected override void Drawing_OnDraw(EventArgs args)
        {
            foreach (var spell in SpellList)
            {
                var menuItem = Menu[spell.Slot + "Range"].GetValue<MenuBool>();
                if (menuItem.Enabled)
                    Render.Circle.DrawCircle(Player.Position, spell.Range, new ColorBGRA(100, 255, 0, 255).ToSystemColor());
            }

            if (Menu["cursor"].GetValue<MenuBool>().Enabled)
                Render.Circle.DrawCircle(Player.Position, 475, Color.Aquamarine.ToSystemColor());
        }

        protected override void AntiGapcloser_OnEnemyGapcloser(AIHeroClient sender, AntiGapcloser.GapcloserArgs gapcloser)
        {
            if (!Menu["UseGap"].GetValue<MenuBool>().Enabled) return;

            if (E.IsReady() && sender.IsValidTarget(E.Range))
                E.Cast(sender);
        }

        protected override void Interrupter_OnPosibleToInterrupt(AIHeroClient unit, Interrupter.InterruptSpellArgs args)
        {
            if (!Menu["UseInt"].GetValue<MenuBool>().Enabled) return;

            if (Player.Distance(unit.Position) < E.Range)
            {
                if (E.GetPrediction(unit).Hitchance >= HitChance.Medium && E.IsReady())
                    E.Cast(unit);
            }
        }
    }
}