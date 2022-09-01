using EnsoulSharp;
using EnsoulSharp.SDK.MenuUI;
using SharpDX.Direct3D9;

namespace NabbTracker
{
    using Font = SharpDX.Direct3D9.Font;
    using Color = System.Drawing.Color;    

    /// <summary>
    /// The Variables class.
    /// </summary>
    class Variables
    {
        /// <summary>
        /// The Menu.
        /// </summary>
        public static Menu Menu { get; set; }

        /// <summary>
        /// The Menu name.
        /// </summary>        
        public static readonly string MainMenuName = "nabbtracker";

        /// <summary>
        /// The Menu Codename.
        /// </summary>
        public static readonly string MainMenuCodeName = "NabbTracker";

        /// <summary>
        /// The Text fcnt.
        /// </summary>
        public static Font DisplayTextFont = new Font(Drawing.Direct3DDevice9, new FontDescription
        {
            FaceName =  "Tahoma",
            Height = 15,
            Weight = FontWeight.Normal,
            OutputPrecision =  FontPrecision.Outline,
            Quality = FontQuality.ClearType
        });

        /// <summary>
        /// Gets the SummonerSpell name.
        /// </summary>
        public static string GetSummonerSpellName;

        /// <summary>
        /// Gets the spellslots.
        /// </summary>
        public static SpellSlot[]
            SpellSlots = {
                SpellSlot.Q,
                SpellSlot.W,
                SpellSlot.E,
                SpellSlot.R
            },

            SummonerSpellSlots = {
                SpellSlot.Summoner1,
                SpellSlot.Summoner2
            };

        /// <summary>
        /// The Spells Healthbars X coordinate.
        /// </summary>
        public static int SpellX;

        /// <summary>
        /// The Spells Healthbars Y coordinate.
        /// </summary>
        public static int SpellY;

        /// <summary>
        /// The SummonerSpells Healthbar X coordinate.
        /// </summary>        
        public static int SummonerSpellX;
        
        /// <summary>
        /// The SummonerSpells Healthbar Y coordinate.
        /// </summary>        
        public static int SummonerSpellY;

        /// <summary>
        /// The SpellLevel X coordinate.
        /// </summary>        
        public static int SpellLevelX;
        
        /// <summary>
        /// The Healthbars Y coordinate.
        /// </summary>        
        public static int SpellLevelY;
    }
}