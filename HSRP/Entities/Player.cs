﻿using System;
using System.Collections.Generic;
using System.Globalization;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Xml.Linq;
using Discord;

namespace HSRP
{
    public class Player : IEntity
    {
        public ulong ID { get; set; }
        public Task<Discord.IGuildUser> GuildUser
        {
            get
            {
                return Program.Instance.RpGuild.GetUserAsync(ID);
            }
        }
        public string Name { get; set; }
        /// <summary>
        /// The then-username of the owner of this player.
        /// </summary>
        public string OwnerUsername;

        public BloodType BloodColor { get; set; }
        public bool LikesPineappleOnPizza { get; set; }

        public AbilitySet BaseAbilities { get; set; }

        public Dictionary<EventType, Event> Events { get; set; }
        public string[] Immunities { get; set; }
        public List<StatusEffect> InflictedAilments { get; set; }
        public List<Move> Moves { get; set; }

        public int Health { get; set; }
        public int MaxHealth { get; set; }
        public bool Dead { get; set; }

        public string Specibus { get; set; }
        public Item EquippedWeapon
        {
            get
            {
                return equippedWeapon < 0
                    ? null
                    : Inventory[equippedWeapon];
            }
        }
        private int equippedWeapon { get; set; }
        public int StrifeID { get; set; }

        /// <summary>
        /// Returns the total damage of the character's equipped items.
        /// </summary>
        //TODO: this
        public int DiceRolls
        {
            get
            {
                return 1;
            }
        }

        public int Echeladder{ get; private set; }
        public int PendingSkillPointAllocations { get; set; }
        public int XP { get; set; }
        public int NextLevelXP { get; set; }

        public List<Item> Inventory { get; set; }

        public bool Errored { get; set; }

        public Player()
        {
            BaseAbilities = new AbilitySet();

            Events = new Dictionary<EventType, Event>();
            Immunities = null;
            InflictedAilments = new List<StatusEffect>();
            Moves = new List<Move>();

            Inventory = new List<Item>();

            Name = "";
            Specibus = "";
        }

        public Player(IUser user) : this(user.Id.ToString()) { }
        public Player(ulong ID) : this(ID.ToString()) { }
        public Player(string filePath, bool idOnly = true) : this()
        {
            string path = idOnly
                ? Path.Combine(Dirs.Players, filePath) + ".xml"
                : filePath;

            XDocument doc = XmlToolbox.TryLoadXml(path);
            if (doc == null || doc.Root == null) { Errored = true; return; }
            
            foreach (XElement ele in doc.Root.Elements())
            {
                switch (ele.Name.LocalName)
                {
                    case "info":
                        Name = XmlToolbox.GetAttributeString(ele, "name", string.Empty);
                        ID = XmlToolbox.GetAttributeUnsignedLong(ele, "id", 0);
                        OwnerUsername = XmlToolbox.GetAttributeString(ele, "owner", string.Empty);
                        BloodColor = XmlToolbox.GetAttributeEnum(ele, "blood", BloodType.None);
                        LikesPineappleOnPizza = XmlToolbox.GetAttributeBool(ele, "pineappleOnPizza", false);
                        break;

                    case "status":
                        Health = XmlToolbox.GetAttributeInt(ele, "hp", -1);
                        MaxHealth = XmlToolbox.GetAttributeInt(ele, "maxhp", Health);
                        Dead = XmlToolbox.GetAttributeBool(ele, "dead", false);
                        Specibus = XmlToolbox.GetAttributeString(ele, "specibus", string.Empty);
                        Immunities = XmlToolbox.GetAttributeStringArray(ele, "immune", new string[0]);
                        break;

                    case "levels":
                        Echeladder = XmlToolbox.GetAttributeInt(ele, "echeladder", 0);
                        PendingSkillPointAllocations = XmlToolbox.GetAttributeInt(ele, "pendingSkillPoints", 0);
                        XP = XmlToolbox.GetAttributeInt(ele, "xp", 0);
                        NextLevelXP = XmlToolbox.GetAttributeInt(ele, "nextLevel", 0);
                        break;

                    case "abilities":
                        BaseAbilities = new AbilitySet(ele);
                        break;

                    case "inventory":
                        equippedWeapon = XmlToolbox.GetAttributeInt(ele, "equippedWeapon", -1);
                        foreach (XElement item in ele.Elements())
                        {
                            Inventory.Add(new Item(item));
                        }
                        break;

                    case "strife":
                        //TODO: events
                        StrifeID = XmlToolbox.GetAttributeInt(ele, "id", 0);

                        if (StrifeID > 0)
                        {
                            foreach (XElement strifeEle in ele.Elements("ailment"))
                            {
                                string ailName = XmlToolbox.GetAttributeString(strifeEle, "name", string.Empty);
                                ulong ailController = XmlToolbox.GetAttributeUnsignedLong(strifeEle, "controller", 0);
                                int ailTurns = XmlToolbox.GetAttributeInt(strifeEle, "turns", 0);

                                if (StatusEffect.TryParse(ailName, out StatusEffect sa, ailController, ailTurns))
                                {
                                    InflictedAilments.Add(sa);
                                }
                            }
                        }
                        break;

                }
            }
        }

        public void Save()
        {
            XDocument doc = new XDocument();
            XElement player = new XElement("player");
            
            XElement info = new XElement("info",
                new XAttribute("name", Name),
                new XAttribute("id", ID),
                new XAttribute("owner", OwnerUsername),
                new XAttribute("blood", BloodColor.ToString()),
                new XAttribute("pineappleOnPizza", LikesPineappleOnPizza)
                );

            XElement status = new XElement("status",
                new XAttribute("hp", Health),
                new XAttribute("maxhp", MaxHealth),
                new XAttribute("dead", Dead),
                new XAttribute("specibus", Specibus)
                );
            if (Immunities.Length > 0)
            {
                status.Add(new XAttribute("immune", string.Join(",", Immunities)));
            }

            XElement levels = new XElement("levels",
                new XAttribute("echeladder", Echeladder),
                new XAttribute("pendingSkillPoints", PendingSkillPointAllocations),
                new XAttribute("xp", XP),
                new XAttribute("nextLevel", NextLevelXP)
                );

            XElement abilities = BaseAbilities.ToXmlElement();

            XElement inventory = new XElement("inventory",
                new XAttribute("equippedWeapon", equippedWeapon)
                );

            for (int i = 0; i < Inventory.Count; i++)
            {
                bool equipped = EquippedWeapon == Inventory[i];
                XElement ele = Inventory[i].Save(equipped);
                inventory.Add(ele);
            }

            player.Add(info, status, levels, abilities, inventory);

            if (StrifeID > 0)
            {
                //TODO: events
                XElement strife = new XElement("strife", new XAttribute("id", StrifeID));

                foreach (StatusEffect sa in InflictedAilments)
                {
                    XElement ailEle = sa.Save();
                    strife.Add(ailEle);
                }

                player.Add(strife);
            }

            
            doc.Add(player);
            XmlToolbox.WriteXml(this.ToXmlPath(), doc);
        }

        public string ToXmlPath() => Path.Combine(Dirs.Players, ID.ToString() + ".xml");

        public string Display(bool showMods = false)
        {
            StringBuilder result = new StringBuilder();

            result.AppendLine("Name: " + Name);
            result.AppendLine($"Owned by: {OwnerUsername} ({ID})");
            result.AppendLine("Blood Color: " + BloodColor);
            result.AppendLine("");

            result.AppendLine("Health Vial: " + Health + "/" + MaxHealth);
            result.AppendLine("Strife Specibus: " + Specibus);
            result.AppendLine("");

            result.AppendLine("Echeladder Rung: " + Echeladder);
            result.AppendLine("Total XP: " + XP);
            result.AppendLine("Next Level In: " + (NextLevelXP - XP));
            result.AppendLine("Pending Skill Points: " + PendingSkillPointAllocations);
            result.AppendLine("");

            result.AppendLine("Base Statistics");
            result.AppendLine(showMods
                ? BaseAbilities.Display(this.GetModifiers())
                : BaseAbilities.Display());

            return result.ToString();
        }

        public string DisplayInventory()
        {
            string result = "";
            for (int i = 0; i < Inventory.Count; i++)
            {
                result += Inventory[i].Quantity > 1
                    ? $"{i} - {Inventory.ElementAt(i).Name} ({Inventory.ElementAt(i).Quantity})"
                    : $"{i} - {Inventory.ElementAt(i).Name}";

                if (i == equippedWeapon)
                {
                    result += " (Equipped)";
                }
                result += "\n";
            }

            return result;
        }

        public bool Register(string input)
        {
            try
            {
                // Registering.
                if (!Program.Instance.Registers.ContainsKey(ID))
                {
                    File.Create(this.ToXmlPath()).Dispose();
                    Program.Instance.Registers.Add(ID, 1);
                    return true;
                }
                
                int phase = Program.Instance.Registers[ID];
                switch (phase)
                {
                    // Name.
                    case 1:
                        if (string.IsNullOrWhiteSpace(input))
                        {
                            return false;
                        }
                        Name = input;
                        break;
                    
                    // Blood color.
                    case 2:
                        if (Enum.TryParse(input, true, out BloodType result)
                            && result != BloodType.None)
                        {
                            BloodColor = result;
                            break;
                        }
                        return false;

                    // Specibus.
                    case 3:
                        if (string.IsNullOrWhiteSpace(input))
                        {
                            return false;
                        }
                        Specibus = input.ToLower();
                        break;

                    // Pineapple.
                    case 4:
                        if (input == "yes"
                            || input == "y")
                        {
                            LikesPineappleOnPizza = true;
                            break;
                        }
                        else if (input == "no"
                            || input == "n")
                        {
                            LikesPineappleOnPizza = false;
                            break;
                        }
                        return false;
                }

                Program.Instance.Registers[ID] = ++phase;
                return true;
            }
            catch
            {
                return false;
            }
        }

        /// <summary>
        /// Gives a character XP. Levels up the character if they reach a milestone.
        /// </summary>
        /// <returns>The amount of levels they gained from this XP boost.</returns>
        public int GiveXP(int amount)
        {
            XP += amount;
            int i = 0;

            while (NextLevelXP < XP && Echeladder < Constants.XPMilestones.Length)
            {
                this.LevelUp();
                NextLevelXP = Constants.XPMilestones[Echeladder];
                ++i;
            }

            return i;
        }

        /// <summary>
        /// Levels up your character.
        /// </summary>
        public void LevelUp()
        {
            Echeladder++;
            int hp = Toolbox.DiceRoll(1, 6 + BaseAbilities.Constitution);
            MaxHealth += hp;
            Health += hp;

            PendingSkillPointAllocations += Constants.SkillPointsPerLevel;
            if (Echeladder % 5 == 0)
            {
                PendingSkillPointAllocations += Constants.SkillPointsPerLevel;
            }
        }

        // Static utils.
        public static bool Registered(ulong plyer)
        {
            string filePath = Path.Combine(Dirs.Players, plyer.ToString() + ".xml");
            return File.Exists(filePath)
                && !Program.Instance.Registers.ContainsKey(plyer);
        }

        public static async Task<Tuple<bool, Player>> TryParse(string input)
        {
            ulong id;

            // By Mention
            if (MentionUtils.TryParseUser(input, out id))
            {
                goto Finish;
            }

            // By Id
            else if (ulong.TryParse(input, NumberStyles.None, CultureInfo.InvariantCulture, out id))
            {
                goto Finish;
            }
            else
            {
                IReadOnlyCollection<IGuildUser> guildUsers = await Program.Instance.RpGuild.GetUsersAsync();

                // By Username + Discriminator
                int index = input.LastIndexOf('#');
                if (index >= 0)
                {
                    string username = input.Substring(0, index);
                    ushort discriminator;
                    if (ushort.TryParse(input.Substring(index + 1), out discriminator))
                    {
                        IGuildUser user = guildUsers.FirstOrDefault(x => x.DiscriminatorValue == discriminator &&
                            string.Equals(username, x.Username, StringComparison.OrdinalIgnoreCase));
                        if (user != null)
                        {
                            id = user.Id;
                            goto Finish;
                        }
                    }
                }


                // By Username
                // If there's more then one user with that username respond with a warning.
                IEnumerable<IGuildUser> matchedUsers = guildUsers.Where(x => x.Username.Contains(input, StringComparison.OrdinalIgnoreCase));
                if (matchedUsers.Count() == 1)
                {
                    IGuildUser user = matchedUsers.FirstOrDefault();
                    if (user != null)
                    {
                        id = user.Id;
                        goto Finish;
                    }
                }
                // Multiple people with that username exist on the server.
                else if (matchedUsers.Count() > 1)
                {
                    foreach (IGuildUser resident in matchedUsers)
                    {
                        if (Registered(resident.Id))
                        {
                            id = resident.Id;
                            goto Finish;
                        }
                    }
                }

                // By Nickname
                matchedUsers = guildUsers.Where(x => !string.IsNullOrWhiteSpace(x.Nickname) && x.Nickname.Contains(input, StringComparison.OrdinalIgnoreCase));
                if (matchedUsers.Count() == 1)
                {
                    IGuildUser user = matchedUsers.FirstOrDefault();
                    if (user != null)
                    {
                        id = user.Id;
                        goto Finish;
                    }
                }
                // Multiple people with that username exist on the server.
                else if (matchedUsers.Count() > 1)
                {
                    foreach (IGuildUser resident in matchedUsers)
                    {
                        if (Registered(resident.Id))
                        {
                            id = resident.Id;
                            goto Finish;
                        }
                    }
                }
            }

        Finish:
            {
                if (id != 0 && Player.Registered(id))
                {
                    return Tuple.Create(true, new Player(id));
                }
                
                return Tuple.Create<bool, Player>(false, null);
            }
        }
    }
}
