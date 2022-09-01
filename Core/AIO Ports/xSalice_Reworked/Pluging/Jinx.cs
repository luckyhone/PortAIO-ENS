using System;
using EnsoulSharp;
using EnsoulSharp.SDK;
using EnsoulSharp.SDK.MenuUI;
using xSalice_Reworked.Base;
using xSalice_Reworked.Managers;
using xSalice_Reworked.Utilities;
using KeyBindType = EnsoulSharp.SDK.MenuUI.KeyBindType;
using Keys = EnsoulSharp.SDK.MenuUI.Keys;
using Menu = EnsoulSharp.SDK.MenuUI.Menu;

namespace xSalice_Reworked.Pluging
{
    internal class Jinx : Champion
    {
        public Jinx()
        {
            SpellManager.Q = new Spell(SpellSlot.Q);
            SpellManager.W = new Spell(SpellSlot.W, 1300);

            SpellManager.W.SetSkillshot(0.55f, 50, 3500, true, SpellType.Line);
            
            var combo = new Menu("Combo", "Combo");
            {
                combo.Add(new MenuBool("UseQCombo", "Use Q", true).SetValue(true));
                combo.Add(new MenuBool("UseECombo", "Use W", true).SetValue(true));
                Menu.Add(combo);
            }

            var harass = new Menu("Harass", "Harass");
            {
                harass.Add(new MenuBool("UseWHarass", "Use W", true).SetValue(false));
                harass.Add(new MenuKeyBind("FarmT", "Harass (toggle)!", Keys.N,KeyBindType.Toggle));
                ManaManager.AddManaManagertoMenu(harass, "Harass", 50);
                Menu.Add(harass);
            }

            var farm = new Menu("LaneClear", "LaneClear");
            {
                farm.Add(new MenuBool("UseQFarm", "Use Q", true).SetValue(true));
                farm.Add(new MenuBool("UseEFarm", "Use W", true).SetValue(false));
                Menu.Add(farm);
            }

            var miscMenu = new Menu("Misc", "Misc");
            {
                //aoe
                miscMenu.Add(AOESpellManager.AddHitChanceMenuCombo(true, false, false, true));
                miscMenu.Add(new MenuBool("smartKS", "Smart KS", true).SetValue(true));
                //add to menu
                Menu.Add(miscMenu);
            }

            var drawMenu = new Menu("Drawing", "Drawing");
            {
                drawMenu.Add(new MenuBool("Draw_Disabled", "Disable All", true).SetValue(false));

                var drawComboDamageMenu = new MenuBool("Draw_ComboDamage", "Draw Combo Damage", true).SetValue(true);
                var drawFill = new MenuBool("Draw_Fill", "Draw Combo Damage Fill");
                drawMenu.Add(drawComboDamageMenu);
                drawMenu.Add(drawFill);

                Menu.Add(drawMenu);
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
                Menu.Add(customMenu);
            }
        }
        private float GetComboDamage(AIBaseClient target)
        {
            double comboDamage = 0;

            if (Q.IsReady())
                comboDamage += Player.GetSpellDamage(target, SpellSlot.Q);

            if (W.IsReady())
                comboDamage += Player.GetSpellDamage(target, SpellSlot.W);


            return (float)(comboDamage + Player.GetAutoAttackDamage(target) * 2);
        }

        private void Combo()
        {
            var itemTarget = TargetSelector.GetTarget(750, DamageType.Physical);

            if (itemTarget != null)
            {
                var dmg = GetComboDamage(itemTarget);
            }

            var target = TargetSelector.GetTarget(550, DamageType.Magical);

            if (target != null && Menu["Always_Use"].GetValue<MenuBool>().Enabled)
                return;

            if (Menu["UseWCombo"].GetValue<MenuBool>().Enabled && W.IsReady())
                W.CastIfHitchanceEquals(target, HitChance.High);
        }

        private void Harass()
        {
            if (!ManaManager.HasMana("Harass"))
                return;

            var target = TargetSelector.GetTarget(550, DamageType.Magical);

            if (Menu["UseWHarass"].GetValue<MenuBool>().Enabled && W.IsReady())
                W.CastIfHitchanceEquals(target, HitChance.High);
        }


        protected void AfterAttack(AttackableUnit unit, AttackableUnit mytarget)
        {
            if (Menu["UseQCombo"].GetValue<MenuBool>().Enabled && Q.IsReady() && Orbwalker.ActiveMode == OrbwalkerMode.Combo)
            {
                if (ObjectManager.Player.CountEnemyHeroesInRange(450) == 0 && ObjectManager.Player.AttackRange < 600)
                {
                    return;
                }

                Q.Cast();
            }
        }

        private void Farm()
        {
        }

        private void CheckKs()
        {
        }
        
        protected override void Game_OnGameUpdate(EventArgs args)
        {
            if (Player.IsDead) return;

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
                case OrbwalkerMode.LastHit:
                    break;
                case OrbwalkerMode.LaneClear:
                    Farm();
                    break;
                case OrbwalkerMode.None:
                    break;
                case OrbwalkerMode.Flee:
                    break;
                default:
                    throw new ArgumentOutOfRangeException();
            }
        }
    }
}