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

        // TODO: Can weapons be modifiers?
        private AbilitySet _abilities { get; set; }

        /// <summary>
        /// Total ability skill set.
        /// </summary>
        public AbilitySet Abilities
        {
            get
            {
                AbilitySet tru = new AbilitySet();
                foreach (Item i in Inventory)
                {
                    if (i.equipped)
                    {
                        tru += i.abilities;
                    }
                }

                return tru + _abilities;
            }
        }

        public int Health { get; set; }
        public int Armor { get; set; }
        public string Specibus { get; set; }

        public int Echeladder { get; private set; }
        public int PendingSkillPointAllocations { get; set; }

        private LinkedList<Item> Inventory { get; set; }

        public bool Errored { get; set; }

        private Player() { }

        public Player(Discord.IUser user) : this(user.Id.ToString()) { }
        public Player(ulong ID) : this(ID.ToString()) { }
        public Player(string filePath)
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

            Type type = _abilities.GetType();
            foreach (XElement ele in doc.Root.Elements())
            {
                switch (ele.Name.LocalName)
                {
                    case "status":
                        Health = XmlToolbox.GetAttributeInt(ele, "hp", -1);
                        Armor = XmlToolbox.GetAttributeInt(ele, "ac", 0);
                        Specibus = XmlToolbox.GetAttributeString(ele, "specibus", string.Empty);
                        break;

                    case "levels":
                        Echeladder = XmlToolbox.GetAttributeInt(ele, "echeladder", 0);
                        PendingLevelUps = XmlToolbox.GetAttributeInt(ele, "pendingLevelUps", 0);
                        PendingSkillPointAllocations = XmlToolbox.GetAttributeInt(ele, "pendingSkillPoints", 0);
                        break;

                    case "abilities":
                        _abilities = new AbilitySet(ele);
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

                            foreach (PropertyInfo property in type.GetProperties())
                            {
                                if (property.CanWrite)
                                {
                                    int value = XmlToolbox.GetAttributeInt(item.Element(property.Name.ToLower()), "score", 0);
                                    property.SetValue(i.abilities, value);
                                }
                            }
                        }
                        break;
                }
            }
        }

        public void Save()
        {
            Type type = _abilities.GetType();
            XDocument doc = new XDocument();
            XElement player = new XElement("player",
                new XAttribute("name", Name),
                new XAttribute("id", ID),
                new XAttribute("blood", BloodColor.ToString()),
                new XAttribute("pineappleOnPizza", LikesPineappleOnPizza)
                );

            XElement status = new XElement("status",
                new XAttribute("hp", Health),
                new XAttribute("ac", Armor),
                new XAttribute("specibus", Specibus)
                );

            XElement levels = new XElement("levels",
                new XAttribute("echeladder", Echeladder),
                new XAttribute("pendingLevelUps", PendingLevelUps),
                new XAttribute("pendingSKillPoints", PendingSkillPointAllocations)
                );

            XElement abilities = _abilities.ToXmlElement();

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
                
                foreach (PropertyInfo property in type.GetProperties())
                {
                    if (property.CanRead)
                    {
                        int value = (int)property.GetValue(item.abilities);
                        if (value != 0)
                        {
                            ele.Add(new XElement(property.Name.ToLower(), value));
                        }
                    }
                }
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
                if (Errored)
                {
                    Errored = false;
                    File.Create(this.ToXmlPath());
                    Program.Instance.Registers.Add(ID, 1);
                    return true;
                }
                
                int phase = Program.Instance.Registers[ID];
                switch (phase)
                {
                    // Name.
                    case 1:
                        Name = input;
                        break;
                    
                    // Blood color.
                    case 2:
                        if (Enum.TryParse(input, out BloodType result))
                        {
                            BloodColor = result;
                            break;
                        }
                        return false;

                    // Specibus.
                    case 3:
                        Specibus = input;
                        break;

                    // Lusus description.
                    case 4:
                        if (input.Length > 1 && input.Length <= 60)
                        {
                            LususDescription = input;
                            break;
                        }
                        return false;

                    // Pineapple.
                    case 5:
                        if (input == "yes"
                            || input == "y"
                            || (bool.TryParse(input, out bool bl)
                                && bl)
                            )
                        {
                            LikesPineappleOnPizza = true;
                            break;
                        }
                        else if (input == "no"
                            || input == "n"
                            || !bl)
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
        /// Levels up your character.
        /// </summary>
        /// <returns>Whether or not you're due for a skill point increase.</returns>
        public bool LevelUp()
        {
            Echeladder++;
            PendingLevelUps--;

            Health += Toolbox.DiceRoll(1, 6 + Abilities.Constitution);

            if (Math.Log(Echeladder, 2) % 1 == 0)
            {
                PendingSkillPointAllocations++;
                return true;
            }
            return false;
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
