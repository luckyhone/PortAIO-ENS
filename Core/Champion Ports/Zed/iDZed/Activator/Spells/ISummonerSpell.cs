using EnsoulSharp.SDK;
using EnsoulSharp.SDK.MenuUI;

namespace iDZed.Activator.Spells
{
    interface ISummonerSpell
    {
        void OnLoad();
        string GetDisplayName();
        void AddToMenu(Menu menu);
        bool RunCondition();
        void Execute();
        SummonerSpell GetSummonerSpell();
        string GetName();
    }
}