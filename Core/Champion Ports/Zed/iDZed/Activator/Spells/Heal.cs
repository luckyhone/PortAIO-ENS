using System.Linq;
using EnsoulSharp;
using EnsoulSharp.SDK;
using EnsoulSharp.SDK.MenuUI;

namespace iDZed.Activator.Spells
{
    class Heal : ISummonerSpell
    {
        public void OnLoad() { }
        public string GetDisplayName()
        {
            return "Heal";
        }
        

        public void AddToMenu(Menu menu)
        {
            menu.Add(
                new MenuSlider("com.idz.zed.activator.summonerspells." + GetName() + ".hpercent", "Health %",25, 1));
        }

        public bool RunCondition()
        {
            return GetSummonerSpell().IsReady() &&
                   Zed.Menu["com.idz.zed.activator"]["com.idz.zed.activator.summonerspells"]["com.idz.zed.activator.summonerspells." + GetName() + ".enabled"].GetValue<MenuBool>().Enabled &&
                   ObjectManager.Player.HealthPercent <=
                   Zed.Menu["com.idz.zed.activator"]["com.idz.zed.activator.summonerspells"]["com.idz.zed.activator.summonerspells." + GetName() + ".hpercent"].GetValue<MenuSlider>().Value &&
                   ObjectManager.Player.CountEnemyHeroesInRange(ObjectManager.Player.AttackRange) >= 1;
        }

        public void Execute()
        {
            GetSummonerSpell().Cast();
        }

        public SummonerSpell GetSummonerSpell()
        {
            return SummonerSpells.Heal;
        }

        public string GetName()
        {
            return GetSummonerSpell().Names.First().ToLowerInvariant();
        }
    }
}