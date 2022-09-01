using System;
using EnsoulSharp;
using EnsoulSharp.SDK;
using Entropy.Awareness.Bases;

namespace Entropy.Awareness.Models
{
    public class TeleportInformation : InformationBase
    {
        public AIHeroClient Source { get; }

        public int TeleportStart { get; set; }
        public int TeleportEnd { get; set; }
        public int Duration { get; set; }
        
        public Teleport.TeleportType Type { get; set; }

        public bool IsValid => TeleportEnd > Environment.TickCount;
        
        public TeleportInformation( AIHeroClient source)
        {
            Source = source;
        }
        
        public override void UpdateInformation()
        {
            
        }

        public void NewTeleport(int start, int duration, Teleport.TeleportType type)
        {
            TeleportStart = start;
            TeleportEnd = TeleportStart + duration;
            Duration = duration;
            Type = type;
        }

        public void AbortTeleport()
        {
            TeleportEnd = 0;
            TeleportStart = 0;
            Duration = 0;
            Type = Teleport.TeleportType.Unknown;
        }
    }
}