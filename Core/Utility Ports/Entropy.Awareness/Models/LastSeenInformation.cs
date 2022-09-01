using System;
using EnsoulSharp;
using Entropy.Awareness.Bases;
using LeagueSharpCommon;
using PortAIO.Library_Ports.Entropy.Lib.Geometry;
using SharpDX;

namespace Entropy.Awareness.Models
{
    public class LastSeenInformation : InformationBase
    {
        public AIHeroClient Source { get;  }

        public Vector2 LastTacticalMapPosition { get; set; }
        public Vector3 LastWorldPosition { get; set; }

        public int LastTickSeen { get; set; }
        
        public LastSeenInformation(AIHeroClient source)
        {
            Source = source;
        }
        
        public override void UpdateInformation()
        {
            //Ignore
        }

        public void RecordLastPosition()
        {
            LastWorldPosition = Source.Position;
            LastTacticalMapPosition = LastWorldPosition.WorldToTacticalMap();
            LastTickSeen = Environment.TickCount;
        }

        public void Spawn()
        {
            LastWorldPosition       = ObjectManager.Get<Obj_SpawnPoint>().Find(o => o.IsEnemy).Position;
            LastTacticalMapPosition = LastWorldPosition.WorldToTacticalMap();
        }
        
    }
}