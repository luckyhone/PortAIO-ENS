using System;
using System.Collections.Generic;
using System.Linq;
using EnsoulSharp;
using EnsoulSharp.SDK;
using EnsoulSharp.SDK.MenuUI;
using EnsoulSharp.SDK.Utility;
using PortAIO;
using SharpDX;
using SPrediction;

namespace hi_im_gosu
{
    public class Vayne
    {
        public static Spell E;
        public static Spell Q;
        public static Spell R;

        private static Menu menu;

        private static Dictionary<string, SpellSlot> spellData;
        
        private static AIHeroClient tar;
        public const string ChampName = "Vayne";
        public static AIHeroClient Player;
        private static BuffType[] buffs;
        private static Spell cleanse;
        private static Menu Itemsmenu;
        private static Menu qmenu;
        private static Menu emenu;
        private static Menu botrk;
        private static Menu qss;

        /* Asuna VayneHunter Copypasta */
        private static readonly Vector2 MidPos = new Vector2(6707.485f, 8802.744f);

        private static readonly Vector2 DragPos = new Vector2(11514, 4462);

        private static float LastMoveC;
        
        private static void TumbleHandler()
        {
            if (Player.Distance(MidPos) >= Player.Distance(DragPos))
            {
                if (Player.Position.X < 12000 || Player.Position.X > 12070 || Player.Position.Y < 4800 ||
                    Player.Position.Y > 4872)
                {
                    MoveToLimited(new Vector2(12050, 4827).To3D());
                }
                else
                {
                    MoveToLimited(new Vector2(12050, 4827).To3D());
                    Q.Cast(DragPos);
                }
            }
            else
            {
                if (Player.Position.X < 6908 || Player.Position.X > 6978 || Player.Position.Y < 8917 ||
                    Player.Position.Y > 8989)
                {
                    MoveToLimited(new Vector2(6958, 8944).To3D());
                }
                else
                {
                    MoveToLimited(new Vector2(6958, 8944).To3D());
                    Q.Cast(MidPos);
                }
            }
        }

        private static void MoveToLimited(Vector3 where)
        {
            if (Environment.TickCount - LastMoveC < 80)
            {
                return;
            }

            LastMoveC = Environment.TickCount;
            ObjectManager.Player.IssueOrder(GameObjectOrder.MoveTo, where);
        }

        public static void Game_OnGameLoad()
        {
            Player = ObjectManager.Player;
            //Utils.PrintMessage("Vayne loaded");
            if (Player.CharacterName != ChampName) return;
            spellData = new Dictionary<string, SpellSlot>();
            menu = new Menu("Gosu", "Gosu", true);
            
            menu.Add(
                new MenuKeyBind("aaqaa", "Auto -> Q -> AA",Keys.X, KeyBindType.Press));
            
            qmenu = menu.Add(new Menu("Tumble", "Tumble"));
            qmenu.Add(new MenuBool("UseQC", "Use Q Combo").SetValue(true));
            qmenu.Add(new MenuBool("hq", "Use Q Harass").SetValue(true));
            qmenu.Add(new MenuBool("restrictq", "Restrict Q usage?").SetValue(true));
            qmenu.Add(new MenuBool("UseQJ", "Use Q Farm").SetValue(true));
            qmenu.Add(new MenuSlider("Junglemana", "Minimum Mana to Use Q Farm",60, 1, 100));

            emenu = menu.Add(new Menu("Condemn", "Condemn"));
            emenu.Add(new MenuBool("UseEC", "Use E Combo").SetValue(true));
            emenu.Add(new MenuBool("he", "Use E Harass").SetValue(true));
            emenu.Add(
                new MenuKeyBind("UseET", "Use E (Toggle)",Keys.T, KeyBindType.Toggle));

            emenu.Add(new MenuBool("Int_E", "Use E To Interrupt").SetValue(true));
            emenu.Add(new MenuBool("Gap_E", "Use E To Gabcloser").SetValue(true));
            emenu.Add(new MenuSlider("PushDistance", "E Push Distance",425, 300, 475));
            emenu.Add(
                new MenuKeyBind("UseEaa", "Use E after auto",Keys.G, KeyBindType.Toggle));
            
            menu.Add(new MenuKeyBind("walltumble", "Wall Tumble",Keys.U, KeyBindType.Press));
            menu.Add(new MenuBool("useR", "Use R Combo").SetValue(true));
            menu.Add(new MenuSlider("enemys", "If Enemys Around >=",2, 1, 5));
            
            Itemsmenu = menu.Add(new Menu("Items", "Items"));
            var Potions = Itemsmenu.Add(new Menu("Potions", "Potions"));
            Potions.Add(new MenuBool("usehppotions", "Use Healt potion/Refillable/Hunters/Corrupting/Biscuit"))
                .SetValue(true);
            Potions.Add(new MenuSlider("usepotionhp", "If Health % <",35, 1, 100));
            Potions.Add(new MenuBool("usemppotions", "Use Hunters/Corrupting/Biscuit"))
                .SetValue(true);
            Potions.Add(new MenuSlider("usepotionmp", "If Mana % <",35, 1, 100));
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
            E = new Spell(SpellSlot.E, float.MaxValue);
            
            var cde = ObjectManager.Player.Spellbook.GetSpell(ObjectManager.Player.GetSpellSlot("SummonerBoost"));
            if (cde != null)
            {
                if (cde.Slot != SpellSlot.Unknown) //trees
                {
                    cleanse = new Spell(cde.Slot);
                }
            }
            
            E.SetTargetted(0.25f, 2200f);
            AIBaseClient.OnProcessSpellCast += Game_ProcessSpell;
            Game.OnUpdate += Game_OnGameUpdate;
            Orbwalker.OnAfterAttack += Orbwalking_AfterAttack;
            AntiGapcloser.OnGapcloser += AntiGapcloser_OnEnemyGapcloser;
            Interrupter.OnInterrupterSpell += Interrupter2_OnInterruptableTarget;

            Game.Print("<font color='#881df2'>Blm95 Vayne reworked by Diabaths</font> Loaded.");
            Game.Print(
                "<font color='#f2f21d'>Do you like it???  </font> <font color='#ff1900'>Drop 1 Upvote in Database </font>");
            Game.Print(
                "<font color='#f2f21d'>Buy me cigars </font> <font color='#ff1900'>ssssssssssmith@hotmail.com</font> (10) S");
            menu.Attach();
        }

        private static void Interrupter2_OnInterruptableTarget(AIHeroClient unit, Interrupter.InterruptSpellArgs args)
        {
            if (E.IsReady() && unit.IsValidTarget(550) && emenu["Condemn"]["Int_E"].GetValue<MenuBool>().Enabled)
            {
                E.Cast(unit);
            }
        }

        private static void AntiGapcloser_OnEnemyGapcloser(AIHeroClient sender, AntiGapcloser.GapcloserArgs args)
        {
            if (E.IsReady() && sender.IsValidTarget(200) && emenu["Condemn"]["Gap_E"].GetValue<MenuBool>().Enabled)
            {
                E.Cast(sender);
            }
        }

        private static void Game_ProcessSpell(AIBaseClient hero, AIBaseClientProcessSpellCastEventArgs args)
        {
            if (args.SData.Name.ToLower() == "zedult" && args.Target.IsMe)
            {
                if (Items.CanUseItem(ObjectManager.Player,(int) ItemId.Quicksilver_Sash))
                {
                    DelayAction.Add(1000, () => Items.UseItem(ObjectManager.Player,(int) ItemId.Quicksilver_Sash));
                }
                else if (Items.CanUseItem(ObjectManager.Player,(int) ItemId.Mercurial_Scimitar))
                {
                    DelayAction.Add(1000, () => Items.UseItem(ObjectManager.Player,(int) ItemId.Mercurial_Scimitar));
                }
                else if (cleanse != null && cleanse.IsReady())
                {
                    DelayAction.Add(1000, () => cleanse.Cast());
                }
            }
        }

        private static void Orbwalking_AfterAttack(object unit, AfterAttackEventArgs target)
        {

            if (Orbwalker.ActiveMode.ToString() == "LaneClear"
                && 100 * (Player.Mana / Player.MaxMana) > qmenu["Tumble"]["Junglemana"].GetValue<MenuSlider>().Value)
            {
                var mob =
                    MinionManager.GetMinions(
                        Player.Position,
                        E.Range,
                        MinionManager.MinionTypes.All,
                        MinionManager.MinionTeam.Neutral,
                        MinionManager.MinionOrderTypes.MaxHealth).FirstOrDefault();
                var Minions = MinionManager.GetMinions(
                    Player.Position.Extend(Game.CursorPos, Q.Range),
                    Player.AttackRange,
                    MinionManager.MinionTypes.All);
                var useQ = qmenu["Tumble"]["UseQJ"].GetValue<MenuBool>().Enabled;
                int countMinions = 0;
                foreach (var minions in
                         Minions.Where(
                             minion =>
                                 minion.Health < Player.GetAutoAttackDamage(minion)
                                 || minion.Health < Q.GetDamage(minion) + Player.GetAutoAttackDamage(minion) || minion.Health < Q.GetDamage(minion)))
                {
                    countMinions++;
                }

                if (countMinions >= 2 && useQ && Q.IsReady()) Q.Cast(Player.Position.Extend(Game.CursorPos, Q.Range / 2));

                if (useQ && Q.IsReady() && ObjectManager.Player.InAutoAttackRange(mob) && mob != null)
                {
                    Q.Cast(Game.CursorPos);
                }
            }
            if (!(target.Target is AIHeroClient)) return;

            tar = (AIHeroClient) target.Target;

            if (menu["aaqaa"].GetValue<MenuKeyBind>().Active)
            {
                if (Q.IsReady())
                {
                    Q.Cast(Game.CursorPos);
                }

                Orbwalker.Orbwalk(TargetSelector.GetTarget(625, DamageType.Physical), Game.CursorPos);
            }
            if (emenu["Condemn"]["UseEaa"].GetValue<MenuKeyBind>().Active)
            {
                E.Cast((AIBaseClient)target.Target);
                //emenu["Condemn"]["UseEaa"].GetValue<MenuKeyBind>() = <MenuKeyBind>(new KeyBind("G".ToCharArray()[0], KeyBindType.Toggle));
            }

            if (Q.IsReady()
                && ((Orbwalker.ActiveMode.ToString() == "Combo" && qmenu["Tumble"]["UseQC"].GetValue<MenuBool>().Enabled)
                    || (Orbwalker.ActiveMode.ToString() == "Harass" && qmenu["Tumble"]["hq"].GetValue<MenuBool>().Enabled)))
            {
                if (qmenu["Tumble"]["restrictq"].GetValue<MenuBool>().Enabled)
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
                    }

                    if (Vector3.DistanceSquared(tar.Position, ObjectManager.Player.Position) > 630 * 630
                        && disafter < 630 * 630)
                    {
                        Q.Cast(Game.CursorPos);
                    }
                }
                else
                {
                    Q.Cast(Game.CursorPos);
                }
                //Q.Cast(Game.CursorPos);
            }
        }
        
        public static Vector3 Normalize(Vector3 A)
        {
            double distance = Math.Sqrt(A.X * A.X + A.Y * A.Y);
            return new Vector3(new Vector2((float)(A.X / distance)), (float)(A.Y / distance));
        }

        private static void Usepotion()
        {
            var iusehppotion = menu["Items"]["Potions"]["usehppotions"].GetValue<MenuBool>().Enabled;
            var iusepotionhp = Player.Health
                               <= (Player.MaxHealth * (menu["Items"]["Potions"]["usepotionhp"].GetValue<MenuSlider>().Value) / 100);
            var iusemppotion = menu["Items"]["Potions"]["usemppotions"].GetValue<MenuBool>().Enabled;
            var iusepotionmp = Player.Mana
                               <= (Player.MaxMana * (menu["Items"]["Potions"]["usepotionmp"].GetValue<MenuSlider>().Value) / 100);
            if (Player.InFountain() || ObjectManager.Player.HasBuff("Recall")) return;

            if (ObjectManager.Player.CountEnemyHeroesInRange(800) > 0)
            {
                if (iusepotionhp && iusehppotion
                    && !(ObjectManager.Player.HasBuff("RegenerationPotion")
                         || ObjectManager.Player.HasBuff("ItemMiniRegenPotion")
                         || ObjectManager.Player.HasBuff("ItemCrystalFlask")
                         || ObjectManager.Player.HasBuff("ItemCrystalFlaskJungle")
                         || ObjectManager.Player.HasBuff("ItemDarkCrystalFlask")))
                {
                    if (Items.HasItem(ObjectManager.Player,(int)ItemId.Health_Potion) && Items.CanUseItem(ObjectManager.Player,(int)ItemId.Health_Potion))
                    {
                        Items.UseItem(ObjectManager.Player,(int)ItemId.Health_Potion);
                    }

                    if (Items.HasItem(ObjectManager.Player,(int) ItemId.Refillable_Potion) && Items.CanUseItem(ObjectManager.Player,(int) ItemId.Refillable_Potion))
                    {
                        Items.UseItem(ObjectManager.Player,(int) ItemId.Refillable_Potion);
                    }

                    if (Items.HasItem(ObjectManager.Player,(int) ItemId.Corrupting_Potion) && Items.CanUseItem(ObjectManager.Player,(int) ItemId.Corrupting_Potion))
                    {
                        Items.UseItem(ObjectManager.Player,(int) ItemId.Corrupting_Potion);
                    }
                }

                if (iusepotionmp && iusemppotion
                    && !(ObjectManager.Player.HasBuff("ItemDarkCrystalFlask")
                         || ObjectManager.Player.HasBuff("ItemMiniRegenPotion")
                         || ObjectManager.Player.HasBuff("ItemCrystalFlaskJungle")
                         || ObjectManager.Player.HasBuff("ItemCrystalFlask")))
                {
                    
                    if (Items.HasItem(ObjectManager.Player,(int) ItemId.Refillable_Potion) && Items.CanUseItem(ObjectManager.Player,(int) ItemId.Refillable_Potion))
                    {
                        Items.UseItem(ObjectManager.Player,(int) ItemId.Refillable_Potion);
                    }

                    if (Items.HasItem(ObjectManager.Player,(int) ItemId.Refillable_Potion) && Items.CanUseItem(ObjectManager.Player,(int) ItemId.Refillable_Potion))
                    {
                        Items.UseItem(ObjectManager.Player,(int) ItemId.Refillable_Potion);
                    }
                }
            }
        }

        private static void Game_OnGameUpdate(EventArgs args)
        {
            if (menu["useR"].GetValue<MenuBool>().Enabled && R.IsReady()
                                                   && ObjectManager.Player.CountEnemyHeroesInRange(1000) >= menu["enemys"].GetValue<MenuSlider>().Value
                                                   && Orbwalker.ActiveMode.ToString() == "Combo")
            {
                R.Cast();
            }
            
            Usepotion();
            
            if (menu["walltumble"].GetValue<MenuKeyBind>().Active)
            {
                TumbleHandler();
            }

            
            if (menu["aaqaa"].GetValue<MenuKeyBind>().Active)
            {
                Orbwalker.Orbwalk(TargetSelector.GetTarget(625, DamageType.Physical), Game.CursorPos);
            }

            
            if (menu["Items"]["QSS"].GetValue<MenuBool>().Enabled)
            {
                for (int i = 0; i < buffs.Length; i++)
                {
                    if (ObjectManager.Player.HasBuffOfType(buffs[i]) && qss[buffs[i].ToString()].GetValue<MenuBool>().Enabled)
                    {
                        if (Items.CanUseItem(ObjectManager.Player,(int) ItemId.Mercurial_Scimitar))
                        {
                            Items.UseItem(ObjectManager.Player,(int) ItemId.Mercurial_Scimitar);
                        }
                        else if (cleanse != null && cleanse.IsReady())
                        {
                            cleanse.Cast();
                        }
                    }
                }
            }
            if (!E.IsReady()) return; //||
            //(orbwalker.ActiveMode.ToString() != "Combo" || !menu.Item("UseEC").GetValue<bool>()) &&
            //!menu.Item("UseET").GetValue<KeyBind>().Active)) return;
            if ((Orbwalker.ActiveMode.ToString() == "Combo" &&
                 menu["Condemn"]["UseEC"].GetValue<MenuBool>().Enabled) ||
                (Orbwalker.ActiveMode.ToString() == "Harass" && menu["Condemn"]["he"].GetValue<MenuBool>().Enabled) ||
                menu["Condemn"]["UseET"].GetValue<MenuKeyBind>().Active)
            {
                foreach (var hero in from hero in ObjectManager.Get<AIHeroClient>()
                             .Where(hero => hero.IsValidTarget(550f))
                         let prediction = E.GetPrediction(hero)
                         where NavMesh.GetCollisionFlags(
                                 prediction.UnitPosition.To2D()
                                     .Extend(ObjectManager.Player.Position.To2D(),
                                         -menu["Condemn"]["PushDistance"].GetValue<MenuSlider>().Value)
                                     .To3D())
                             .HasFlag(CollisionFlags.Wall) || NavMesh.GetCollisionFlags(
                                 prediction.UnitPosition.To2D()
                                     .Extend(ObjectManager.Player.Position.To2D(),
                                         -(menu["Condemn"]["PushDistance"].GetValue<MenuSlider>().Value / 2))
                                     .To3D())
                             .HasFlag(CollisionFlags.Wall)
                         select hero)
                {
                    E.Cast(hero);
                }
            }
        }
    }
}