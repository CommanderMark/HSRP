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

        private AbilitySet _abilities { get; set; }
        private AbilitySet _abilityModifiers
        {
            get
            {
                return _abilities.GetModifiers();
            }
        }

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

                return tru + _abilities + _abilityModifiers;
            }
        }

        public int Health { get; set; }
        public int Armor { get; set; }
        public string Specibus { get; set; }

        public int Echeladder { get; private set; }
        public int PendingLevelUps { get; set; }
        public int PendingSkillPointAllocations { get; set; }

        // Stats based on previously mentioned variables.
        public int Will
        {
            get
            {
                return 10 + Abilities.Wisdom + Abilities.Charisma;
            }
        }

        public int Persuation
        {
            get
            {
                return Abilities.Intelligence + Abilities.Charisma;
            }
        }

        public int Intimidation
        {
            get
            {
                return Abilities.Strength - Abilities.Charisma;
            }
        }

        private LinkedList<Item> Inventory { get; set; }

        public bool Errored { get; set; }

        public Player(Discord.IUser user) : this(user.Id.ToString()) { }
        public Player(ulong ID) : this(ID.ToString()) { }
        public Player(string filePath)
        {
            string path = filePath.Contains(Dirs.Players)
                ? filePath + ".xml"
                : Path.Combine(Dirs.Players, filePath) + ".xml";

            XDocument doc = XmlToolbox.TryLoadXml(filePath);
            if (doc == null) { Errored = true; return; }

            Name = XmlToolbox.GetAttributeString(doc.Root, "name", string.Empty);
            ID = XmlToolbox.GetAttributeUnsignedLong(doc.Root, "id", 0);
            BloodColor = XmlToolbox.GetAttributeEnum(doc.Root, "blood", BloodType.None);

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
                        foreach (PropertyInfo property in type.GetProperties())
                        {
                            if (property.CanWrite)
                            {
                                int score = XmlToolbox.GetAttributeInt(ele.Element(property.Name.ToLower()), "score", 0);
                                int modifier = XmlToolbox.GetAttributeInt(ele.Element(property.Name.ToLower()), "modifier", 0);
                                property.SetValue(_abilities, score);
                                property.SetValue(_abilityModifiers, modifier);
                            }
                        }
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
                new XAttribute("blood", BloodColor.ToString())
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

            XElement abilities = new XElement("abilities");

            foreach (PropertyInfo property in type.GetProperties())
            {
                if (property.CanRead)
                {
                    int value = (int)property.GetValue(_abilities);
                    int modifier = (int)property.GetValue(_abilityModifiers);
                    abilities.Add(
                        new XElement(property.Name.ToLower(),
                            new XAttribute("score", value),
                            new XAttribute("modifier", modifier)
                            )
                        );
                }
            }

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

            player.Add(status, levels, abilities, inventory);
            doc.Add(player);
            XmlToolbox.WriteXml(Path.Combine(Dirs.Players, ID.ToString()), doc);
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

        // Static utils.
        public static bool Registered(ulong plyer)
        {
            return Directory.Exists(Path.Combine(Dirs.Players, plyer.ToString()));
        }
    }
}
