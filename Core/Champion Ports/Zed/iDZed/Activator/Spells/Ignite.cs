using System.Linq;
using EnsoulSharp;
using EnsoulSharp.SDK;
using EnsoulSharp.SDK.Damages.SummonerSpells;
using EnsoulSharp.SDK.MenuUI;
using PortAIO;

namespace iDZed.Activator.Spells
{
    class Ignite : ISummonerSpell
    {
        public void OnLoad() { }
        public string GetDisplayName()
        {
            return "Ignite";
        }

        public void AddToMenu(Menu menu)
        {
        }

        public bool RunCondition()
        {
            if (GetSummonerSpell().IsReady() &&
                GameObjects.EnemyHeroes.FirstOrDefault(
                    x => x.IsValidTarget(GetSummonerSpell().Range) && x.HasBuff("zedulttargetmark")) != null)
            {
                return true;
            }

            return GetSummonerSpell().IsReady() &&
                   Zed.Menu["com.idz.zed.activator"]["com.idz.zed.activator.summonerspells"]["com.idz.zed.activator.summonerspells." + GetName() + ".enabled"].GetValue<MenuBool>().Enabled &&
                   ObjectManager.Player.GetEnemiesInRange(GetSummonerSpell().Range)
                       .Any(
                           h =>
                               h.Health + 20 <
                               ObjectManager.Player.GetSummonerSpellDamage(h, EnsoulSharp.SDK.SummonerSpell.Ignite) &&
                               h.IsValidTarget(GetSummonerSpell().Range) && h.CountAllyHeroesInRange(550f) < 3 && !((h.Health + 20 > Zed._spells[SpellSlot.Q].GetDamage(h) + Zed._spells[SpellSlot.E].GetDamage(h) + ObjectManager.Player.GetAutoAttackDamage(h))));
        }

        public void Execute()
        {
            AIHeroClient target = ObjectManager.Player.GetEnemiesInRange(GetSummonerSpell().Range).Find(h => h.Health + 20 < ObjectManager.Player.GetSummonerSpellDamage(h, EnsoulSharp.SDK.SummonerSpell.Ignite) || h.HasBuff("zedulttargetmark"));
            if (target.IsValidTarget(GetSummonerSpell().Range))
            {
                GetSummonerSpell().Cast(target);
            }
        }

        public SummonerSpell GetSummonerSpell()
        {
            return SummonerSpells.Ignite;
        }

        public string GetName()
        {
            return GetSummonerSpell().Names.First().ToLowerInvariant();
        }
    }
}