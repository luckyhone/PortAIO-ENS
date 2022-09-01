using EnsoulSharp;
using EnsoulSharp.SDK;
using EnsoulSharp.SDK.MenuUI;
using EnsoulSharp.SDK.Rendering;
using SharpDX;
using Z.aio.Common;

namespace Z.aio.Champions
{
    class Leona : Champion
    {

        internal Leona()
        {
            this.SetSpells();
            this.SetMenu();
            this.SetEvents();
        }


        protected override void Combo()
        {
            bool useQ = RootMenu["combo"]["useq"].GetValue<MenuBool>().Enabled;
            bool useW = RootMenu["combo"]["usew"].GetValue<MenuBool>().Enabled;
            bool useE = RootMenu["combo"]["usee"].GetValue<MenuBool>().Enabled;
            bool useR = RootMenu["combo"]["user"].GetValue<MenuBool>().Enabled;


            var target = (R.Range).GetBestEnemyHeroTargetInRange();

            if (!target.IsValidTarget())
            {

                return;
            }

            
            if (target.IsValidTarget(E.Range) && useE && RootMenu["whitelist"][target.CharacterName.ToLower()].GetValue<MenuBool>().Enabled)
            {

                if (target != null)
                {
                    var pred = E.GetPrediction(target);
                    if (pred.Hitchance >= HitChance.High)
                    {
                        E.Cast(pred.CastPosition);
                    }
                }
            }
            if (target.IsValidTarget(300) && useQ)
            {

                if (target != null)
                {
                    if (RootMenu["combo"]["eq"].GetValue<MenuBool>().Enabled)
                    {
                        if (target.HasBuff("leonazenithbladeroot"))
                        {

                            Q.Cast();
                        }
                    }
                    if (!RootMenu["combo"]["eq"].GetValue<MenuBool>().Enabled)
                    {

                        Q.Cast();

                    }
                }
            }
            if (target.IsValidTarget(W.Range) && useW)
            {

                if (target != null)
                {
                    W.Cast();
                }
            }


            if (useR)
            {



                if (RootMenu["combo"]["hitr"].GetValue<MenuSlider>().Value > 1)
                {
                    if (target != null &&
                        target.CountEnemyHeroesInRange(300) >= RootMenu["combo"]["hitr"].GetValue<MenuSlider>().Value &&
                        target.IsValidTarget(R.Range))
                    {
                        var pred = R.GetPrediction(target);
                        if (pred.Hitchance >= HitChance.High)
                        {
                            R.Cast(pred.CastPosition);
                        }
                    }
                }
                if (RootMenu["combo"]["hitr"].GetValue<MenuSlider>().Value == 1)
                {
                    if (target != null &&

                        target.IsValidTarget(R.Range))
                    {
                        var pred = R.GetPrediction(target);
                        if (pred.Hitchance >= HitChance.High)
                        {
                            R.Cast(pred.CastPosition);
                        }
                    }
                }


            }
        }

        protected override void SemiR()
        {
            if (RootMenu["combo"]["semir"].GetValue<MenuKeyBind>().Active)
            {
                var target = (R.Range).GetBestEnemyHeroTargetInRange();

                if (!target.IsValidTarget())
                {

                    return;
                }


                if (target != null &&
                    target.IsValidTarget(R.Range))
                {
                    var pred = R.GetPrediction(target);
                    if (pred.Hitchance >= HitChance.High)
                    {
                        R.Cast(pred.CastPosition);
                    }
                }
            }
        }

        
    

        protected override void Farming()
        {
        }

        protected override void Drawings()
        {
            if (RootMenu["drawings"]["drawr"].GetValue<MenuBool>().Enabled)
            {
                CircleRender.Draw(Player.Position, R.Range, Color.Crimson);
            }
            if (RootMenu["drawings"]["drawe"].GetValue<MenuBool>().Enabled)
            {
                CircleRender.Draw(Player.Position, E.Range, Color.Yellow);
            }
        }

        protected override void Killsteal()
        {
            
        }

        protected override void Harass()
        {
            bool useQ = RootMenu["harass"]["useq"].GetValue<MenuBool>().Enabled;
            bool useW = RootMenu["harass"]["usew"].GetValue<MenuBool>().Enabled;
            bool useE = RootMenu["harass"]["usee"].GetValue<MenuBool>().Enabled;



            var target = (R.Range).GetBestEnemyHeroTargetInRange();

            if (!target.IsValidTarget())
            {

                return;
            }


            if (target.IsValidTarget(E.Range) && useE)
            {

                if (target != null)
                {
                    var pred = E.GetPrediction(target);
                    if (pred.Hitchance >= HitChance.High)
                    {
                        E.Cast(pred.CastPosition);
                    }
                }
            }
            if (target.IsValidTarget(1000) && useQ)
            {

                if (target != null)
                {
                    if (RootMenu["harass"]["eq"].GetValue<MenuBool>().Enabled)
                    {
                        if (target.HasBuff("leonazenithbladeroot"))
                        {

                            Q.Cast();
                        }
                    }
                    if (!RootMenu["harass"]["eq"].GetValue<MenuBool>().Enabled)
                    {

                        Q.Cast();

                    }
                }
            }
            if (target.IsValidTarget(W.Range) && useW)
            {

                if (target != null)
                {
                    W.Cast();
                }
            }
        }

        protected override void SetMenu()
        {
            RootMenu = new Menu("root", "Z.Aio Leona", true);

            ComboMenu = new Menu("combo", "Combo");
            {
                ComboMenu.Add(new MenuBool("useq", "Use Q"));
                ComboMenu.Add(new MenuBool("eq", "^- Only if E hits"));
                ComboMenu.Add(new MenuBool("usew", "Use W"));
                ComboMenu.Add(new MenuBool("usee", "Use E"));
                ComboMenu.Add(new MenuBool("user", "Use R"));
                ComboMenu.Add(new MenuSlider("hitr", "^- If Enemies hits >=", 2, 1, 5));
                ComboMenu.Add(new MenuKeyBind("semir", "Semi-Automatic R", Keys.T, KeyBindType.Press));


            }
            RootMenu.Add(ComboMenu);
            var HarassMenu = new Menu("harass", "Harass");
            {
                HarassMenu.Add(new MenuBool("useq", "Use Q"));
                HarassMenu.Add(new MenuBool("eq", "^- Only if E hits"));
                HarassMenu.Add(new MenuBool("usew", "Use W"));
                HarassMenu.Add(new MenuBool("usee", "Use E"));


            }
            RootMenu.Add(HarassMenu);
            WhiteList = new Menu("whitelist", "E White List");
            {
                foreach (var target in GameObjects.EnemyHeroes)
                {
                    WhiteList.Add(new MenuBool(target.CharacterName.ToLower(), "Enable: " + target.CharacterName));
                }
            }
            RootMenu.Add(WhiteList);
            DrawMenu = new Menu("drawings", "Drawings");
            {
                DrawMenu.Add(new MenuBool("drawe", "Show E Range"));
                DrawMenu.Add(new MenuBool("drawr", "Show R Range"));

            }
            RootMenu.Add(DrawMenu);



            RootMenu.Attach();
        }

        protected override void SetSpells()
        {
            Q = new Spell(SpellSlot.Q, 0);
            W = new Spell(SpellSlot.W, 275);
            E = new Spell(SpellSlot.E, 875);
            R = new Spell(SpellSlot.R, 1200);

            E.SetSkillshot(0.25f, 20, 2400, false, SpellType.Line, HitChance.None);
            R.SetSkillshot(1.3f, 300, float.MaxValue, false, SpellType.Circle);
        }
    }
}