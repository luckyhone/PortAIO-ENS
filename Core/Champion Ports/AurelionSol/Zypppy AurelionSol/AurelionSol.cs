using EnsoulSharp;
using EnsoulSharp.SDK;
using EnsoulSharp.SDK.MenuUI;
using SharpDX;

namespace Zypppy_AurelionSol
{
    using System;
    using System.Drawing;
    using System.Linq;

    internal class AurelionSol
    {
        public static Menu Menu = new Menu("AurelionSol by Zypppy", "AurelionSol by Zypppy", true);
        public static AIHeroClient Player = ObjectManager.Player;
        public static Spell Q, W, W2, E, R;
        private MissileClient missiles;

        public void LoadSpells()
        {
            Q = new Spell(SpellSlot.Q, 1500f);//AurelionSolQ AurelionSolQCancelButton
            Q.SetSkillshot(0.5f, 110f, 850f, false, SpellType.Line);
            W = new Spell(SpellSlot.W, 350f);//AurelionSolW
            W2 = new Spell(SpellSlot.W, 650f);//AurelionSolWToggleOff aurelionsolwactive
            E = new Spell(SpellSlot.E, 400f);//AurelionSolE
            R = new Spell(SpellSlot.R, 1419f);//AurelionSolR
            R.SetSkillshot(0.5f, 150, 4500f, false, SpellType.Line);
        }
        public AurelionSol()
        {
            var ComboMenu = new Menu("combo", "Combo");
            {
                ComboMenu.Add(new MenuBool("useq", "Use Q"));
                ComboMenu.Add(new MenuBool("usew", "Use W"));
                ComboMenu.Add(new MenuBool("user", "Use R"));
                ComboMenu.Add(new MenuSlider("hitr", "R Minimum Enemeies Hit", 3, 1, 5));
                ComboMenu.Add(new MenuKeyBind("key", "Manual R Key:", Keys.T, KeyBindType.Press));
            }
            Menu.Add(ComboMenu);
            var HarassMenu = new Menu("harass", "Harass");
            {
                HarassMenu.Add(new MenuBool("useq", "Use Q"));
                HarassMenu.Add(new MenuSlider("manaq", "Minimum Mana To Use Q", 70, 0, 100));
                HarassMenu.Add(new MenuBool("usew", "Use W"));
                HarassMenu.Add(new MenuSlider("manaw", "Minimum Mana To Use W", 70, 0, 100));
            }
            Menu.Add(HarassMenu);
            var MiscMenu = new Menu("misc", "Misc");
            {
                MiscMenu.Add(new MenuBool("usewlock", "Use Outer W Movement Lock", false));
                MiscMenu.Add(new MenuBool("aa", "Disable AA Combo When W Enabled", false));
                MiscMenu.Add(new MenuBool("aa2", "Disable AA Harass When W Enabled", false));
            }
            Menu.Add(MiscMenu);
            var KillstealMenu = new Menu("killsteal", "Killsteal");
            {
                KillstealMenu.Add(new MenuBool("RKS", "Use R to Killsteal"));
            }
            Menu.Add(KillstealMenu);
            var DrawingsMenu = new Menu("drawings", "Drawings");
            {
                DrawingsMenu.Add(new MenuBool("drawq", "Draw Q Range"));
                DrawingsMenu.Add(new MenuBool("drawq2", "Draw Circle Around Q"));
                DrawingsMenu.Add(new MenuBool("draww", "Draw W Range"));
                DrawingsMenu.Add(new MenuBool("draww2", "Draw Active W Range"));
                DrawingsMenu.Add(new MenuBool("drawr", "Draw R Range"));
            }
            Menu.Add(DrawingsMenu);
            Menu.Attach();

            Drawing.OnDraw += Render_OnPresent;
            Game.OnUpdate += Game_OnUpdate;
            GameObject.OnCreate += OnCreate;
            GameObject.OnDelete += OnDestroy;

            LoadSpells();
            Console.WriteLine("AurelionSol by Zypppy - Loaded");
        }

        public void OnCreate(GameObject obj, EventArgs args)
        {
            var missile = obj as MissileClient;
            if (missile == null)
            {
                return;
            }

            if (missile.SpellCaster == null || !missile.SpellCaster.IsValid ||
                missile.SpellCaster.Team != ObjectManager.Player.Team)
            {
                return;
            }
            var hero = missile.SpellCaster as AIHeroClient;
            if (hero == null)
            {
                return;
            }
            if (missile.SData.Name == "AurelionSolQMissile")
            {
                missiles = missile;
            }

        }
        private void OnDestroy(GameObject obj, EventArgs args)
        {
            var missile = obj as MissileClient;
            if (missile == null || !missile.IsValid)
            {
                return;
            }

            if (missile.SpellCaster == null || !missile.SpellCaster.IsValid ||
                missile.SpellCaster.Team != ObjectManager.Player.Team)
            {
                return;
            }
            var hero = missile.SpellCaster as AIHeroClient;
            if (hero == null)
            {
                return;
            }
            if (missile.SData.Name == "AurelionSolQMissile")
            {
                missiles = null;
            }
        }
        private void Render_OnPresent(EventArgs args)
        {
            Vector2 maybeworks;
            var heropos = Drawing.WorldToScreen(Player.Position, out maybeworks);
            var xaOffset = (int)maybeworks.X;
            var yaOffset = (int)maybeworks.Y;

            if (Q.IsReady() && Menu["drawings"]["drawq"].GetValue<MenuBool>().Enabled)
            {
                Render.Circle.DrawCircle(Player.Position, Q.Range, Color.Indigo);
            }
            if (Q.IsReady() && Menu["drawings"]["drawq2"].GetValue<MenuBool>().Enabled)
            {
                if (missiles != null)
                {
                    Render.Circle.DrawCircle(missiles.Position, 250, Color.DeepPink);
                }
            }
            if (W.IsReady() && Menu["drawings"]["draww2"].GetValue<MenuBool>().Enabled)
            {
                Render.Circle.DrawCircle(Player.Position, W2.Range, Color.DeepSkyBlue);
            }
            if (W.IsReady() && Menu["drawings"]["draww"].GetValue<MenuBool>().Enabled)
            {
                Render.Circle.DrawCircle(Player.Position, W.Range, Color.Cornsilk);
            }
            if (R.IsReady() && Menu["drawings"]["drawr"].GetValue<MenuBool>().Enabled)
            {
                Render.Circle.DrawCircle(Player.Position, R.Range, Color.Crimson);
            }
        }
        private void Game_OnUpdate(EventArgs args)
        {
            if (Player.IsDead || MenuGUI.IsChatOpen)
            {
                return;
            }
            switch (Orbwalker.ActiveMode)
            {
                case OrbwalkerMode.Combo:
                    OnCombo();
                    break;
                case OrbwalkerMode.Harass:
                    OnHarass();
                    break;
                case OrbwalkerMode.LaneClear:
                    break;
            }
            if (Menu["combo"]["key"].GetValue<MenuKeyBind>().Active)
            {
                ManualR();
            }
            Killsteal();
            WLock();
        }
        public static AIHeroClient GetBestKillableHero(Spell spell, DamageType damageType = DamageType.True,
            bool ignoreShields = false)
        {
            return TargetSelector.GetTargets(spell.Range, DamageType.Magical).FirstOrDefault(x=>x.IsValidTarget(spell.Range));
        }
        private void Killsteal()
        {
            if (R.IsReady() && Menu["killsteal"]["RKS"].GetValue<MenuBool>().Enabled)
            {
                var besttarget = GetBestKillableHero(R, DamageType.Magical, false);
                var RPrediction = R.GetPrediction(besttarget);
                if (besttarget != null && Player.GetSpellDamage(besttarget, SpellSlot.R) >= besttarget.Health && besttarget.IsValidTarget(R.Range))
                {
                    if (RPrediction.Hitchance >= HitChance.High)
                    {
                        R.Cast(RPrediction.CastPosition);
                    }
                }
            }
        }
        public static AIHeroClient GetBestEnemyHeroTarget()
        {
            return GetBestEnemyHeroTargetInRange(float.MaxValue);
        }

        public static AIHeroClient GetBestEnemyHeroTargetInRange(float range)
        {
            var target = TargetSelector.GetTarget(range,DamageType.Magical);
            if (target != null && target.IsValidTarget())
            {
                return target;
            }

            var firstTarget = TargetSelector.GetTargets(range,DamageType.Magical)
                .FirstOrDefault(t => t.IsValidTarget());
            if (firstTarget != null)
            {
                return firstTarget;
            }

            return null;
        }

        public static bool AnyWallInBetween(Vector3 startPos, Vector2 endPos)
        {
            for (var i = 0; i < startPos.Distance(endPos); i++)
            {
                var point = NavMesh.WorldToGrid(startPos.Extend(endPos, i));
                if(point.IsWall() || point.IsBuilding())
                {
                    return true;
                }
            }

            return false;
        }
        
        private void WLock()
        {
            if (Menu["misc"]["usewlock"].GetValue<MenuBool>().Enabled)
            {
                if (Player.GetSpell(SpellSlot.W).ToggleState == (SpellToggleState)2)
                {
                    Orbwalker.AttackEnabled = false;
                }
                if (Player.GetSpell(SpellSlot.W).ToggleState == 0)
                {
                    Orbwalker.AttackEnabled = true;
                }
                var target = GetBestEnemyHeroTargetInRange(W2.Range);
                {
                    if (target.IsValidTarget(W2.Range) && target != null)
                    {
                        if (target.ServerPosition.Distance(Player.ServerPosition) < W2.Range)
                        {
                            if (Player.GetSpell(SpellSlot.W).ToggleState == (SpellToggleState)2)
                            {
                                Orbwalker.Move(target.ServerPosition.Extend(Player.ServerPosition, W2.Range));
                            }
                        }
                    }
                }
            }
            var target2 = GetBestEnemyHeroTargetInRange(W2.Range + 200);
            if (target2.IsValidTarget(W2.Range + 200) && target2 != null)
            {
                if (target2.ServerPosition.Distance(Player.ServerPosition) > W2.Range - 50)
                {
                    if (Player.GetSpell(SpellSlot.W).ToggleState == (SpellToggleState)2 && Menu["combo"]["usewlock"].GetValue<MenuBool>().Enabled)
                    {
                        Orbwalker.Move(target2.ServerPosition);
                    }
                }
            }
        }
        private void OnCombo()
        {
            var target = GetBestEnemyHeroTargetInRange(R.Range);
            if (!target.IsValidTarget())
            {
                return;
            }

            bool useQ = Menu["combo"]["useq"].GetValue<MenuBool>().Enabled;
            if (Q.IsReady() && useQ)
            {
                switch (Player.GetSpell(SpellSlot.Q).ToggleState)
                {
                    case (SpellToggleState)1:
                        if (target.IsValidTarget(Q.Range) && Player.GetSpell(SpellSlot.Q).ToggleState == (SpellToggleState)1)
                        {
                            Q.Cast(target);
                        }
                        break;
                    case (SpellToggleState)2:
                        if (missiles != null && target.IsValidTarget(200f, false,missiles.Position) &&
                            Player.GetSpell(SpellSlot.Q).ToggleState == (SpellToggleState)2)
                        {
                            Q.Cast();
                        }
                        break;
                }
            }
            
            bool AA = Menu["misc"]["aa"].GetValue<MenuBool>().Enabled;
            if (AA && target.IsValidTarget(Player.GetCurrentAutoAttackRange()))
            {
                if (Player.GetSpell(SpellSlot.W).ToggleState == (SpellToggleState)2)
                {
                    Orbwalker.AttackEnabled = false;
                }
                if (Player.GetSpell(SpellSlot.W).ToggleState == 0)
                {
                    Orbwalker.AttackEnabled = true;
                }
            }

            bool useW = Menu["combo"]["usew"].GetValue<MenuBool>().Enabled;
            if (W.IsReady() && useW)
            {
                switch (Player.GetSpell(SpellSlot.W).ToggleState)
                {
                    case (SpellToggleState)0:
                        if (target.IsValidTarget(W2.Range) && Player.GetSpell(SpellSlot.W).ToggleState == 0)
                        {
                            W2.Cast();
                        }
                        break;
                    case (SpellToggleState)2:
                        if (target.IsValidTarget(W.Range) && Player.GetSpell(SpellSlot.W).ToggleState == (SpellToggleState)2)
                        {
                            W.Cast();
                        }
                        break;
                }
            }
            
            bool useR = Menu["combo"]["user"].GetValue<MenuBool>().Enabled;
            if (R.IsReady() && target.IsValidTarget(R.Range) && useR)
            {
                R.CastIfWillHit(target, Menu["combo"]["hitr"].GetValue<MenuSlider>().Value - 1);
            }
        }
        private void OnHarass()
        {
            var target = GetBestEnemyHeroTargetInRange(Q.Range);

            if (!target.IsValidTarget())
            {
                return;
            }

            bool useQ = Menu["harass"]["useq"].GetValue<MenuBool>().Enabled;
            float manaQ = Menu["harass"]["manaq"].GetValue<MenuSlider>().Value;
            if (Q.IsReady() && useQ)
            {
                switch (Player.GetSpell(SpellSlot.Q).ToggleState)
                {
                    case (SpellToggleState)1:
                        if (target.IsValidTarget(Q.Range) && Player.ManaPercent >= manaQ && Player.GetSpell(SpellSlot.Q).ToggleState == (SpellToggleState)1)
                        {
                            Q.Cast(target);
                        }
                        break;
                    case (SpellToggleState)2:
                        if (missiles != null && target.IsValidTarget(200f, false, missiles.Position) &&
                            Player.GetSpell(SpellSlot.Q).ToggleState == (SpellToggleState)2)
                        {
                            Q.Cast();
                        }
                        break;
                }
            }

            bool AA = Menu["misc"]["aa2"].GetValue<MenuBool>().Enabled;
            if (AA && target.IsValidTarget(Player.GetCurrentAutoAttackRange()))
            {
                if (Player.GetSpell(SpellSlot.W).ToggleState == (SpellToggleState)2)
                {
                    Orbwalker.AttackEnabled = false;
                }
                if (Player.GetSpell(SpellSlot.W).ToggleState == 0)
                {
                    Orbwalker.AttackEnabled = true;
                }
            }

            bool useW = Menu["harass"]["usew"].GetValue<MenuBool>().Enabled;
            float manaW = Menu["harass"]["manaw"].GetValue<MenuSlider>().Value;
            if (W.IsReady() && useW)
            {
                switch (Player.GetSpell(SpellSlot.W).ToggleState)
                {
                    case 0:
                        if (target.IsValidTarget(W2.Range) && Player.ManaPercent >= manaW && Player.GetSpell(SpellSlot.W).ToggleState == 0)
                        {
                            W2.Cast();
                        }
                        break;
                    case (SpellToggleState)2:
                        if (target.IsValidTarget(W.Range) || !target.IsValidTarget(W2.Range) || Player.ManaPercent < manaW && Player.GetSpell(SpellSlot.W).ToggleState == (SpellToggleState)2)
                        {
                            W.Cast();
                        }
                        break;
                }
            }
        }
        private void ManualR()
        {
            var target = GetBestEnemyHeroTargetInRange(R.Range);
            Player.IssueOrder(GameObjectOrder.MoveTo, Game.CursorPos);
            if (R.IsReady() && target.IsValidTarget(R.Range))
            {
                R.Cast(target);
            }
        }
    }
}