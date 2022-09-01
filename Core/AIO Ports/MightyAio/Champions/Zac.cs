﻿using EnsoulSharp;
using EnsoulSharp.SDK;
using EnsoulSharp.SDK.MenuUI;
using EnsoulSharp.SDK.Utility;
using SharpDX;
using SharpDX.Direct3D9;
using System;
using System.Linq;

namespace MightyAio.Champions
{
    internal class Zac
    {
        public static Menu Menu, Emotes;
        public static Spell Q, W, E, R;
        public static readonly AIHeroClient player = ObjectManager.Player;
        public static int[] eRanges = new int[] { 1150, 1300, 1450, 1600, 1750 };
        public static float[] eChannelTimes = new float[] { 0.9f, 1.05f, 1.2f, 1.35f, 1.5f };
        public static Vector3 farmPos, pos;
        public static float zacETime;
        public static int mykills = 0 + player.ChampionsKilled;
        public static int[] SpellLevels;
        public static Font Berlinfont = new Font(Drawing.Direct3DDevice9, new FontDescription
        {
            FaceName = "Berlin San FB Demi",
            Height = 23,
            Weight = FontWeight.DemiBold,
            OutputPrecision = FontPrecision.Default,
            Quality = FontQuality.ClearTypeNatural,
        });
        private static void CreateMenu()
        {
            Menu = new Menu("Zac", "Zac", true);

            // Q
            var QMenu = new Menu("Q", "Q")
            {
                new MenuBool("QC", "Use Q in Combo"),
                new MenuBool("QH", "Use Q in Harass"),
            };
            Menu.Add(QMenu);

            // W
            var WMenu = new Menu("W", "W")
            {
                new MenuBool("WC", "Use W in Combo"),
                new MenuBool("WH", "Use W in Harass")
            };
            Menu.Add(WMenu);
            // E
            var EMenu = new Menu("E", "E")
            {
                new MenuBool("EC", "Use E in Combo"),
                //new MenuBool("EF", "Use E in Feel"),
                //new MenuKeyBind("FeelKey","Feel Key",System.Windows.Forms.Keys.Z,KeyBindType.Press)
            };
            Menu.Add(EMenu);
            // R
            var RMenu = new Menu("R", "R")
            {
                new MenuBool("RC", "Use R in Combo Only"),
                new MenuSlider("RCT", "Use R When Enemy count are more than",2,1,5)
                
            };
            Menu.Add(RMenu);
            // lane clear
            var laneclear = new Menu("laneclear", "Lane Clear")
            {
                new MenuBool("Q", "Use Q for Lane Clear"),
                new MenuBool("W", "Use W for Lane Clear"),
                new MenuBool("E", "Use E for Lane Clear"),
                new MenuBool("B", "Collect blobs")
             
            };
            Menu.Add(laneclear);
            // auto item buy
            var itembuy = new Menu("autoitem", "Starter Item")
            {
                new MenuList("selectitem", "Select Item", new[] { "Hunters Talisman", "none" })
            };
            Menu.Add(itembuy);
            // kill steal
            var killsteal = new Menu("KillSteal", "Kill Steal")
            {
                new MenuBool("Q", "Use Q"),
                new MenuBool("W","Use W"),
            };
            Menu.Add(killsteal);

            // Misc
            var miscMenu = new Menu("Misc", "Misc")
            {
                new MenuBool("UseSkin", "Use Skin Changer"),
                new MenuSlider("setskin", "set skin", 6, 0, 55),
                new MenuBool("autolevel", "Auto Level")
            };


            // use emotes
            Emotes = new Menu("Emotes", "Emotes")
            {
                new MenuList("selectitem", "Select Item", new[] { "Center", "East", "West", "South", "North", "Mastery" }),
                new MenuBool("Kill", "Use on kill")
            };
            miscMenu.Add(Emotes);
            Menu.Add(miscMenu);

            // Drawing
            var drawMenu = new Menu("Drawing", "Draw")
            {
                new MenuBool("DrawQ", "Draw Q"),
                new MenuBool("DrawE", "Draw E"),
                new MenuBool("DrawR", "Draw R"),
                new MenuBool("Drawkillabeabilities", "Draw kill abe abilities")
            };
            Menu.Add(drawMenu);

            Menu.Attach();
        }
        public Zac()
        {
            InitZac();
            CreateMenu();
            Drawing.OnDraw += Game_OnDraw;
            Game.OnUpdate += Game_OnGameUpdate;
            AIBaseClient.OnProcessSpellCast += AIBaseClientOnProcessSpellCast;
        }

        private void AIBaseClientOnProcessSpellCast(AIBaseClient sender, AIBaseClientProcessSpellCastEventArgs args)
        {
            if (args.SData.Name == "ZacE")
            {
                if (zacETime == 0f)
                {
                    zacETime = System.Environment.TickCount;
                    DelayAction.Add(4000, () => { zacETime = 0f; });
                }
            }
        }

        private void InitZac()
        {
            SpellLevels = new[] { 2, 1, 3, 3, 3, 4, 3, 2, 3, 2, 4, 2, 2, 1, 1, 4, 1, 1 };
            Q = new Spell(SpellSlot.Q, 550);
            Q.SetSkillshot(0.55f, 120, float.MaxValue, false, SpellType.Line);
            W = new Spell(SpellSlot.W, 320);
            E = new Spell(SpellSlot.E);
            E.SetSkillshot(0.75f, 230, 1500, false, SpellType.Circle);
            E.SetCharged("ZacE", "ZacE", 295, eRanges[0], eChannelTimes[0]);
            R = new Spell(SpellSlot.R, 300);

        }

        private void Game_OnGameUpdate(EventArgs args)
        {   
            
            var useonkill = Emotes.GetValue<MenuBool>("Kill").Enabled;

            if (ObjectManager.Player.ChampionsKilled > mykills && useonkill)
            {
                mykills = ObjectManager.Player.ChampionsKilled;
                Emote();
            }

            var getskin = Menu["Misc"].GetValue<MenuSlider>("setskin").Value;
            var skin = Menu["Misc"].GetValue<MenuBool>("UseSkin").Enabled;
            if (skin && player.SkinId != getskin) { player.SetSkin(getskin); }
            var gold = player.Gold;
            var time = (Game.Time / 60);
            var item = Menu["autoitem"].GetValue<MenuList>("selectitem").SelectedValue;

            if (item != "none")
            {
                if (item == "Hunters Talisman)")
                {
                    if (time < 1 && player.InShop())
                    {
                        if (gold >= 150 && !player.HasItem(ItemId.Refillable_Potion))
                        {
                            player.BuyItem(ItemId.Refillable_Potion);
                        }
                    }
                }
            }

            if (E.IsCharging || eActive || rActive)
            {
                Orbwalker.AttackEnabled = false;
                Orbwalker.MoveEnabled = false;
            }
            else
            {
                Orbwalker.AttackEnabled = true;
                Orbwalker.MoveEnabled = true;
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
                    Clear();
                    break;

                case OrbwalkerMode.LastHit:
                    break;

                default:
                    break;
            }
            if (Menu["Misc"].GetValue<MenuBool>("autolevel").Enabled && player.Level < 18)
            {
                Levelup();
            }

            
            Killsteal();
        

        }
        private void Emote()
        {
            var b = Emotes.GetValue<MenuList>("selectitem").SelectedValue;
            switch (b)
            {
                case "Mastery":
                    Game.SendSummonerEmote(SummonerEmoteSlot.Mastery);
                    break;

                case "Center":
                    Game.SendSummonerEmote(SummonerEmoteSlot.Center);
                    break;

                case "South":
                    Game.SendSummonerEmote(SummonerEmoteSlot.South);
                    break;

                case "West":
                    Game.SendSummonerEmote(SummonerEmoteSlot.West);
                    break;

                case "East":
                    Game.SendSummonerEmote(SummonerEmoteSlot.East);
                    break;

                case "North":
                    Game.SendSummonerEmote(SummonerEmoteSlot.North);
                    break;
            }
        }
        private void Harass()
        {
            AIHeroClient target = TargetSelector.GetTarget(Q.Range,DamageType.Magical);
            var QA = Menu["Q"].GetValue<MenuBool>("QH").Enabled;
            var WA = Menu["W"].GetValue<MenuBool>("WH").Enabled;
            if (target == null)
            {
                return;
            }
            bool QPassive = player.HasBuff("zacqempowered");
            var qpre = Q.GetPrediction(target);
            if (QA && Q.CanCast(target) && qpre.Hitchance >= HitChance.High)
            {
                Q.Cast(qpre.CastPosition);
            }
            if (QPassive)
            {
                var tars = GameObjects.EnemyHeroes.OrderBy(o => o.Distance(player));
                foreach (var tar in tars.Where(x => !x.HasBuff("zacqslow") && x.IsValidTarget(player.AttackRange)))
                {
                    if (!tar.IsMinion() && !tar.IsJungle())
                    {
                        Orbwalker.Move(tar.Position);
                        Orbwalker.Attack(tar);
                    }
                    else
                    {
                        Orbwalker.Move(tar.Position);
                        Orbwalker.Attack(tar);
                    }
                }

            }
            if (WA && W.IsReady())
            {
                if (player.Distance(target) < W.Range)
                {
                    W.Cast();
                }
            }
        }

        private void Clear()
        {
            var QA = Menu["laneclear"].GetValue<MenuBool>("Q").Enabled;
            var WA = Menu["laneclear"].GetValue<MenuBool>("W").Enabled;
            var EA = Menu["laneclear"].GetValue<MenuBool>("E").Enabled;
            var Blobs = Menu["laneclear"].GetValue<MenuBool>("B").Enabled;
            var targets = GameObjects.GetJungles(ObjectManager.Player.Position, GetTargetRange(), JungleType.All);
            foreach (var target in targets)
            {
                if (Q.IsReady() && QA && !E.IsCharging)
                {
                    if (target != null && Q.CanCast(target))
                    {
                        Q.Cast(target.Position);
                    }
                    else
                    {
                        FarmLocation bestPositionQ =
                              Q.GetLineFarmLocation(GameObjects.GetMinions(Q.Range, MinionTypes.All, MinionTeam.Enemy));
                    }
                }
                if (W.IsReady() && WA && !E.IsCharging)
                {
                    if (target != null && target.Distance(player) < W.Range)
                    {
                        W.Cast();
                    }
                }
                if (!E.IsCharging && Blobs)
                {
                    var blob =
                        ObjectManager.Get<AIBaseClient>()
                            .Where(
                                o =>
                                    !o.IsDead && o.IsValid && o.Name == "BlobDrop" && o.Team == player.Team &&
                                    o.Distance(player) < player.GetRealAutoAttackRange(player))
                            .OrderBy(o => o.Distance(player))
                            .FirstOrDefault();
                    if (blob != null && Orbwalker.CanMove(300, true) && !Orbwalker.CanAttack() && !player.IsWindingUp)
                    {
                        Orbwalker.MoveEnabled = false;
                        player.IssueOrder(GameObjectOrder.MoveTo, blob.Position);
                    }
                }
                if (E.IsReady() && EA)
                {
                    if (target != null && target.IsValidTarget())
                    {
                        CastE(target);
                    }
                    else
                    {
                        FarmLocation bestPositionE =
                            E.GetCircularFarmLocation(
                               GameObjects.GetMinions(eRanges[E.Level - 1], MinionTypes.All, MinionTeam.Enemy));
                        var castPos = Vector3.Zero;
                        if (
                            farmPos.IsValid())
                        {
                            castPos = farmPos;
                        }

                        if (castPos.IsValid())
                        {
                            farmPos = bestPositionE.Position.ToVector3();
                            DelayAction.Add(5000, () => { farmPos = Vector3.Zero; });
                            CastE(castPos);
                        }
                    }
                }
            }
        }

        private void Combo()
        {
            var num = Menu["R"].GetValue<MenuSlider>("RCT").Value;
            var QA = Menu["Q"].GetValue<MenuBool>("QC").Enabled;
            var WA = Menu["W"].GetValue<MenuBool>("WC").Enabled;
            var EA = Menu["E"].GetValue<MenuBool>("EC").Enabled;
            var RA = Menu["R"].GetValue<MenuBool>("RC").Enabled;
            var target = TargetSelector.GetTarget(E.Range,DamageType.Magical);
            if (target == null)
            {
                return;
            }

            if (WA && W.CanCast(target) && !E.IsCharging)
            {
                W.Cast();
            }

            if (E.IsReady() && player.CanMove && EA)
            {
                CastE(target);
            }
            bool QPassive = player.HasBuff("zacqempowered");
            var qpre = Q.GetPrediction(target);
            if (Q.CanCast(target) && QA && qpre.Hitchance >= HitChance.High &&
                !E.IsCharging)
            {
                Q.Cast(qpre.CastPosition);
            }
            if (QPassive)
            {
                var tars = GameObjects.EnemyHeroes.OrderBy(o => o.Distance(player));
                foreach (var tar in tars.Where(x => !x.HasBuff("zacqslow") && x.IsValidTarget(player.AttackRange)))
                {
                    if (!tar.IsMinion() && !tar.IsJungle())
                    {
                        Orbwalker.Move(tar.Position);
                        Orbwalker.Attack(tar);
                    }
                    else
                    {
                        Orbwalker.Move(tar.Position);
                        Orbwalker.Attack(tar);
                    }
                }

            }


            if (R.IsReady() && RA &&
                !target.HasBuffOfType(BuffType.Knockback) && !target.HasBuffOfType(BuffType.Knockup) &&
                !target.HasBuffOfType(BuffType.Stun) && R.IsInRange(target) && player.CountEnemyHeroesInRange(500) == num)
            {
                R.Cast();
            }
            if (!E.IsCharging )
            {
                var blob =
                    ObjectManager.Get<AIBaseClient>()
                        .Where(
                            o =>
                                !o.IsDead && o.IsValid && o.Name == "BlobDrop" && o.Team == player.Team &&
                                o.Distance(player) < player.GetRealAutoAttackRange(player))
                        .OrderBy(o => o.Distance(player))
                        .FirstOrDefault();
                if (blob != null && Orbwalker.CanMove(300, true) && !Orbwalker.CanAttack() && !player.IsWindingUp)
                {
                    Orbwalker.MoveEnabled = false;
                    player.IssueOrder(GameObjectOrder.MoveTo, blob.Position);
                }
            }
        }

        private void CastE(AIBaseClient target)
        {
            if (target.Distance(player) > eRanges[E.Level - 1])
            {
                return;
            }
            var eFlyPred = E.GetPrediction(target);
            var enemyPred = E.GetPrediction(
                target);
            if (E.IsCharging)
            {
                if (!eFlyPred.CastPosition.IsValid())
                {
                    return;
                }
                if (eFlyPred.Hitchance >= HitChance.Immobile)
                {
                    E.Cast(eFlyPred.CastPosition);
                }
                else if (eFlyPred.CastPosition.Distance(player) < 200)
                {
                    E.Cast(eFlyPred.CastPosition);
                }
                else if (eFlyPred.CastPosition.Distance(player.Position) < E.Range && eFlyPred.Hitchance >= HitChance.Medium)
                {
                    E.Cast(eFlyPred.CastPosition);
                }
                else if (eFlyPred.UnitPosition.Distance(player.Position) < E.Range && target.Distance(player) < 500f && eFlyPred.Hitchance >= HitChance.Medium)
                {
                    E.Cast(eFlyPred.CastPosition);
                }
                else if ((eFlyPred.CastPosition.Distance(player.Position) < E.Range &&
                          eRanges[E.Level - 1] - eFlyPred.CastPosition.Distance(player.Position) < 200) && eFlyPred.Hitchance >= HitChance.Medium)
                {
                    E.Cast(eFlyPred.CastPosition);
                }
                else if (eFlyPred.CastPosition.Distance(player.Position) < E.Range && zacETime != 0 &&
                         Variables.GameTimeTickCount - zacETime > 2500 && eFlyPred.Hitchance >= HitChance.Medium)
                {
                    E.Cast(eFlyPred.CastPosition);
                }
            }
            else if (enemyPred.CastPosition.Distance(player.Position) < eRanges[E.Level - 1])
            {
                E.SetCharged("ZacE", "ZacE", 300, eRanges[E.Level - 1], eChannelTimes[E.Level - 1]);
                E.StartCharging(eFlyPred.UnitPosition);
            }
        }

        private void CastE(Vector3 target)
        {
            if (target.Distance(player.Position) > eRanges[E.Level - 1])
            {
                return;
            }
            if (E.IsCharging)
            {
                if (target.Distance(player.Position) < E.Range)
                {
                    E.Cast(target);
                }
            }
            else if (target.Distance(player.Position) < eRanges[E.Level - 1])
            {
                E.SetCharged("ZacE", "ZacE", 295, eRanges[E.Level - 1], eChannelTimes[E.Level - 1]);
                E.StartCharging(target);
            }
        }

        private float GetTargetRange()
        {
            if (E.IsReady())
            {
                return eRanges[E.Level - 1];
            }
            else
            {
                return 600;
            }
        }

        private float GetERange()
        {
            if (E.Level > 0)
            {
                return eRanges[E.Level - 1];
            }
            else
            {
                return eRanges[0];
            }
        }
     
        private static void Killsteal()
        {
            var target = TargetSelector.GetTarget(Q.Range,DamageType.Magical);
            var QA = Menu["KillSteal"].GetValue<MenuBool>("Q").Enabled;
            var WQA = Menu["KillSteal"].GetValue<MenuBool>("W").Enabled;
       
            if (target == null || target.HasBuffOfType(BuffType.Invulnerability))
            {
                return;
            }

            if (QA && target.Health + target.AllShield < Q.GetDamage(target) && Q.IsReady() && Q.IsInRange(target))
            {
                var qpre = Q.GetPrediction(target);
                if (qpre.Hitchance >= HitChance.High) { 
                Q.Cast(target);
                }
            }
            if (WQA && target.Health + target.AllShield < W.GetDamage(target) && W.IsReady() && W.IsInRange(target))
            {
               
                W.Cast();
            }
          
           
        }
        private static bool rActive => player.Buffs.Any(buff => buff.Name == "ZacR");

        private static bool eActive => player.Buffs.Any(buff => buff.Name == "ZacE");

        public static void DrawText(Font aFont, String aText, int aPosX, int aPosY, ColorBGRA aColor)
        {
            aFont.DrawText(null, aText, aPosX, aPosY, aColor);
        }
        private void Game_OnDraw(EventArgs args)
        {
            var drawQ = Menu["Drawing"].GetValue<MenuBool>("DrawQ").Enabled;
            var drawE = Menu["Drawing"].GetValue<MenuBool>("DrawE").Enabled;
            var drawR = Menu["Drawing"].GetValue<MenuBool>("DrawR").Enabled;
            var drawKill = Menu["Drawing"].GetValue<MenuBool>("Drawkillabeabilities").Enabled;
            var p = player.Position;


            if (drawQ  && Q.IsReady())
            {
                Drawing.DrawCircleIndicator(p, Q.Range, System.Drawing.Color.DarkCyan);
            }

            if (drawE && E.IsReady())
            {

                Drawing.DrawCircleIndicator(p, E.Range, System.Drawing.Color.Red);
            }

            if (drawR && R.IsReady())
            {
                Drawing.DrawCircleIndicator(p, R.Range, System.Drawing.Color.DarkCyan);
            }

            foreach (
                   var enemyVisible in
                       ObjectManager.Get<AIHeroClient>().Where(enemyVisible => enemyVisible.IsValidTarget()))
            {

                if (enemyVisible == null) { return; }

                var aa = string.Format("AA Left:" + Math.Floor((enemyVisible.Health / (player.CalculatePhysicalDamage(enemyVisible, player.TotalAttackDamage)))));
                var qdmg = Q.GetDamage(enemyVisible);
                var wdmg = W.GetDamage(enemyVisible);
                var edmg = E.GetDamage(enemyVisible);
                var rdmg = R.GetDamage(enemyVisible) + (R.GetDamage(enemyVisible) / 2) * 3;

                if (drawKill)
                {
                    if (qdmg > enemyVisible.Health)
                    {
                        DrawText(Berlinfont, "Killable Skills (Q):", (int)Drawing.WorldToScreen(enemyVisible.Position)[0] - 38, (int)Drawing.WorldToScreen(enemyVisible.Position)[1] + 10, SharpDX.Color.White);
                    }
                    else if (qdmg + wdmg > enemyVisible.Health)
                    {
                        DrawText(Berlinfont, "Killable Skills (Q+W):", (int)Drawing.WorldToScreen(enemyVisible.Position)[0] - 38, (int)Drawing.WorldToScreen(enemyVisible.Position)[1] + 10, SharpDX.Color.White);
                    }
                    else if (qdmg + wdmg + edmg > enemyVisible.Health)
                    {
                        DrawText(Berlinfont, "Killable Skills (Q+W+E):", (int)Drawing.WorldToScreen(enemyVisible.Position)[0] - 38, (int)Drawing.WorldToScreen(enemyVisible.Position)[1] + 10, SharpDX.Color.White);
                    }
                    else if (qdmg + wdmg + edmg + rdmg > enemyVisible.Health)
                    {
                        DrawText(Berlinfont, "Killable Skills (Q+W+E+ R):", (int)Drawing.WorldToScreen(enemyVisible.Position)[0] - 38, (int)Drawing.WorldToScreen(enemyVisible.Position)[1] + 10, SharpDX.Color.White);
                    }
                    else
                    {
                        DrawText(Berlinfont, aa, (int)Drawing.WorldToScreen(enemyVisible.Position)[0] - 38, (int)Drawing.WorldToScreen(enemyVisible.Position)[1] + 10, SharpDX.Color.White);
                    }

                }


            }
        }

        private static void Levelup()
        {
           
            var qLevel = Q.Level;
            var wLevel = W.Level;
            var eLevel = E.Level;
            var rLevel = R.Level;

            if (qLevel + wLevel + eLevel + rLevel >= ObjectManager.Player.Level || player.Level > 18)
            {
                return;
            }

            var level = new[] { 0, 0, 0, 0 };
            for (var i = 0; i < ObjectManager.Player.Level; i++)
            {
                level[SpellLevels[i] - 1] = level[SpellLevels[i] - 1] + 1;
            }

            if (qLevel < level[0])
            {
                ObjectManager.Player.Spellbook.LevelSpell(SpellSlot.Q);
            }

            if (wLevel < level[1])
            {
                ObjectManager.Player.Spellbook.LevelSpell(SpellSlot.W);
            }

            if (eLevel < level[2])
            {
                ObjectManager.Player.Spellbook.LevelSpell(SpellSlot.E);
            }
            if (rLevel < level[3])
            {
                ObjectManager.Player.Spellbook.LevelSpell(SpellSlot.R);
            }
        }
    }
}