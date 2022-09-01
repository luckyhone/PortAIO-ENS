using EnsoulSharp;
using EnsoulSharp.SDK;
using EnsoulSharp.SDK.MenuUI;
using EnsoulSharp.SDK.Rendering;
using SharpDX;
using Z.aio.Common;
using Extensions = Z.aio.Common.Extensions;

namespace Z.aio.Champions
{
    class Blitzcrank : Champion
    {
        internal Blitzcrank()
        {
            this.SetSpells();
            this.SetMenu();
            this.SetEvents();
        }

        protected override void Combo()
        {
            var useQ = RootMenu["combo"]["useq"].GetValue<MenuBool>().Enabled;
            var useE = RootMenu["combo"]["usee"].GetValue<MenuBool>().Enabled;
            var useR = RootMenu["combo"]["user"].GetValue<MenuBool>().Enabled;
            float enemies = RootMenu["combo"]["hitr"].GetValue<MenuSlider>().Value;
            var target = (Q.Range).GetBestEnemyHeroTargetInRange();

            if (!target.IsValidTarget())
            {
                return;
            }
            
            if (Q.IsReady() && useQ && target.IsValidTarget(Q.Range))
            {

                if (target != null && !RootMenu["black"][target.CharacterName.ToLower()].GetValue<MenuBool>().Enabled)
                {
                    var pred = Q.GetPrediction(target);
                    if (pred.Hitchance >= HitChance.High)
                    {
                        Q.Cast(pred.CastPosition);
                    }
                }
            }
            if (E.IsReady() && useE)
            {

                if (target != null)
                {
                    if (!RootMenu["combo"]["eq"].GetValue<MenuBool>().Enabled && target.IsValidTarget(E.Range))
                    {
                        E.Cast();
                    }
                    if (RootMenu["combo"]["eq"].GetValue<MenuBool>().Enabled)
                    {
                        if (target.HasBuff("rocketgrab2"))
                        {
                            E.Cast();
                        }
                    }
                }
            }

            if (R.IsReady() && useR && target.IsValidTarget(R.Range))
            {

                if (target != null && enemies <= Player.CountEnemyHeroesInRange(R.Range))
                {
                    R.Cast();
                }
            }
        }
        
        protected override void SemiR()
        {

            if (RootMenu["qset"]["autoq"].GetValue<MenuBool>().Enabled)
            {
                var target = Q.Range.GetBestEnemyHeroTargetInRange();

                if (!target.IsValidTarget())
                {
                    return;
                }

                if (Q.IsReady() && target.IsValidTarget(Q.Range))
                {

                    if (target != null)
                    {
                        var pred = Q.GetPrediction(target);
                        if (pred.Hitchance == HitChance.Immobile)
                        {

                            Q.Cast(pred.CastPosition);

                        }
                    }
                }
            }
            if (RootMenu["qset"]["grabq"].GetValue<MenuKeyBind>().Active)
            {
                var target = Q.Range.GetBestEnemyHeroTargetInRange();

                if (!target.IsValidTarget())
                {
                    return;
                }

                if (Q.IsReady() && target.IsValidTarget(Q.Range))
                {

                    if (target != null)
                    {
                        var pred = Q.GetPrediction(target);
                        if (pred.Hitchance >= HitChance.High)
                        {
                            Q.Cast(pred.CastPosition);
                        }
                    }
                }
            }
        }
        protected override void Farming()
        {
            
        }

        protected override void Drawings()
        {
            if (RootMenu["drawings"]["qmin"].GetValue<MenuBool>().Enabled)
            {
                CircleRender.Draw(Player.Position, RootMenu["qset"]["minq"].GetValue<MenuSlider>().Value, Color.Crimson);
            }
            if (RootMenu["drawings"]["qmax"].GetValue<MenuBool>().Enabled)
            {
                CircleRender.Draw(Player.Position, RootMenu["qset"]["maxq"].GetValue<MenuSlider>().Value, Color.Yellow);
            }
            if (RootMenu["drawings"]["drawr"].GetValue<MenuBool>().Enabled)
            {
                CircleRender.Draw(Player.Position, R.Range, Color.Crimson);
            }
        }
        protected override void Killsteal()
        {
            if (Q.IsReady() &&
                RootMenu["killsteal"]["ksq"].GetValue<MenuBool>().Enabled)
            {
                var bestTarget =Q.GetBestKillableHero(DamageType.Magical);
                if (bestTarget != null &&
                    !bestTarget.IsValidTarget(Player.GetRealAutoAttackRange(bestTarget)) &&
                    Player.GetSpellDamage(bestTarget, SpellSlot.Q) > bestTarget.Health)
                {
                    Q.Cast(bestTarget);
                }
            }

            if (R.IsReady() &&
                RootMenu["killsteal"]["ksr"].GetValue<MenuBool>().Enabled)
            {
                var bestTarget = R.GetBestKillableHero(DamageType.Magical);
                if (bestTarget != null &&
                    !bestTarget.IsValidTarget(Player.GetRealAutoAttackRange(bestTarget)) &&
                    Player.GetSpellDamage(bestTarget, SpellSlot.R) > bestTarget.Health)
                {
                    R.Cast();
                }
            }
        }

        protected override void Harass()
        {
        }

        protected override void SetMenu()
        {
            RootMenu = new Menu("root", "Z.Aio Blitzcrank", true);

            ComboMenu = new Menu("combo", "Combo");
            {
                ComboMenu.Add(new MenuBool("useq", "Use Q"));
                ComboMenu.Add(new MenuBool("usee", "Use E "));
                ComboMenu.Add(new MenuBool("eq", "^- Only Q and after E",false));
                ComboMenu.Add(new MenuBool("user", "Use R"));
                ComboMenu.Add(new MenuSlider("hitr", "^- If Enemies hits >=", 2, 1, 5));
            }
            RootMenu.Add(ComboMenu);
            var QSet = new Menu("qset", "Q Settings");
            {
                QSet.Add(new MenuKeyBind("grabq", "Semi-Automatic Q", Keys.T, KeyBindType.Press));
                QSet.Add(new MenuBool("autoq", "Auto Q", true));
                QSet.Add(new MenuSlider("minq", "Min Q Dist", 300, 10, 400));
                QSet.Add(new MenuSlider("maxq", "Max Q Dist", 900, 500, 900));
            }
            RootMenu.Add(QSet);
            WhiteList = new Menu("black", "Q BlackList");
            {
                foreach (var target in GameObjects.EnemyHeroes)
                {
                    WhiteList.Add(new MenuBool(target.CharacterName.ToLower(), "Block Q: " + target.CharacterName, false));
                }
            }

            RootMenu.Add(WhiteList);
            var DrawMenu = new Menu("drawings", "Drawings");
            {
                DrawMenu.Add(new MenuBool("qmax", "Show Q Max range"));
                DrawMenu.Add(new MenuBool("qmin", "Show Q Min range",false));
                DrawMenu.Add(new MenuBool("drawr", "Show R Range"));
            }
            RootMenu.Add(DrawMenu);
            KillstealMenu = new Menu("killsteal", "Kill Steal");
            {
                KillstealMenu.Add(new MenuBool("ksq", "Use Q"));
                KillstealMenu.Add(new MenuBool("ksr", "Use R"));
            }
            RootMenu.Add(KillstealMenu);
        

            RootMenu.Attach();
        }

        protected override void SetSpells()
        {
            Q = new Spell(SpellSlot.Q, 900);
            W = new Spell(SpellSlot.W, 0);
            E = new Spell(SpellSlot.E, 300);
            R = new Spell(SpellSlot.R, 600);

            Q.SetSkillshot(0.25f, 70, 2000, true, SpellType.Line);
        }
    }
}