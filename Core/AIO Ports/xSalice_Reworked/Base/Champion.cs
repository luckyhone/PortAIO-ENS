using System;
using EnsoulSharp;
using EnsoulSharp.SDK;
using EnsoulSharp.SDK.MenuUI;
using xSalice_Reworked.PluginLoad;

namespace xSalice_Reworked.Base
{
    public class Champion : SpellBase
    {
        public static Menu Menu;
        protected readonly AIHeroClient Player = ObjectManager.Player;

        protected Champion()
        {
            //Events
            Game.OnUpdate += Game_OnGameUpdateEvent;
            Drawing.OnDraw += Drawing_OnDrawEvent;
            Interrupter.OnInterrupterSpell += Interrupter_OnPosibleToInterruptEvent;
            AntiGapcloser.OnGapcloser += AntiGapcloser_OnEnemyGapcloserEvent;
            //AttackableUnit. += ObjAiHeroOnOnDamage;

            GameObject.OnCreate += GameObject_OnCreateEvent;
            GameObject.OnDelete += GameObject_OnDeleteEvent;

            Spellbook.OnUpdateChargedSpell += Spellbook_OnUpdateChargeableSpellEvent;
            Spellbook.OnCastSpell += SpellbookOnOnCastSpell;
            Spellbook.OnStopCast += SpellbookOnOnStopCast;

            Orbwalker.OnAfterAttack += AfterAttackEvent;
            Orbwalker.OnBeforeAttack += BeforeAttackEvent;
            //Orbwalking.OnAttack += OnAttack;

            Spellbook.OnCastSpell += Obj_AI_Base_OnSpellCast;
            AIBaseClient.OnProcessSpellCast += Obj_AI_Base_OnProcessSpellCastEvent;
            AIBaseClient.OnIssueOrder += ObjAiHeroOnOnIssueOrderEvent;
            AIBaseClient.OnBuffAdd += ObjAiBaseOnOnBuffAdd;
            AIBaseClient.OnBuffRemove += ObjAiBaseOnOnBuffLose;
        }

        private void Drawing_OnDrawEvent(EventArgs args)
        {
            if (Player.IsDead) return;
            Drawing_OnDraw(args);
        }
        protected virtual void Drawing_OnDraw(EventArgs args)
        {
            
        }

        private void Obj_AI_Base_OnSpellCast(Spellbook sender, SpellbookCastSpellEventArgs args)
        {
            OnSpellCast(sender, args);
        }
        private void OnSpellCast(Spellbook sender, SpellbookCastSpellEventArgs args)
        {
            
        }

        private void AntiGapcloser_OnEnemyGapcloserEvent(AIHeroClient sender, AntiGapcloser.GapcloserArgs args)
        {
            AntiGapcloser_OnEnemyGapcloser(sender, args);
        }
        protected virtual void AntiGapcloser_OnEnemyGapcloser(AIHeroClient sender, AntiGapcloser.GapcloserArgs args)
        {
            
        }

        private void Interrupter_OnPosibleToInterruptEvent(AIHeroClient sender, Interrupter.InterruptSpellArgs args)
        {
            Interrupter_OnPosibleToInterrupt(sender, args);
        }
        protected virtual void Interrupter_OnPosibleToInterrupt(AIHeroClient sender, Interrupter.InterruptSpellArgs args)
        {
            
        }

        private void Game_OnGameUpdateEvent(EventArgs args)
        {
            if (Player.IsDead && Player.CharacterName.ToLower() != "karthus") return;
            Game_OnGameUpdate(args);
        }
        protected virtual void Game_OnGameUpdate(EventArgs args)
        {
            
        }

        private void GameObject_OnDeleteEvent(GameObject sender, EventArgs args)
        {
            GameObject_OnDelete(sender,args);
        }

        private void GameObject_OnDelete(GameObject sender, EventArgs args)
        {
            
        }

        private void GameObject_OnCreateEvent(GameObject sender, EventArgs args)
        {
            GameObject_OnCreate(sender, args);
        }

        private void GameObject_OnCreate(GameObject sender, EventArgs args)
        {
        }

        private void Obj_AI_Base_OnProcessSpellCastEvent(AIBaseClient sender, AIBaseClientProcessSpellCastEventArgs args)
        {
            Obj_AI_Base_OnProcessSpellCast(sender, args);
        }
        private void Obj_AI_Base_OnProcessSpellCast(AIBaseClient sender, AIBaseClientProcessSpellCastEventArgs args)
        {
        }

        private void AfterAttackEvent(object sender, AfterAttackEventArgs e)
        {
            AfterAttack(sender, e);
        }
        protected virtual void AfterAttack(object sender, AfterAttackEventArgs e)
        {
            
        }

        private void BeforeAttackEvent(object sender, BeforeAttackEventArgs e)
        {
            BeforeAttack(sender, e);
        }
        protected virtual void BeforeAttack(object sender, BeforeAttackEventArgs e)
        {
           
        }

        private void ObjAiHeroOnOnIssueOrderEvent(AIBaseClient sender, AIBaseClientIssueOrderEventArgs args)
        {
            ObjAiHeroOnOnIssueOrder(sender, args);
        }
        protected virtual void ObjAiHeroOnOnIssueOrder(AIBaseClient sender, AIBaseClientIssueOrderEventArgs args)
        {
           
        }

        private void Spellbook_OnUpdateChargeableSpellEvent(Spellbook sender, SpellbookUpdateChargedSpellEventArgs args)
        {
            Spellbook_OnUpdateChargeableSpell(sender, args);
        }

        private void Spellbook_OnUpdateChargeableSpell(Spellbook sender, SpellbookUpdateChargedSpellEventArgs args)
        {
            
        }
        private void SpellbookOnOnCastSpell(Spellbook sender, SpellbookCastSpellEventArgs args)
        {
            
        }

        private void SpellbookOnOnStopCast(Spellbook sender, SpellbookStopCastEventArgs args)
        {
            
        }

        private void ObjAiBaseOnOnBuffAdd(AIBaseClient sender, AIBaseClientBuffAddEventArgs args)
        {
            
        }

        private void ObjAiBaseOnOnBuffLose(AIBaseClient sender, AIBaseClientBuffRemoveEventArgs args)
        {
            
        }

        public Champion(bool load)
        {
            if (load)
            {
                GameOnLoad();
            }
        }
        private void GameOnLoad()
        {
            Game.Print(
                "<font color = \"#FFB6C1\">xSalice's Ressurected AIO</font> by <font color = \"#00FFFF\">xSalice</font>, imsosharp Update, NightMoon Rework!");

            Menu = new Menu("xSalice's " + Player.CharacterName, Player.CharacterName, true);

            Menu.Attach();

            var pluginLoader = new PluginLoader();
        }
    }
}