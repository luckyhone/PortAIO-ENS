using System;
using System.Linq;
using EnsoulSharp;
using EnsoulSharp.SDK;
using EnsoulSharp.SDK.MenuUI;
using EnsoulSharp.SDK.Utility;
using hikiMarksmanRework.Core.Drawings;
using hikiMarksmanRework.Core.Menus;
using hikiMarksmanRework.Core.Spells;
using hikiMarksmanRework.Core.Utilitys;
using LeagueSharpCommon;
using SharpDX;
using SPrediction;
using Color = System.Drawing.Color;
using Menu = EnsoulSharp.SDK.MenuUI.Menu;
using Render = LeagueSharpCommon.Render;

namespace hikiMarksmanRework.Champions
{
    public class Sivir
    {
        public Sivir()
        {
            SivirOnLoad();
        }
        private static readonly Render.Sprite HikiSprite = new Render.Sprite(PortAIO.Properties.Resources.logo, new Vector2((Drawing.Width / 2) - 500, (Drawing.Height / 2) - 350));
        private static void SivirOnLoad()
        {
            SivirMenu.Config =
                new Menu("hikiMarksman:AIO - Sivir", "hikiMarksman:AIO - Sivir", true);
            {
                SivirSpells.Init();
                SivirMenu.MenuInit();
            }
            SivirMenu.Config.SetFontColor(Color.Gold.ToSharpDxColor());
            Game.Print("<font color='#ff3232'>hikiMarksman:AIO - Sivir: </font><font color='#00FF00'>loaded! You can rekt everyone with this assembly</font>", ObjectManager.Player.CharacterName);
            Game.Print(string.Format("<font color='#ff3232'>hikiMarksman:AIO - </font><font color='#00FF00'>Assembly Version: </font><font color='#ff3232'><b>{0}</b></font> ", typeof(Program).Assembly.GetName().Version));
            Game.Print("<font color='#ff3232'>If you like this assembly feel free to upvote on Assembly Database</font>");


            HikiSprite.Add(0);
            HikiSprite.OnDraw();
            DelayAction.Add(8000, () => HikiSprite.Remove());


            Game.OnUpdate += SivirOnUpdate;
            AIHeroClient.OnDoCast += SivirOnProcessSpellCast;
            Orbwalker.OnAfterAttack += AfterAttack;
            Drawing.OnDraw += SivirOnDraw;
        }
        

        private static void SivirOnUpdate(EventArgs args)
        {
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
                    Jungle();
                    break;
            }
            if (SivirMenu.Config["Harass Settings"]["sivir.q.harass"].GetValue<MenuBool>().Enabled && SivirSpells.Q.IsReady() && ObjectManager.Player.ManaPercent > SivirMenu.Config["Harass Settings"]["sivir.harass.mana"].GetValue<MenuSlider>().Value)
            {
                foreach (var enemy in HeroManager.Enemies.Where(x => x.IsValidTarget(SivirSpells.Q.Range) && SivirMenu.Config["Harass Settings"]["Q Toggle"]["sivir.q.toggle." + x.CharacterName].GetValue<MenuBool>().Enabled &&
                    SivirSpells.Q.GetPrediction(x).Hitchance >= HitChance.High))
                {
                    SivirSpells.Q.Cast(enemy);
                }
            }
        }

        private static void Combo()
        {
            if (SivirSpells.Q.IsReady() && Helper.SEnabled("sivir.q.combo"))
            {
                foreach (var enemy in HeroManager.Enemies.Where(x => x.IsValidTarget(SivirSpells.Q.Range) &&
                    SivirSpells.Q.GetPrediction(x).Hitchance >= HitChance.High))
                {
                    SivirSpells.Q.Cast(enemy);
                }
            }
        }

        private static void Harass()
        {
            if (ObjectManager.Player.ManaPercent < Helper.SSlider("sivir.harass.mana"))
            {
                return;
            }
            if (SivirSpells.Q.IsReady() && Helper.SEnabled("sivir.q.harass"))
            {
                foreach (var enemy in HeroManager.Enemies.Where(x => x.IsValidTarget(SivirSpells.Q.Range) &&
                    SivirSpells.Q.GetPrediction(x).Hitchance >= HitChance.High))
                {
                    SivirSpells.Q.Cast(enemy);
                }
            }
            
        }

        private static void Clear()
        {
            if (ObjectManager.Player.ManaPercent < Helper.SSlider("sivir.clear.mana"))
            {
                return;
            }

            if (SivirSpells.Q.IsReady() && Helper.SEnabled("sivir.q.harass") && MinionManager.GetMinions(ObjectManager.Player.Position, SivirSpells.Q.Range, MinionManager.MinionTypes.All, MinionManager.MinionTeam.NotAlly).Count >= Helper.SSlider("sivir.q.minion.hit.count")
                && SivirSpells.Q.GetLineFarmLocation(MinionManager.GetMinions(ObjectManager.Player.Position, SivirSpells.Q.Range, MinionManager.MinionTypes.All, MinionManager.MinionTeam.NotAlly)).MinionsHit >= Helper.SSlider("sivir.q.minion.hit.count"))
            {
                SivirSpells.Q.Cast(SivirSpells.Q.GetLineFarmLocation(MinionManager.GetMinions(ObjectManager.Player.Position, SivirSpells.Q.Range, MinionManager.MinionTypes.All, MinionManager.MinionTeam.NotAlly)).Position);
            }
        }

        private static void Jungle()
        {
            if (ObjectManager.Player.ManaPercent < Helper.SSlider("sivir.jungle.mana") && MinionManager.GetMinions(ObjectManager.Player.ServerPosition, ObjectManager.Player.GetRealAutoAttackRange(ObjectManager.Player) + 100, MinionManager.MinionTypes.All, MinionManager.MinionTeam.Neutral, MinionManager.MinionOrderTypes.MaxHealth) == null ||
                MinionManager.GetMinions(ObjectManager.Player.ServerPosition, ObjectManager.Player.GetRealAutoAttackRange(ObjectManager.Player) + 100, MinionManager.MinionTypes.All, MinionManager.MinionTeam.Neutral, MinionManager.MinionOrderTypes.MaxHealth).Count == 0)
            {
                return;
            }
            if (SivirSpells.Q.IsReady() && Helper.SEnabled("sivir.q.jungle"))
            {
                SivirSpells.Q.Cast(
                    MinionManager.GetMinions(ObjectManager.Player.ServerPosition,
                        ObjectManager.Player.GetRealAutoAttackRange(ObjectManager.Player) + 100, MinionManager.MinionTypes.All,
                        MinionManager.MinionTeam.Neutral, MinionManager.MinionOrderTypes.MaxHealth)[0].Position);
            }
            if (SivirSpells.W.IsReady() && Helper.SEnabled("sivir.w.jungle"))
            {
                SivirSpells.W.Cast(
                    MinionManager.GetMinions(ObjectManager.Player.ServerPosition,
                        ObjectManager.Player.GetRealAutoAttackRange(ObjectManager.Player) + 100, MinionManager.MinionTypes.All,
                        MinionManager.MinionTeam.Neutral, MinionManager.MinionOrderTypes.MaxHealth)[0].Position);
            }
        }

        private static void SivirOnDraw(EventArgs args)
        {
            SivirDrawing.Init();
        }

        private static void AfterAttack(object sender, AfterAttackEventArgs e)
        {
            if (Orbwalker.ActiveMode == OrbwalkerMode.Combo && Helper.SEnabled("sivir.w.combo") && SivirSpells.W.IsReady()
                 && e.Target.IsValidTarget(SivirSpells.W.Range))
            {
                SivirSpells.W.Cast();
            }
        }

        private static void SivirOnProcessSpellCast(AIBaseClient sender, AIBaseClientProcessSpellCastEventArgs spell)
        {
            if (ObjectManager.Player.Distance(spell.End) <= 250 && sender.IsEnemy)
            {
                foreach (var block in EvadeDb.SpellData.SpellDatabase.Spells.Where(o => o.spellName == spell.SData.Name))
                {
                    if (SivirMenu.Config["Miscellaneous"]["(E) Spell Block"]["block." + block.spellName].GetValue<MenuBool>().Enabled)
                    {
                        SivirSpells.E.Cast();
                    }
                }
            } 
        }
    }
}
