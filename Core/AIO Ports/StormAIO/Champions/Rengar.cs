using System;
using System.Drawing;
using System.Linq;
using EnsoulSharp;
using EnsoulSharp.SDK;
using EnsoulSharp.SDK.MenuUI;
using EnsoulSharp.SDK.Utility;
using StormAIO.utilities;

namespace StormAIO.Champions
{
    internal class Rengar
    {
        #region Basics

        private static Spell Q, W, E, R;
        private static Menu ChampMenu;
        private static AIHeroClient Player => ObjectManager.Player;
        private static int EnergyStacks;
        private static int Stacks;

        private static float LastAdd;

        private static float LastW;
        private static float LastE;

        private static float LastWAnimation;
        private static float LastEAnimation;

        private static float LastQ;

        private static bool IsEmpoweredAvailable => (int) Player.Mana == 4;

        private static bool IsUltimateActive => Player.HasBuff("RengarR");

        #endregion

        #region Menu

        private static void CreateMenu()
        {
            ChampMenu = new Menu(Player.CharacterName, Player.CharacterName);

            var comboMenu = new Menu("combo", "Combo")
            {
                ComboMenu.QBool,
                ComboMenu.WBool,
                ComboMenu.EBool,
                ComboMenu.RBool,
                ComboMenu.Prio
            };

            var harassMenu = new Menu("harass", "Harass")
            {
                HarassMenu.QBool,
                HarassMenu.WBool,
                HarassMenu.EBool
            };

            var killStealMenu = new Menu("killSteal", "KillSteal")
            {
                KillStealMenu.EBool
            };

            var laneClearMenu = new Menu("LaneClear", "Lane Clear")
            {
                LaneClearMenu.QBool,
                LaneClearMenu.WBool,
                LaneClearMenu.EBool
            };

            var jungleClearMenu = new Menu("JungleClear", "Jungle Clear")
            {
                JungleClearMenu.QBool,
                JungleClearMenu.WBool,
                JungleClearMenu.EBool
            };


            var drawingMenu = new Menu("Drawing", "Drawing")
            {
                DrawingMenu.DrawQ,
                DrawingMenu.DrawW,
                DrawingMenu.DrawE,
                DrawingMenu.DrawR
            };
            var StructureMenu = new Menu("StructureClear", "Structure Clear")
            {
                StructureClearMenu.QBool,
                StructureClearMenu.StacKBool
            };
            var menuList = new[]
            {
                comboMenu,
                harassMenu,
                killStealMenu,
                laneClearMenu,
                jungleClearMenu,
                StructureMenu,
                drawingMenu
            };

            foreach (var menu in menuList) ChampMenu.Add(menu);
            MainMenu.Main_Menu.Add(ChampMenu);
        }

        #endregion

        #region MenuHelper

        private static class ComboMenu
        {
            public static readonly MenuBool QBool = new MenuBool("comboQ", "Use Q");
            public static readonly MenuBool WBool = new MenuBool("comboW", "Use W");
            public static readonly MenuBool EBool = new MenuBool("comboE", "Use E");
            public static readonly MenuBool RBool = new MenuBool("comboR", "Use R");
            public static readonly MenuList Prio = new MenuList("Select", "Prioritize", new[] {"Q", "W", "E"});
        }

        private static class HarassMenu
        {
            public static readonly MenuBool QBool = new MenuBool("harassQ", "Use Q ");
            public static readonly MenuBool WBool = new MenuBool("harassW", "Use W ");
            public static readonly MenuBool EBool = new MenuBool("harassE", "Use E ");
        }

        private static class KillStealMenu
        {
            public static readonly MenuBool EBool = new MenuBool("killStealE", "Use E");
        }

        private static class JungleClearMenu
        {
            public static readonly MenuBool QBool =
                new MenuBool("jungleClearQ", "Use Q ");

            public static readonly MenuBool WBool =
                new MenuBool("jungleClearW", "Use W");

            public static readonly MenuBool EBool =
                new MenuBool("jungleClearE", "Use E");
        }

        private static class LaneClearMenu
        {
            public static readonly MenuBool QBool =
                new MenuBool("laneClearQ", "Use Q");

            public static readonly MenuBool WBool =
                new MenuBool("laneClearW", "Use W ");

            public static readonly MenuBool EBool =
                new MenuBool("laneClearE", "Use E");
        }

        private static class StructureClearMenu
        {
            public static readonly MenuBool QBool =
                new MenuBool("structClearQ", "Use Q");

            public static readonly MenuBool StacKBool =
                new MenuBool("structClearW", "Stack ");
        }


        private static class DrawingMenu
        {
            public static readonly MenuBool DrawQ = new MenuBool("DrawQ", "Draw Q");
            public static readonly MenuBool DrawW = new MenuBool("DrawW", "Draw W");
            public static readonly MenuBool DrawE = new MenuBool("DrawE", "Draw E");
            public static readonly MenuBool DrawR = new MenuBool("DrawR", "Draw R");
        }

        #endregion

        #region Spells

        private static void InitSpell()
        {
            Q = new Spell(SpellSlot.Q, Player.GetRealAutoAttackRange());
            W = new Spell(SpellSlot.W, 450);
            E = new Spell(SpellSlot.E, 1000);

            E.SetSkillshot(0.125f, 140f, 1500f, true, SpellType.Line);
        }

        #endregion

        #region Gamestart

        public Rengar()
        {
            InitSpell();
            CreateMenu();
            Game.OnUpdate += Game_OnUpdate;
            Drawing.OnDraw += Drawing_OnDraw;
            Orbwalker.OnAfterAttack += OnAfterAttack;
            Orbwalker.OnBeforeAttack += OnBeforeAttack;
            Drawing.OnEndScene += delegate
            {
                var t = TargetSelector.GetTarget(2000f,DamageType.Physical);
                if (!Helper.drawIndicator || t == null) return;
                Helper.Indicator(AllDamage(t));
            };
            AIBaseClient.OnPlayAnimation += AIBaseClientOnOnPlayAnimation;
            Spellbook.OnCastSpell += SpellbookOnOnCastSpell;
            Dash.OnDash += DashOnOnDash;
            Stacks = (int) Player.Mana;
        }

        private void OnBeforeAttack(object sender, BeforeAttackEventArgs args)
        {
            if (Q.IsReady() || Player.CanUseItem((int) ItemId.Tiamat) ||
                Player.CanUseItem((int) ItemId.Ravenous_Hydra) || Helper.CanAttackAnyHero) return;
            var grass = ObjectManager.Get<GrassObject>()
                .FirstOrDefault(x => x.Position == Game.CursorPos && x.IsValid);
            if (grass == null) return;
            args.Process = false;
        }

        private void OnAfterAttack(object sender, AfterAttackEventArgs args)
        {
            switch (Orbwalker.ActiveMode)
            {
                case OrbwalkerMode.Combo:
                    if (ComboMenu.QBool.Enabled) CastQ();
                    Prio();
                    break;
                case OrbwalkerMode.Harass:
                    if (HarassMenu.QBool.Enabled) CastQ();
                    Prio();
                    break;
                case OrbwalkerMode.LaneClear:
                    if (LaneClearMenu.QBool.Enabled && Q.IsReady() && Stacks < 3)
                    {
                        Q.Cast();
                        Stacks += 1;
                        LastQ = Environment.TickCount + 0.25f + Game.Ping / 1000f;
                        EnergyStacks++;
                        LastAdd = Environment.TickCount + 1;
                    }

                    if (LaneClearMenu.QBool.Enabled && Q.IsReady() && Stacks >= 4)
                    {
                        Q.Cast();
                        Stacks += 1;
                    }

                    break;
            }


            if (args.Target is AITurretClient)
            {
                if (StructureClearMenu.QBool.Enabled && Q.IsReady())
                {
                    Q.Cast();
                    Stacks += 1;
                }
                if (StructureClearMenu.StacKBool.Enabled && Stacks < 4 && 4> EnergyStacks)
                {
                    
                    if (E.IsReady())
                    {
                        E.Cast(Game.CursorPos);
                        Stacks += 1;
                    }

                    if (W.IsReady())
                    {
                        W.Cast();
                        Stacks += 1;
                    }

                }
                
            }
        }

        private void DashOnOnDash(AIBaseClient sender, Dash.DashArgs args)
        {
            if (!sender.IsMe) return;
            switch (Orbwalker.ActiveMode)
            {
                case OrbwalkerMode.Combo:
                    if (ComboMenu.QBool.Enabled)
                        DelayAction.Add(args.Duration, CastQ);
                    break;
                case OrbwalkerMode.Harass:
                    if (HarassMenu.QBool.Enabled)
                        DelayAction.Add(args.Duration, CastQ);
                    break;
                case OrbwalkerMode.LaneClear:
                    if (LaneClearMenu.QBool.Enabled && Stacks < 4)
                        DelayAction.Add(args.Duration, () => Q.Cast());
                    if (LaneClearMenu.QBool.Enabled && Stacks > 4 && ComboMenu.Prio.Index == 0)
                        DelayAction.Add(args.Duration, () => Q.Cast());
                    break;
            }
        }

        private void SpellbookOnOnCastSpell(Spellbook sender, SpellbookCastSpellEventArgs args)
        {
            if (sender.Owner.IsMe && args.Slot != SpellSlot.E || args.Slot != SpellSlot.E || args.Slot != SpellSlot.Q) return;
            if (sender.Owner.IsMe && args.Slot == SpellSlot.Q) Orbwalker.ResetAutoAttackTimer();
            if (!sender.Owner.IsMe) return;
            if (Stacks > 4)
            {
                if (args.Slot == SpellSlot.Q && ComboMenu.Prio.Index == 0) return;
                if (args.Slot == SpellSlot.W && ComboMenu.Prio.Index == 1) return;
                if (args.Slot == SpellSlot.E && ComboMenu.Prio.Index == 2) return;

                args.Process = false;
            }
            
            if (IsUltimateActive) args.Process = false;
            if (Player.IsDashing()) args.Process = false;
        }


        private void AIBaseClientOnOnPlayAnimation(AIBaseClient sender, AIBaseClientPlayAnimationEventArgs args)
        {
            if (!sender.IsMe) return;
            if (args.Animation == "Spell2") LastWAnimation = Environment.TickCount + 0.25f;

            if (args.Animation == "Spell3") LastEAnimation = Environment.TickCount + 0.25f;

            if (args.Animation == "Spell5" && Player.HasBuff("rengaroutofcombat"))
            {
                if (Orbwalker.ActiveMode == OrbwalkerMode.Combo)
                {
                    Q.Cast();
                    Stacks += 1;
                    LastQ = Environment.TickCount + 0.25f + Game.Ping / 1000f;
                    EnergyStacks++;
                    LastAdd = Environment.TickCount + 1;
                    return;
                }

                Stacks += 1;
                EnergyStacks++;
                LastAdd = Environment.TickCount + 0.3f;
            }
        }

        #endregion

        #region args
        

        private static void Drawing_OnDraw(EventArgs args)
        {
            if (DrawingMenu.DrawQ.Enabled && Q.IsReady())
                Drawing.DrawCircleIndicator(Player.Position, Q.Range, Color.Violet);
            if (DrawingMenu.DrawW.Enabled && W.IsReady())
                Drawing.DrawCircleIndicator(Player.Position, W.Range, Color.DarkCyan);
            if (DrawingMenu.DrawE.Enabled && E.IsReady())
                Drawing.DrawCircleIndicator(Player.Position, E.Range, Color.DarkCyan);
        }

        #endregion

        #region gameupdate

        private static void Game_OnUpdate(EventArgs args)
        {
            if (Helper.Checker()) return;
            if (LastAdd < Environment.TickCount &&
                !Player.HasBuff("rengarq") &&
                !Player.HasBuff("rengarqemp") ||
                EnergyStacks > 4)
            {
                EnergyStacks = (int) Player.Mana;
                Stacks = (int) Player.Mana;
            }

            switch (Orbwalker.ActiveMode)
            {
                case OrbwalkerMode.Combo:
                    Combo();
                    break;
                case OrbwalkerMode.Harass:
                    Harass();
                    break;
                case OrbwalkerMode.LaneClear:
                    if (MainMenu.SpellFarm.Active) LaneClear();
                    JungleClear();
                    break;
            }

            if (Player.HasBuff("rengaroutofcombat")) Stacks = 0;
          
            KillSteal();
        }

        #endregion

        #region Orbwalker mod

        private static void Combo()
        {
            if (ComboMenu.WBool.Enabled && EnergyStacks <= 3) CastW();
            if (ComboMenu.EBool.Enabled && EnergyStacks <= 3) CastE();
            Prio();
        }

        private static void Harass()
        {
            if (HarassMenu.WBool.Enabled && EnergyStacks <= 3) CastW();
            if (HarassMenu.EBool.Enabled && EnergyStacks <= 3) CastE();
            Prio();
        }

        private static void LaneClear()
        {
            var minions = GameObjects.GetMinions(Player.Position, W.Range).FirstOrDefault();
            if (Stacks >= 4 && minions != null)
            {
                if (E.IsReady() && ComboMenu.Prio.Index == 2)
                {
                    Stacks += 1;
                    E.Cast(minions);
                    return;
                }

                if (ComboMenu.Prio.Index == 1 && W.IsReady() && W.IsInRange(minions))
                {
                    Stacks += 1;
                    W.Cast();
                    return;
                }

                return;
            }

            if (Stacks > 4) return;
            if (minions != null && W.IsReady() && LaneClearMenu.WBool.Enabled)
            {
                if (LastW > Environment.TickCount) return;

                if (EnergyStacks <= 3 && !W.Name.Contains("Emp"))
                {
                    Stacks += 1;
                    W.Cast();
                    LastW = Environment.TickCount + 0.25f + Game.Ping / 1000f;
                    EnergyStacks++;
                    LastAdd = Environment.TickCount + 1;
                }
            }

            if (minions != null && E.GetDamage(minions) > minions.Health && E.IsReady() && LaneClearMenu.EBool.Enabled)
            {
                if (LastE > Environment.TickCount) return;

                if (Player.HasBuff("rengarpassivebuff")) return;


                if (EnergyStacks <= 3 && !E.Name.Contains("Emp"))
                {
                    E.Cast(minions);
                    Stacks += 1;
                    LastE = Environment.TickCount + 0.25f + Game.Ping / 1000f;
                    EnergyStacks++;
                    LastAdd = Environment.TickCount + 1;
                }
            }
        }

        private static void JungleClear()
        {
            var jungles = GameObjects.GetJungles(Player.Position, E.Range).OrderBy(x => x.DistanceToPlayer())
                .FirstOrDefault();
            if (Stacks > 4 && jungles != null)
            {
                if (E.IsReady() && ComboMenu.Prio.Index == 2)
                {
                    Stacks += 1;
                    E.Cast(jungles);
                    return;
                }

                if (ComboMenu.Prio.Index == 1 && W.IsReady() && W.IsInRange(jungles))
                {
                    Stacks += 1;
                    W.Cast();
                    return;
                }

                return;
            }

            if (jungles != null && E.IsReady() && JungleClearMenu.EBool.Enabled)
            {
                if (LastE > Environment.TickCount) return;

                if (Player.HasBuff("rengarpassivebuff")) return;


                if (EnergyStacks <= 3 && !E.Name.Contains("Emp"))
                {
                    E.Cast(jungles);
                    Stacks += 1;
                    LastE = Environment.TickCount + 0.25f + Game.Ping / 1000f;
                    EnergyStacks++;
                    LastAdd = Environment.TickCount + 1;
                }
            }

            if (jungles != null && W.IsReady() && W.IsInRange(jungles) && JungleClearMenu.WBool.Enabled)
            {
                if (LastW > Environment.TickCount) return;

                if (EnergyStacks <= 3 && !W.Name.Contains("Emp"))
                {
                    Stacks += 1;
                    W.Cast();
                    LastW = Environment.TickCount + 0.25f + Game.Ping / 1000f;
                    EnergyStacks++;
                    LastAdd = Environment.TickCount + 1;
                }
            }
        }


        private static void KillSteal()
        {
            if (!KillStealMenu.EBool.Enabled) return;
            var target = TargetSelector.GetTarget(E.Range,DamageType.Physical);
            if (target != null && target.TrueHealth() < E.GetDamage(target))
            {
                var epre = E.GetPrediction(target);
                if (epre.Hitchance >= HitChance.High)
                {
                    Stacks += 1;
                    E.Cast(epre.CastPosition);
                }
            }
        }

        #endregion

        #region Spell Stage

        #endregion

        #region Spell Functions

        private static void CastQ()
        {
            if (IsUltimateActive) return;


            if (!Q.IsReady()) return;

            if (LastQ > Environment.TickCount) return;

            var target = TargetSelector.GetTarget(270,DamageType.Physical);

            if (target != null)
            {
                if (EnergyStacks == 4)
                    if (ComboMenu.Prio.Index == 0)
                    {
                        Stacks += 1;
                        Q.Cast();
                        LastQ = Environment.TickCount + 0.25f + Game.Ping / 1000f;
                        EnergyStacks++;
                        LastAdd = Environment.TickCount + 1;
                        return;
                    }

                if (EnergyStacks <= 3 && !Q.Name.Contains("Emp"))
                {
                    Stacks += 1;
                    Q.Cast();
                    LastQ = Environment.TickCount + 0.25f + Game.Ping / 1000f;
                    EnergyStacks++;
                    LastAdd = Environment.TickCount + 1;
                }
            }
        }

        private static void CastW()
        {
            if (IsUltimateActive) return;


            if (!W.IsReady()) return;

            if (LastW > Environment.TickCount) return;

            var target = TargetSelector.GetTarget((float) (W.Range / 1.3),DamageType.Physical);

            if (target != null)
                if (EnergyStacks <= 3 && !W.Name.Contains("Emp"))
                {
                    Stacks += 1;
                    W.Cast();
                    LastW = Environment.TickCount + 0.25f + Game.Ping / 1000f;
                    EnergyStacks++;
                    LastAdd = Environment.TickCount + 1;
                }
        }

        private static void CastE()
        {
            if (IsUltimateActive) return;

            if (!E.IsReady()) return;

            if (LastE > Environment.TickCount) return;

            if (Player.HasBuff("rengarpassivebuff")) return;

            var target = TargetSelector.GetTarget(E.Range,DamageType.Physical);

            if (target != null)
            {
                if (EnergyStacks <= 3 && !E.Name.Contains("Emp"))
                {
                    var epre = E.GetPrediction(target);
                    if (epre.Hitchance >= HitChance.High)
                    {
                        E.Cast(epre.CastPosition);
                        Stacks += 1;
                        LastE = Environment.TickCount + 0.25f + Game.Ping / 1000f;
                        EnergyStacks++;
                        LastAdd = Environment.TickCount + 1;
                    }


                    if (Environment.TickCount - E.LastCastAttemptTime < 500)
                        if (Player.HasItem((int) ItemId.Ravenous_Hydra) || Player.HasItem(ItemId.Tiamat))
                        {
                            var slotHydra = Player.GetSpellSlot("ItemTitanicHydraCleave");

                            if (slotHydra.ToString() != "Unknown")
                            {
                                var hydra = new Spell(slotHydra, 400);
                                if (hydra.Range >= target.DistanceToPlayer()) hydra.Cast();
                            }

                            var slotTiamat = Player.GetSpellSlot("ItemTiamatCleave");
                            if (slotTiamat.ToString() != "Unknown")
                            {
                                var tiamat = new Spell(slotTiamat, 400);
                                if (tiamat.Range >= target.DistanceToPlayer()) tiamat.Cast();
                            }
                        }
                }

                if (ComboMenu.Prio.Index == 2)
                    if (target.DistanceToPlayer() >= 300 && Player.Position.CountEnemyHeroesInRange(300) == 0)
                    {
                        var epre = E.GetPrediction(target);
                        if (epre.Hitchance >= HitChance.High)
                        {
                            E.Cast(epre.CastPosition);
                            Stacks += 1;
                            LastE = Environment.TickCount + 0.25f + Game.Ping / 1000f;
                            EnergyStacks++;
                            LastAdd = Environment.TickCount + 1;
                        }

                        if (Environment.TickCount - E.LastCastAttemptTime < 500)
                        {
                            if (Player.HasItem(ItemId.Ravenous_Hydra))
                            {
                                var slotHydra = Player.GetSpellSlot("ItemTitanicHydraCleave");
                                var hydra = new Spell(slotHydra, 400);
                                hydra.Cast();
                            }

                            if (Player.HasItem(ItemId.Tiamat))
                            {
                                var slotTiamat = Player.GetSpellSlot("ItemTiamatCleave");
                                var tiamat = new Spell(slotTiamat, 400);
                                tiamat.Cast();
                            }
                        }
                    }
            }
        }

        private static void Prio()
        {
            if (IsUltimateActive) return;

            if (EnergyStacks < 4) return;

            switch (ComboMenu.Prio.Index)
            {
                case 0:

                    if (!Q.IsReady()) return;

                    if (LastQ > Environment.TickCount) return;

                    var targetQ = TargetSelector.GetTarget(270,DamageType.Physical);

                    if (targetQ != null) Q.Cast();

                    break;
                case 1:
                    if (!W.IsReady()) return;

                    if (LastW > Environment.TickCount) return;

                    var targetW = TargetSelector.GetTarget(W.Range,DamageType.Physical);
                    if (!Player.HaveImmovableBuff()) return;
                    if (targetW != null) W.Cast();


                    break;
                case 2:
                    if (!E.IsReady()) return;

                    if (LastE > Environment.TickCount) return;

                    if (Player.HasBuff("rengarpassivebuff")) return;

                    var target = TargetSelector.GetTarget(E.Range,DamageType.Physical);

                    if (target != null)
                    {
                        var epre = E.GetPrediction(target);
                        if (epre.Hitchance >= HitChance.High) E.Cast(epre.CastPosition);

                        if (Environment.TickCount - E.LastCastAttemptTime < 500)
                        {
                            if (Player.HasItem(ItemId.Ravenous_Hydra))
                            {
                                var slotHydra = Player.GetSpellSlot("ItemTitanicHydraCleave");
                                var hydra = new Spell(slotHydra, 400);
                                hydra.Cast();
                            }

                            if (Player.HasItem(ItemId.Tiamat))
                            {
                                var slotTiamat = Player.GetSpellSlot("ItemTiamatCleave");
                                var tiamat = new Spell(slotTiamat, 400);
                                tiamat.Cast();
                            }
                        }
                    }

                    break;
            }
        }

        #endregion

        #region damage

        // Use it if some some damages aren't available by the sdk 
        private static float Qdmg(AIBaseClient t)
        {
            float a = 0;
            if (EnergyStacks >= 4) a = Q.GetDamage(t);
            if (EnergyStacks <= 3) a = Q.GetDamage(t);
            return a;
        }

        private static float Wdmg(AIBaseClient t)
        {
            float a = 0;
            if (EnergyStacks >= 4) a = W.GetDamage(t);
            if (EnergyStacks <= 3) a = W.GetDamage(t);
            return a;
        }

        private static float Edmg(AIBaseClient t)
        {
            float a = 0;
            if (EnergyStacks >= 4) a = E.GetDamage(t);
            if (EnergyStacks <= 3) a = E.GetDamage(t);
            return a;
        }

        #endregion

        #region Extra functions

        private static float AllDamage(AIHeroClient target)
        {
            float Damage = 0;
            if (target == null) return 0;
            if (target.HasBuffOfType(BuffType.Invulnerability)) return 0;
            Damage += (float) Player.GetAutoAttackDamage(target);
            if (Q.IsReady()) Damage += Qdmg(target);
            if (W.IsReady()) Damage += Wdmg(target);
            if (E.IsReady()) Damage += Edmg(target);
            if (Player.GetBuffCount("itemmagicshankcharge") == 100)
                Damage += (float) Player.CalculateMagicDamage(target, 100 + 0.1 * Player.TotalMagicalDamage);
            if (Helper.Ignite) Damage += (float) Player.GetSummonerSpellDamage(target, SummonerSpell.Ignite);
            return Damage;
        }

        #endregion
    }
}