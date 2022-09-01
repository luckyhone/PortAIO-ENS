using System;
using System.Linq;
using EnsoulSharp;
using EnsoulSharp.SDK;
using EnsoulSharp.SDK.MenuUI;
using EnsoulSharp.SDK.Utility;
using SharpDX;
using Utility = EnsoulSharp.SDK.Utility;
using SharpDX.Direct3D9;
using NoobAIO.Misc;
using System.Collections.Generic;
using Render = LeagueSharpCommon.Render;

namespace NoobAIO.Champions
{
    class Twisted_Fate
    {
        //fix
        private static Menu Menu;
        private static Spell q, w, r;
        private static AIHeroClient Player { get { return ObjectManager.Player; } }
        public static bool Estacks
        {
            get { return Player.HasBuff("cardmasterstackparticle"); }
        }
        public static bool Wcard
        {
            get { return Player.HasBuff("BlueCardPreAttack"); }
        }
        #region Menu
        private static void CreateMenu()
        {
            Menu = new Menu("TwistedFate", "Noob Twisted Fate", true);

            // Combo
            var comboMenu = new Menu("Combo", "Combo")
            {
                new MenuBool("comboQ", "Use Q"),
                new MenuBool("comboQStun", "Only use Q when stunned"),
                new MenuSeparator("Head1", "W Usage"),
                new MenuBool("comboW", "Use W"),
                new MenuSlider("comboWBlue", "Use Blue Card if Mana is under X %", 35, 0, 100),
                new MenuKeyBind("comboOneshot", "Oneshot combo with Blue+Estacks", Keys.T, KeyBindType.Toggle)
            };
            Menu.Add(comboMenu);

            // Harass
            var harassMenu = new Menu("Harass", "Harass")
            {
                new MenuBool("harassQ", "Use Q"),
                new MenuSlider("harassQmana", "^minimum mana % for Harass", 40, 0, 100),
                new MenuBool("harassW", "Use W"),
            };
            Menu.Add(harassMenu);

            // Pick a card
            var pickacardMenu = new Menu("Cardpick", "Pick a card")
            {
                new MenuKeyBind("SelectBlue", "Blue Card", Keys.E, KeyBindType.Press),
                new MenuKeyBind("SelectRed", "Red Card", Keys.U, KeyBindType.Press),
                new MenuKeyBind("SelectYellow", "Gold Card", Keys.W, KeyBindType.Press)
            };
            Menu.Add(pickacardMenu);

            // Lane clear
            var laneclearMenu = new Menu("Clear", "Farming")
            {
                new MenuSeparator("Head1", "Lane Clear"),
                new MenuBool("laneclearQ", "Use Q"),
                new MenuBool("laneclearW", "Use W"),
                new MenuSeparator("Head2", "Jungle Clear"),
                new MenuBool("jungleclearQ", "Use Q"),
                new MenuBool("jungleclearW", "Use W"),
            };
            Menu.Add(laneclearMenu);

            // Kill steal
            var killstealMenu = new Menu("KillSteal", "Kill Steal")
            {
                new MenuBool("ksQ", "Use Q"),
            };
            Menu.Add(killstealMenu);

            // Misc
            var miscMenu = new Menu("Misc", "Misc")
            {
                new MenuBool("OnGapYellowCard", "Use Gold Card on Gapclose")
            };
            Menu.Add(miscMenu);

            // Drawing
            var drawMenu = new Menu("Drawing", "Draw")
            {
                new MenuBool("Qd", "Draw Q"),
                new MenuBool("Rd", "Draw R")
            };
            Menu.Add(drawMenu);
            Menu.Attach();
        }
        #endregion
        public Twisted_Fate()
        {
            q = new Spell(SpellSlot.Q, 1450);
            q.SetSkillshot(0.25f, 40f, 1000f, false, SpellType.Line);
            w = new Spell(SpellSlot.W, Player.GetRealAutoAttackRange());
            r = new Spell(SpellSlot.R, 5500);

            CreateMenu();
            Game.OnUpdate += GameOnGameUpdate;
            Drawing.OnDraw += OnDraw;
            AIBaseClient.OnProcessSpellCast += ObjAiBaseOnOnProcessSpellCast;
            AntiGapcloser.OnGapcloser += Gapcloser_OnGapcloser;
        }
        private static void ObjAiBaseOnOnProcessSpellCast(AIBaseClient sender, AIBaseClientProcessSpellCastEventArgs args)
        {
            if (!sender.IsMe)
            {
                return;
            }
            if (args.SData.Name.Equals("Gate", StringComparison.InvariantCultureIgnoreCase))
            {
                TFCardSelector.CardSelector.StartSelecting(Cards.Yellow);
            }
        }
        private static void OnDraw(EventArgs args)
        {
            var drawQ = Menu["Drawing"].GetValue<MenuBool>("Qd").Enabled;
            var drawR = Menu["Drawing"].GetValue<MenuBool>("Rd").Enabled;
            var p = Player.Position;

            if (drawQ && q.IsReady())
            {
                Render.Circle.DrawCircle(p, q.Range, System.Drawing.Color.Aqua);
            }
            if (drawR && r.IsReady())
            {
                Render.Circle.DrawCircle(p, r.Range, System.Drawing.Color.Aqua);
            }

            var playerPos = Drawing.WorldToScreen(Player.Position);

            if (!playerPos.IsZero)
            {
                Drawing.DrawText(playerPos.X - 53, playerPos.Y + 13, Menu["Combo"].GetValue<MenuKeyBind>("comboOneshot").Active ? System.Drawing.Color.Lime : System.Drawing.Color.Gray, Menu["Combo"].GetValue<MenuKeyBind>("comboOneshot").Active ? "OneShot Combo: ON" : "OneShot Combo: OFF");
            }
        }
        private static void GameOnGameUpdate(EventArgs args)
        {
            if (Player.IsDead)
            {
                return;
            }

            switch (Orbwalker.ActiveMode)
            {
                case OrbwalkerMode.Combo:
                    DoCombo();
                    break;
                case OrbwalkerMode.Harass:
                    DoHarass();
                    break;
                case OrbwalkerMode.LaneClear:
                    DoLaneclear();
                    break;
            }
            // KS and Q on Immobile
            Active();

            // Select cards.
            if (Menu["Cardpick"].GetValue<MenuKeyBind>("SelectYellow").Active)
            {
                SelectACard(Cards.Yellow);
            }

            if (Menu["Cardpick"].GetValue<MenuKeyBind>("SelectBlue").Active)
            {
                SelectACard(Cards.Blue);
            }

            if (Menu["Cardpick"].GetValue<MenuKeyBind>("SelectRed").Active)
            {
                SelectACard(Cards.Red);
            }
        }
        static void SelectACard(Cards aCard)
        {

            TFCardSelector.CardSelector.StartSelecting(aCard);
        }
        public static void Active()
        {
            if (Player.IsDead) return;

            var target = TargetSelector.GetTarget(1450,DamageType.Mixed);
            var qdmg = Player.GetSpellDamage(target, SpellSlot.Q);

            if (target != null)
            {
                if (!target.IsInvulnerable)
                {
                    var UseQStun = Menu["Combo"].GetValue<MenuBool>("comboQStun").Enabled;
                    var killsteal = Menu["KillSteal"].GetValue<MenuBool>("ksQ").Enabled;
                    if (killsteal)
                    {
                        if (q.IsReady() && q.IsInRange(target))
                        {
                            var Qpredicticon = q.GetPrediction(target);

                            if (qdmg > target.Health && Qpredicticon.Hitchance >= HitChance.High)
                            {
                                q.Cast(Qpredicticon.CastPosition);
                            }
                        }
                    }
                    // Auto Q on CC
                    if (q.IsReady() && q.IsInRange(target) && UseQStun)
                    {
                        if (target.HasBuffOfType(BuffType.Stun) || 
                            target.HasBuffOfType(BuffType.Snare) || 
                            target.HasBuffOfType(BuffType.Knockup) ||
                            target.HasBuffOfType(BuffType.Suppression) || 
                            target.HasBuffOfType(BuffType.Charm) || 
                            target.IsRecalling())
                        {
                            q.Cast(target);
                        }
                    }
                }
            }
        }
        private static void DoCombo()
        {
            var target = TargetSelector.GetTarget(q.Range,DamageType.Magical);
            var UseQ = Menu["Combo"].GetValue<MenuBool>("comboQ").Enabled;
            var UseQStun = Menu["Combo"].GetValue<MenuBool>("comboQStun").Enabled;
            var UseW = Menu["Combo"].GetValue<MenuBool>("comboW").Enabled;
            var UseWBlue = Menu["Combo"].GetValue<MenuSlider>("comboWBlue").Value;
            if (Player.IsDead)
            {
                return;
            }
            if (Menu["Combo"].GetValue<MenuKeyBind>("comboOneshot").Active)
            {
                var targetw = TargetSelector.GetTarget(w.Range,DamageType.Magical);
                var mobs = GameObjects.Jungle;
                var minions = GameObjects.Minions;
                if (Estacks)
                {
                    
                    Orbwalker.AttackEnabled = false;
                    if (target == null) { return; }

                    if (w.IsReady() && target.IsValidTarget(1100)) { TFCardSelector.CardSelector.StartSelecting(Cards.Blue); }

                    if (Wcard)
                    {
                        Orbwalker.AttackEnabled = true;
                        //const UInt32 WM_KEYDOWN = 0x0100;
                        var Qprediction = q.GetPrediction(target);

                        if (Qprediction.Hitchance >= HitChance.High && q.IsReady())
                        {
                            q.Cast(Qprediction.CastPosition);
                        }
                    }
                }
                else
                {                  
                    if (!Estacks)
                    {
                        Orbwalker.AttackEnabled = true;
                        if (targetw.IsValidTarget(w.Range) || mobs.Count() >= 1 || minions.Count() >= 1)
                        {
                            foreach(var mob in mobs)
                            {
                                if (!mob.IsValid) { return; }
                                Orbwalker.Orbwalk(mob, Game.CursorPos);
                            }
                            foreach (var minion in minions)
                            {
                                if (!minion.IsValid) { return; }
                                Orbwalker.Orbwalk(minion, Game.CursorPos);
                            }
                            if (target == null) { return; }

                            return;
                        }

                    }
                } 
            }
            else
            {
                if (target != null)
                {
                    if(TFCardSelector.CardSelector.Status == SelectStatus.Selecting)
                    {
                        Orbwalker.AttackEnabled = false;
                    }
                    else
                    {
                        Orbwalker.AttackEnabled = true;
                    }
                    if (UseW && target.IsValidTarget(1100) && w.IsReady())
                    {
                        if (Player.ManaPercent < UseWBlue)
                        {
                            TFCardSelector.CardSelector.StartSelecting(Cards.Blue);
                        }
                        else
                        {
                            TFCardSelector.CardSelector.StartSelecting(Cards.Yellow);
                        }
                    }
                    if (UseQ && q.IsReady() && q.IsInRange(target))
                    {
                        if (UseQStun)
                        {
                            if (target.HasBuffOfType(BuffType.Stun) ||
                                    target.HasBuffOfType(BuffType.Snare) ||
                                    target.HasBuffOfType(BuffType.Knockup) ||
                                    target.HasBuffOfType(BuffType.Suppression) ||
                                    target.IsRecalling())
                            {
                                q.Cast(target);
                            }
                        }
                        else
                        {
                            var Qprediction = q.GetPrediction(target);

                            if (Qprediction.Hitchance >= HitChance.High)
                            {
                                q.Cast(Qprediction.CastPosition);
                            }
                        }
                    }
                }
            }
        }
        private static void DoLaneclear()
        {
            var LaneclearQ = Menu["Clear"].GetValue<MenuBool>("laneclearQ");
            var LaneclearW = Menu["Clear"].GetValue<MenuBool>("laneclearW"); 
            var JungleclearQ = Menu["Clear"].GetValue<MenuBool>("jungleclearQ");
            var JungleclearW = Menu["Clear"].GetValue<MenuBool>("jungleclearW");

            var laneW = GameObjects.GetMinions(ObjectManager.Player.Position, Player.GetRealAutoAttackRange() + 100);
            var laneWJ = GameObjects.GetJungles(ObjectManager.Player.Position, Player.GetRealAutoAttackRange() + 100);
            var Wfarmpos = w.GetLineFarmLocation(laneW, 100);
            var WfarmposJ = w.GetLineFarmLocation(laneWJ, 100);

            var allJgl = GameObjects.GetJungles(ObjectManager.Player.Position, q.Range, JungleType.All);
            var allMinions = GameObjects.GetMinions(ObjectManager.Player.Position, q.Range, MinionTypes.All);

            if (Player.IsDead)
            {
                return;
            }
            if (allJgl == null || allMinions == null)
            {
                return;
            }
            Orbwalker.AttackEnabled = true;
            foreach (var minion in allMinions)
            {
                if (q.IsReady() && LaneclearQ.Enabled && minion.IsValidTarget() && q.IsInRange(minion))
                {
                    q.Cast(minion);
                }
                if (w.IsReady() && LaneclearW.Enabled && minion.IsValidTarget() && w.IsInRange(minion))
                {
                    if (minion.IsValidTarget(Player.GetRealAutoAttackRange()) &&
                            Wfarmpos.MinionsHit >= 3 && laneW.Count >= 3)
                    {
                        TFCardSelector.CardSelector.StartSelecting(Cards.Red);
                    }
                    else
                    {
                        TFCardSelector.CardSelector.StartSelecting(Cards.Blue);
                    }
                }
            }
            foreach (var jgl in allJgl)
            {
                if (q.IsReady() && jgl.IsValidTarget() && q.IsInRange(jgl) && JungleclearQ.Enabled)
                {
                    q.Cast(jgl);
                }

                if (w.IsReady() && jgl.IsValidTarget() && w.IsInRange(jgl) && JungleclearW.Enabled)
                {
                    if (jgl.IsValidTarget(Player.GetRealAutoAttackRange()) &&
                            WfarmposJ.MinionsHit >= 3 && laneWJ.Count >= 3)
                    {
                        TFCardSelector.CardSelector.StartSelecting(Cards.Red);
                    }
                    else
                    {
                        TFCardSelector.CardSelector.StartSelecting(Cards.Blue);
                    }
                }
            }
        }
        private static void DoHarass()
        {
            var target = TargetSelector.GetTarget(q.Range,DamageType.Magical);
            var UseQ = Menu["Harass"].GetValue<MenuBool>("harassQ").Enabled;
            var minmana = Menu["Harass"].GetValue<MenuSlider>("harassQmana").Value;
            var UseW = Menu["Harass"].GetValue<MenuBool>("harassW").Enabled;
            var UseWBlue = Menu["Combo"].GetValue<MenuSlider>("comboWBlue").Value;

            if (Player.IsDead)
            {
                return;
            }
            Orbwalker.AttackEnabled = true;
            if (target != null)
            {
                if (UseW && target.IsValidTarget(1100) && w.IsReady())
                {
                    if (Player.ManaPercent < UseWBlue)
                    {
                        TFCardSelector.CardSelector.StartSelecting(Cards.Blue);
                    }
                    else
                    {
                        TFCardSelector.CardSelector.StartSelecting(Cards.Yellow);
                    }
                }
                if (UseQ && q.IsReady() && q.IsInRange(target) && Player.ManaPercent > minmana)
                {
                    if (target.HasBuffOfType(BuffType.Stun) ||
                            target.HasBuffOfType(BuffType.Snare) ||
                            target.HasBuffOfType(BuffType.Knockup) ||
                            target.HasBuffOfType(BuffType.Suppression) ||
                            target.IsRecalling())
                    {
                        q.Cast(target);
                    }
                    else
                    {
                        var Qprediction = q.GetPrediction(target);

                        if (Qprediction.Hitchance >= HitChance.VeryHigh)
                        {
                            q.Cast(Qprediction.CastPosition);
                        }
                    }
                }
            }
        }
        private static void Gapcloser_OnGapcloser(AIHeroClient sender, AntiGapcloser.GapcloserArgs args)
        {
            if (!Menu["Misc"].GetValue<MenuBool>("OnGapYellowCard").Enabled)
            {
                return;
            }
            Orbwalker.AttackEnabled = true;
            if (w.IsReady() && sender != null && sender.IsValidTarget(w.Range))
            {
                if (sender.IsMelee)
                {
                    if (sender.IsValidTarget(sender.AttackRange + sender.BoundingRadius + 100))
                    {
                        SelectACard(Cards.Yellow);
                    }
                }

                if (sender.IsDashing())
                {
                    if (args.EndPosition.DistanceToPlayer() <= 250 ||
                        sender.PreviousPosition.DistanceToPlayer() <= 300)
                    {
                        SelectACard(Cards.Yellow);
                    }
                }

                if (sender.IsCastingImporantSpell())
                {
                    if (sender.PreviousPosition.DistanceToPlayer() <= 300)
                    {
                        SelectACard(Cards.Yellow);
                    }
                }
            }
        }
    }
}