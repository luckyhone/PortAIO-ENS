using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using EnsoulSharp;
using EnsoulSharp.SDK;
using EnsoulSharp.SDK.MenuUI;
using Entropy.AIO.Bases;
using hikiMarksmanRework.Champions;
using hikiMarksmanRework.Core.Menus;
using hikiMarksmanRework.Core.Spells;
 namespace hikiMarksmanRework.Core.Utilitys
{
    class Helper
    {
        public static bool Enabled(string menuName)
        {
            return GravesMenu.Config[menuName].GetValue<MenuBool>().Enabled;
        }

        public static int Slider(string menuName)
        {
            return GravesMenu.Config[menuName].GetValue<MenuSlider>().Value;
        }

        public static bool SEnabled(string menuName)
        {
            return SivirMenu.Config[menuName].GetValue<MenuBool>().Enabled;
        }

        public static int SSlider(string menuName)
        {
            return SivirMenu.Config[menuName].GetValue<MenuSlider>().Value;
        }

        public static int CSlider(string menuName)
        {
            return CorkiMenu.Config[menuName].GetValue<MenuSlider>().Value;
        }
        public static bool CEnabled(string menuName)
        {
            return CorkiMenu.Config[menuName].GetValue<MenuBool>().Enabled;
        }
        /*public static bool EEnabled(string menuName)
        {
            return EzrealMenu.Config.Item(menuName).GetValue<bool>();
        }

        public static int ESlider(string menuName)
        {
            return EzrealMenu.Config.Item(menuName).GetValue<Slider>().Value;
        }*/

        public static bool LEnabled(string menuName)
        {
            return LucianMenu.Config[menuName].GetValue<MenuBool>().Enabled;
        }

        public static int LSlider(string menuName)
        {
            return LucianMenu.Config[menuName].GetValue<MenuSlider>().Value;
        }

        public static bool VEnabled(string menuName)
        {
            return VayneMenu.Config[menuName].GetValue<MenuBool>().Enabled;
        }

        public static int VSlider(string menuName)
        {
            return VayneMenu.Config[menuName].GetValue<MenuSlider>().Value;
        }
        public static bool DEnabled(string menuName)
        {
            return DravenMenu.Config[menuName].GetValue<MenuBool>().Enabled;
        }

        public static int DSlider(string menuName)
        {
            return DravenMenu.Config[menuName].GetValue<MenuSlider>().Value;
        }

        public static void GravesAntiGapcloser(AIBaseClient sender, AIBaseClientProcessSpellCastEventArgs spell)
        {
            try
            {
                if (sender.IsEnemy && spell.End.Distance(ObjectManager.Player.Position) < GravesSpells.E.Range &&
                    spell.Target.IsMe)
                {
                    foreach (var gapclose in AntiGapcloseSpell.GapcloseableSpells
                                 .Where(x => spell.SData.Name == ((AIHeroClient)sender).GetSpell(x.Slot).Name)
                                 .OrderByDescending(c =>
                                     GravesMenu.Config["Miscellaneous"]["Anti-Gapclose Settings"][
                                         "gapclose.slider." + sender.CharacterName].GetValue<MenuSlider>().Value))
                    {
                        if (GravesMenu.Config["Miscellaneous"]["Anti-Gapclose Settings"][
                                "gapclose." + ((AIHeroClient)sender).CharacterName].GetValue<MenuBool>().Enabled)
                        {
                            GravesSpells.E.Cast(ObjectManager.Player.Position.Extend(spell.End, -GravesSpells.E.Range));
                        }
                    }
                }
            }
            catch (Exception e)
            {
                //
            }
        }

        /*public static void EzrealAntiGapcloser(Obj_AI_Base sender, GameObjectProcessSpellCastEventArgs spell)
        {
            if (sender.IsEnemy && spell.End.Distance(ObjectManager.Player.Position) < EzrealSpells.E.Range && !spell.SData.IsAutoAttack() && spell.Target.IsMe)
            {
                foreach (var gapclose in AntiGapcloseSpell.GapcloseableSpells.Where(x => spell.SData.Name == ((AIHeroClient)sender).GetSpell(x.Slot).Name).OrderByDescending(c => Slider("gapclose.slider."+sender.CharData.BaseSkinName)))
                {
                    if (Enabled("gapclose." + ((AIHeroClient)sender).ChampionName))
                    {
                        EzrealSpells.E.Cast(ObjectManager.Player.Position.Extend(spell.End, -EzrealSpells.E.Range));
                    }
                }
            }
        }*/

        public static void CorkiAntiGapcloser(AIBaseClient sender, AIBaseClientProcessSpellCastEventArgs spell)
        {
            if (sender.IsEnemy && spell.End.Distance(ObjectManager.Player.Position) < CorkiSpells.E.Range && spell.Target.IsMe)
            {
                foreach (var gapclose in AntiGapcloseSpell.GapcloseableSpells.Where(x => spell.SData.Name == ((AIHeroClient)sender).GetSpell(x.Slot).Name).OrderByDescending(c => CSlider("gapclose.slider." + sender.CharacterName)))
                {
                    if (CEnabled("gapclose." + ((AIHeroClient)sender).CharacterName))
                    {
                        CorkiSpells.W.Cast(ObjectManager.Player.Position.Extend(spell.End, -CorkiSpells.W.Range));
                    }
                }
            }
        }

        /*public static void LucianAntiGapcloser(Obj_AI_Base sender, GameObjectProcessSpellCastEventArgs spell)
        {
            if (sender.IsEnemy && spell.End.Distance(ObjectManager.Player.Position) < LucianSpells.E.Range && !spell.SData.IsAutoAttack() && spell.Target.IsMe)
            {
                foreach (var gapclose in AntiGapcloseSpell.GapcloseableSpells.Where(x => spell.SData.Name == ((AIHeroClient)sender).GetSpell(x.Slot).Name).OrderByDescending(c => Slider("gapclose.slider." + sender.CharData.BaseSkinName)))
                {
                    if (Enabled("gapclose." + ((AIHeroClient)sender).ChampionName))
                    {
                        CorkiSpells.W.Cast(ObjectManager.Player.Position.Extend(spell.End, -LucianSpells.W.Range));
                    }
                }
            }
        }*/

        public static void VayneAntiGapcloser(AIBaseClient sender, AIBaseClientProcessSpellCastEventArgs spell)
        {
            if (sender.IsEnemy && spell.End.Distance(ObjectManager.Player.Position) < VayneSpells.E.Range  && spell.Target.IsMe)
            {
                foreach (var gapclose in AntiGapcloseSpell.GapcloseableSpells
                             .Where(x => spell.SData.Name == ((AIHeroClient)sender).GetSpell(x.Slot).Name)
                             .OrderByDescending(c =>
                                 VayneMenu.Config[":: Miscellaneous"]["Anti-Gapclose Settings"][
                                     "gapclose.slider." + sender.CharacterName].GetValue<MenuSlider>().Value))
                {
                    if (VayneMenu.Config[":: Miscellaneous"]["Anti-Gapclose Settings"][
                            "gapclose." + ((AIHeroClient)sender).CharacterName].GetValue<MenuBool>().Enabled)
                    {
                        VayneSpells.E.Cast(sender);
                    }
                }
            }
        }

        /*public static void SivirSpellBlockInit(Obj_AI_Base sender, GameObjectProcessSpellCastEventArgs spell)
        {
            if (sender.IsEnemy && !spell.SData.IsAutoAttack() && spell.Target.IsMe)
            {
                foreach (var gapclose in EvadeDb.SpellData.SpellDatabase.Spells.Where(x => x.spellName == spell.SData.Name))
                {
                    switch (gapclose.spellType)
                    {
                        case EvadeDb.SpellType.Cone:
                            if (ObjectManager.Player.Distance(spell.End) <= 250 && SivirSpells.E.IsReady())
                            {
                                SivirSpells.E.Cast();
                            }
                            break;
                        case EvadeDb.SpellType.Line:
                            if (ObjectManager.Player.Distance(spell.End) <= 100 && SivirSpells.E.IsReady())
                            {
                                SivirSpells.E.Cast();
                            }
                            break;
                        case EvadeDb.SpellType.Circular:
                            if (ObjectManager.Player.Distance(spell.End) <= 250 && SivirSpells.E.IsReady())
                            {
                                SivirSpells.E.Cast();
                            }
                            break;
                    }
                }
            }
        }*/
    }
}
