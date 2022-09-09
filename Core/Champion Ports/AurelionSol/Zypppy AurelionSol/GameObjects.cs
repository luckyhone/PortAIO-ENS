using System;
using EnsoulSharp;

namespace Zypppy_AurelionSol
{
    using System.Collections.Generic;
    using System.Linq;
    using System.Text.RegularExpressions;


    /// <summary>
    ///     A static (stack) class which contains a sort-of cached versions of the important game objects.
    /// </summary>
    public static class GameObjects
    {
        #region Static Fields

        /// <summary>
        ///     The ally heroes list.
        /// </summary>
        private static readonly List<AIHeroClient> AllyHeroesList = new List<AIHeroClient>();

        /// <summary>
        ///     The ally list.
        /// </summary>
        private static readonly List<AIBaseClient> AllyList = new List<AIBaseClient>();

        /// <summary>
        ///     The ally minions list.
        /// </summary>
        private static readonly List<AIMinionClient> AllyMinionsList = new List<AIMinionClient>();

        /// <summary>
        ///     The ally turrets list.
        /// </summary>
        private static readonly List<AITurretClient> AllyTurretsList = new List<AITurretClient>();

        /// <summary>
        ///     The ally wards list.
        /// </summary>
        private static readonly List<AIMinionClient> AllyWardsList = new List<AIMinionClient>();

        /// <summary>
        ///     The attackable unit list.
        /// </summary>
        private static readonly List<AttackableUnit> AttackableUnitsList = new List<AttackableUnit>();

        /// <summary>
        ///     The enemy heroes list.
        /// </summary>
        private static readonly List<AIHeroClient> EnemyHeroesList = new List<AIHeroClient>();

        /// <summary>
        ///     The enemy list.
        /// </summary>
        private static readonly List<AIBaseClient> EnemyList = new List<AIBaseClient>();

        /// <summary>
        ///     The enemy minions list.
        /// </summary>
        private static readonly List<AIMinionClient> EnemyMinionsList = new List<AIMinionClient>();

        /// <summary>
        ///     The enemy turrets list.
        /// </summary>
        private static readonly List<AITurretClient> EnemyTurretsList = new List<AITurretClient>();

        /// <summary>
        ///     The enemy wards list.
        /// </summary>
        private static readonly List<AIMinionClient> EnemyWardsList = new List<AIMinionClient>();

        /// <summary>
        ///     The game objects list.
        /// </summary>
        private static readonly List<GameObject> GameObjectsList = new List<GameObject>();

        /// <summary>
        ///     The heroes list.
        /// </summary>
        private static readonly List<AIHeroClient> HeroesList = new List<AIHeroClient>();

        /// <summary>
        ///     The jungle large list.
        /// </summary>
        private static readonly List<AIMinionClient> JungleLargeList = new List<AIMinionClient>();

        /// <summary>
        ///     The jungle legendary list.
        /// </summary>
        private static readonly List<AIMinionClient> JungleLegendaryList = new List<AIMinionClient>();

        /// <summary>
        ///     The jungle list.
        /// </summary>
        private static readonly List<AIMinionClient> JungleList = new List<AIMinionClient>();

        /// <summary>
        ///     The jungle small list.
        /// </summary>
        private static readonly List<AIMinionClient> JungleSmallList = new List<AIMinionClient>();

        /// <summary>
        ///     The large name regex list.
        /// </summary>
        private static readonly string[] LargeNameRegex =
            {
                "SRU_Murkwolf[0-9.]{1,}", "SRU_Gromp", "SRU_Blue[0-9.]{1,}",
                "SRU_Razorbeak[0-9.]{1,}", "SRU_Red[0-9.]{1,}",
                "SRU_Krug[0-9]{1,}"
            };

        /// <summary>
        ///     The legendary name regex list.
        /// </summary>
        private static readonly string[] LegendaryNameRegex = { "SRU_Dragon", "SRU_Baron", "SRU_RiftHerald" };

        /// <summary>
        ///     The minions list.
        /// </summary>
        private static readonly List<AIMinionClient> MinionsList = new List<AIMinionClient>();

        /// <summary>
        ///     The small name regex list.
        /// </summary>
        private static readonly string[] SmallNameRegex = { "SRU_[a-zA-Z](.*?)Mini", "Sru_Crab" };

        /// <summary>
        ///     The turrets list.
        /// </summary>
        private static readonly List<AITurretClient> TurretsList = new List<AITurretClient>();

        /// <summary>
        ///     The wards list.
        /// </summary>
        private static readonly List<AIMinionClient> WardsList = new List<AIMinionClient>();

        /// <summary>
        ///     Indicates whether the <see cref="GameObjects" /> stack was initialized and saved required instances.
        /// </summary>
        private static bool initialized;

        #endregion

        #region Constructors and Destructors

        /// <summary>
        ///     Initializes static members of the <see cref="GameObjects" /> class.
        /// </summary>
        static GameObjects()
        {
            Initialize();
        }

        #endregion

        #region Enums

        /// <summary>
        ///     The jungle mob types.
        /// </summary>
        public enum JungleType
        {
            /// <summary>
            ///     The unknown type.
            /// </summary>
            Unknown,

            /// <summary>
            ///     The small type.
            /// </summary>
            Small,

            /// <summary>
            ///     The large type.
            /// </summary>
            Large,

            /// <summary>
            ///     The legendary type.
            /// </summary>
            Legendary
        }

        #endregion

        #region Public Properties

        /// <summary>
        ///     Gets the game objects.
        /// </summary>
        public static IEnumerable<GameObject> AllGameObjects
        {
            get
            {
                return GameObjectsList;
            }
        }

        /// <summary>
        ///     Gets the ally.
        /// </summary>
        public static IEnumerable<AIBaseClient> Ally
        {
            get
            {
                return AllyList;
            }
        }

        /// <summary>
        ///     Gets the ally heroes.
        /// </summary>
        public static IEnumerable<AIHeroClient> AllyHeroes
        {
            get
            {
                return AllyHeroesList;
            }
        }

        /// <summary>
        ///     Gets the ally minions.
        /// </summary>
        public static IEnumerable<AIMinionClient> AllyMinions
        {
            get
            {
                return AllyMinionsList;
            }
        }

        /// <summary>
        ///     Gets the ally turrets.
        /// </summary>
        public static IEnumerable<AITurretClient> AllyTurrets
        {
            get
            {
                return AllyTurretsList;
            }
        }

        /// <summary>
        ///     Gets the ally wards.
        /// </summary>
        public static IEnumerable<AIMinionClient> AllyWards
        {
            get
            {
                return AllyWardsList;
            }
        }

        /// <summary>
        ///     Gets the attackable units.
        /// </summary>
        public static IEnumerable<AttackableUnit> AttackableUnits
        {
            get
            {
                return AttackableUnitsList;
            }
        }

        /// <summary>
        ///     Gets the enemy.
        /// </summary>
        public static IEnumerable<AIBaseClient> Enemy
        {
            get
            {
                return EnemyList;
            }
        }

        /// <summary>
        ///     Gets the enemy heroes.
        /// </summary>
        public static IEnumerable<AIHeroClient> EnemyHeroes
        {
            get
            {
                return EnemyHeroesList;
            }
        }

        /// <summary>
        ///     Gets the enemy minions.
        /// </summary>
        public static IEnumerable<AIMinionClient> EnemyMinions
        {
            get
            {
                return EnemyMinionsList;
            }
        }

        /// <summary>
        ///     Gets the enemy turrets.
        /// </summary>
        public static IEnumerable<AITurretClient> EnemyTurrets
        {
            get
            {
                return EnemyTurretsList;
            }
        }

        /// <summary>
        ///     Gets the enemy wards.
        /// </summary>
        public static IEnumerable<AIMinionClient> EnemyWards
        {
            get
            {
                return EnemyWardsList;
            }
        }

        /// <summary>
        ///     Gets the heroes.
        /// </summary>
        public static IEnumerable<AIHeroClient> Heroes
        {
            get
            {
                return HeroesList;
            }
        }

        /// <summary>
        ///     Gets the jungle.
        /// </summary>
        public static IEnumerable<AIMinionClient> Jungle
        {
            get
            {
                return JungleList;
            }
        }

        /// <summary>
        ///     Gets the jungle large.
        /// </summary>
        public static IEnumerable<AIMinionClient> JungleLarge
        {
            get
            {
                return JungleLargeList;
            }
        }

        /// <summary>
        ///     Gets the jungle legendary.
        /// </summary>
        public static IEnumerable<AIMinionClient> JungleLegendary
        {
            get
            {
                return JungleLegendaryList;
            }
        }

        /// <summary>
        ///     Gets the jungle small.
        /// </summary>
        public static IEnumerable<AIMinionClient> JungleSmall
        {
            get
            {
                return JungleSmallList;
            }
        }

        /// <summary>
        ///     Gets the minions.
        /// </summary>
        public static IEnumerable<AIMinionClient> Minions
        {
            get
            {
                return MinionsList;
            }
        }

        /// <summary>
        ///     Gets or sets the player.
        /// </summary>
        public static AIHeroClient Player { get; set; }

        /// <summary>
        ///     Gets the turrets.
        /// </summary>
        public static IEnumerable<AITurretClient> Turrets
        {
            get
            {
                return TurretsList;
            }
        }

        /// <summary>
        ///     Gets the wards.
        /// </summary>
        public static IEnumerable<AIMinionClient> Wards
        {
            get
            {
                return WardsList;
            }
        }

        #endregion

        #region Public Methods and Operators

        /// <summary>
        ///     Compares two <see cref="GameObject" /> and returns if they are identical.
        /// </summary>
        /// <param name="gameObject">The GameObject</param>
        /// <param name="object">The Compare GameObject</param>
        /// <returns>Whether the <see cref="GameObject" />s are identical.</returns>
        public static bool Compare(this GameObject gameObject, GameObject @object)
        {
            return gameObject != null && gameObject.IsValid && @object != null && @object.IsValid
                   && gameObject.NetworkId == @object.NetworkId;
        }

        /// <summary>
        ///     The get operation from the GameObjects stack.
        /// </summary>
        /// <typeparam name="T">
        ///     The requested <see cref="GameObject" /> type.
        /// </typeparam>
        /// <returns>
        ///     The List containing the requested type.
        /// </returns>
        public static IEnumerable<T> Get<T>()
            where T : GameObject, new()
        {
            return AllGameObjects.OfType<T>();
        }

        /// <summary>
        ///     Get the minion jungle type.
        /// </summary>
        /// <param name="minion">
        ///     The minion
        /// </param>
        /// <returns>
        ///     The <see cref="JungleType" />
        /// </returns>
        public static JungleType GetJungleType(this AIMinionClient minion)
        {
            if (SmallNameRegex.Any(regex => Regex.IsMatch(minion.Name, regex)))
            {
                return JungleType.Small;
            }

            if (LargeNameRegex.Any(regex => Regex.IsMatch(minion.Name, regex)))
            {
                return JungleType.Large;
            }

            if (LegendaryNameRegex.Any(regex => Regex.IsMatch(minion.Name, regex)))
            {
                return JungleType.Legendary;
            }

            return JungleType.Unknown;
        }

        /// <summary>
        ///     Get get operation from the native GameObjects stack.
        /// </summary>
        /// <typeparam name="T">
        ///     The requested <see cref="GameObject" /> type.
        /// </typeparam>
        /// <returns>
        ///     The List containing the requested type.
        /// </returns>
        public static IEnumerable<T> GetNative<T>()
            where T : GameObject, new()
        {
            return ObjectManager.Get<T>();
        }

        #endregion

        #region Methods

        /// <summary>
        ///     The initialize method.
        /// </summary>
        internal static void Initialize()
        {
            if (initialized)
            {
                return;
            }

            initialized = true;

            Player = Player;

            HeroesList.AddRange(ObjectManager.Get<AIHeroClient>());
            MinionsList.AddRange(ObjectManager.Get<AIMinionClient>().Where(o => o.Team != GameObjectTeam.Neutral && !o.Name.Contains("ward")));
            TurretsList.AddRange(ObjectManager.Get<AITurretClient>());
            JungleList.AddRange(ObjectManager.Get<AIMinionClient>().Where(o => o.Team == GameObjectTeam.Neutral && o.Name != "WardCorpse" && o.Name != "Barrel"));
            WardsList.AddRange(ObjectManager.Get<AIMinionClient>().Where(o => o.Name.Contains("ward")));

            GameObjectsList.AddRange(ObjectManager.Get<GameObject>());
            AttackableUnitsList.AddRange(ObjectManager.Get<AttackableUnit>());

            EnemyHeroesList.AddRange(HeroesList.Where(o => o.IsEnemy));
            EnemyMinionsList.AddRange(MinionsList.Where(o => o.IsEnemy));
            EnemyTurretsList.AddRange(TurretsList.Where(o => o.IsEnemy));
            EnemyList.AddRange(EnemyHeroesList.Cast<AIBaseClient>().Concat(EnemyMinionsList).Concat(EnemyTurretsList));

            AllyHeroesList.AddRange(HeroesList.Where(o => o.IsAlly));
            AllyMinionsList.AddRange(MinionsList.Where(o => o.IsAlly));
            AllyTurretsList.AddRange(TurretsList.Where(o => o.IsAlly));
            AllyList.AddRange(
                AllyHeroesList.Cast<AIBaseClient>().Concat(AllyMinionsList).Concat(AllyTurretsList));

            JungleSmallList.AddRange(JungleList.Where(o => o.GetJungleType() == JungleType.Small));
            JungleLargeList.AddRange(JungleList.Where(o => o.GetJungleType() == JungleType.Large));
            JungleLegendaryList.AddRange(JungleList.Where(o => o.GetJungleType() == JungleType.Legendary));

            AllyWardsList.AddRange(WardsList.Where(o => o.IsAlly));
            EnemyWardsList.AddRange(WardsList.Where(o => o.IsEnemy));

            GameObject.OnCreate += OnCreate;
            GameObject.OnDelete += OnDelete;
        }
        

        /// <summary>
        ///     OnCreate event.
        /// </summary>
        /// <param name="sender">
        ///     The sender
        /// </param>
        private static void OnCreate(GameObject sender, EventArgs args)
        {
            GameObjectsList.Add(sender);

            var attackableUnit = sender as AttackableUnit;
            if (attackableUnit != null)
            {
                AttackableUnitsList.Add(attackableUnit);
            }

            var hero = sender as AIHeroClient;
            if (hero != null)
            {
                HeroesList.Add(hero);
                if (hero.IsEnemy)
                {
                    EnemyHeroesList.Add(hero);
                    EnemyList.Add(hero);
                }
                else
                {
                    AllyHeroesList.Add(hero);
                    AllyList.Add(hero);
                }

                return;
            }

            var minion = sender as AIMinionClient;
            if (minion != null)
            {
                if (minion.Team != GameObjectTeam.Neutral)
                {
                    if (minion.Name.Contains("ward"))
                    {
                        WardsList.Add(minion);
                        if (minion.IsEnemy)
                        {
                            EnemyWardsList.Add(minion);
                        }
                        else
                        {
                            AllyWardsList.Add(minion);
                        }
                    }
                    else
                    {
                        MinionsList.Add(minion);
                        if (minion.IsEnemy)
                        {
                            EnemyMinionsList.Add(minion);
                            EnemyList.Add(minion);
                        }
                        else
                        {
                            AllyMinionsList.Add(minion);
                            AllyList.Add(minion);
                        }
                    }
                }
                else if (minion.Name != "WardCorpse" && minion.Name != "Barrel")
                {
                    JungleList.Add(minion);
                    switch (minion.GetJungleType())
                    {
                        case JungleType.Small:
                            JungleSmallList.Add(minion);
                            break;
                        case JungleType.Large:
                            JungleLargeList.Add(minion);
                            break;
                        case JungleType.Legendary:
                            JungleLegendaryList.Add(minion);
                            break;
                    }
                }

                return;
            }

            var turret = sender as AITurretClient;
            if (turret != null)
            {
                TurretsList.Add(turret);
                if (turret.IsEnemy)
                {
                    EnemyTurretsList.Add(turret);
                    EnemyList.Add(turret);
                }
                else
                {
                    AllyTurretsList.Add(turret);
                    AllyList.Add(turret);
                }
            }
        }

        /// <summary>
        ///     OnDelete event.
        /// </summary>
        /// <param name="sender">
        ///     The sender
        /// </param>
        private static void OnDelete(GameObject sender, EventArgs args)
        {
            foreach (var gameObject in GameObjectsList.Where(o => o.Compare(sender)).ToList())
            {
                GameObjectsList.Remove(gameObject);
            }

            var attackableUnit = sender as AttackableUnit;
            if (attackableUnit != null)
            {
                foreach (var attackableUnitObject in AttackableUnitsList.Where(a => a.Compare(attackableUnit)).ToList())
                {
                    AttackableUnitsList.Remove(attackableUnitObject);
                }
            }

            var hero = sender as AIHeroClient;
            if (hero != null)
            {
                foreach (var heroObject in HeroesList.Where(h => h.Compare(hero)).ToList())
                {
                    HeroesList.Remove(heroObject);
                    if (hero.IsEnemy)
                    {
                        EnemyHeroesList.Remove(heroObject);
                        EnemyList.Remove(heroObject);
                    }
                    else
                    {
                        AllyHeroesList.Remove(heroObject);
                        AllyList.Remove(heroObject);
                    }
                }

                return;
            }

            var minion = sender as AIMinionClient;
            if (minion != null)
            {
                if (minion.Team != GameObjectTeam.Neutral)
                {
                    if (minion.Name.Contains("ward"))
                    {
                        foreach (var wardObject in WardsList.Where(w => w.Compare(minion)).ToList())
                        {
                            WardsList.Remove(wardObject);
                            if (minion.IsEnemy)
                            {
                                EnemyWardsList.Remove(wardObject);
                            }
                            else
                            {
                                AllyWardsList.Remove(wardObject);
                            }
                        }
                    }
                    else
                    {
                        foreach (var minionObject in MinionsList.Where(m => m.Compare(minion)).ToList())
                        {
                            MinionsList.Remove(minionObject);
                            if (minion.IsEnemy)
                            {
                                EnemyMinionsList.Remove(minionObject);
                                EnemyList.Remove(minionObject);
                            }
                            else
                            {
                                AllyMinionsList.Remove(minionObject);
                                AllyList.Remove(minionObject);
                            }
                        }
                    }
                }
                else
                {
                    foreach (var jungleObject in JungleList.Where(j => j.Compare(minion)).ToList())
                    {
                        JungleList.Remove(jungleObject);
                        switch (jungleObject.GetJungleType())
                        {
                            case JungleType.Small:
                                JungleSmallList.Remove(jungleObject);
                                break;
                            case JungleType.Large:
                                JungleLargeList.Remove(jungleObject);
                                break;
                            case JungleType.Legendary:
                                JungleLegendaryList.Remove(jungleObject);
                                break;
                        }
                    }
                }

                return;
            }

            var turret = sender as AITurretClient;
            if (turret != null)
            {
                foreach (var turretObject in TurretsList.Where(t => t.Compare(turret)).ToList())
                {
                    TurretsList.Remove(turretObject);
                    if (turret.IsEnemy)
                    {
                        EnemyTurretsList.Remove(turretObject);
                        EnemyList.Remove(turretObject);
                    }
                    else
                    {
                        AllyTurretsList.Remove(turretObject);
                        AllyList.Remove(turretObject);
                    }
                }
            }
        }

        #endregion
    }
}