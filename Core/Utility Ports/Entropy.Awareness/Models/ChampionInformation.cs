using System.Collections.Generic;
using EnsoulSharp;
using Entropy.Awareness.Bases;
using PortAIO;

namespace Entropy.Awareness.Models
{
    public class ChampionInformation : InformationBase
    {
        public readonly AIHeroClient Source;
        
        public ChampionInformation(AIHeroClient source)
        {
            Source = source;
            
            SpellInformations = new List<SpellInformation>
            {
                new SpellInformation(source, SpellSlot.Q),
                new SpellInformation(source, SpellSlot.W),
                new SpellInformation(source, SpellSlot.E),
                new SpellInformation(source, SpellSlot.R),
            };


            Summoner1 = new SpellInformation(source, SpellSlot.Summoner1);
            Summoner2 = new SpellInformation(source, SpellSlot.Summoner2);
            
            SummonerInformations.Add(Summoner1);
            SummonerInformations.Add(Summoner2);
            
            TeleportInformation = new TeleportInformation(source);
            LastSeenInformation = new LastSeenInformation(source);
        }

        public readonly List<SpellInformation> SpellInformations;
        public readonly List<SpellInformation> SummonerInformations = new List<SpellInformation>();
        
        public readonly SpellInformation Summoner1;
        public readonly SpellInformation Summoner2;

        public readonly TeleportInformation TeleportInformation;

        public readonly LastSeenInformation LastSeenInformation;

        public float CurrentEXP;
        public float MaxEXP;
        public float EXPProgress;

        public override void UpdateInformation()
        {
            var currentLevel = Source.Level;
            var lastExp = Source.TotalMaxEXP(currentLevel - 1);
            CurrentEXP = Source.Experience - lastExp;
            MaxEXP = Source.TotalMaxEXP(currentLevel) - lastExp;
            EXPProgress = CurrentEXP / MaxEXP;
            
            //GameConsole.Print($"{CurrentEXP} / {MaxEXP} = {EXPProgress}");
            
            foreach (var spellInformation in SpellInformations)
            {
                spellInformation.UpdateInformation();
            }
            
            Summoner1.UpdateInformation();
            Summoner2.UpdateInformation();
            
            TeleportInformation.UpdateInformation();
            
            LastSeenInformation.UpdateInformation();

        }
        
    }
}