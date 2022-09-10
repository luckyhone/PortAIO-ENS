using System;
using System.Collections.Generic;
using System.Linq;
using Challenger_Series.Utils;
using EnsoulSharp;
using EnsoulSharp.SDK;
using EnsoulSharp.SDK.MenuUI;
using EnsoulSharp.SDK.Utility;
using ExorAIO.Utilities;
using LeagueSharpCommon;
using SharpDX;
using SPrediction;
using Geometry = LeagueSharpCommon.Geometry.Geometry;
using KeyBindType = EnsoulSharp.SDK.MenuUI.KeyBindType;
using Keys = EnsoulSharp.SDK.MenuUI.Keys;
using Menu = EnsoulSharp.SDK.MenuUI.Menu;

namespace hi_im_gosu_Reborn
{
    public class Vayne
    {
        public static Spell E;
        public static Spell E2;
        public static Spell Q;
        public static Spell W;
        public static Spell R;




        public static Vector3 TumblePosition = Vector3.Zero;

        
        private static string News = "Added New Custom Orbwalker for better cs'ing and movement";

        public static Menu menu;

        public static Dictionary<string, SpellSlot> spellData;
        public static Items.Item zzrot = new Items.Item(3512, 400);

        public static AIHeroClient tar;
        public const string ChampName = "Vayne";
        public static AIHeroClient Player;
        public static BuffType[] buffs;
        public static Spell cleanse;
        public static Menu Itemsmenu;
        public static Menu Potionsmenu;
        public static Menu qmenu;
        public static Menu emenu;
        public static Menu gmenu;
        public static Menu imenu;
        public static Menu rmenu;
        public static Menu botrk;
        public static Menu qss;

        //private static Obj_AI_Base target;
        /* Asuna VayneHunter Copypasta */
        public static readonly Vector2 MidPos = new Vector2(6707.485f, 8802.744f);

        public static readonly Vector2 DragPos = new Vector2(11514, 4462);

        public static float LastMoveC;
        //private static Obj_AI_Base target;
        public static bool CheckTarget(AIBaseClient target, float range = float.MaxValue)
        {
            if (target == null)
            {
                return false;
            }

            if (target.DistanceToPlayer() > range)
            {
                return false;
            }

            if (target.HasBuff("KindredRNoDeathBuff"))
            {
                return false;
            }

            if (target.HasBuff("UndyingRage") && target.GetBuff("UndyingRage").EndTime - Game.Time > 0.3)
            {
                return false;
            }

            if (target.HasBuff("JudicatorIntervention"))
            {
                return false;
            }

            if (target.HasBuff("ChronoShift") && target.GetBuff("ChronoShift").EndTime - Game.Time > 0.3)
            {
                return false;
            }

            if (target.HasBuff("ShroudofDarkness"))
            {
                return false;
            }

            if (target.HasBuff("SivirShield"))
            {
                return false;
            }

            return !target.HasBuff("FioraW");
        }
       

        public static void MoveToLimited(Vector3 where)
        {
            if (Environment.TickCount - LastMoveC < 80)
            {
                return;
            }

            LastMoveC = Environment.TickCount;
            ObjectManager.Player.IssueOrder(GameObjectOrder.MoveTo, where);
        }

        /* End Asuna VayneHunter Copypasta */

        public static void Loads()
        {
            try
            {
                Game_OnGameLoad(new EventArgs());

            }
            catch (Exception e)
            {
                Console.WriteLine(e.ToString());
                return;
            }
        }
        /*public static void DrawingOnOnDraw(EventArgs args)
        {
            // if vayne got Q ready
            if (Q.IsReady())
            {
                var s = ObjectManager.Player.Position;
                var e = s.Extend(Game.CursorPos, Q.Range);
                DrawPointer(s, e, Q.Range);
            }
        }*/

        /* public static void DrawPointer(Vector3 start, Vector3 end, float len)
         {
             var line = new Geometry.Polygon.Line(start, end, len);
             var endNext = end.Extend(new Vector3(1, 0, 0), 100).To2D()
                 .RotateAroundPoint(start.To2D(), 90 * (float)Math.PI / 180);
             var endNext2 = end.Extend(new Vector3(1, 0, 0), 100).To2D()
                 .RotateAroundPoint(start.To2D(), -90 * (float)Math.PI / 180);
             var line2 = new Geometry.Polygon.Line(end.To2D(), endNext, 50);
             var line3 = new Geometry.Polygon.Line(end.To2D(), endNext2, 50);
             line2.Draw(System.Drawing.Color.Crimson);
             line3.Draw(System.Drawing.Color.Crimson);
             line.Draw(System.Drawing.Color.Crimson);
         }*/
        public static void Game_OnGameLoad(EventArgs args)
        {
            Q = new Spell(SpellSlot.Q, 300f);
            W = new Spell(SpellSlot.W);
            E2 = new Spell(
               SpellSlot.E,(uint)(650 + ObjectManager.Player.BoundingRadius));
            E = new Spell(SpellSlot.E, 550f);
            E.SetTargetted(0.25f, 1600f);
            R = new Spell(SpellSlot.R);

            Player = ObjectManager.Player;
            //Utils.PrintMessage("Vayne loaded");
            if (Player.CharacterName != ChampName) return;
            spellData = new Dictionary<string, SpellSlot>();
            //Chat.Print("Riven");
            menu = new Menu("Gosu", "Gosu", true);

            menu.Add(
                new MenuKeyBind("aaqaa", "Auto -> Q -> AA",Keys.X, KeyBindType.Press));

            qmenu = menu.Add(new Menu("Tumble", "Tumble"));
            qmenu.Add(new MenuBool("UseQC", "Use Q Combo").SetValue(true));
            qmenu.Add(new MenuBool("hq", "Use Q Harass").SetValue(true));
            qmenu.Add(new MenuBool("restrictq", "Restrict Q usage?").SetValue(true));
            qmenu.Add(new MenuBool("UseQJ", "Use Q Farm").SetValue(true));
            qmenu.Add(new MenuSlider("Junglemana", "Minimum Mana to Use Q Farm",60, 1, 100));
            qmenu.Add(new MenuBool("AntiMQ", "Use Anti - Melee [Q]").SetValue(true));
           // qmenu.AddItem(new MenuItem("FastQ", "Fast Q").SetValue(true).SetValue(new KeyBind("Q".ToCharArray()[0], KeyBindType.Press)));
            qmenu.Add(new MenuBool("FocusTwoW", "Focus 2 W Stacks").SetValue(true));
            //qmenu.AddItem(new MenuItem("DrawQ", "Draw Q Arrow").SetValue(true));


            emenu = menu.Add(new Menu("Condemn", "Condemn"));
            emenu.Add(new MenuBool("UseEC", "Use E Combo").SetValue(true));
            emenu.Add(new MenuBool("he", "Use E Harass").SetValue(true));
            emenu.Add(new MenuKeyBind("UseET", "Use E (Toggle)",Keys.T, KeyBindType.Toggle));
           // emenu.AddItem(new MenuItem("FlashE", "Flash E").SetValue(true).SetValue(new KeyBind("Y".ToCharArray()[0], KeyBindType.Press)));


            //emenu.AddItem(new MenuItem("Gap_E", "Use E To Gabcloser").SetValue(true));
            // emenu.AddItem(new MenuItem("GapD", "Anti GapCloser Delay").SetValue(new Slider(0, 0, 1000)).SetTooltip("Sets a delay before the Condemn for Antigapcloser is casted."));
            emenu.Add(new MenuList("EMode", "Use E Mode:",new[] { "Lord's", "Gosu", "Flowers", "VHR", "Marksman", "Sharpshooter", "OKTW", "Shine", "PRADASMART", "PRADAPERFECT", "OLDPRADA", "PRADALEGACY" }));
            emenu.Add(new MenuSlider("PushDistance", "E Push Distance",415,300, 475));
            emenu.Add(new MenuSlider("EHitchance", "E Hitchance",50, 1, 100)).SetTooltip("Only use this for Prada Condemn Methods");
            emenu.Add(new MenuKeyBind("UseEaa", "Use E after auto",Keys.M, KeyBindType.Press));



            rmenu = menu.Add(new Menu("Ult", "Ult"));
            rmenu.Add(new MenuBool("visibleR", "Smart Invisible R").SetValue(true).SetTooltip("Wether you want to set a delay to stay in R before you Q"));
            rmenu.Add(new MenuSlider("Qtime", "Duration to wait",700, 0, 1000));

            imenu = menu.Add(new Menu("Interrupt Settings", "Interrupt Settings"));
            imenu.Add(new MenuBool("Int_E", "Use E To Interrupt").SetValue(true));
            imenu.Add(new MenuBool("Interrupt", "Interrupt Danger Spells", true).SetValue(true));
            imenu.Add(new MenuBool("AntiAlistar", "Interrupt Alistar W", true).SetValue(true));
            imenu.Add(new MenuBool("AntiRengar", "Interrupt Rengar Jump", true).SetValue(true));
            imenu.Add(new MenuBool("AntiKhazix", "Interrupt Khazix R", true).SetValue(true));


            gmenu = menu.Add(new Menu("Gap Closer", "Gap Closer"));
            gmenu.Add(new MenuBool("Gapcloser", "Anti Gapcloser", true).SetValue(false));
            foreach (var target in HeroManager.Enemies)
            {
                gmenu.Add(
                    new MenuBool("AntiGapcloser" + target.CharacterName.ToLower(), target.CharacterName, true)
                        .SetValue(false));
            }


            menu.Add(new MenuKeyBind("walltumble", "Wall Tumble",Keys.U, KeyBindType.Press));
            menu.Add(new MenuBool("useR", "Use R Combo").SetValue(true));
            menu.Add(new MenuSlider("enemys", "If Enemys Around >=",2, 1, 5));
            Itemsmenu = menu.Add(new Menu("Items", "Items"));
            Potionsmenu=Itemsmenu.Add(new Menu("Potions", "Potions"));
            Potionsmenu.Add(new MenuBool("usehppotions", "Use Health potion/Refillable/Hunters/Corrupting/Biscuit"))
                .SetValue(true);
            Potionsmenu.Add(new MenuSlider("usepotionhp", "If Health % <",35, 1, 100));
            Potionsmenu.Add(new MenuBool("usemppotions", "Use Refillable/Corrupting/Biscuit"))
                .SetValue(true);
            Potionsmenu.Add(new MenuSlider("usepotionmp", "If Mana % <",35, 1, 100));
            Itemsmenu.Add(new MenuBool("Ghostblade", "Use Ghostblade").SetValue(true));
            Itemsmenu.Add(new MenuBool("QSS", "Use QSS/Merc Scimitar/Cleanse").SetValue(true));
            qss = Itemsmenu.Add(new Menu("useqss","QSS/Merc/Cleanse Settings"));

            buffs = new[]
                        {
                            BuffType.Blind, BuffType.Charm, BuffType.CombatDehancer, BuffType.Fear, BuffType.Flee,
                            BuffType.Knockback, BuffType.Knockup, BuffType.Polymorph, BuffType.Silence,
                            BuffType.Snare, BuffType.Stun, BuffType.Suppression, BuffType.Taunt
                        };

            for (int i = 0; i < buffs.Length; i++)
            {
                qss.Add(new MenuBool(buffs[i].ToString(), buffs[i].ToString()).SetValue(true));
            }

            Q = new Spell(SpellSlot.Q, 0f);
            R = new Spell(SpellSlot.R, float.MaxValue);
            E = new Spell(SpellSlot.E, 650f);
            E.SetTargetted(0.25f, 1600f);

            var cde = ObjectManager.Player.Spellbook.GetSpell(ObjectManager.Player.GetSpellSlot("SummonerBoost"));
            if (cde != null)
            {
                if (cde.Slot != SpellSlot.Unknown) //trees
                {
                    cleanse = new Spell(cde.Slot);
                }
            }

            E.SetTargetted(0.25f, 2200f);
            AIBaseClient.OnDoCast += Game_ProcessSpell;
            Game.OnUpdate += Game_OnGameUpdate;
            Orbwalker.OnAfterAttack += Orbwalking_AfterAttack;
            Orbwalker.OnBeforeAttack += Orbwalking_BeforeAttack;
            AntiGapcloser.OnGapcloser += AntiGapcloser_OnEnemyGapcloser;
            Interrupter.OnInterrupterSpell += Interrupter2_OnInterruptableTarget;
            AIBaseClient.OnPlayAnimation += OnPlay;
            GameObject.OnCreate += OnCreate;
            //  Drawing.OnDraw += DrawingOnOnDraw;


            //Chat.Print("<font color='#881df2'>Blm95 Vayne Reborn by LordZEDith</font> Loaded.");
            Game.Print("<font size='30'>hi_im_gosu Reborn</font> <font color='#b756c5'>by LordZEDith</font>");
            Game.Print("<font color='#b756c5'>NEWS: </font>" + News);
            //Chat.Print(
            // "<font color='#f2f21d'>Do you like it???  </font> <font color='#ff1900'>Drop 1 Upvote in Database </font>");
            //  Chat.Print(
            //  "<font color='#f2f21d'>Buy me cigars </font> <font color='#ff1900'>ssssssssssmith@hotmail.com</font> (10) S");
            menu.Attach();
        }


        private static void AntiGapcloser_OnEnemyGapcloser(AIHeroClient sender, AntiGapcloser.GapcloserArgs gapcloser)
        {
            if (E.IsReady())
            {
                if (imenu["AntiAlistar"].GetValue<MenuBool>().Enabled && sender.CharacterName == "Alistar" &&
                    gapcloser.Type == AntiGapcloser.GapcloserType.Targeted)
                {
                    E.CastOnUnit(sender);

                    if (gmenu["Gapcloser"].GetValue<MenuBool>().Enabled &&
                    gmenu["AntiGapcloser" + sender.CharacterName.ToLower()].GetValue<MenuBool>().Enabled)
                    {
                        if (sender.DistanceToPlayer() <= 200 && sender.IsValid)
                        {
                            E.CastOnUnit(sender);
                        }
                    }
                }
            }
        }
        public static void OnCreate(GameObject sender, EventArgs Args)
        {
            var Rengar = HeroManager.Enemies.Find(heros => heros.CharacterName.Equals("Rengar"));
            var Khazix = HeroManager.Enemies.Find(heros => heros.CharacterName.Equals("Khazix"));

            if (Rengar != null && imenu["AntiRengar"].GetValue<MenuBool>().Enabled)
            {
                if (sender.Name == "Rengar_LeapSound.troy" && sender.Position.Distance(ObjectManager.Player.Position) < E.Range)
                {
                    E.CastOnUnit(Rengar);
                }
            }

            if (Khazix != null && imenu["AntiKhazix"].GetValue<MenuBool>().Enabled)
            {
                if (sender.Name == "Khazix_Base_E_Tar.troy" && sender.Position.Distance(ObjectManager.Player.Position) <= 300)
                {
                    E.CastOnUnit(Khazix);
                }
            }
        }

        public static void Interrupter2_OnInterruptableTarget(AIHeroClient unit, Interrupter.InterruptSpellArgs args)
        {
            if (imenu["Int_E"].GetValue<MenuBool>().Enabled && E.IsReady() && unit.IsEnemy &&
                unit.IsValidTarget(E.Range))
            {
                if (args.DangerLevel >= Interrupter.DangerLevel.High)
                {
                    E.CastOnUnit(unit);
                }
            }
        }

        public static void Game_ProcessSpell(AIBaseClient hero, AIBaseClientProcessSpellCastEventArgs args)
        {
            if (args.SData.Name.ToLower() == "zedult" && args.Target.IsMe)
            {
                if (Items.CanUseItem(Player,3140))
                {
                    DelayAction.Add(1000, () => Items.UseItem(Player,3140));
                }
                else if (Items.CanUseItem(Player,3139))
                {
                    DelayAction.Add(1000, () => Items.UseItem(Player,3139));
                }
                else if (cleanse != null && cleanse.IsReady())
                {
                    DelayAction.Add(1000, () => cleanse.Cast());
                }
            }
            if (hero != null)
                if (args.Target != null)
                    if (args.Target.IsMe)
                        if (hero.Type == GameObjectType.AIHeroClient)
                            if (hero.IsEnemy)
                                if (hero.IsMelee)
                                        if (qmenu["AntiMQ"].GetValue<MenuBool>().Enabled)
                                            if (Q.IsReady())
                                                Q.Cast(ObjectManager.Player.Position.Extend(hero.Position, -Q.Range));

        }

        public static void Usepotion()
        {
            var iusehppotion = Potionsmenu["usehppotions"].GetValue<MenuBool>().Enabled;
            var iusepotionhp = Player.Health
                               <= (Player.MaxHealth * (Potionsmenu["usepotionhp"].GetValue<MenuSlider>().Value) / 100);
            var iusemppotion = Potionsmenu["usemppotions"].GetValue<MenuBool>().Enabled;
            var iusepotionmp = Player.Mana
                               <= (Player.MaxMana * (Potionsmenu["usepotionmp"].GetValue<MenuSlider>().Value) / 100);
            if (Player.InFountain() || ObjectManager.Player.HasBuff("Recall")) return;

            if (Player.CountEnemyHeroesInRange(800) > 0)
            {
                if (iusepotionhp && iusehppotion
                    && !(ObjectManager.Player.HasBuff("RegenerationPotion")
                         || ObjectManager.Player.HasBuff("ItemMiniRegenPotion")
                         || ObjectManager.Player.HasBuff("ItemCrystalFlask")
                         || ObjectManager.Player.HasBuff("ItemCrystalFlaskJungle")
                         || ObjectManager.Player.HasBuff("ItemDarkCrystalFlask")))
                {
                    if (Items.HasItem(Player,ItemId.Health_Potion) && Items.CanUseItem(Player,(int)ItemId.Health_Potion))
                    {
                        Items.UseItem(Player,(int)ItemId.Health_Potion);
                    }

                    if (Items.HasItem(Player,ItemId.Refillable_Potion) && Items.CanUseItem(Player,2031))
                    {
                        Items.UseItem(Player,2031);
                    }

                    if (Items.HasItem(Player,ItemId.Corrupting_Potion) && Items.CanUseItem(Player,2033))
                    {
                        Items.UseItem(Player,2033);
                    }

                    if (Items.HasItem(Player,ItemId.Total_Biscuit_of_Everlasting_Will) && Items.CanUseItem(Player,2010))
                    {
                        Items.UseItem(Player,2010);
                    }
                    
                }

                if (iusepotionmp && iusemppotion
                    && !(ObjectManager.Player.HasBuff("ItemDarkCrystalFlask")
                         || ObjectManager.Player.HasBuff("ItemMiniRegenPotion")
                         || ObjectManager.Player.HasBuff("ItemCrystalFlaskJungle")
                         || ObjectManager.Player.HasBuff("ItemCrystalFlask")))
                {
                    if (Items.HasItem(Player,ItemId.Health_Potion) && Items.CanUseItem(Player,(int)ItemId.Health_Potion))
                    {
                        Items.UseItem(Player,(int)ItemId.Health_Potion);
                    }

                    if (Items.HasItem(Player,ItemId.Refillable_Potion) && Items.CanUseItem(Player,2031))
                    {
                        Items.UseItem(Player,2031);
                    }

                    if (Items.HasItem(Player,ItemId.Corrupting_Potion) && Items.CanUseItem(Player,2033))
                    {
                        Items.UseItem(Player,2033);
                    }

                    if (Items.HasItem(Player,ItemId.Total_Biscuit_of_Everlasting_Will) && Items.CanUseItem(Player,2010))
                    {
                        Items.UseItem(Player,2010);
                    }
                }
            }
        }

        /* public static void Farm()
         {
             var mob =
                 MinionManager.GetMinions(
                     Player.ServerPosition,
                     E.Range,
                     MinionTypes.All,
                     MinionTeam.Neutral,
                     MinionOrderTypes.MaxHealth).FirstOrDefault();
             var Minions = MinionManager.GetMinions(Player.Position.Extend(Game.CursorPos, Q.Range), Player.AttackRange, MinionTypes.All);
             var useQ = qmenu.Item("UseQJ").GetValue<bool>();
             int countMinions = 0;
             foreach (var minions in Minions.Where(minion => minion.Health < Player.GetAutoAttackDamage(minion) || minion.Health < Q.GetDamage(minion)))
             {
                 countMinions++;
             }
             if (countMinions >= 2 && useQ && Q.IsReady() && Minions != null)
                 Q.Cast(Player.Position.Extend(Game.CursorPos, Q.Range/2));
             if (useQ && Q.IsReady() && Orbwalking.InAutoAttackRange(mob) && mob != null)
             {
                 Q.Cast(Game.CursorPos);
             }
         }*/
        public static void OnPlay(AIBaseClient sender, AIBaseClientPlayAnimationEventArgs args)
        {
            if (sender.IsMe)
            {
                switch (args.Animation)
                {
                    case "Spell1a":
                        Game.SendEmote(EmoteId.Dance);                            
                        break;
                }
            }
        }

        private static void Orbwalking_AfterAttack(object unit, AfterAttackEventArgs args)
        {
            //if (!args.Target.IsMe) return;

            if (Orbwalker.ActiveMode == OrbwalkerMode.LaneClear
                && 100 * (Player.Mana / Player.MaxMana) > qmenu["Junglemana"].GetValue<MenuSlider>().Value)
            {
                var mob =
                    MinionManager.GetMinions(
                        Player.ServerPosition,
                        E.Range,
                        MinionManager.MinionTypes.All,
                        MinionManager.MinionTeam.Neutral,
                        MinionManager.MinionOrderTypes.MaxHealth).FirstOrDefault();
                var Minions = MinionManager.GetMinions(
                    Player.Position.Extend(Game.CursorPos, Q.Range),
                    Player.AttackRange,
                    MinionManager.MinionTypes.All);
                var useQ = qmenu["UseQJ"].GetValue<MenuBool>().Enabled;
                int countMinions = 0;
                foreach (var minions in
                    Minions.Where(
                        minion =>
                        minion.Health < Player.GetAutoAttackDamage(minion)
                        || minion.Health < Q.GetDamage(minion) + Player.GetAutoAttackDamage(minion) || minion.Health < Q.GetDamage(minion)))
                {
                    countMinions++;
                }

                if (countMinions >= 2 && useQ && Q.IsReady() && Minions != null) Q.Cast(Player.Position.Extend(Game.CursorPos, Q.Range / 2));

                if (useQ && Q.IsReady() && Player.InAutoAttackRange(mob) && mob != null)
                {
                    Q.Cast(Game.CursorPos);
                    Game.SendEmote(EmoteId.Dance);

                }
            }


            if (!(args.Target is AIHeroClient)) return;

            tar = (AIHeroClient)args.Target;

            if (menu["aaqaa"].GetValue<MenuKeyBind>().Active)
            {
                if (Q.IsReady())
                {

                    Q.Cast(Game.CursorPos);
                    Game.SendEmote(EmoteId.Dance);

                }


                Orbwalker.Orbwalk(TargetSelector.GetTarget(625, DamageType.Physical), Game.CursorPos);
            }

           // Condemn.FlashE();
           


            if (Orbwalker.ActiveMode == OrbwalkerMode.Combo)
            {

                if (Itemsmenu["Ghostblade"].GetValue<MenuBool>().Enabled && tar.IsValidTarget(800))
                {
                    if (Items.CanUseItem(Player,3142))
                    {
                        Items.UseItem(Player,3142);
                    }
                }
            }

            if (emenu["UseEaa"].GetValue<MenuKeyBind>().Active)
            {
                E.Cast((AIBaseClient)args.Target);
            }

            if (Q.IsReady()
                && ((Orbwalker.ActiveMode == OrbwalkerMode.Combo && qmenu["UseQC"].GetValue<MenuBool>().Enabled)
                    || (Orbwalker.ActiveMode == OrbwalkerMode.Harass && qmenu["hq"].GetValue<MenuBool>().Enabled)))
            {
                if (qmenu["restrictq"].GetValue<MenuBool>().Enabled)
                {
                    var after = ObjectManager.Player.Position
                                + Normalize(Game.CursorPos - ObjectManager.Player.Position) * 300;
                    //Chat.Print("After: {0}", after);
                    var disafter = Vector3.DistanceSquared(after, tar.Position);
                    //Chat.Print("DisAfter: {0}", disafter);
                    //Chat.Print("first calc: {0}", (disafter) - (630*630));
                    if ((disafter < 630 * 630) && disafter > 150 * 150)
                    {
                        Q.Cast(Game.CursorPos);
                        Game.SendEmote(EmoteId.Dance);

                    }

                    if (Vector3.DistanceSquared(tar.Position, ObjectManager.Player.Position) > 630 * 630
                        && disafter < 630 * 630)
                    {
                        Q.Cast(Game.CursorPos);
                        Game.SendEmote(EmoteId.Dance);

                    }
                }
                else
                {
                    Q.Cast(Game.CursorPos);
                    Game.SendEmote(EmoteId.Dance);


                }
                //Q.Cast(Game.CursorPos);
            }
        }
        private static void Orbwalking_BeforeAttack(object sender, BeforeAttackEventArgs args)
        {
            if (args.Target.IsMe)
            {
                var buff = ObjectManager.Player.GetBuff("vaynetumblefade");
                if (buff != null)
                    if (buff.IsValid)
                        if (rmenu["visibleR"].GetValue<MenuBool>().Enabled && Player.CountEnemyHeroesInRange(800) >= 1)
                        {
                            if (buff.EndTime - Game.Time >
                            buff.EndTime - buff.StartTime -
                            // ReSharper disable once PossibleLossOfFraction
                            (rmenu["Qtime"].GetValue<MenuSlider>().Value / 1000))
                                if (!ObjectManager.Player.Position.UnderTurret(true))
                                    Orbwalker.AttackEnabled = false;
                        }
                        else
                        {
                            Orbwalker.AttackEnabled = true;
                        }
            }
        }

        public static Vector3 Normalize(Vector3 A)
        {
            double distance = Math.Sqrt(A.X * A.X + A.Y * A.Y);
            return new Vector3(new Vector2((float)(A.X / distance)), (float)(A.Y / distance));
        }


        public static void Game_OnGameUpdate(EventArgs args)
        {
            if (menu["useR"].GetValue<MenuBool>().Enabled && R.IsReady()
                                                  && ObjectManager.Player.CountEnemyHeroesInRange(1000) >= menu["enemys"].GetValue<MenuSlider>().Value
                                                  && Orbwalker.ActiveMode == OrbwalkerMode.Combo)
            {
                R.Cast();
            }

            Usepotion(); 

            if (menu["aaqaa"].GetValue<MenuKeyBind>().Active)
            {
               Orbwalker.Orbwalk(TargetSelector.GetTarget(625, DamageType.Physical), Game.CursorPos);
              
            }

            if (Itemsmenu["QSS"].GetValue<MenuBool>().Enabled)
            {
                for (int i = 0; i < buffs.Length; i++)
                {
                    if (ObjectManager.Player.HasBuffOfType(buffs[i]) && qss[buffs[i].ToString()].GetValue<MenuBool>().Enabled)
                    {
                        if (Items.CanUseItem(Player,3140))
                        {
                            Items.UseItem(Player,3140);
                        }
                        else if (Items.CanUseItem(Player,3139))
                        {
                            Items.UseItem(Player,3140);
                        }
                        else if (cleanse != null && cleanse.IsReady())
                        {
                            cleanse.Cast();
                        }
                    }
                }
            }

            //||
            //(orbwalker.ActiveMode.ToString() != "Combo" || !menu.Item("UseEC").GetValue<bool>()) &&
            //!menu.Item("UseET").GetValue<KeyBind>().Active)) return;
            if ((Orbwalker.ActiveMode == OrbwalkerMode.Combo && emenu["UseEC"].GetValue<MenuBool>().Enabled) || (Orbwalker.ActiveMode == OrbwalkerMode.Harass && emenu["he"].GetValue<MenuBool>().Enabled) || emenu["UseET"].GetValue<MenuKeyBind>().Active)
            {
                switch (emenu["EMode"].GetValue<MenuList>().Index)
                {
                    case 0:
                        {
                            Condemn.Run();
                        }
                        break;
                    case 1:
                        {
                            foreach (var hero in from hero in ObjectManager.Get<AIHeroClient>().Where(hero => hero.IsValidTarget(550f))
                                                 let prediction = E.GetPrediction(hero)
                                                 where NavMesh.GetCollisionFlags(
                                                     prediction.UnitPosition.To2D()
                                                         .Extend(ObjectManager.Player.ServerPosition.To2D(),
                                                             -emenu["PushDistance"].GetValue<MenuSlider>().Value).ToVector3())
                                                     .HasFlag(CollisionFlags.Wall) || NavMesh.GetCollisionFlags(
                                                         prediction.UnitPosition.To2D()
                                                             .Extend(ObjectManager.Player.ServerPosition.To2D(),
                                                                 -(emenu["PushDistance"].GetValue<MenuSlider>().Value / 2))
                                                             .ToVector3())
                                                         .HasFlag(CollisionFlags.Wall)
                                                 select hero)
                            {
                                E.CastOnUnit(hero);
                            }
                        }
                        break;
                    case 2:
                        {
                            var target = TargetSelector.GetTarget(E.Range, DamageType.Physical);
                            if (target != null)
                            {
                                var EPred = E.GetPrediction(target);
                                var PD = emenu["PushDistance"].GetValue<MenuSlider>().Value;
                                var PP = EPred.UnitPosition.Extend(ObjectManager.Player.Position, -PD);

                                for (int i = 1; i < PD; i += (int)target.BoundingRadius)
                                {
                                    var VL = EPred.UnitPosition.Extend(ObjectManager.Player.Position, -i);
                                    var J4 = ObjectManager.Get<AIBaseClient>()
                                        .Any(f => f.Distance(PP) <= target.BoundingRadius &&
                                                  f.Name.ToLower() == "beacon");
                                    var CF = NavMesh.GetCollisionFlags(VL);

                                    if (CF.HasFlag(CollisionFlags.Wall) || CF.HasFlag(CollisionFlags.Building) || J4)
                                    {
                                        if (CheckTarget(target, E.Range))
                                        {
                                            E.CastOnUnit(target);
                                            return;
                                        }
                                    }
                                }
                            }
                        }
                        break;
                    case 3:
                        {
                            var target = TargetSelector.GetTarget(E.Range, DamageType.Physical);
                            if (target != null)
                            {
                                var pushDistance = emenu["PushDistance"].GetValue<MenuSlider>().Value;
                                var Prediction = E.GetPrediction(target);
                                var endPosition = Prediction.UnitPosition.Extend
                                    (ObjectManager.Player.ServerPosition, -pushDistance);

                                if (Prediction.Hitchance >= HitChance.VeryHigh)
                                {
                                    if (Vector3Extensions.IsWall(endPosition))
                                    {
                                        var condemnRectangle = new Geometry.Polygon.Rectangle(
                                            target.ServerPosition.To2D(),
                                            endPosition.To2D(), target.BoundingRadius);

                                        if (
                                            condemnRectangle.Points.Count(
                                                point =>
                                                    NavMesh.GetCollisionFlags(point.X, point.Y)
                                                        .HasFlag(CollisionFlags.Wall)) >=
                                            condemnRectangle.Points.Count * (20 / 100f))
                                        {
                                            if (CheckTarget(target, E.Range))
                                            {
                                                E.CastOnUnit(target);
                                            }
                                        }
                                    }
                                    else
                                    {
                                        var step = pushDistance / 5f;
                                        for (float i = 0; i < pushDistance; i += step)
                                        {
                                            var endPositionEx =
                                                Prediction.UnitPosition.Extend(ObjectManager.Player.ServerPosition, -i);
                                            if (Vector3Extensions.IsWall(endPositionEx))
                                            {
                                                var condemnRectangle =
                                                    new Geometry.Polygon.Rectangle(target.ServerPosition.To2D(),
                                                        endPosition.To2D(), target.BoundingRadius);

                                                if (
                                                    condemnRectangle.Points.Count(
                                                        point =>
                                                            NavMesh.GetCollisionFlags(point.X, point.Y)
                                                                .HasFlag(CollisionFlags.Wall)) >=
                                                    condemnRectangle.Points.Count * (20 / 100f))
                                                {
                                                    if (CheckTarget(target, E.Range))
                                                    {
                                                        E.CastOnUnit(target);
                                                    }
                                                }

                                                return;
                                            }
                                        }
                                    }
                                }
                            }
                        }
                        break;

                    case 4:
                        {
                            var target = TargetSelector.GetTarget(E.Range, DamageType.Physical);
                            if (target != null)
                            {
                                for (var i = 1; i < 8; i++)
                                {
                                    var targetBehind = target.Position +
                                                       Vector3.Normalize(target.ServerPosition -
                                                                         ObjectManager.Player.Position) * i * 50;

                                    if (Vector3Extensions.IsWall(targetBehind) && target.IsValidTarget(E.Range))
                                    {
                                        if (CheckTarget(target, E.Range))
                                        {
                                            E.CastOnUnit(target);
                                            return;
                                        }
                                    }
                                }
                            }
                        }
                        break;
                    case 5:
                        {
                            var target = TargetSelector.GetTarget(E.Range, DamageType.Physical);
                            if (target != null)
                            {
                                var prediction = E.GetPrediction(target);

                                if (prediction.Hitchance >= HitChance.High)
                                {
                                    var finalPosition =
                                        prediction.UnitPosition.Extend(ObjectManager.Player.Position, -400);

                                    if (Vector3Extensions.IsWall(finalPosition))
                                    {
                                        E.CastOnUnit(target);
                                        return;
                                    }

                                    for (var i = 1; i < 400; i += 50)
                                    {
                                        var loc3 = prediction.UnitPosition.Extend(ObjectManager.Player.Position, -i);

                                        if (Vector3Extensions.IsWall(loc3))
                                        {
                                            if (CheckTarget(target, E.Range))
                                            {
                                                E.CastOnUnit(target);
                                                return;
                                            }
                                        }
                                    }
                                }
                            }
                        }
                        break;
                    case 6:
                        {
                            var target = TargetSelector.GetTarget(E.Range, DamageType.Physical);
                            if (target != null)
                            {
                                var prepos = E.GetPrediction(target);
                                float pushDistance = 470;
                                var radius = 250;
                                var start2 = target.ServerPosition;
                                var end2 = prepos.CastPosition.Extend(ObjectManager.Player.ServerPosition,
                                    -pushDistance);
                                var start = start2.To2D();
                                var end = end2.To2D();
                                var dir = (end - start).Normalized();
                                var pDir = dir.Perpendicular();
                                var rightEndPos = end + pDir * radius;
                                var leftEndPos = end - pDir * radius;
                                var rEndPos = new Vector3(rightEndPos.X, rightEndPos.Y,
                                    ObjectManager.Player.Position.Z);
                                var lEndPos = new Vector3(leftEndPos.X, leftEndPos.Y, ObjectManager.Player.Position.Z);
                                var step = start2.Distance(rEndPos) / 10;

                                for (var i = 0; i < 10; i++)
                                {
                                    var pr = start2.Extend(rEndPos, step * i);
                                    var pl = start2.Extend(lEndPos, step * i);

                                    if (Vector3Extensions.IsWall(pr) && Vector3Extensions.IsWall(pl))
                                    {
                                        if (CheckTarget(target, E.Range))
                                        {
                                            E.CastOnUnit(target);
                                            return;
                                        }
                                    }
                                }
                            }
                        }
                        break;
                    case 7:
                        foreach (var target in HeroManager.Enemies.Where(h => h.IsValidTarget(E.Range)))
                        {

                            var pushDistance = emenu["PushDistance"].GetValue<MenuSlider>().Value;
                            var targetPosition = E.GetPrediction(target).UnitPosition;
                            var pushDirection = (targetPosition - ObjectManager.Player.ServerPosition).Normalized();
                            float checkDistance = pushDistance / 40f;
                            for (int i = 0; i < 40; i++)
                            {
                                Vector3 finalPosition = targetPosition + (pushDirection * checkDistance * i);
                                var collFlags = NavMesh.GetCollisionFlags(finalPosition);
                                if (collFlags.HasFlag(CollisionFlags.Wall) || collFlags.HasFlag(CollisionFlags.Building)) //not sure about building, I think its turrets, nexus etc
                                {
                                    E.CastOnUnit(target);
                                }
                            }


                        }
                        break;
                    case 8:
                        {
                            var target = TargetSelector.GetTarget(E.Range, DamageType.Physical);
                            if (target != null)
                            {
                                var pP = Player.ServerPosition;
                                var p = target.ServerPosition;
                                var pD = Vayne.emenu["PushDistance"].GetValue<MenuSlider>().Value;
                                // var mode = Vayne.emenu.Item("EMode", true).GetValue<StringList>().SelectedIndex;
                                if ((p.Extend(pP, -pD).IsCollisionable() || p.Extend(pP, -pD / 2f).IsCollisionable() ||
                                     p.Extend(pP, -pD / 3f).IsCollisionable()))
                                {
                                    if (!target.CanMove ||
                                        (target.Spellbook.IsAutoAttack))
                                    {
                                        E.CastOnUnit(target);
                                    }

                                    var enemiesCount = ObjectManager.Player.CountEnemyHeroesInRange(1200);
                                    if (enemiesCount > 1 && enemiesCount <= 3)
                                    {
                                        var prediction = Vayne.E.GetPrediction(target);
                                        for (var i = 15; i < pD; i += 75)
                                        {
                                            var posFlags = NavMesh.GetCollisionFlags(
                                                prediction.UnitPosition.To2D()
                                                    .Extend(
                                                        pP.To2D(),
                                                        -i)
                                                    .ToVector3());
                                            if (posFlags.HasFlag(CollisionFlags.Wall) ||
                                                posFlags.HasFlag(CollisionFlags.Building))
                                            {
                                                E.CastOnUnit(target);
                                            }

                                            else
                                            {
                                                var hitchance = emenu["EHitchance"].GetValue<MenuSlider>().Value;
                                                var angle = 0.20 * hitchance;
                                                const float travelDistance = 0.5f;
                                                var alpha = new Vector2(
                                                    (float)(p.X + travelDistance * Math.Cos(Math.PI / 180 * angle)),
                                                    (float)(p.X + travelDistance * Math.Sin(Math.PI / 180 * angle)));
                                                var beta = new Vector2(
                                                    (float)(p.X - travelDistance * Math.Cos(Math.PI / 180 * angle)),
                                                    (float)(p.X - travelDistance * Math.Sin(Math.PI / 180 * angle)));

                                                for (var j = 15; j < pD; j += 100)
                                                {
                                                    if (pP.To2D().Extend(alpha,
                                                                j)
                                                            .ToVector3().IsCollisionable() && pP.To2D().Extend(beta, j)
                                                            .ToVector3().IsCollisionable())
                                                    {
                                                        E.CastOnUnit(target);
                                                    }
                                                }

                                            }
                                        }
                                    }




                                }
                            }

                            break;
                        }
                    case 9:
                        {
                            var target = TargetSelector.GetTarget(E.Range, DamageType.Physical);
                            if (target != null)
                            {
                                var pP = Player.ServerPosition;
                                var p = target.ServerPosition;
                                var pD = Vayne.emenu["PushDistance"].GetValue<MenuSlider>().Value;
                                //  var mode = Vayne.emenu.Item("EMode", true).GetValue<StringList>().SelectedIndex;
                                if (
                                    (p.Extend(pP, -pD).IsCollisionable() || p.Extend(pP, -pD / 2f).IsCollisionable() ||
                                     p.Extend(pP, -pD / 3f).IsCollisionable()))
                                {
                                    if (!target.CanMove ||
                                        (target.Spellbook.IsAutoAttack))
                                    {
                                        E.CastOnUnit(target);
                                    }

                                    var hitchance = emenu["EHitchance"].GetValue<MenuSlider>().Value;
                                    var angle = 0.20 * hitchance;
                                    const float travelDistance = 0.5f;
                                    var alpha = new Vector2(
                                        (float)(p.X + travelDistance * Math.Cos(Math.PI / 180 * angle)),
                                        (float)(p.X + travelDistance * Math.Sin(Math.PI / 180 * angle)));
                                    var beta = new Vector2(
                                        (float)(p.X - travelDistance * Math.Cos(Math.PI / 180 * angle)),
                                        (float)(p.X - travelDistance * Math.Sin(Math.PI / 180 * angle)));

                                    for (var i = 15; i < pD; i += 100)
                                    {
                                        if (pP.To2D().Extend(alpha,
                                                    i)
                                                .ToVector3().IsCollisionable() && pP.To2D().Extend(beta, i).ToVector3()
                                                .IsCollisionable())
                                        {
                                            E.CastOnUnit(target);
                                        }
                                    }

                                }
                            }
                        }
                        break;
                    case 10:
                        {
                            var target = TargetSelector.GetTarget(E.Range, DamageType.Physical);
                            if (target != null)
                            {
                                var pP = Player.ServerPosition;
                                var p = target.ServerPosition;
                                var pD = Vayne.emenu["PushDistance"].GetValue<MenuSlider>().Value;
                                // var mode = Vayne.emenu.Item("EMode", true).GetValue<StringList>().SelectedIndex;
                                if (!target.CanMove ||
                                    (target.Spellbook.IsAutoAttack))
                                {
                                    E.CastOnUnit(target);
                                }

                                var hitchance = emenu["EHitchance"].GetValue<MenuSlider>().Value;
                                var angle = 0.20 * hitchance;
                                const float travelDistance = 0.5f;
                                var alpha = new Vector2((float)(p.X + travelDistance * Math.Cos(Math.PI / 180 * angle)),
                                    (float)(p.X + travelDistance * Math.Sin(Math.PI / 180 * angle)));
                                var beta = new Vector2((float)(p.X - travelDistance * Math.Cos(Math.PI / 180 * angle)),
                                    (float)(p.X - travelDistance * Math.Sin(Math.PI / 180 * angle)));

                                for (var i = 15; i < pD; i += 100)
                                {
                                    if (pP.To2D().Extend(alpha,
                                                i)
                                            .ToVector3().IsCollisionable() ||
                                        pP.To2D().Extend(beta, i).ToVector3().IsCollisionable())
                                    {
                                        E.CastOnUnit(target);
                                    }
                                }
                            }
                        }
                        break;
                    case 11:
                        {
                            var target = TargetSelector.GetTarget(E.Range, DamageType.Physical);
                            if (target != null)
                            {
                                var pP = Player.ServerPosition;
                                var p = target.ServerPosition;
                                var pD = Vayne.emenu["PushDistance"].GetValue<MenuSlider>().Value;
                                // var mode = Vayne.emenu.Item("EMode", true).GetValue<StringList>().SelectedIndex;
                                var prediction = Vayne.E.GetPrediction(target);
                                for (var i = 15; i < pD; i += 75)
                                {
                                    var posCF = NavMesh.GetCollisionFlags(
                                        prediction.UnitPosition.To2D()
                                            .Extend(
                                                pP.To2D(),
                                                -i)
                                            .ToVector3());
                                    if (posCF.HasFlag(CollisionFlags.Wall) || posCF.HasFlag(CollisionFlags.Building))
                                    {

                                        E.CastOnUnit(target);

                                    }
                                }
                            }
                        }
                        break;
                }
            }
        }
    }
}