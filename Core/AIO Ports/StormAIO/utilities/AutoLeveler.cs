using System;
using EnsoulSharp;
using EnsoulSharp.SDK.MenuUI;
using EnsoulSharp.SDK.Utility;

namespace StormAIO.utilities
{
    public class AutoLeveler
    {
        private static int[] SpellLevels;
        private static AIHeroClient Player => ObjectManager.Player;
        private static bool Urf => Math.Abs(Player.PercentCooldownMod) >= 0.8;
        private static bool LevelMenu => MainMenu.Level.GetValue<MenuBool>("autolevel").Enabled;

        public AutoLeveler()
        {
            Champ();
            if (!LevelMenu || Urf || SpellLevels == null) return;
            DelayAction.Add(500, MyLevelLogic);
            AIHeroClient.OnLevelUp += AIHeroClientOnOnLevelUp;
        }

        #region Args

        private void AIHeroClientOnOnLevelUp(AIHeroClient sender, AIHeroClientLevelUpEventArgs args)
        {
            if (sender.IsMe && LevelMenu) DelayAction.Add(100, MyLevelLogic);
        }

        #endregion

        #region Functions

        private void MyLevelLogic()
        {
            var qLevel = Player.Spellbook.GetSpell(SpellSlot.Q).Level;
            var wLevel = Player.Spellbook.GetSpell(SpellSlot.W).Level;
            var eLevel = Player.Spellbook.GetSpell(SpellSlot.E).Level;
            var rLevel = Player.Spellbook.GetSpell(SpellSlot.R).Level;
            if (qLevel + wLevel + eLevel + rLevel >= Player.Level || Player.Level > 18) return;

            var level = new[] {0, 0, 0, 0};
            for (var i = 0; i < Player.Level; i++) level[SpellLevels[i] - 1] = level[SpellLevels[i] - 1] + 1;

            if (qLevel < level[0]) Player.Spellbook.LevelSpell(SpellSlot.Q);
            if (wLevel < level[1]) Player.Spellbook.LevelSpell(SpellSlot.W);
            if (eLevel < level[2]) Player.Spellbook.LevelSpell(SpellSlot.E);
            if (rLevel < level[3]) Player.Spellbook.LevelSpell(SpellSlot.R);
        }

        private static void Champ()
        {
            var champ = Player.CharacterName;
            switch (champ)
            {
                case "Yone":
                    SpellLevels = new[] {1, 2, 3, 1, 1, 4, 1, 3, 1, 3, 4, 3, 3, 2, 2, 4, 2, 2};
                    break;
                case "Warwick":
                    SpellLevels = new[] {1, 2, 3, 2, 2, 4, 1, 1, 1, 1, 4, 3, 3, 3, 3, 4, 2, 2};
                    break;
                case "Akali":
                    SpellLevels = new[] {1, 3, 2, 1, 1, 4, 1, 3, 1, 3, 4, 3, 3, 2, 2, 4, 2, 2};
                    break;
                case "Yorick":
                    SpellLevels = new[] {1, 3, 2, 1, 1, 4, 1, 2, 1, 2, 4, 2, 2, 3, 3, 4, 3, 3};
                    break;
                case "KogMaw":
                    SpellLevels = new[] {2, 1, 3, 2, 2, 4, 2, 1, 2, 1, 4, 1, 1, 3, 3, 4, 3, 3};
                    break;
                case "DrMundo":
                    SpellLevels = new[] {1, 3, 2, 1, 1, 4, 1, 3, 1, 3, 4, 3, 3, 2, 2, 4, 2, 2};
                    break;
                case "Rengar":
                    SpellLevels = new[] {1, 2, 3, 1, 1, 4, 1, 3, 1, 3, 4, 3, 3, 2, 2, 4, 2, 2};
                    break;
                case "Garen":
                    SpellLevels = new[] {1, 3, 2, 3, 3, 4, 3, 1, 3, 1, 4, 1, 1, 2, 2, 4, 2, 2};
                    break;
                case "Urgot":
                    SpellLevels = new[] {1, 3, 2, 2, 2, 4, 2, 3, 2, 3, 4, 3, 3, 1, 1, 4, 1, 1};
                    break;
                case "Lucian":
                    SpellLevels = new[] {1, 3, 2, 1, 1, 4, 1, 2, 1, 3, 4, 3, 3, 3, 2, 4, 2, 2};
                    break;
                case "Chogath":
                    SpellLevels = new[] {1, 3, 1, 2, 1, 4, 1, 2, 1, 2, 4, 2, 2, 3, 3, 4, 3, 3};
                    break;
                case "Zed":
                    SpellLevels = new[] {1, 2, 3, 1, 1, 4, 1, 3, 1, 3, 4, 3, 3, 2, 2, 4, 2, 2};
                    break;
            }
        }

        #endregion
    }
}