using System;
using System.Linq;
using EnsoulSharp;
using EnsoulSharp.SDK;
using EnsoulSharp.SDK.Utility;
using PortAIO;

namespace KoreanZed
{
    class ZedSpellDodge
    {
        private readonly ZedSpell r;

        private readonly ZedMenu zedMenu;

        private readonly AIHeroClient player;

        public ZedSpellDodge(ZedSpells spells, ZedMenu mainMenu)
        {
            r = spells.R;
            zedMenu = mainMenu;
            player = ObjectManager.Player;

            AIBaseClient.OnProcessSpellCast += Obj_AI_Base_OnProcessSpellCast;
        }

        private void Obj_AI_Base_OnProcessSpellCast(AIBaseClient sender, AIBaseClientProcessSpellCastEventArgs args)
        {
            if (sender == null || args == null || !r.IsReady() || r.Instance.ToggleState != 0
                || !player.GetEnemiesInRange(r.Range).Any() || args.Slot != SpellSlot.R
                || !sender.IsEnemy || !zedMenu.GetParamBool("koreanzed.miscmenu.rdodge.user"))
            {
                return;
            }

            if (((args.Target != null && args.Target.IsMe)
                 || player.Distance(args.End) < Math.Max(args.SData.CastRadiusSecondary, args.SData.LineWidth))
                && zedMenu.GetParamBool("koreanzed.miscmenu.rdodge." + args.SData.Name.ToLowerInvariant()))
            {
                int delay = (int) Math.Truncate((double)(player.Distance(sender) / args.SData.MissileSpeed)) - 1;
                DelayAction.Add(delay, () => { r.Cast(TargetSelector.GetTarget(r.Range, r.DamageType)); });
            }
        }
    }
}