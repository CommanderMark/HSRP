using System;
using System.Collections.Generic;
using System.Globalization;
using System.IO;
using System.Linq;
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
        public string LususDescription { get; set; }
        public bool LikesPineappleOnPizza { get; set; }

        public AbilitySet Abilities { get; set; }
        public AbilitySet Modifiers { get; set; }
        public Dictionary<int, AbilitySet> TempMods { get; set; }
        public AbilitySet TotalMods
        {
            get
            {
                AbilitySet aSet = Modifiers;
                if (TempMods.Any())
                {
                    foreach (KeyValuePair<int, AbilitySet> set in TempMods)
                    {
                        aSet += set.Value;
                    }
                }

                return aSet;
            }
        }

        public AbilitySet TotalAbilities
        {
            get
            {
                return Abilities + TotalMods;
            }
        }

        public int Health { get; set; }
        public int MaxHealth { get; set; }
        public bool Dead { get; set; }
        public string Specibus { get; set; }
        public int StrifeID { get; set; }

        /// <summary>
        /// Returns the total damage of the character's equipped items.
        /// </summary>
        public int DiceRolls
        {
            get
            {
                int dmg = 0;
                foreach (Item i in Inventory)
                {
                    if (i.Equipped && i.Damage > 0)
                    {
                        dmg += i.Damage;
                    }
                }
                if (dmg == 0) { dmg = 1; }
                
                return dmg;
            }
        }
        public ulong Controller { get; set; }

        public int Echeladder{ get; private set; }
        public int PendingSkillPointAllocations { get; set; }
        public int XP { get; set; }
        public int NextLevelXP { get; set; }

        public List<Item> Inventory { get; set; }

        public bool Errored { get; set; }

        public Player()
        {
            Abilities = new AbilitySet();
            Modifiers = new AbilitySet();
            TempMods = new Dictionary<int, AbilitySet>();
            Inventory = new List<Item>();

            Name = "";
            LususDescription = "";
            Specibus = "";
        }

        public Player(Discord.IUser user) : this(user.Id.ToString()) { }
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

                    case "lusus":
                        LususDescription = ele.ElementInnerText();
                        break;

                    case "status":
                        Health = XmlToolbox.GetAttributeInt(ele, "hp", -1);
                        MaxHealth = XmlToolbox.GetAttributeInt(ele, "maxhp", Health);
                        Dead = XmlToolbox.GetAttributeBool(ele, "dead", false);
                        Specibus = XmlToolbox.GetAttributeString(ele, "specibus", string.Empty);
                        break;

                    case "levels":
                        Echeladder = XmlToolbox.GetAttributeInt(ele, "echeladder", 0);
                        PendingSkillPointAllocations = XmlToolbox.GetAttributeInt(ele, "pendingSkillPoints", 0);
                        XP = XmlToolbox.GetAttributeInt(ele, "xp", 0);
                        NextLevelXP = XmlToolbox.GetAttributeInt(ele, "nextLevel", 0);
                        break;

                    case "abilities":
                        Abilities = new AbilitySet(ele);
                        break;

                    case "inventory":
                        foreach (XElement item in ele.Elements())
                        {
                            Item i = new Item();
                            i.Name = XmlToolbox.GetAttributeString(item, "value", string.Empty);
                            i.Quantity = XmlToolbox.GetAttributeUnsignedInt(item, "quantity", 1);
                            i.Damage = XmlToolbox.GetAttributeInt(item, "dmg", 0);
                            i.Equipped = XmlToolbox.GetAttributeBool(item, "equipped", false);
                            Inventory.Add(i);
                        }
                        break;

                    case "strife":
                        StrifeID = XmlToolbox.GetAttributeInt(ele, "id", 0);

                        if (StrifeID > 0)
                        {
                            foreach (XElement strifeEle in ele.Elements())
                            {
                                int? turns = XmlToolbox.GetAttributeNullableInt(strifeEle, "turns", null);
                                if (turns == null)
                                {
                                    Modifiers = new AbilitySet(strifeEle);
                                }
                                else
                                {
                                    TempMods.Add((int)turns, new AbilitySet(strifeEle));
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

            XElement lusus = new XElement("lusus", new XText(LususDescription));

            XElement status = new XElement("status",
                new XAttribute("hp", Health),
                new XAttribute("maxhp", MaxHealth),
                new XAttribute("dead", Dead),
                new XAttribute("specibus", Specibus)
                );

            XElement levels = new XElement("levels",
                new XAttribute("echeladder", Echeladder),
                new XAttribute("pendingSkillPoints", PendingSkillPointAllocations),
                new XAttribute("xp", XP),
                new XAttribute("nextLevel", NextLevelXP)
                );

            XElement abilities = Abilities.ToXmlElement();

            XElement inventory = new XElement("inventory");

            foreach (Item item in Inventory)
            {
                XElement ele = new XElement("item", new XAttribute("value", item.Name),
                    new XAttribute("quantity", item.Quantity));
                if (item.Damage > 0)
                {
                    ele.Add(new XAttribute("dmg", item.Damage));
                }
                if (item.Equipped)
                {
                    ele.Add(new XAttribute("equipped", item.Equipped));
                }
                inventory.Add(ele);
            }

            player.Add(info, lusus, status, levels, abilities, inventory);

            if (StrifeID > 0)
            {
                XElement strife = new XElement("strife", new XAttribute("id", StrifeID));
                strife.Add(Modifiers.ToXmlWithoutEmpties());
                foreach (KeyValuePair<int, AbilitySet> mod in TempMods)
                {
                    XElement modEle = mod.Value.ToXmlWithoutEmpties();
                    modEle.Add(new XAttribute("turns", mod.Key));

                    strife.Add(modEle);
                }
                player.Add(strife);
            }

            
            doc.Add(player);
            XmlToolbox.WriteXml(this.ToXmlPath(), doc);
        }

        public string ToXmlPath() => Path.Combine(Dirs.Players, ID.ToString() + ".xml");

        public string Display(bool showMods = false)
        {
            string result = "";

            result = result.AddLine("Name: " + Name);
            result = result.AddLine($"Owned by: {OwnerUsername} ({ID})");
            result = result.AddLine("Blood Color: " + BloodColor);
            result = result.AddLine("Lusus Desc: " + LususDescription);
            result = result.AddLine("");

            result = result.AddLine("Health Vial: " + Health + "/" + MaxHealth);
            result = result.AddLine("Strife Specibus: " + Specibus);
            result = result.AddLine("");

            result = result.AddLine("Echeladder Rung: " + Echeladder);
            result = result.AddLine("Total XP: " + XP);
            result = result.AddLine("Next Level In: " + (NextLevelXP - XP));
            result = result.AddLine("Pending Skill Points: " + PendingSkillPointAllocations);
            result = result.AddLine("");

            result = result.AddLine("Base Statistics");
            result = result.AddLine(showMods
                ? Abilities.Display(TotalMods)
                : Abilities.Display());

            return result;
        }

        public string DisplayInventory()
        {
            string result = "";
            for (int i = 0; i < Inventory.Count; i++)
            {
                result += Inventory.ElementAt(i).Quantity > 1
                    ? $"{i} - {Inventory.ElementAt(i).Name} ({Inventory.ElementAt(i).Quantity})"
                    : $"{i} - {Inventory.ElementAt(i).Name}";

                if (Inventory.ElementAt(i).Equipped)
                {
                    result += " (Equipped)";
                }
                result = result.AddLine("");
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

                    // Lusus description.
                    case 4:
                        if (input.Length > 1 && input.Length <= Constants.LususDescCharLimit)
                        {
                            LususDescription = input;
                            break;
                        }
                        return false;

                    // Pineapple.
                    case 5:
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
            int hp = Toolbox.DiceRoll(1, 6 + Abilities.Constitution);
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
                var guildUsers = await Program.Instance.RpGuild.GetUsersAsync();

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
                var matchedUsers = guildUsers.Where(x => x.Username.Contains(input, StringComparison.OrdinalIgnoreCase));
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
