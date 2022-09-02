using System;
using EnsoulSharp;
using EnsoulSharp.SDK;
using EnsoulSharp.SDK.MenuUI;

namespace OKTWPredictioner
{
    public class OKTWPredictioner
    {
        public static Spell[] Spells = {null, null, null, null};
        public static Menu Config;

        public static void Initialize()
        {
            #region Initialize Menu

            Config = new Menu("oktwpredictioner", "OKTW Predictioner", true);
            Config.Add(new MenuKeyBind("COMBOKEY", "Combo", Keys.Space, KeyBindType.Press));
            Config.Add(new MenuKeyBind("HARASSKEY", "Harass", Keys.C, KeyBindType.Press));
            Config.Add(new MenuBool("ENABLED", "Enabled"));

            #region Initialize Spells

            Menu skillshots = new Menu("spredskillshots", "Skillshots");
            foreach (var spell in SpellDatabase.Spells)
            {
                if (spell.ChampionName == ObjectManager.Player.CharacterName)
                {
                    Spells[(int) spell.Slot] = new Spell(spell.Slot, spell.Range);
                    Spells[(int) spell.Slot].SetSkillshot(spell.Delay / 1000f, spell.Radius, spell.MissileSpeed,
                        spell.Collisionable, spell.Type);
                    skillshots.Add(new MenuBool(String.Format("{0}{1}", spell.ChampionName, spell.Slot),
                        "Convert Spell " + spell.Slot));
                }
            }

            Config.Add(skillshots);

            #endregion

            //SPrediction.Prediction.Initialize(Config, "SPREDFORSPREDICTONER");
            //onfig["SPREDFORSPREDICTONER"].Name = "SPrediction";
            Config.Add(new MenuList("SPREDHITC", "Hit Chance", Utility.HitchanceNameArray, 2))
                .SetTooltip("High is recommended");
            Config.Attach();

            #endregion

            #region Initialize Events

            Spellbook.OnCastSpell += EventHandlers.Spellbook_OnCastSpell;
            AIBaseClient.OnDoCast += EventHandlers.AIHeroClient_OnProcessSpellCast;

            #endregion

        }
    }
}