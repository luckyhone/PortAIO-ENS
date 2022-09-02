using System;
using System.Linq;
using EnsoulSharp;
using EnsoulSharp.SDK;
using EnsoulSharp.SDK.MenuUI;
using EnsoulSharp.SDK.Rendering;
using SharpDX;
using SPrediction;

namespace Sharpy_AIO.Plugins
{
    public class Zed
    {
        private Menu Menu;
        private AIHeroClient Player = ObjectManager.Player;
        private Spell Q, W, E, R;
        private SpellSlot Ignite = ObjectManager.Player.GetSpellSlot("summonerDot");
        private int LastSwitch;
        private AIMinionClient shadow
        {
            get
            {
                return ObjectManager.Get<AIMinionClient>().FirstOrDefault(x => x.IsVisible && x.IsAlly && x.Name == "Shadow" && !x.IsDead);
            }
        }

        private enum wCheck
        {
            First,
            Second,
            Cooltime
        }

        private enum rCheck
        {
            First,
            Second,
            Cooltime
        }

        private wCheck wReady
        {
            get
            {
                if (!W.IsReadyPerfectly())
                {
                    return
                        wCheck.Cooltime;
                }
                return
                    (Player.Spellbook.GetSpell(SpellSlot.W).Name == "ZedW" ? wCheck.First : wCheck.Second);
            }
        }

        private rCheck rReady
        {
            get
            {
                if (!R.IsReadyPerfectly())
                {
                    return
                        rCheck.Cooltime;
                }
                return
                    (Player.Spellbook.GetSpell(SpellSlot.R).Name == "ZedR" ? rCheck.First : rCheck.Second);
            }
        }

        public Zed()
        {
            Game.Print("Sharpy AIO :: Zed Loaded :)");

            Q = new Spell(SpellSlot.Q, 900f) { MinHitChance = HitChance.High };
            W = new Spell(SpellSlot.W, 700f);
            E = new Spell(SpellSlot.E, 290f);
            R = new Spell(SpellSlot.R, 625f);

            Q.SetSkillshot(.25f, 70f, 1750f, false, SpellType.Line);
            W.SetSkillshot(.25f, 0f, 1750f, false, SpellType.Circle);
            R.SetTargetted(0f, float.MaxValue);

            // 메인 메뉴
            Menu = new Menu("mainmenu","Sharpy AIO :: Zed", true);

            // 콤보 메뉴
            var combo = new Menu("Combo", "Combo");
            combo.Add(new MenuBool("CQ", "Use Q").SetValue(true));
            combo.Add(new MenuBool("CW", "Use W (Line Combo Only)").SetValue(true));
            combo.Add(new MenuBool("CE", "Use E").SetValue(true));
            combo.Add(new MenuBool("CI", "Use Item").SetValue(true));
            combo.Add(new MenuKeyBind("CM", "Combo Mode",Keys.L, KeyBindType.Toggle, true));
            combo.Add(new MenuSeparator("C", "On : Normal / Off : Line"));
            Menu.Add(combo);
            
            // 궁극기 메뉴
            var ult = new Menu("Ult Setting", "Ult Setting");
            ult.Add(new MenuKeyBind("UR", "Cast R",Keys.T, KeyBindType.Press));
            ult.Add(new MenuBool("UO", "Cast R Only Selected").SetValue(true));
            combo.Add(ult);

            // 견제 메뉴
            var harass = new Menu("Harass", "Harass");
            harass.Add(new MenuBool("HQ", "Use Q").SetValue(true));
            harass.Add(new MenuKeyBind("HW1", "Use W",Keys.Y, KeyBindType.Toggle));
            harass.Add(new MenuBool("HW2", "Use W2").SetValue(false));
            harass.Add(new MenuBool("HE", "Use E").SetValue(true));
            harass.Add(new MenuKeyBind("HA", "Auto Harass Long Poke",Keys.G, KeyBindType.Toggle));
            Menu.Add(harass);

            // 이동 메뉴
            var flee = new Menu("Flee", "Flee");
            flee.Add(new MenuBool("FW", "Use W").SetValue(true));
            flee.Add(new MenuBool("FI", "Use Item").SetValue(true));
            Menu.Add(flee);

            // 막타 메뉴
            var lasthit = new Menu("LastHit", "LastHit");
            lasthit.Add(new MenuBool("LHQ", "Use Q (Long)").SetValue(true));
            lasthit.Add(new MenuBool("LHE", "Use E (Short)").SetValue(true));
            Menu.Add(lasthit);

            // 라인클리어 메뉴
            var laneclear = new Menu("LaneClear", "LaneClear");
            laneclear.Add(new MenuBool("LCQ", "Use Q").SetValue(true));
            laneclear.Add(new MenuBool("LCE", "Use E").SetValue(true));
            laneclear.Add(new MenuBool("LCI", "Use Item").SetValue(true));
            Menu.Add(laneclear);

            // 정글클리어 메뉴
            var jungleclear = new Menu("JungleClear", "JungleClear");
            jungleclear.Add(new MenuBool("JCQ", "Use Q").SetValue(true));
            jungleclear.Add(new MenuBool("JCE", "Use E").SetValue(true));
            jungleclear.Add(new MenuBool("JCI", "Use Item").SetValue(true));
            Menu.Add(jungleclear);

            // 기타 메뉴
            var misc = new Menu("Misc", "Misc");
            misc.Add(new MenuBool("MK", "Use Killsteal").SetValue(true));
            misc.Add(new MenuBool("ME", "Auto Shadow E").SetValue(true));
            Menu.Add(misc);

            // 킬스틸 메뉴
            var killsteal = new Menu("Killsteal Setting","Killsteal Setting");
            killsteal.Add(new MenuBool("K0", "Use Only On Shadow"));
            killsteal.Add(new MenuBool("KQ", "Use Q").SetValue(true));
            killsteal.Add(new MenuBool("KE", "Use E").SetValue(true));
            killsteal.Add(new MenuBool("KI", "Use Ignite").SetValue(true));
            misc.Add(killsteal);
            
            // 아이템 메뉴
            var item = new Menu("Item Setting", "Item Setting");
            item.Add(new MenuBool("IY", "Use Youmuu").SetValue(true));
            misc.Add(item);
            
            // 드로잉 메뉴
            var drawing = new Menu("Drawing", "Drawing");
            drawing.Add(new MenuBool("DQ", "Draw Q Range")); // Green 
            drawing.Add(new MenuBool("DW", "Draw W Range")); // Green
            drawing.Add(new MenuBool("DE", "Draw E Range")); // Green
            drawing.Add(new MenuBool("DR", "Draw R Range")); // Green
            drawing.Add(new MenuBool("DWQ", "Draw WQ Range")); // Green
            drawing.Add(new MenuBool("DS", "Draw Combo Mode").SetValue(true));
            drawing.Add(new MenuBool("DO", "Disable All Drawings").SetValue(false));
            Menu.Add(drawing);

            // 그림자 드로잉 메뉴
            var sd = new Menu("Shadow Drawing", "Shadow Drawing");
            sd.Add(new MenuBool("WQ", "Shadow Q Range")); // WhiteSmoke
            sd.Add(new MenuBool("WE", "Shadow E Range")); // WhiteSmoke
            drawing.Add(sd);

            // 데미지 인디케이터 메뉴
            var da = new Menu("Damage Indicator", "Damage Indicator");
            da.Add(new MenuBool("DIA", "Use Damage Indicator").SetValue(true));
            da.Add(new MenuBool("DIF", "Damage Indicator Fill Color")); // Goldenrod
            Menu.Add(da);

            Menu.Attach();

            Menu["DIA"].GetValue<MenuBool>().ValueChanged += Zed_ValueChanged;
            Menu["DIF"].GetValue<MenuBool>().ValueChanged += Zed_ValueChanged1;
            
            Game.OnUpdate += Game_OnUpdate;
            Orbwalker.OnBeforeAttack += Orbwalking_BeforeAttack;
            Drawing.OnDraw += Drawing_OnDraw;
        }
        
        private void Drawing_OnDraw(EventArgs args)
        {
            if (!Menu["Drawing"]["DO"].GetValue<MenuBool>().Enabled)
            {
                var DQ = Menu["Drawing"]["DQ"].GetValue<MenuBool>();
                if (DQ.Enabled)
                {
                    if (Q.IsReadyPerfectly())
                    {
                        CircleRender.Draw(Player.Position, Q.Range, Color.Green, 3);
                    }
                }

                var DW = Menu["Drawing"]["DW"].GetValue<MenuBool>();
                if (DW.Enabled)
                {
                    if (W.IsReadyPerfectly())
                    {
                        CircleRender.Draw(Player.Position, W.Range, Color.Green, 3);
                    }
                }

                var DE = Menu["Drawing"]["DE"].GetValue<MenuBool>();
                if (DE.Enabled)
                {
                    if (E.IsReadyPerfectly())
                    {
                        CircleRender.Draw(Player.Position, E.Range, Color.Green, 3);
                    }
                }

                var DR = Menu["Drawing"]["DR"].GetValue<MenuBool>();
                if (DR.Enabled)
                {
                    if (R.IsReadyPerfectly())
                    {
                        if (R.IsReadyPerfectly())
                        {
                            CircleRender.Draw(Player.Position, R.Range, Color.Green, 3);
                        }
                    }
                }

                var DWQ = Menu["Drawing"]["DWQ"].GetValue<MenuBool>();
                if (DWQ.Enabled)
                {
                    if (Q.IsReadyPerfectly() && W.IsReadyPerfectly())
                    {
                        CircleRender.Draw(Player.Position, Q.Range + W.Range, Color.Green, 3);
                    }
                }

                var WQ = Menu["Drawing"]["WQ"].GetValue<MenuBool>();
                if (WQ.Enabled)
                {
                    if (shadow != null)
                    {
                        if (Q.IsReadyPerfectly())
                        {
                            CircleRender.Draw(shadow.Position, Q.Range, Color.WhiteSmoke, 3);
                        }
                    }
                }

                var WE = Menu["Drawing"]["WE"].GetValue<MenuBool>();
                if (WE.Enabled)
                {
                    if (shadow != null)
                    {
                        if (E.IsReadyPerfectly())
                        {
                            CircleRender.Draw(shadow.Position, E.Range, Color.WhiteSmoke, 3);
                        }
                    }
                }

                if (Menu["Drawing"]["DS"].GetValue<MenuBool>().Enabled)
                {
                    var position = Drawing.WorldToScreen(ObjectManager.Player.Position);
                    if (Menu["Combo"]["CM"].GetValue<MenuKeyBind>().Active)
                    {
                        Drawing.DrawText(position.X, position.Y + 40, System.Drawing.Color.White, "Combo Mode : Normal");
                    }
                    else
                    {
                        Drawing.DrawText(position.X, position.Y + 40, System.Drawing.Color.White, "Combo Mode : Line");
                    }
                }
            }
        }
        
        private void Orbwalking_BeforeAttack(object sender, BeforeAttackEventArgs args)
        {
            
            if (Orbwalker.ActiveMode == OrbwalkerMode.LastHit)
            {
                if (Player.Mana >= E.Mana)
                {
                    if (Menu["LastHit"]["LHE"].GetValue<MenuBool>().Enabled)
                    {
                        if (E.IsReadyPerfectly())
                        {
                            args.Process = false;
                        }
                    }
                }
            }
        }
        
        private void Game_OnUpdate(EventArgs args)
        {
            if(Orbwalker.CanMove())
            {
                if (Menu["Combo"]["Ult Setting"]["UR"].GetValue<MenuKeyBind>().Active)
                {
                    Player.IssueOrder(GameObjectOrder.MoveTo, Game.CursorPos);
                    if (R.IsReadyPerfectly() && rReady == rCheck.First)
                    {
                        if (Menu["Combo"]["CI"].GetValue<MenuBool>().Enabled)
                        {
                            var target = TargetSelector.GetTarget(1300f, DamageType.Physical);
                            if (target != null)
                            {
                                castYoumuu();
                            }
                        }

                        if (Menu["Combo"]["Ult Setting"]["UO"].GetValue<MenuBool>().Enabled)
                        {
                            
                            var target = TargetSelector.SelectedTarget;
                            if (target != null)
                            {
                                if (Player.Position.Distance(target.Position) <= R.Range && !target.IsZombie())
                                {
                                    R.CastOnUnit(target);
                                }
                            }
                        }
                        else
                        {
                            var target = TargetSelector.GetTarget(R.Range, R.DamageType);
                            if (target != null)
                            {
                                R.CastOnUnit(target);
                            }
                        }
                    }
                    
                }

                AIHeroClient starget;
                switch (Orbwalker.ActiveMode)
                {
                    case OrbwalkerMode.Flee:
                        if (Menu["Flee"]["FI"].GetValue<MenuBool>().Enabled)
                        {
                            castYoumuu();
                        }

                        if (Menu["Flee"]["FW"].GetValue<MenuBool>().Enabled)
                        {
                            if (Player.Mana >= W.Mana)
                            {
                                if (W.IsReadyPerfectly())
                                {
                                    if (wReady == wCheck.First)
                                    {
                                        W.Cast(Game.CursorPos);
                                    }
                                }
                            }

                            if (wReady == wCheck.Second)
                            {
                                W.Cast();
                            }
                        }

                        break;
                    case OrbwalkerMode.LastHit:
                        if (Menu["LastHit"]["LHQ"].GetValue<MenuBool>().Enabled)
                        {
                            if (Player.Mana >= Q.Mana)
                            {
                                if (Q.IsReadyPerfectly())
                                {
                                    var target = MinionManager.GetMinions(Q.Range).FirstOrDefault(x => x.IsKillableAndValidTarget(Q.GetDamage(x), Q.DamageType, Q.Range));
                                    if (target != null)
                                    {
                                        if (Player.Position.Distance(target.Position) > E.Range)
                                        {
                                            Q.UpdateSourcePosition(Player.Position, Player.Position);
                                            QCast(target);
                                        }
                                    }
                                }
                            }
                        }
                        if (Menu["LastHit"]["LHE"].GetValue<MenuBool>().Enabled)
                        {
                            if (Player.Mana >= E.Mana)
                            {
                                if (E.IsReadyPerfectly())
                                {
                                    var target = MinionManager.GetMinions(E.Range).FirstOrDefault(x => x.IsKillableAndValidTarget(E.GetDamage(x), E.DamageType, E.Range));
                                    if (target != null)
                                    {
                                        E.Cast();
                                    }
                                }
                            }
                        }

                        break;
                    case OrbwalkerMode.Harass:
                        starget = TargetSelector.SelectedTarget;
                        var WEQMana = W.Mana + E.Mana + Q.Mana;
                        if (Menu["Harass"]["HW1"].GetValue<MenuKeyBind>().Active)
                        {
                            if (shadow == null)
                            {
                                if (W.IsReadyPerfectly() && wReady == wCheck.First)
                                {
                                    if (Player.Mana >= WEQMana)
                                    {
                                        if (starget != null && starget.IsValidTarget(W.Range) && !starget.IsDead)
                                        {
                                            if (!starget.IsZombie())
                                            {
                                                W.Cast(starget);
                                            }
                                        }
                                        else
                                        {
                                            var target = TargetSelector.GetTarget(W.Range, DamageType.Physical);
                                            if (target != null)
                                            {
                                                W.Cast(target);
                                            }
                                        }
                                    }
                                }
                            }
                        }
                        if (Menu["Harass"]["HE"].GetValue<MenuBool>().Enabled)
                        {
                            if (E.IsReadyPerfectly())
                            {
                                if (shadow != null)
                                {
                                    if (starget != null && starget.IsValidTarget(E.Range) && !starget.IsDead || starget != null && starget.IsValidTarget(E.Range,true,shadow.Position) && !starget.IsDead)
                                    {
                                        if (!starget.IsZombie())
                                        {
                                            E.Cast();
                                        }
                                    }
                                    else
                                    {
                                        var shadowtarget = TargetSelector.GetTarget(E.Range, E.DamageType, true, shadow.Position);
                                        if (shadowtarget != null)
                                        {
                                            E.Cast();
                                        }
                                        else
                                        {
                                            var target = TargetSelector.GetTarget(E.Range, E.DamageType);
                                            if (target != null)
                                            {
                                                E.Cast();
                                            }
                                        }
                                    }
                                }
                                else
                                {
                                    if (!W.IsReadyPerfectly() || Player.Mana < WEQMana || !Menu["Harass"]["HW1"].GetValue<MenuKeyBind>().Active)
                                    {
                                        if (starget != null && starget.IsValidTarget(E.Range) && !starget.IsDead)
                                        {
                                            if (!starget.IsZombie())
                                            {
                                                E.Cast();
                                            }
                                        }
                                        else
                                        {
                                            var target = TargetSelector.GetTarget(E.Range, E.DamageType);
                                            if (target != null)
                                            {
                                                E.Cast();
                                            }
                                        }
                                    }
                                }
                            }
                        }
                        if (Menu["Harass"]["HQ"].GetValue<MenuBool>().Enabled)
                        {
                            if (Q.IsReadyPerfectly())
                            {
                                if (shadow != null)
                                {
                                    if (starget != null && starget.IsValidTarget(Q.Range) && !starget.IsDead || starget != null && starget.IsValidTarget(Q.Range, true, shadow.Position) && !starget.IsDead)
                                    {
                                        if (!starget.IsZombie())
                                        {
                                            if (starget.IsValidTarget(Q.Range, true, shadow.Position)) 
                                            {
                                                Q.UpdateSourcePosition(shadow.Position, shadow.Position);
                                                QCast(starget);
                                            }
                                            else
                                            {
                                                if (starget.IsValidTarget(Q.Range))
                                                {
                                                    Q.UpdateSourcePosition(Player.Position, Player.Position);
                                                    QCast(starget);
                                                }
                                            }
                                        }
                                    }
                                    else
                                    {
                                        var shadowtarget = TargetSelector.GetTarget(Q.Range, Q.DamageType, true, shadow.Position);
                                        if (shadowtarget != null)
                                        {
                                            Q.UpdateSourcePosition(shadow.Position, shadow.Position);
                                            QCast(shadowtarget);
                                        }
                                        else
                                        {
                                            var target = TargetSelector.GetTarget(Q.Range, Q.DamageType);
                                            if (target != null)
                                            {
                                                Q.UpdateSourcePosition(Player.Position, Player.Position);
                                                QCast(target);
                                            }
                                        }
                                    }
                                }
                                else
                                {
                                    if (!W.IsReadyPerfectly() || Player.Mana < WEQMana || !Menu["Harass"]["HW1"].GetValue<MenuKeyBind>().Active)
                                    {
                                        if (starget != null && starget.IsValidTarget(Q.Range) && !starget.IsDead)
                                        {
                                            if (!starget.IsZombie())
                                            {
                                                Q.UpdateSourcePosition(Player.Position, Player.Position);
                                                QCast(starget);
                                            }
                                        }
                                        else
                                        {
                                            var target = TargetSelector.GetTarget(Q.Range, Q.DamageType);
                                            if (target != null)
                                            {
                                                Q.UpdateSourcePosition(Player.Position, Player.Position);
                                                QCast(target);
                                            }
                                        }
                                    }
                                }
                            }
                        }
                        if (Menu["Harass"]["HW2"].GetValue<MenuBool>().Enabled)
                        {
                            if (Player.HasBuff("ZedWHandler"))
                            {
                                if (W.IsReadyPerfectly() && wReady == wCheck.Second)
                                {
                                    if (starget != null && starget.IsValidTarget(Player.AttackRange, true, shadow.Position) && !starget.IsDead)
                                    {
                                        if (!starget.IsZombie())
                                        {
                                            W.Cast();
                                            Player.IssueOrder(GameObjectOrder.AttackUnit, starget);
                                        }
                                    }
                                    else
                                    {
                                        var target = TargetSelector.GetTarget(Player.AttackRange, DamageType.Physical, true,  shadow.Position);
                                        if (target != null)
                                        {
                                            W.Cast();
                                            Player.IssueOrder(GameObjectOrder.AttackUnit, target);
                                        }
                                    }
                                }
                            }
                        }
                        break;
                    case OrbwalkerMode.LaneClear:
                        if (Player.Mana >= Q.Mana)
                        {
                            if (Q.IsReadyPerfectly())
                            {
                                if (Menu["LaneClear"]["LCQ"].GetValue<MenuBool>().Enabled)
                                {
                                    var target = MinionManager.GetMinions(Q.Range).FirstOrDefault(x => x.IsValidTarget(Q.Range));
                                    if (target != null)
                                    {
                                        Q.UpdateSourcePosition(Player.Position, Player.Position);
                                        QCast(target);
                                    }
                                }

                                if (Menu["JungleClear"]["JCQ"].GetValue<MenuBool>().Enabled)
                                {
                                    var target = MinionManager.GetMinions(Q.Range, MinionManager.MinionTypes.All, MinionManager.MinionTeam.Neutral, MinionManager.MinionOrderTypes.MaxHealth)
                                        .FirstOrDefault(x => x.IsValidTarget(Q.Range));
                                    if (target != null)
                                    {
                                        Q.UpdateSourcePosition(Player.Position, Player.Position);
                                        QCast(target);
                                    }
                                }
                            }
                        }

                        if (Player.Mana >= E.Mana)
                        {
                            if (E.IsReadyPerfectly())
                            {
                                if (Menu["LaneClear"]["LCE"].GetValue<MenuBool>().Enabled)
                                {
                                    var target = MinionManager.GetMinions(E.Range).FirstOrDefault(x => x.IsValidTarget(E.Range));
                                    if (target != null)
                                    {
                                        E.Cast();
                                    }
                                }

                                if (Menu["JungleClear"]["JCE"].GetValue<MenuBool>().Enabled)
                                {
                                    var target = MinionManager.GetMinions(E.Range, MinionManager.MinionTypes.All, MinionManager.MinionTeam.Neutral, MinionManager.MinionOrderTypes.MaxHealth)
                                        .FirstOrDefault(x => x.IsValidTarget(E.Range));
                                    if (target != null)
                                    {
                                        E.Cast();
                                    }
                                }
                            }
                        }
                        break;
                    case OrbwalkerMode.Combo:
                        starget = TargetSelector.SelectedTarget;
                        if (Menu["Combo"]["CI"].GetValue<MenuBool>().Enabled)
                        {
                            if (starget != null && starget.IsValidTarget(1500f) && !starget.IsDead)
                            {
                                if (!starget.IsZombie())
                                {
                                    castYoumuu();
                                }
                            }
                            else
                            {
                                var target = TargetSelector.GetTarget(1500f, DamageType.Physical);
                                if (target != null)
                                {
                                    castYoumuu();
                                }
                            }
                        }

                        if (Menu["Combo"]["CM"].GetValue<MenuKeyBind>().Active)
                        {
                            if (shadow != null)
                            {
                                if (Menu["Combo"]["CE"].GetValue<MenuBool>().Enabled)
                                {
                                    if (E.IsReadyPerfectly())
                                    {
                                        if (starget != null && starget.IsValidTarget(E.Range) && !starget.IsDead ||
                                            starget != null && starget.IsValidTarget(E.Range, true, shadow.Position) &&
                                            !starget.IsDead)
                                        {
                                            if (!starget.IsZombie())
                                            {
                                                E.Cast();
                                            }
                                        }
                                        else
                                        {
                                            var shadowtarget = TargetSelector.GetTarget(E.Range, E.DamageType, true,
                                                shadow.Position);
                                            if (shadowtarget != null)
                                            {
                                                E.Cast();
                                            }
                                            else
                                            {
                                                var target = TargetSelector.GetTarget(E.Range, E.DamageType);
                                                if (target != null)
                                                {
                                                    E.Cast();
                                                }
                                            }
                                        }
                                    }
                                }

                                if (Menu["Combo"]["CQ"].GetValue<MenuBool>().Enabled)
                                {
                                    if (Q.IsReadyPerfectly())
                                    {
                                        if (starget != null && starget.IsValidTarget(Q.Range) && !starget.IsDead ||
                                            starget != null && starget.IsValidTarget(Q.Range, true, shadow.Position))
                                        {
                                            if (!starget.IsZombie())
                                            {
                                                if (starget.IsValidTarget(Q.Range, true, shadow.Position))
                                                {
                                                    Q.UpdateSourcePosition(shadow.Position, shadow.Position);
                                                    QCast(starget);
                                                }
                                                else
                                                {
                                                    if (starget.IsValidTarget(Q.Range))
                                                    {
                                                        Q.UpdateSourcePosition(Player.Position, Player.Position);
                                                        QCast(starget);
                                                    }
                                                }
                                            }
                                        }
                                        else
                                        {
                                            var shadowtarget = TargetSelector.GetTarget(Q.Range, Q.DamageType, true,
                                                shadow.Position);
                                            if (shadowtarget != null)
                                            {
                                                Q.UpdateSourcePosition(shadow.Position, shadow.Position);
                                                QCast(shadowtarget);
                                            }
                                            else
                                            {
                                                var target = TargetSelector.GetTarget(Q.Range, Q.DamageType);
                                                if (target != null)
                                                {
                                                    Q.UpdateSourcePosition(Player.Position, Player.Position);
                                                    QCast(target);
                                                }
                                            }
                                        }
                                    }
                                }
                                else
                                {
                                    if (Menu["Combo"]["CQ"].GetValue<MenuBool>().Enabled)
                                    {
                                        if (Q.IsReadyPerfectly())
                                        {
                                            if (starget != null && starget.IsValidTarget(Q.Range) && !starget.IsDead)
                                            {
                                                if (!starget.IsZombie())
                                                {
                                                    Q.UpdateSourcePosition(Player.Position, Player.Position);
                                                    QCast(starget);
                                                }
                                            }
                                            else
                                            {
                                                var target = TargetSelector.GetTarget(Q.Range, Q.DamageType);
                                                if (target != null)
                                                {
                                                    Q.UpdateSourcePosition(Player.Position, Player.Position);
                                                    QCast(target);
                                                }
                                            }
                                        }
                                    }

                                    if (Menu["Combo"]["CE"].GetValue<MenuBool>().Enabled)
                                    {
                                        if (E.IsReadyPerfectly())
                                        {
                                            if (starget != null && starget.IsValidTarget(E.Range) && !starget.IsDead)
                                            {
                                                if (!starget.IsZombie())
                                                {
                                                    E.Cast();
                                                }
                                            }
                                            else
                                            {
                                                var target = TargetSelector.GetTarget(E.Range, E.DamageType);
                                                if (target != null)
                                                {
                                                    E.Cast();
                                                }
                                            }
                                        }
                                    }
                                }
                            }
                            else
                            {
                                var target = TargetSelector.GetTarget(W.Range, DamageType.Physical);
                                if (target != null)
                                {
                                    W.Cast(target.Position);
                                }
                            }
                        }
                        else
                        {
                            if (rReady == rCheck.Second)
                            {
                                if (wReady == wCheck.First)
                                {
                                    if (E.IsReadyPerfectly())
                                    {
                                        if (starget != null && starget.IsValidTarget(E.Range) && !starget.IsDead)
                                        {
                                            if (!starget.IsZombie())
                                            {
                                                E.Cast();
                                                Player.IssueOrder(GameObjectOrder.AttackUnit, starget);
                                            }
                                        }
                                        else
                                        {
                                            var target = TargetSelector.GetTarget(E.Range, E.DamageType);
                                            if (target != null)
                                            {
                                                E.Cast();
                                                Player.IssueOrder(GameObjectOrder.AttackUnit, target);
                                            }
                                        }
                                    }

                                    if (W.IsReadyPerfectly())
                                    {
                                        if (starget != null && starget.IsValidTarget(W.Range) && !starget.IsDead)
                                        {
                                            if (!starget.IsZombie())
                                            {
                                                var spos = starget.Position.Extend(Player.Position, -650f);
                                                W.Cast(spos);
                                            }
                                        }
                                        else
                                        {
                                            var target = TargetSelector.GetTarget(W.Range, DamageType.Physical);
                                            if (target != null)
                                            {
                                                var wpos = target.Position.Extend(Player.Position, -650f);
                                                W.Cast(wpos);
                                            }
                                        }
                                    }
                                }
                                else
                                {
                                    if (wReady == wCheck.Second)
                                    {
                                        if (Q.IsReadyPerfectly())
                                        {
                                            if (starget != null && starget.IsValidTarget(Q.Range) && !starget.IsDead)
                                            {
                                                if (!starget.IsZombie())
                                                {
                                                    Q.UpdateSourcePosition(Player.Position, Player.Position);
                                                    QCast(starget);
                                                }
                                            }
                                            else
                                            {
                                                var target = TargetSelector.GetTarget(Q.Range, Q.DamageType);
                                                if (target != null)
                                                {
                                                    Q.UpdateSourcePosition(Player.Position, Player.Position);
                                                    QCast(target);
                                                }
                                            }
                                        }

                                        if (!Q.IsReadyPerfectly())
                                        {
                                            if (Environment.TickCount - LastSwitch >= 350)
                                            {
                                                //Menu["Combo"]["CM"].GetValue<MenuKeyBind>().SetValue(Menu["Combo"]["CM"].GetValue<MenuKeyBind>().Key, KeyBindType.Toggle, true));
                                                LastSwitch = Environment.TickCount;
                                            }
                                        }
                                    }
                                }
                            }
                        }
                        
                        break;
                    case OrbwalkerMode.None:
                        break;
                }

                if (Menu["Harass"]["HA"].GetValue<MenuKeyBind>().Active)
                {
                    if (Orbwalker.ActiveMode != OrbwalkerMode.Combo)
                    {
                        starget = TargetSelector.SelectedTarget;
                        if (W.IsReadyPerfectly() && wReady == wCheck.First)
                        {
                            if (Player.Mana >= Q.Mana + W.Mana)
                            {
                                if (Q.IsReadyPerfectly())
                                {
                                    if (starget != null && starget.IsValidTarget(Q.Range + W.Range) && !starget.IsDead)
                                    {
                                        if (!starget.IsZombie())
                                        {
                                            if (Player.Position.Distance(starget.Position) > W.Range)
                                            {
                                                var spos = starget.Position.Extend(Player.Position, -(starget.Position.Distance(Player.Position) + W.Range));
                                                W.Cast(spos);
                                            }
                                        }
                                    }
                                    else
                                    {
                                        var target = TargetSelector.GetTarget(Q.Range + W.Range, DamageType.Physical);
                                        if (target != null)
                                        {
                                            if (Player.Position.Distance(target.Position) > W.Range)
                                            {
                                                var wpos = target.Position.Extend(Player.Position, -(target.Position.Distance(Player.Position) + W.Range));
                                                W.Cast(wpos);
                                            }
                                        }
                                    }
                                }
                            }
                        }

                        if (Q.IsReadyPerfectly())
                        {
                            if (shadow != null)
                            {
                                if (starget != null && starget.IsValidTarget(Q.Range, true, shadow.Position) && !starget.IsDead)
                                {
                                    if (!starget.IsZombie())
                                    {
                                        Q.UpdateSourcePosition(shadow.Position, shadow.Position);
                                        QCast(starget);
                                    }
                                }
                                else
                                {
                                    var target = TargetSelector.GetTarget(Q.Range, Q.DamageType, true,  shadow.Position);
                                    if (target != null)
                                    {
                                        Q.UpdateSourcePosition(shadow.Position, shadow.Position);
                                        QCast(target);
                                    }
                                }
                            }
                        }
                    }
                }

                if (Menu["Misc"]["ME"].GetValue<MenuBool>().Enabled)
                {
                    if (E.IsReadyPerfectly())
                    {
                        if (shadow != null)
                        {
                            starget = TargetSelector.SelectedTarget;
                            if (starget != null && starget.IsValidTarget(E.Range, true, shadow.Position))
                            {
                                if (!starget.IsZombie())
                                {
                                    E.Cast();
                                }
                            }
                            else
                            {
                                var target = TargetSelector.GetTarget(E.Range, E.DamageType, true,  shadow.Position);
                                if (target != null)
                                {
                                    E.Cast();
                                }
                            }
                        }
                    }
                }
            }
            Killsteal();
        }

        private void QCast(AIBaseClient target)
        {
            var qinput = Q.GetPrediction(target);
            if (qinput.Hitchance >= HitChance.High)
            {
                Q.Cast(qinput.CastPosition);
            }
        }

        private void Killsteal()
        {
            if (Menu["Misc"]["MK"].GetValue<MenuBool>().Enabled)
            {
                if (shadow != null)
                {
                    if (Menu["Killsteal Setting"]["KQ"].GetValue<MenuBool>().Enabled)
                    {
                        if (Q.IsReadyPerfectly())
                        {
                            var target = ObjectManager.Get<AIHeroClient>().FirstOrDefault(x => x.IsKillableAndValidTarget(Q.GetDamage(x), Q.DamageType, Q.Range + Player.Position.Distance(shadow.Position)));
                            if (target != null)
                            {
                                if (!target.IsDead)
                                {
                                    if (shadow.Position.Distance(target.Position) <= Q.Range)
                                    {
                                        Q.UpdateSourcePosition(shadow.Position, shadow.Position);
                                        QCast(target);
                                    }
                                }
                            }
                        }
                    }

                    if (Menu["Killsteal Setting"]["KE"].GetValue<MenuBool>().Enabled)
                    {
                        if (E.IsReadyPerfectly())
                        {
                            var target = ObjectManager.Get<AIHeroClient>().FirstOrDefault(x => x.IsKillableAndValidTarget(E.GetDamage(x), E.DamageType, Player.Position.Distance(shadow.Position) + E.Range));
                            if (target != null)
                            {
                                if (!target.IsDead)
                                {
                                    if (shadow.Position.Distance(target.Position) <= E.Range)
                                    {
                                        E.Cast();
                                    }
                                }
                            }
                        }
                    }
                }

                if (Menu["Killsteal Setting"]["KI"].GetValue<MenuBool>().Enabled)
                {
                    var target = ObjectManager.Get<AIHeroClient>().FirstOrDefault(x => x.IsKillableAndValidTarget(Player.GetSummonerSpellDamage(x, SummonerSpell.Ignite), DamageType.True, 600f));
                    if (target != null)
                    {
                        if (!target.IsZombie())
                        {
                            if (Ignite != SpellSlot.Unknown)
                            {
                                if (Ignite.IsReady())
                                {
                                    Player.Spellbook.CastSpell(Ignite, target.Position);
                                }
                            }
                        }
                    }
                }
            }
        }
        
        private void castYoumuu()
        {
            if (Menu["Item Setting"]["IY"].GetValue<MenuBool>().Enabled)
            {
                //var yomu = ItemData.Youmuus_Ghostblade.GetItem();
                Items.UseItem(Player, (int) ItemId.Youmuus_Ghostblade);
            }
        }

        private void Zed_ValueChanged1(MenuBool menuitem, EventArgs args)
        {
            
        }

        private void Zed_ValueChanged(MenuBool menuitem, EventArgs args)
        {
            
        }
    }
}