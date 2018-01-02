using System;
using System.Reflection;
using System.Text;
using System.Xml.Linq;
using HSRP.Commands;

namespace HSRP
{
    public class AbilitySet
    {
        [Ability(AbilityType.Physical, true,
            "Encompasses your brawn. Look at those guns. You use traditional brute forcing to fix and further "
            + "create new problems. You may end up being solo most of the time in your endeavours, but you "
            + "are all you need to dish out pain."

            + "\n\nHaving a high value in this stat grants you excelled use of melee weapons and hand-to-hand combat skill. "
            + "Your beefy nature makes you less approachable and more threatening. You can even reach a point where you "
            + "end up skipping combat altogether against weaker physical opponents.")]
        public Ability Strength { get; set; }
        
        [Ability(AbilityType.Physical, false,
            "Muscle strength helps with getting out in the world, but without enough muscle endurance "
            + "to back that up you won't be getting far. This stat determines your resilience to physical activities. "
            + "Whether it involves overcoming treacherous environments or picking off hostiles from a distance, "
            + "your body is capable of responding to the task at hand with utmost precision."

            + "\n\nHaving a high value in this stat grants you more resistance to environmental hazards. "
            + "The maximum value your max HP can increase by every level up is increased."
            + "Your ability to use projectile-based weapons is significantly improved. "
            + "It also grants the user better dexterity, making them better equipped at handling "
            + "hazardous terrain and stealth, depending on your playstyle. ")]
        public Ability Constitution { get; set; }
        
        [Ability(AbilityType.Mental, true,
            "The best offense is a good defense, by which I mean you don't need defense if you can take down "
            + "everyone with your mind. Straight out of sci-fi, this stat determines your ability to use special "
            + "attacks that bypass any form of physicality the target may have. Granted your attacks are not "
            + "completely unavoidable, but you bypass physical-oriented opponents with ease."
            
            + "\n\nHaving a high value in this stat grants you access to mind-based attacks. The types of attacks, "
            + "and their effectiveness, is based upon the enemy's Fortitude compared to your Psion. "
            + "Attacks can range from mild disorientation, suggestion, to full on mind control if the opponent is "
            + "weak minded.")]
        public Ability Psion { get; set; }
        
        [Ability(AbilityType.Mental, false,
            "The best defense is a good offense, said no one ever. Regardless, you possess a strong will with this trait, "
            + "not only good for countering mind-based attacks, but allows you to be less susceptible to "
            + "common battleground fatigue. You can remain clear and focus under the least hospitable scenarios."
            
            + "\n\nHaving a high value in this stat grants you protection to Psion special attacks. "
            + "You think best when you're under pressure, so your Fortitude if weighed in on various actions "
            + "when under stress.")]
        public Ability Fortitude { get; set; }
        
        [Ability(AbilityType.Speech, true,
            "You're fucking nuts, but people still listen to what you say regardless. This stat lets your words "
            + "do the work. You can get out of bad situations using a silver tongue and fish out more information "
            + "from people you talk to with your irresistable charm, or if not that your magical eyepatch. "
            + "Don't have an eyepatch? You can probably convince someone you do with this stat."
            
            + "\n\nHaving a high value in this stat grants you the ability to 'talk' your way out of danger. "
            + "You can also gather intel more fleshy sources much easier, based upon the target's Persuation "
            + "in relation to your Intimidation.")]
        public Ability Intimidation { get; set; }
        
        [Ability(AbilityType.Speech, false,
            "You're fucking nuts, but people can't convince you otherwise. This stat benefits your speech "
            + "in a different manner as opposed to Intimidation. It instead focuses on using your natural charm "
            + "to well, persuade people to underestimate or assist you. You don't talk your way out of a negative scenario, "
            + "you talk your way into a positive one."
            
            + "\n\nHaving a high value in this stat makes you resilient to high Intidimation stats. You appear less suspicious "
            + "and can use speech to bypass hazards in unusual fashions. Your charm makes it more likely "
            + "for people to perceive you as non-threatening, regardless of your actual motive.")]
        public Ability Persuasion { get; set; }

        public static AbilitySet operator +(AbilitySet set1, AbilitySet set2)
        {
            AbilitySet newSet = new AbilitySet();
            newSet.Strength.Value = set1.Strength.Value + set2.Strength.Value;
            newSet.Constitution.Value = set1.Constitution.Value + set2.Constitution.Value;
            newSet.Psion.Value = set1.Psion.Value + set2.Psion.Value;
            newSet.Fortitude.Value = set1.Fortitude.Value + set2.Fortitude.Value;
            newSet.Intimidation.Value = set1.Intimidation.Value + set2.Intimidation.Value;
            newSet.Persuasion.Value = set1.Persuasion.Value + set2.Persuasion.Value;

            return newSet;
        }

        public override bool Equals(Object obj)
        {
            // If parameter cannot be cast to Point return false.
            AbilitySet set = obj as AbilitySet;
            if (set == null)
            {
                return false;
            }

            foreach (PropertyInfo property in GetType().GetProperties())
            {
                if (property.CanWrite && property.CanRead)
                {
                    Object val1 = property.GetValue(this);
                    Object val2 = property.GetValue(set);

                    if (val1 != val2)
                    {
                        return false;
                    }
                }
            }

            return true;
        }

        public override int GetHashCode()
        {
            return base.GetHashCode();
        }

        public static AbilitySet ToFixedNumber(AbilitySet mod, AbilitySet set)
        {
            AbilitySet newSet = new AbilitySet();
            newSet.Strength.Value = mod.Strength.Value + (int) (mod.Strength.Percentage * set.Strength.Value);
            newSet.Constitution.Value = mod.Constitution.Value + (int) (mod.Constitution.Percentage * set.Constitution.Value);
            newSet.Psion.Value = mod.Psion.Value + (int) (mod.Psion.Percentage * set.Psion.Value);
            newSet.Fortitude.Value = mod.Fortitude.Value + (int) (mod.Fortitude.Percentage * set.Fortitude.Value);
            newSet.Intimidation.Value = mod.Intimidation.Value + (int) (mod.Intimidation.Percentage * set.Intimidation.Value);
            newSet.Persuasion.Value = mod.Persuasion.Value + (int) (mod.Persuasion.Percentage * set.Persuasion.Value);

            return newSet;
        }

        public AbilitySet() 
        {
            Strength = new Ability();
            Constitution = new Ability();
            Psion = new Ability();
            Fortitude = new Ability();
            Intimidation = new Ability();
            Persuasion = new Ability();
        }

        public AbilitySet(XElement element) : this()
        {
            foreach (XElement ele in element.Elements())
            {
                switch (ele.Name.LocalName.FirstCharUpper())
                {
                    case Ability.STR:
                    {
                        Strength = new Ability(ele);
                    }
                    break;

                    case Ability.CON:
                    {
                        Constitution = new Ability(ele);
                    }
                    break;

                    case Ability.PSI:
                    {
                        Psion = new Ability(ele);
                    }
                    break;

                    case Ability.FOR:
                    {
                        Fortitude = new Ability(ele);
                    }
                    break;

                    case Ability.INT:
                    {
                        Intimidation = new Ability(ele);
                    }
                    break;

                    case Ability.PER:
                    {
                        Persuasion = new Ability(ele);
                    }
                    break;
                }
            }
        }

        public XElement ToXmlElement()
        {
            XElement element = new XElement("abilities");

            Type type = this.GetType();
            foreach (PropertyInfo property in type.GetProperties())
            {
                if (property.CanRead)
                {
                    Ability value = ((Ability) property.GetValue(this));
                    XElement propEle = value.Save(property.Name);

                    element.Add(propEle);
                }
            }

            return element;
        }

        /// <summary>
        /// Outputs the abilities to an XML.
        /// If an ability is set to 0 then it doesn't add it.
        /// </summary>
        public XElement ToXmlWithoutEmpties()
        {
            XElement element = new XElement("abilities");

            Type type = this.GetType();
            foreach (PropertyInfo property in type.GetProperties())
            {
                if (property.CanRead)
                {
                    Ability value = ((Ability) property.GetValue(this));
                    if (value.Value != 0)
                    {
                        element.Add(value.Save(property.Name));
                    }
                }
            }

            return element;
        }

        public string Display(AbilitySet mod = null)
        {
            StringBuilder disp = new StringBuilder();
            
            if (mod == null)
            {
                foreach (PropertyInfo prop in GetType().GetProperties())
                {
                    int value = ((Ability) prop.GetValue(this)).Value;
                    disp.AppendLine(prop.Name + ": " + value);
                }
            }
            else
            {
                foreach (PropertyInfo prop in GetType().GetProperties())
                {
                    int value = ((Ability) prop.GetValue(this)).Value;
                    int modVal = ((Ability) prop.GetValue(mod)).Value;

                    disp.Append(prop.Name + ": " + value);
                    disp.AppendLine(" " + (modVal - value).ToString("+0;-#"));
                }
            }

            return disp.ToString();
        }
    }
}