using System;
using System.Collections.Generic;
using System.Linq;
using EnsoulSharp;
using EnsoulSharp.SDK;
using Entropy.Awareness.Models;

namespace Entropy.Awareness.Managers
{
    public static class InformationManager
    {
        private static bool Initialized { get; set; }
        
        public static readonly Dictionary<uint, ChampionInformation> ChampionInformations = new Dictionary<uint, ChampionInformation>();
        public static List<ChampionInformation> VisibleChampionInformations = new List<ChampionInformation>();

        #region Trackers Data Updating
        
        
        #endregion

        private static AIHeroClient LocalPlayer => ObjectManager.Player;
        public static void Initialize()
        {
            if (Initialized)
            {
                return;
            }

            Initialized = true;
            
            
            var heroes = TrackersCommon.Enemies;

            if (MenuComponents.SpellTracker.TrackAllies.Enabled)
            {
                heroes.AddRange(TrackersCommon.Allies);
            };
            
            heroes.Add(LocalPlayer);

            foreach (var hero in heroes)
            {
                //GameConsole.Print($"Adding {hero.CharName}");
                ChampionInformations.Add((uint)hero.NetworkId, new ChampionInformation(hero));
            }
            
            GameEvent.OnGameTick += OnFastUpdateTick;

            GameEvent.OnGameTick += UpdateTickOnOnTick;
            
            return;
            
            //TeleportInformation Events
            Teleport.OnTeleport += OnTeleport;
            
            //SpellsInformation Events
            
            //LastSeenInformation Events
            /*AIBaseClient.OnDeath += OnDeath;
            AIBaseClient.OnSpawn += OnSpawn;
            AttackableUnit.OnLeaveVisibilityClient += OnLeaveVisibilityClient;
            AttackableUnit.OnEnterVisibilityClient += OnEnterVisibilityClient;*/
            
            //EXP 
            //HeroExperience.OnAddExp += HeroExperienceOnOnAddExp;

        }

        private static void OnFastUpdateTick(EventArgs args)
        {
            VisibleChampionInformations = ChampionInformations.Values.Where(x => !x.Source.IsDead && x.Source.Health > 0f && x.Source.IsVisible && x.Source.IsVisibleOnScreen).ToList();
        }
        

        /*private static void OnEnterVisibilityClient(AttackableUnitEnterVisibilityClientEventArgs args)
        {
            if (args.Sender is AIHeroClient hero && hero.IsEnemy())
            {
                var lastSeenInformation = ChampionInformations[hero.NetworkID].LastSeenInformation;
                
                lastSeenInformation.RecordLastPosition();
            }
        }

        private static void OnLeaveVisibilityClient(AttackableUnitLeaveVisibilityClientEventArgs args)
        {
            if (args.Sender is AIHeroClient hero && hero.IsEnemy())
            {
                var lastSeenInformation = ChampionInformations[hero.NetworkID].LastSeenInformation;
                
                lastSeenInformation.RecordLastPosition();
            }
           
        }

        private static void OnSpawn(AIBaseClientSpawnEventArgs args)
        {
            if (args.Sender == null || !args.Sender.IsValid || args.Sender.IsAlly())
            {
                return;
            }
            
            var information = ChampionInformations[args.Sender.NetworkID];
            
            information.LastSeenInformation.Spawn();
        }

        private static void OnDeath(AIBaseClientDeathEventArgs args)
        {
            if (args.Sender == null || !args.Sender.IsValid || args.Sender.IsAlly())
            {
                return;
            }
            
            var information = ChampionInformations[args.Sender.NetworkID];
            
            information.LastSeenInformation.Spawn();
        }*/

        private static void OnTeleport(AIBaseClient Sender, Teleport.TeleportEventArgs args)
        {
            if (Sender == null || !Sender.IsValid || Sender.IsAlly)
            {
                return;
            }

            var information = ChampionInformations[(uint)Sender.NetworkId];

            switch (args.Status)
            {
                case Teleport.TeleportStatus.Start:
                    information.TeleportInformation.NewTeleport(Environment.TickCount, (int)args.Duration, args.Type);
                    break;
                case Teleport.TeleportStatus.Abort:
                    information.TeleportInformation.AbortTeleport();
                    break;
            }
        }

        private static void UpdateTickOnOnTick(EventArgs args)
        {
            foreach (var champInfo in ChampionInformations.Values)
            {
                champInfo.UpdateInformation();
            }
        }

        #region Trackers Data

        #endregion
    }
}