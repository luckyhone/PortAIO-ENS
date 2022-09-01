using System.Linq;
using EnsoulSharp;
using EnsoulSharp.SDK;
using EnsoulSharp.SDK.MenuUI;
using HERMES_Kalista.MyUtils;

namespace HERMES_Kalista.MyLogic.Others
{
    public static partial class Events
    {
        public static void OnProcessSpellcast(AIBaseClient sender, AIBaseClientProcessSpellCastEventArgs args)
        {
            #region Anti-Stealth

            if (args.SData.Name.ToLower().Contains("talonshadow")) //#TODO get the actual buff name
            {
                if (Items.HasItem(ObjectManager.Player,(int) ItemId.Oracle_Lens) &&
                    Items.CanUseItem(ObjectManager.Player,(int) ItemId.Oracle_Lens))
                {
                    Items.UseItem(ObjectManager.Player,(int) ItemId.Oracle_Lens);
                }
                else if (Items.HasItem(ObjectManager.Player,(int) ItemId.Control_Ward))
                {
                    Items.UseItem(ObjectManager.Player,(int) ItemId.Control_Ward);
                }
            }

            #endregion

            if (Program.ComboMenu.GetValue<MenuBool>("RComboSelf").Enabled && Program.R.IsReady() && sender.IsEnemy &&
                args.Target.NetworkId == ObjectManager.Player.NetworkId)
            {
                var cctype = SpellDb.GetByName(args.SData.Name).CcType;
                //if (ObjectManager.Player.CountEnemyHeroesInRange(600) > 1 && cctype == CcType.Suppression ||
                if (ObjectManager.Player.CountEnemyHeroesInRange(600) > 1 &&  cctype == CcType.Suppression ||
                    (cctype == CcType.Knockback &&
                     GameObjects.EnemyHeroes.Any(e => e.CharacterName == "Yasuo" && e.Distance(ObjectManager.Player) < 1100)) ||
                    cctype == CcType.Pull)
                {
                    Program.R.Cast();
                }
            }
        }
    }
}