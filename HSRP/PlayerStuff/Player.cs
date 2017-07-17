using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Xml.Linq;

namespace HSRP
{
    public class Player
    {
        public ulong ID { get; set; }
        public string Name { get; set; }

        public BloodType BloodColor { get; set; }
        public string LususDescription { get; set; }
        public bool LikesPineappleOnPizza { get; set; }
        
        public AbilitySet Abilities { get; set; }

        public int Health { get; set; }
        public int MaxHealth { get; set; }
        public int Armor { get; set; }
        public string Specibus { get; set; }

        public int Echeladder{ get; private set; }
        public int PendingSkillPointAllocations { get; set; }
        public int XP { get; set; }
        public int NextLevelXP { get; set; }

        public LinkedList<Item> Inventory { get; set; }

        public bool Errored { get; set; }

        public Player()
        {
            Abilities = new AbilitySet();
            Inventory = new LinkedList<Item>();

            Name = "";
            LususDescription = "";
            Specibus = "";
        }

        public Player(Discord.IUser user) : this(user.Id.ToString()) { }
        public Player(ulong ID) : this(ID.ToString()) { }
        public Player(string filePath) : this()
        {
            string path = filePath.Contains(Dirs.Players)
                ? filePath + ".xml"
                : Path.Combine(Dirs.Players, filePath) + ".xml";

            XDocument doc = XmlToolbox.TryLoadXml(path);
            if (doc == null) { Errored = true; return; }

            Name = XmlToolbox.GetAttributeString(doc.Root, "name", string.Empty);
            ID = XmlToolbox.GetAttributeUnsignedLong(doc.Root, "id", 0);
            BloodColor = XmlToolbox.GetAttributeEnum(doc.Root, "blood", BloodType.None);
            LikesPineappleOnPizza = XmlToolbox.GetAttributeBool(doc.Root, "pineappleOnPizza", false);
            
            foreach (XElement ele in doc.Root.Elements())
            {
                switch (ele.Name.LocalName)
                {
                    case "status":
                        Health = XmlToolbox.GetAttributeInt(ele, "hp", -1);
                        MaxHealth = XmlToolbox.GetAttributeInt(ele, "maxhp", Health);
                        Armor = XmlToolbox.GetAttributeInt(ele, "ac", 0);
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

                    case "lusus":
                        LususDescription = ele.ElementInnerText();
                        break;

                    case "inventory":
                        foreach (XElement item in ele.Elements())
                        {
                            Item i = new Item();
                            i.name = XmlToolbox.GetAttributeString(item, "value", string.Empty);
                            i.equipped = XmlToolbox.GetAttributeBool(item, "equipped", false);
                            Inventory.AddLast(i);
                        }
                        break;
                }
            }
        }

        public void Save()
        {
            XDocument doc = new XDocument();
            XElement player = new XElement("player");
            player.Add(new XAttribute("name", Name));
            player.Add(new XAttribute("id", ID));
            player.Add(new XAttribute("blood", BloodColor.ToString()));
            player.Add(new XAttribute("pineappleOnPizza", LikesPineappleOnPizza));

            XElement status = new XElement("status",
                new XAttribute("hp", Health),
                new XAttribute("maxhp", MaxHealth),
                new XAttribute("ac", Armor),
                new XAttribute("specibus", Specibus)
                );

            XElement levels = new XElement("levels",
                new XAttribute("echeladder", Echeladder),
                new XAttribute("pendingSkillPoints", PendingSkillPointAllocations),
                new XAttribute("xp", XP),
                new XAttribute("nextLevel", NextLevelXP)
                );

            XElement abilities = Abilities.ToXmlElement();

            XElement lusus = new XElement("lusus",
                new XText(LususDescription));

            XElement inventory = new XElement("inventory");

            foreach (Item item in Inventory)
            {
                XElement ele = new XElement("item", new XAttribute("value", item.name));
                if (item.equipped)
                {
                    ele.Add(new XAttribute("equipped", item.equipped));
                }
                inventory.Add(ele);
            }

            player.Add(status, levels, abilities, lusus, inventory);
            doc.Add(player);
            XmlToolbox.WriteXml(this.ToXmlPath(), doc);
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
                        Specibus = input;
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
        public int GiveXP(int xp)
        {
            XP += xp;
            NextLevelXP -= xp;
            int i = 0;

            while (NextLevelXP <= 0 && Echeladder < 30)
            {
                this.LevelUp();
                NextLevelXP += Constants.XPMilestones[Echeladder];
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

        public string Display(Discord.IUser user)
        {
            string result = "";

            result = result.AddLine("Name: " + Name);
            result = result.AddLine("Owned by: " + user.Username);
            result = result.AddLine("Blood Color: " + BloodColor);
            result = result.AddLine("Lusus Desc: " + LususDescription);
            result = result.AddLine("");

            result = result.AddLine("Health Vial: " + Health + "/" + MaxHealth);
            result = result.AddLine("Armor: " + Armor);
            result = result.AddLine("Strife Specibus " + Specibus);
            result = result.AddLine("");

            result = result.AddLine("Echeladder Rung: " + Echeladder);
            result = result.AddLine("Total XP: " + XP);
            result = result.AddLine("Next Level In: " + NextLevelXP);
            result = result.AddLine("Pending Skill Points: " + PendingSkillPointAllocations);
            result = result.AddLine("");

            result = result.AddLine("Base Statistics");
            foreach (PropertyInfo prop in Abilities.GetType().GetProperties())
            {
                int value = (int)prop.GetValue(Abilities);
                result = result.AddLine(prop.Name + ": " + value);
            }

            return result;
        }

        public string DisplayInventory()
        {
            string result = "";
            for (int i = 0; i < Inventory.Count; i++)
            {
                result = Inventory.ElementAt(i).equipped
                    ? result.AddLine($"{i} - {Inventory.ElementAt(i).name} (Equipped)")
                    : result.AddLine($"{i} - {Inventory.ElementAt(i).name}");
            }

            return result;
        }

        public string ToXmlPath() => Path.Combine(Dirs.Players, ID.ToString() + ".xml");

        // Static utils.
        public static bool Registered(ulong plyer)
        {
            string filePath = Path.Combine(Dirs.Players, plyer.ToString() + ".xml");
            return File.Exists(filePath)
                && !Program.Instance.Registers.ContainsKey(plyer);
        }
    }
}
