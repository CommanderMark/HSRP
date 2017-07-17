using System;
using System.ComponentModel;
using System.Reflection;
using System.Xml.Linq;
using HSRP.Commands;

namespace HSRP
{
    public class AbilitySet
    {
        [Ability("Physical Offense",
            "Encompasses your brawn. Look at those guns. You use traditional brute forcing to fix and further "
            + "create new problems. You may end up being solo most of the time in your endeavours, but you "
            + "are all you need to dish out pain."

            + "\n\nHaving a high value in this stat grants you excelled use of melee weapons and hand-to-hand combat skill. "
            + "Your beefy nature makes you less approachable and more threatening. You can even reach a point where you "
            + "end up skipping combat altogether against weaker physical opponents.")]
        public int Strength { get; set; }
        
        [Ability("Physical Defense",
            "Muscle strength helps with getting out in the world, but without enough muscle endurance "
            + "to back that up you won't be getting far. This stat determines your resilience to physical activities. "
            + "Whether it involves overcoming treacherous environments or picking off hostiles from a distance, "
            + "your body is capable of responding to the task at hand with utmost precision."

            + "\n\nHaving a high value in this stat grants you more resistance to environmental hazards. "
            + "The maximum value your max HP can increase by every level up is increased."
            + "Your ability to use projectile-based weapons is significantly improved. "
            + "It also grants the user better dexterity, making them better equipped at handling "
            + "hazardous terrain and stealth, depending on your playstyle. ")]
        public int Constitution { get; set; }
        
        [Ability("Mental Offense",
            "The best offense is a good defense, by which I mean you don't need defense if you can take down "
            + "everyone with your mind. Straight out of sci-fi, this stat determines your ability to use special "
            + "attacks that bypass any form of physicality the target may have. Granted your attacks are not "
            + "completely unavoidable, but you bypass physical-oriented opponents with ease."
            
            + "\n\nHaving a high value in this stat grants you access to mind-based attacks. The types of attacks, "
            + "and their effectiveness, is based upon the enemy's Fortitude compared to your Psion. "
            + "Attacks can range from mild disorientation, suggestion, to full on mind control if the opponent is "
            + "weak minded.")]
        public int Psion { get; set; }
        
        [Ability("Mental Defense",
            "The best defense is a good offense, said no one ever. Regardless, you possess a strong will with this trait, "
            + "not only good for countering mind-based attacks, but allows you to be less susceptible to "
            + "common battleground fatigue. You can remain clear and focus under the least hospitable scenarios."
            
            + "\n\nHaving a high value in this stat grants you protection to Psion special attacks. "
            + "You think best when you're under pressure, so your Fortitude if weighed in on various actions "
            + "when under stress.")]
        public int Fortitude { get; set; }
        
        [Ability("Speech Offense",
            "You're fucking nuts, but people still listen to what you say regardless. This stat lets your words "
            + "do the work. You can get out of bad situations using a silver tongue and fish out more information "
            + "from people you talk to with your irresistable charm, or if not that your magical eyepatch. "
            + "Don't have an eyepatch? You can probably convince someone you do with this stat."
            
            + "\n\nHaving a high value in this stat grants you the ability to 'talk' your way out of danger. "
            + "You can also gather intel more fleshy sources much easier, based upon the target's Persuation "
            + "in relation to your Intimidation.")]
        public int Intimidation { get; set; }
        
        [Ability("Speech Defense",
            "You're fucking nuts, but people can't convince you otherwise. This stat benefits your speech "
            + "in a different manner as opposed to Intimidation. It instead focuses on using your natural charm "
            + "to well, persuade people to underestimate or assist you. You don't talk your way out of a negative scenario, "
            + "you talk your way into a positive one."
            
            + "\n\nHaving a high value in this stat makes you resilient to high Intidimation stats. You appear less suspicious "
            + "and can use speech to bypass hazards in unusual fashions. Your charm makes it more likely "
            + "for people to perceive you as non-threatening, regardless of your actual motive.")]
        public int Persuasion { get; set; }

        public static AbilitySet operator +(AbilitySet set1, AbilitySet set2)
        {
            AbilitySet newSet = new AbilitySet();
            foreach (PropertyInfo prop in newSet.GetType().GetProperties())
            {
                int val1 = (int)prop.GetValue(set1);
                int val2 = (int)prop.GetValue(set2);
                prop.SetValue(newSet, val1 + val2);
            }

            return newSet;
        }

        public AbilitySet() { }
        public AbilitySet(XElement ele)
        {
            Type type = this.GetType();
            foreach (PropertyInfo property in type.GetProperties())
            {
                if (property.CanWrite)
                {
                    int value = XmlToolbox.GetAttributeInt(ele.Element(property.Name.ToLower()), "value", 0);
                    property.SetValue(this, value);
                }
            }
        }

        public XElement ToXmlElement()
        {
            XElement ele = new XElement("abilities");
            Type type = this.GetType();
            foreach (PropertyInfo property in type.GetProperties())
            {
                if (property.CanRead)
                {
                    int value = (int)property.GetValue(this);
                    ele.Add(
                        new XElement(property.Name.ToLower(),
                            new XAttribute("value", value)
                            )
                        );
                }
            }

            return ele;
        }

        public AbilitySet GetModifiers()
        {
            AbilitySet modifierSet = new AbilitySet();

            Type type = this.GetType();
            foreach (PropertyInfo property in type.GetProperties())
            {
                if (property.CanWrite && property.CanRead)
                {
                    int value = (int)property.GetValue(this);
                    // y = 0.5x - 5, rounded down.
                    float modiVal = (float)Math.Floor((0.5 * value) - 5);

                    property.SetValue(modifierSet, (int)modiVal);
                }
            }

            return modifierSet;
        }

        public string Display(AbilitySet modifiers)
        {
            string disp = "";

            Type type = this.GetType();
            foreach (PropertyInfo property in type.GetProperties())
            {
                if (property.CanRead)
                {
                    int value = (int)property.GetValue(this);
                    int modiVal = (int)property.GetValue(modifiers);

                    disp += $"{property.Name}: {value} ({modiVal})\n";
                }
            }

            return disp;
        }
    }
}