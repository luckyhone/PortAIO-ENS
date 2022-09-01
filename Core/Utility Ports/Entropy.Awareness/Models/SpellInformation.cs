using System;
using EnsoulSharp;
using Entropy.Awareness.Bases;
using Entropy.Awareness.Models.RenderObjects;

namespace Entropy.Awareness.Models
{
    public class SpellInformation : InformationBase
    {
        public AIHeroClient Source { get; }

        public SpellSlot Slot { get; }

        public SpellLine       SpellRenderObject { get; set; }
        public SummonerTexture SummonerTexture   { get; set; }

        public string Name { get; }

        public SpellInformation(AIHeroClient source, SpellSlot slot)
        {
            Source = source;
            Slot   = slot;
            Name = source.Spellbook.GetSpell(slot).SData.Name;
        }
        
        public bool IsReady;
        public bool Learned;
        public bool NotAvailable;

        public uint Level;
        
        public float StartTime;
        public float EndTime;
        public float TimeUntilReady;
        public float Cooldown;
        public float Progress;

        /// <summary>
        /// Getting the spell information and sending to the SpellLineObject
        /// </summary>
        public override void UpdateInformation()
        {
            if (!Source.IsValid)
            {
                return;
            }
            
            var spellBook = Source.Spellbook;

            //Cant get the information directly so simulating it
            if (false)
            {
                Console.WriteLine("Spell book is not valid");
                TimeUntilReady = EndTime - Game.Time;

                IsReady = TimeUntilReady < 0f;
            }
            else
            {
                var spellDataInst = spellBook.GetSpell(Slot);

                if (spellDataInst != null && spellDataInst.Learned)
                {
                    var flags = Slot;
                    
                    Learned = !flags.HasFlag(SpellState.NotLearned);

                    if (Learned)
                    {
                        if (flags.HasFlag(SpellState.NoMana)     ||
                            flags.HasFlag(SpellState.Surpressed) ||
                            flags.HasFlag(SpellState.NotLearned))
                        {
                            NotAvailable = true;
                        }
                        else
                        {
                            NotAvailable = false;

                            //Updating spell info
                            if (spellDataInst.Ammo == 0)
                            {
                                EndTime = spellDataInst.AmmoRechargeTime; // + rechargeTime;
                            }

                            Cooldown = spellDataInst.Cooldown;
                            EndTime  = spellDataInst.CooldownExpires;
                            TimeUntilReady = EndTime - Game.Time;
                            IsReady = TimeUntilReady < 0f;
                            Level = (uint)spellDataInst.Level;

                            var progress = 1f - (TimeUntilReady / Cooldown);

                            if (progress < 0f)
                            {
                                progress = 0f;
                            }

                            if (progress > 1f)
                            {
                                progress = 1f;
                            }

                            Progress = progress;
                        }
                    }
                }
            }

            if (Slot == SpellSlot.Summoner1 || Slot == SpellSlot.Summoner2)
            {
                SummonerTexture.RenderInformation = this;
                SummonerTexture.UpdateInformation();
            }
            else
            {
                SpellRenderObject.RenderInformation = this;
                SpellRenderObject.UpdateInformation();
            }
        }
    }
}