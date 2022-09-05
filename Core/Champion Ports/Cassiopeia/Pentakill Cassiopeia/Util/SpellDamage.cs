using EnsoulSharp;
using EnsoulSharp.SDK;
using EnsoulSharp.SDK.MenuUI;

namespace Pentakill_Cassiopeia.Util
{
    class SpellDamage
    {
        public static float getComboDamage(AIHeroClient target)
        {
            float damage = 0f;
            if (Program.menuController.getMenu()["comboUseQ"].GetValue<MenuBool>().Enabled)
            {
                if (Program.q.IsReady())
                {
                    //Multipled by 1.5 since Q will likely hit at least twice in a combo
                    damage += Program.q.GetDamage(target) * 1.5f;
                }
            }
            if (Program.menuController.getMenu()["comboUseW"].GetValue<MenuBool>().Enabled)
            {
                if (Program.w.IsReady())
                {
                    damage += Program.w.GetDamage(target);
                }
            }
            if (Program.menuController.getMenu()["comboUseE"].GetValue<MenuBool>().Enabled)
            {
                if (Program.e.IsReady())
                {
                    //Multipled by 3 since E will likely hit at least thrice in a combo
                    damage += Program.e.GetDamage(target) * 3f;
                }
            }
            if (Program.menuController.getMenu()["comboUseR"].GetValue<MenuBool>().Enabled)
            {
                if (Program.r.IsReady())
                {
                    damage += Program.r.GetDamage(target);
                }
            }
            if (Program.menuController.getMenu()["useIgnite"].GetValue<MenuBool>().Enabled)
            {
                if (Program.ignite.IsReady())
                {
                    damage += (float)Program.player.GetSummonerSpellDamage(target, SummonerSpell.Ignite);
                }
            }
            return damage;
        }

        //TODO: More Accurate Methods
    }
}