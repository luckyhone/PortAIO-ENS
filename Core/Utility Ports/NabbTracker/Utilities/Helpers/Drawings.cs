using System.Linq;
using EnsoulSharp;
using EnsoulSharp.SDK;
using EnsoulSharp.SDK.MenuUI;
using SharpDX;

namespace NabbTracker
{
    class Drawings
    {
        /// <summary>
        /// Loads the range drawings.
        /// </summary>
        public static void Initialize()
        {
            Drawing.OnDraw += delegate
            {
                foreach (var pg in GameObjects.Heroes
                    .Where(pg =>
                        pg.IsHPBarRendered &&
                        (pg.IsEnemy && Variables.Menu[$"{Variables.MainMenuName}.enemies"].GetValue<MenuBool>().Enabled ||
                         (pg.IsMe || pg.IsAlly) && Variables.Menu[$"{Variables.MainMenuName}.allies"].GetValue<MenuBool>().Enabled)))
                {
                    for (int Spell = 0; Spell < Variables.SpellSlots.Count(); Spell++)
                    {
                        Variables.SpellX = (int)pg.HPBarPosition.X - 30 + (Spell * 25);
                        Variables.SpellY = (int)pg.HPBarPosition.Y + (pg.CharacterName.Equals("Jhin") ? 25 : 5);

                        Variables.DisplayTextFont.DrawText(
                            null,
                            pg.Spellbook.GetSpell(Variables.SpellSlots[Spell]).CooldownExpires - Game.Time + 1 > 0 ?
                                string.Format("{0:0}", pg.Spellbook.GetSpell(Variables.SpellSlots[Spell]).CooldownExpires - Game.Time + 1) :
                                Variables.SpellSlots[Spell].ToString(),
                            
                            Variables.SpellX,
                            Variables.SpellY,

                            pg.Spellbook.GetSpell(Variables.SpellSlots[Spell]).Level < 1 ?
                                Color.Gray :

                            pg.Spellbook.GetSpell(Variables.SpellSlots[Spell]).SData.ManaArray
                            .MaxOrDefault((value) => value) > pg.Mana ?
                                Color.Cyan :

                            pg.Spellbook.GetSpell(Variables.SpellSlots[Spell]).CooldownExpires - Game.Time + 1 > 0 && 
                            pg.Spellbook.GetSpell(Variables.SpellSlots[Spell]).CooldownExpires - Game.Time + 1 <= 4 ?
                                Color.Red :

                            pg.Spellbook.GetSpell(Variables.SpellSlots[Spell]).CooldownExpires - Game.Time + 1 > 4 ?
                                Color.Yellow :
                                Color.LightGreen
                        );

                        for (int DrawSpellLevel = 0; DrawSpellLevel <= pg.Spellbook.GetSpell(Variables.SpellSlots[Spell]).Level - 1; DrawSpellLevel++)
                        {
                            Variables.SpellLevelX = Variables.SpellX + (DrawSpellLevel * 3) - 4;
                            Variables.SpellLevelY = Variables.SpellY + 4;
                            
                            Variables.DisplayTextFont.DrawText(
                                null,
                                ".",

                                Variables.SpellLevelX,
                                Variables.SpellLevelY,
                                
                                Color.White
                            );
                        }
                    }
                    
                    for (int SummonerSpell = 0; SummonerSpell < Variables.SummonerSpellSlots.Count(); SummonerSpell++)
                    {
                        Variables.SummonerSpellX = (int)pg.HPBarPosition.X - 80 + (SummonerSpell * 88);
                        Variables.SummonerSpellY = (int)pg.HPBarPosition.Y + (pg.CharacterName.Equals("Jhin") ? -6 : -50);

                        switch (pg.Spellbook.GetSpell(Variables.SummonerSpellSlots[SummonerSpell]).Name.ToLower())
                        {
                            case "summonerflash":        Variables.GetSummonerSpellName = "Flash";        break;
                            case "summonerdot":          Variables.GetSummonerSpellName = "Ignite";       break;
                            case "summonerheal":         Variables.GetSummonerSpellName = "Heal";         break;
                            case "summonerteleport":     Variables.GetSummonerSpellName = "Teleport";     break;
                            case "summonerexhaust":      Variables.GetSummonerSpellName = "Exhaust";      break;
                            case "summonerhaste":        Variables.GetSummonerSpellName = "Ghost";        break;
                            case "summonerbarrier":      Variables.GetSummonerSpellName = "Barrier";      break;
                            case "summonerboost":        Variables.GetSummonerSpellName = "Cleanse";      break;
                            case "summonermana":         Variables.GetSummonerSpellName = "Clarity";      break;
                            case "summonerclairvoyance": Variables.GetSummonerSpellName = "Clairvoyance"; break;
                            case "summonerodingarrison": Variables.GetSummonerSpellName = "Garrison";     break;
                            case "summonersnowball":     Variables.GetSummonerSpellName = "Mark";         break;
                            
                            default: Variables.GetSummonerSpellName = "Smite"; break;
                        }
                        
                        Variables.DisplayTextFont.DrawText(
                            null,
                            pg.Spellbook.GetSpell(Variables.SummonerSpellSlots[SummonerSpell]).CooldownExpires - Game.Time + 1 > 0 ?
                                Variables.GetSummonerSpellName + ":" + string.Format("{0:0}", pg.Spellbook.GetSpell(Variables.SummonerSpellSlots[SummonerSpell]).CooldownExpires - Game.Time + 1) :
                                Variables.GetSummonerSpellName + ": UP ",
                            
                            Variables.SummonerSpellX,
                            Variables.SummonerSpellY,
                            
                            pg.Spellbook.GetSpell(Variables.SummonerSpellSlots[SummonerSpell]).CooldownExpires - Game.Time + 1 > 0 ?
                                SharpDX.Color.Red :
                                SharpDX.Color.Yellow
                        );
                    }
                }
            };
        }
    }
}