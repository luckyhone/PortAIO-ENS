using System;
using EnsoulSharp;
using EnsoulSharp.SDK;
using EnsoulSharp.SDK.MenuUI;

namespace Challenger_Series.Utils.Plugins
{
    public class ChampionName : CSPlugin
    {
        public ChampionName()
        {
            base.Q = new Spell(SpellSlot.Q);
            base.W = new Spell(SpellSlot.W, 1100);
            base.W.SetSkillshot(250f, 75f, 1500f, true, SpellType.Line);
            base.E = new Spell(SpellSlot.E, 25000);
            base.R = new Spell(SpellSlot.R, 1400);
            base.R.SetSkillshot(250f, 80f, 1500f, false, SpellType.Line);
            InitMenu();
            AIHeroClient.OnProcessSpellCast += OnSpellCast;
            Orbwalker.OnAttack += OnAttack;
            Orbwalker.OnBeforeAttack += OnBeforeAttack;
            DelayedOnUpdate += OnUpdate;
            Drawing.OnDraw += OnDraw;
            AntiGapcloser.OnGapcloser += EventsOnOnGapCloser;
            Interrupter.OnInterrupterSpell += OnInterruptableTarget;
        }

        private void OnInterruptableTarget(AIHeroClient sender, Interrupter.InterruptSpellArgs args)
        {
        }

        private void EventsOnOnGapCloser(AIHeroClient sender, AntiGapcloser.GapcloserArgs args)
        {
        }

        private void OnBeforeAttack(object sender, BeforeAttackEventArgs e)
        {
        }

        private void OnAttack(object sender, AttackingEventArgs e)
        {
        }

        public override void OnDraw(EventArgs args)
        {
        }

        public override void OnUpdate(EventArgs args)
        {
        }

        private void OnSpellCast(AIBaseClient sender, AIBaseClientProcessSpellCastEventArgs args)
        {
        }

        private Menu ComboMenu;
        private MenuBool UseQCombo;
        private MenuBool UseWCombo;
        private MenuBool UseECombo;
        private MenuBool UseRCombo;
        public void InitMenu()
        {
            ComboMenu = MainMenu.Add(new Menu("ChampionNamecombomenu", "Combo Settings: "));
            UseQCombo = ComboMenu.Add(new MenuBool("ChampionNameqcombo", "Use Q", true));
            UseWCombo = ComboMenu.Add(new MenuBool("ChampionNamewcombo", "Use W", true));
            UseECombo = ComboMenu.Add(new MenuBool("ChampionNameecombo", "Use E", true));
            UseRCombo = ComboMenu.Add(new MenuBool("ChampionNamercombo", "Use R", true));
            MainMenu.Attach();
        }

    }
}