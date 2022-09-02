using System;
using System.Linq;
using EnsoulSharp;
using EnsoulSharp.SDK;
using EnsoulSharp.SDK.MenuUI;
using EnsoulSharp.SDK.Rendering;
using SharpDX;

namespace Flowers__Illaoi
{
    internal class Program
    {
        private static AIHeroClient Me;
        private static Menu Menu;
        private static Spell Q, W, E, R;
        private static SpellSlot Ignite = SpellSlot.Unknown;
        private static HpBarDraw DrawHpBar = new HpBarDraw();
        private static string[] AutoEnableList =
        {
            "Annie", "Ahri", "Akali", "Anivia", "Annie", "Brand", "Cassiopeia", "Diana", "Evelynn", "FiddleSticks", "Fizz", "Gragas", "Heimerdinger", "Karthus",
            "Kassadin", "Katarina", "Kayle", "Kennen", "Leblanc", "Lissandra", "Lux", "Malzahar", "Mordekaiser", "Morgana", "Nidalee", "Orianna",
            "Ryze", "Sion", "Swain", "Syndra", "Teemo", "TwistedFate", "Veigar", "Viktor", "Vladimir", "Xerath", "Ziggs", "Zyra", "Velkoz", "Azir", "Ekko",
            "Ashe", "Caitlyn", "Corki", "Draven", "Ezreal", "Graves", "Jayce", "Jinx", "KogMaw", "Lucian", "MasterYi", "MissFortune", "Quinn", "Shaco", "Sivir",
            "Talon", "Tristana", "Twitch", "Urgot", "Varus", "Vayne", "Yasuo", "Zed", "Kindred", "AurelionSol"
        };
        
        public static void Loads()
        {
            //Bootstrap.Init();

            if (GameObjects.Player.CharacterName.ToLower() != "illaoi")
            {
                return;
            }

            Me = GameObjects.Player;

            Game.Print(Me.CharacterName + " : This is Old Version and i dont update it anymore, Please Use Flowers' Series!");

            LoadSpell();
            LoadMenu();
            LoadEvent();
        }
        
        private static void LoadSpell()
        {
            Q = new Spell(SpellSlot.Q, 800f);
            W = new Spell(SpellSlot.W, 400f);
            E = new Spell(SpellSlot.E, 900f);
            R = new Spell(SpellSlot.R, 450f);

            Q.SetSkillshot(0.75f, 100f, float.MaxValue, false,SpellType.Line);
            E.SetSkillshot(0.066f, 50f, 1900f, true,SpellType.Line);

            Ignite = Me.GetSpellSlot("SummonerDot");
        }
        
        private static void LoadMenu()
        {
            Menu = new Menu("Illaoi - The Kraken Priestess", "Illaoi - The Kraken Priestess", true).Attach();
            Menu.Add(new MenuSeparator("OLD", "This Is Old Version and i dont update it"));
            Menu.Add(new MenuSeparator("OLD1", "Please Use Flowers' Series"));
            var ComboMenu = Menu.Add(new Menu("Combo", "Combo"));
            {
                ComboMenu.Add(new MenuBool("Q", "Use Q", true));
                ComboMenu.Add(new MenuBool("QGhost", "Use Q | To Ghost", true));
                ComboMenu.Add(new MenuBool("W", "Use W", true));
                ComboMenu.Add(new MenuBool("WOutRange", "Use W | Out of Attack Range"));
                ComboMenu.Add(new MenuBool("WUlt", "Use W | Ult Active", true));
                ComboMenu.Add(new MenuBool("E", "Use E", true));
                ComboMenu.Add(new MenuBool("R", "Use R", true));
                ComboMenu.Add(new MenuBool("RSolo", "Use R | 1v1 Mode", true));
                ComboMenu.Add(new MenuSlider("RCount", "Use R | Counts Enemies >=", 2, 1, 5));
                ComboMenu.Add(new MenuBool("Ignite", "Use Ignite", true));
                ComboMenu.Add(new MenuBool("Item", "Use Item", true));
            }

            var HarassMenu = Menu.Add(new Menu("Harass", "Harass"));
            {
                HarassMenu.Add(new MenuBool("Q", "Use Q", true));
                HarassMenu.Add(new MenuBool("W", "Use W", true));
                HarassMenu.Add(new MenuBool("WOutRange", "Use W | Only Out of Attack Range", true));
                HarassMenu.Add(new MenuBool("E", "Use E", true));
                HarassMenu.Add(new MenuBool("Ghost", "Attack Ghost", true));
                HarassMenu.Add(new MenuSlider("Mana", "Harass Mode | Min ManaPercent >= %", 60));
            }

            var LaneClearMenu = Menu.Add(new Menu("LaneClear", "Lane Clear"));
            {
                LaneClearMenu.Add(new MenuSliderButton("Q", "Use Q | Min Hit Count >= ", 3, 1, 5, true));
                LaneClearMenu.Add(new MenuSlider("Mana", "LaneClear Mode | Min ManaPercent >= %", 60));
            }

            var JungleClearMenu = Menu.Add(new Menu("JungleClear", "Jungle Clear"));
            {
                JungleClearMenu.Add(new MenuBool("Q", "Use Q", true));
                JungleClearMenu.Add(new MenuBool("W", "Use W", true));
                JungleClearMenu.Add(new MenuBool("Item", "Use Item", true));
                JungleClearMenu.Add(new MenuSlider("Mana", "JungleClear Mode | Min ManaPercent >= %", 60));
            }

            var KillStealMenu = Menu.Add(new Menu("KillSteal", "Kill Steal"));
            {
                KillStealMenu.Add(new MenuBool("Q", "Use Q", true));
                KillStealMenu.Add(new MenuBool("E", "Use E", true));
                KillStealMenu.Add(new MenuBool("R", "Use R", true));
                KillStealMenu.Add(new MenuSeparator("RList", "R List"));
                if (GameObjects.EnemyHeroes.Any())
                {
                    GameObjects.EnemyHeroes.ForEach(i => KillStealMenu.Add(new MenuBool(i.CharacterName.ToLower(), i.CharacterName, AutoEnableList.Contains(i.CharacterName))));
                }
            }

            var EBlacklist = Menu.Add(new Menu("EBlackList", "E BlackList"));
            {
                EBlacklist.Add(new MenuSeparator("Adapt", "Only Adapt to Harass & KillSteal & Anti GapCloser Mode!"));
                if (GameObjects.EnemyHeroes.Any())
                {
                    GameObjects.EnemyHeroes.ForEach(i => EBlacklist.Add(new MenuBool(i.CharacterName.ToLower(), i.CharacterName, false)));
                }
            }

            /*var ItemMenu = Menu.Add(new Menu("Items", "Items"));
            {
                ItemMenu.Add(new MenuBool("Youmuus", "Use Youmuus", true));
                ItemMenu.Add(new MenuBool("Cutlass", "Use Cutlass", true));
                ItemMenu.Add(new MenuBool("Botrk", "Use Botrk", true));
                ItemMenu.Add(new MenuBool("Hydra", "Use Hydra", true));
                ItemMenu.Add(new MenuBool("Tiamat", "Use Tiamat", true));
            }*/

            var SkinMenu = Menu.Add(new Menu("Skin", "Skin"));
            {
                SkinMenu.Add(new MenuBool("Enable", "Enabled", false));
                SkinMenu.Add(new MenuList("SkinName", "Select Skin", new[] { "Classic", "Void Bringer" }));
            }

            var MiscMenu = Menu.Add(new Menu("Misc", "Misc"));
            {
                MiscMenu.Add(new MenuBool("EGap", "Use E Anti GapCloset", true));
            }

            var DrawMenu = Menu.Add(new Menu("Draw", "Draw"));
            {
                DrawMenu.Add(new MenuBool("Q", "Q Range"));
                DrawMenu.Add(new MenuBool("W", "W Range"));
                DrawMenu.Add(new MenuBool("E", "E Range"));
                DrawMenu.Add(new MenuBool("R", "R Range"));
                DrawMenu.Add(new MenuBool("DrawDamage", "Draw Combo Damage", true));
            }

            //DelayAction.Add(5000, () => Variables.Enabled = true);
        }
        
        private static void LoadEvent()
        {
            AntiGapcloser.OnGapcloser += OnGapCloser;
            AIBaseClient.OnDoCast += OnSpellCast;
            Orbwalker.OnAfterAttack += OnAction;
            Game.OnUpdate += OnUpdate;
            Drawing.OnDraw += OnDraw;
        }

        private static void OnUpdate(EventArgs args)
        {
            if (Me.IsDead)
            {
                return;
            }

            if (Orbwalker.ActiveMode == OrbwalkerMode.Combo)
            {
                Combo();
            }

            if (Orbwalker.ActiveMode == OrbwalkerMode.Harass)
            {
                Harass();
            }

            if (Orbwalker.ActiveMode == OrbwalkerMode.LaneClear)
            {
                Lane();
                Jungle();
            }

            KillSteal();
            Skin();
        }
        
        private static void Combo()
        {
            var target = TargetSelector.GetTarget(Q.Range, DamageType.Physical);
            var Ghost = ObjectManager.Get<AIMinionClient>().Where(x => x.IsEnemy && x.IsHPBarRendered && x.IsValidTarget(Q.Range)).FirstOrDefault(x => x.HasBuff("illaoiespirit"));

            if (target != null && !target.IsDead && !target.IsZombie() && target.IsHPBarRendered)
            {
                if (Menu["Combo"].GetValue<MenuBool>("Q").Enabled && Q.IsReady() && target.IsValidTarget(Q.Range) && Q.CanCast(target) && !((W.IsReady() || Me.HasBuff("IllaoiW")) && target.IsValidTarget(W.Range)))
                {
                    Q.Cast(target);
                }

                if (Menu["Combo"].GetValue<MenuBool>("E").Enabled && E.IsReady() && !InAutoAttackRange(target) && target.IsValidTarget(E.Range))
                {
                    E.Cast(target);
                }

                if (Menu["Combo"].GetValue<MenuBool>("R").Enabled && R.IsReady() && target.IsValidTarget(R.Range))
                {
                    if (Menu["Combo"].GetValue<MenuBool>("RSolo").Enabled && target.Health <= GetDamage(target) + Me.GetAutoAttackDamage(target) * 3 && Me.CountEnemyHeroesInRange(R.Range) == 1)
                    {
                        R.Cast(target);
                    }

                    if (Me.CountEnemyHeroesInRange(R.Range - 50) >= Menu["Combo"]["RCount"].GetValue<MenuSlider>().Value)
                    {
                        R.Cast();
                    }
                }

                if (Menu["Combo"].GetValue<MenuBool>("Ignite").Enabled && Ignite != SpellSlot.Unknown && Ignite.IsReady() && target.IsValidTarget(600) && target.HealthPercent < 20)
                {
                    Me.Spellbook.CastSpell(Ignite, target);
                }
            }
            else if (target == null && Ghost != null)
            {
                if (Ghost != null && Q.IsReady() && Menu["Combo"].GetValue<MenuBool>("Q").Enabled && Menu["Combo"].GetValue<MenuBool>("QGhost").Enabled)
                {
                    Q.Cast(Ghost.Position);
                }
            }
        }
        
        private static void Harass()
        {
            if (Me.ManaPercent >= Menu["Harass"].GetValue<MenuSlider>("Mana").Value && !Me.IsUnderEnemyTurret())
            {
                var target = TargetSelector.GetTarget(Q.Range, DamageType.Physical);
                var Ghost = ObjectManager.Get<AIHeroClient>().Where(x => x.IsEnemy && x.IsHPBarRendered && x.IsValidTarget(Q.Range)).FirstOrDefault(x => x.HasBuff("illaoiespirit"));

                if (target != null && !target.IsDead && !target.IsZombie() && target.IsHPBarRendered)
                {
                    if (Menu["Harass"].GetValue<MenuBool>("Q").Enabled && Q.IsReady() && target.IsValidTarget(Q.Range))
                    {
                        Q.Cast(target);
                    }

                    if (Menu["Harass"].GetValue<MenuBool>("W").Enabled && W.IsReady() && Menu["Harass"].GetValue<MenuBool>("WOutRange").Enabled && !InAutoAttackRange(target) && target.IsValidTarget(W.Range))
                    {
                        W.Cast();
                    }

                    if (Menu["Harass"].GetValue<MenuBool>("E").Enabled && E.IsReady() && E.CanCast(target) && !Menu["EBlackList"].GetValue<MenuBool>(target.CharacterName.ToLower()).Enabled && !InAutoAttackRange(target) && target.IsValidTarget(E.Range))
                    {
                        E.Cast(target);
                    }
                }
                else if (target == null && Ghost != null)
                {
                    if (Q.IsReady() && Menu["Harass"].GetValue<MenuBool>("Q").Enabled)
                        Q.Cast(Ghost);

                    if (W.IsReady() && Menu["Harass"].GetValue<MenuBool>("W").Enabled)
                    {
                        W.Cast();
                    }

                    if (Menu["Harass"].GetValue<MenuBool>("Ghost").Enabled)
                    {
                        Orbwalker.ForceTarget = Ghost;
                    }
                }
            }
        }
        
        private static void Lane()
        {
            if (Me.ManaPercent >= Menu["LaneClear"].GetValue<MenuSlider>("Mana").Value)
            {
                if (Menu["LaneClear"].GetValue<MenuSliderButton>("Q").Enabled && Q.IsReady())
                {
                    var Minions = GameObjects.EnemyMinions.Where(x => x.IsValidTarget(Q.Range)).ToList();

                    if (Minions.Count() > 0)
                    {
                        var QFarm = Q.GetLineFarmLocation(Minions, Q.Width);

                        if (QFarm.MinionsHit >= Menu["LaneClear"].GetValue<MenuSliderButton>("Q").Value)
                        {
                            Q.Cast(QFarm.Position);
                        }
                    }
                }
            }
        }

        private static void Jungle()
        {
            var Mobs = GameObjects.Jungle.Where(x => x.IsValidTarget(Q.Range) && !GameObjects.JungleSmall.Contains(x)).ToList();

            if (Mobs.Count() > 0)
            {
                if (Me.ManaPercent >= Menu["JungleClear"].GetValue<MenuSlider>("Mana").Value)
                {
                    if (Menu["JungleClear"].GetValue<MenuBool>("Q").Enabled && Q.IsReady() && !Orbwalker.IsAutoAttack(Me.CharacterName))
                    {
                        Q.Cast(Mobs.FirstOrDefault());
                    }
                }
            }
        }
        
        private static void KillSteal()
        {
            if (Menu["KillSteal"].GetValue<MenuBool>("Q").Enabled && Q.IsReady())
            {
                var qt = GameObjects.EnemyHeroes.Where(x => x.IsValidTarget(Q.Range) && x.Health < Q.GetDamage(x)).FirstOrDefault();

                if (qt != null)
                {
                    Q.Cast(qt);
                    return;
                }
            }

            if (Menu["KillSteal"].GetValue<MenuBool>("E").Enabled && E.IsReady())
            {
                var et = GameObjects.EnemyHeroes.Where(x => (!Menu["EBlackList"].GetValue<MenuBool>(x.CharacterName.ToLower()).Enabled) && x.IsValidTarget(E.Range) && x.Health < E.GetDamage(x)).FirstOrDefault();

                if (et != null)
                {
                    E.Cast(et);
                    return;
                }
            }

            if (Menu["KillSteal"].GetValue<MenuBool>("R").Enabled && R.IsReady())
            {
                var rt = GameObjects.EnemyHeroes.Where(x => x.IsValidTarget(R.Range - 50) && x.Health < R.GetDamage(x) && Menu["KillSteal"].GetValue<MenuBool>(x.CharacterName.ToLower()).Enabled).FirstOrDefault();

                if (rt != null)
                {
                    R.Cast(rt);
                    return;
                }
            }
        }

        private static void Skin()
        {
            if (Menu["Skin"].GetValue<MenuBool>("Enable").Enabled)
            {
            }
            else if (!Menu["Skin"].GetValue<MenuBool>("Enable").Enabled)
            {
            }
        }

        private static void OnDraw(EventArgs args)
        {
            if (Me.IsDead)
            {
                return;
            }

            if ((Menu["Draw"].GetValue<MenuBool>("Q").Enabled) && Q.IsReady())
                CircleRender.Draw(Me.Position, Q.Range, Color.AliceBlue, 2);

            if ((Menu["Draw"].GetValue<MenuBool>("W").Enabled) && (W.IsReady() || Me.HasBuff("IllaoiW")))
                CircleRender.Draw(Me.Position, W.Range, Color.LightSeaGreen, 2);

            if ((Menu["Draw"].GetValue<MenuBool>("E").Enabled) && E.IsReady())
                CircleRender.Draw(Me.Position, E.Range, Color.LightYellow, 2);

            if ((Menu["Draw"].GetValue<MenuBool>("R").Enabled) && E.IsReady())
                CircleRender.Draw(Me.Position, R.Range, Color.OrangeRed, 2);

            if (Menu["Draw"].GetValue<MenuBool>("DrawDamage").Enabled)
            {
                foreach (var target in ObjectManager.Get<AIHeroClient>().Where(e => e.IsValidTarget() && e.IsValid && !e.IsDead && !e.IsZombie()))
                {
                    HpBarDraw.Unit = target;
                    HpBarDraw.DrawDmg(GetDamage(target), new SharpDX.ColorBGRA(255, 204, 0, 170));
                }
            }
        }

        private static void OnAction(object sender, AfterAttackEventArgs e)
        {
            if (Orbwalker.ActiveMode == OrbwalkerMode.Combo)
            {
                var target = TargetSelector.GetTarget(W.Range, DamageType.Physical);

                if (target != null && !target.IsDead && !target.IsZombie() && target.IsHPBarRendered)
                {
                    if (Menu["Combo"].GetValue<MenuBool>("W").Enabled && W.IsReady() && target.IsValidTarget(W.Range))
                    {
                        if (Menu["Combo"].GetValue<MenuBool>("WOutRange").Enabled && !InAutoAttackRange(target))
                        {
                            W.Cast();
                        }

                        if (Menu["Combo"].GetValue<MenuBool>("WUlt").Enabled && Me.HasBuff("IllaoiR"))
                        {
                            W.Cast();
                        }
                    }
                }
            }

            if (Orbwalker.ActiveMode == OrbwalkerMode.Harass && !Me.IsUnderEnemyTurret())
            {
                if (Me.ManaPercent >= Menu["Harass"].GetValue<MenuSlider>("Mana").Value)
                {
                    var target = TargetSelector.GetTarget(W.Range, DamageType.Physical);

                    if (target != null && !target.IsDead && !target.IsZombie() && target.IsHPBarRendered)
                    {
                        if (Menu["Harass"].GetValue<MenuBool>("W").Enabled && W.IsReady() && target.IsValidTarget(W.Range))
                        {
                            if (Menu["Harass"].GetValue<MenuBool>("WOutRange").Enabled && !InAutoAttackRange(target))
                            {
                                W.Cast();
                            }
                            else if (!Menu["Harass"].GetValue<MenuBool>("WOutRange").Enabled)
                            {
                                W.Cast();
                            }
                        }
                    }
                }
            }

            if (Orbwalker.ActiveMode == OrbwalkerMode.LaneClear)
            {
                if (Me.ManaPercent >= Menu["JungleClear"]["Mana"].GetValue<MenuSlider>().Value)
                {
                    var Mobs = GameObjects.Jungle
                        .Where(x => x.IsValidTarget(Q.Range) && !GameObjects.JungleSmall.Contains(x)).ToList();

                    if (Mobs.Count() > 0)
                    {
                        if (Menu["JungleClear"].GetValue<MenuBool>("W").Enabled && W.IsReady() && !Orbwalker.IsAutoAttack(Me.CharacterName))
                        {
                            W.Cast();
                        }
                    }
                }
            }
        }

        private static void OnSpellCast(AIBaseClient sender, AIBaseClientProcessSpellCastEventArgs args)
        {
            if (Orbwalker.ActiveMode == OrbwalkerMode.Combo)
            {
                var target = TargetSelector.GetTarget(W.Range, DamageType.Physical);

                if (target != null && !target.IsDead && !target.IsZombie() && target.IsHPBarRendered)
                {
                    if (Menu["Combo"].GetValue<MenuBool>("W").Enabled && W.IsReady() && target.IsValidTarget(W.Range))
                    {
                        if (Menu["Combo"].GetValue<MenuBool>("WOutRange").Enabled && !InAutoAttackRange(target))
                        {
                            W.Cast();
                        }
                        else if (!Menu["Combo"].GetValue<MenuBool>("WOutRange").Enabled)
                        {
                            W.Cast();
                        }

                        if (Menu["Combo"].GetValue<MenuBool>("WUlt").Enabled && Me.HasBuff("IllaoiR"))
                        {
                            W.Cast();
                        }
                    }
                }
            }
        }

        private static void OnGapCloser(AIHeroClient sender, AntiGapcloser.GapcloserArgs Args)
        {
            if (Menu.GetValue<MenuBool>("EGap").Enabled)
            {
                if (sender.IsEnemy && (Args.EndPosition.DistanceToPlayer() <= 200 || sender.DistanceToPlayer() <= 250) && !Menu["EBlackList"].GetValue<MenuBool>(sender.CharacterName.ToLower()).Enabled)
                {
                    E.Cast(sender);
                }
            }
        }
        
        private static float GetDamage(AIHeroClient target)
        {
            float Damage = 0f;

            if (Q.IsReady())
            {
                Damage += Q.GetDamage(target);
            }

            if (W.IsReady())
            {
                Damage += W.GetDamage(target);
            }

            if (E.IsReady())
            {
                Damage += E.GetDamage(target);
            }

            if (R.IsReady())
            {
                Damage += R.GetDamage(target);
            }

            return Damage;
        }
        
        private static float AttackRange()
        {
            return Me.GetRealAutoAttackRange();
        }
        
        public static bool InAutoAttackRange(AttackableUnit target)
        {
            var baseTarget = (AIBaseClient)target;
            var myRange = AttackRange();

            if (baseTarget != null)
            {
                return baseTarget.IsHPBarRendered && Vector2.DistanceSquared(baseTarget.Position.ToVector2(), Me.Position.ToVector2()) <= myRange * myRange;
            }

            return target.IsValidTarget() && Vector2.DistanceSquared(target.Position.ToVector2(), Me.Position.ToVector2()) <= myRange * myRange;
        }
    }
}