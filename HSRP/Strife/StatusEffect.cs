using System;
using System.Text;
using System.Xml.Linq;

namespace HSRP
{
    public enum ExplosionType
    {
        NONE,
        Oppose,
        Same,
        All
    }
    
    public class StatusEffect
    {
        // Inflict damage stuff.
        private bool inflictsDamage = false;
        private float minDamagePercentage = 0f;
        private float maxDamagePercentage = 0f;

        // Skip a turn.
        private bool skipsTurn = false;

        // Mind control.
        public ulong Controller = 0;

        // Explosion
        private bool explodes = false;
        private ExplosionType explosionTarget = ExplosionType.NONE;
        private float explosionDamage = 0f;

        // Stat buffs/debuffs.
        public AbilitySet Modifiers = new AbilitySet();

        // Misc. general status effect stuff.
        public string Name = string.Empty;
        private int turns = 0;
        private string inflictMsg = string.Empty;
        private string statusMsg = string.Empty;
        private string endMsg = string.Empty;

        //TODO:1`00% next to.
        private const int explosionFalloffFactor = 2;

        public StatusEffect(XElement element)
        {
            Name = XmlToolbox.GetAttributeString(element, "name", string.Empty);
            inflictsDamage = XmlToolbox.GetAttributeBool(element, "canDamage", false);
            skipsTurn = XmlToolbox.GetAttributeBool(element, "skipTurns", false);
            Controller = XmlToolbox.GetAttributeUnsignedLong(element, "controller", 0);
            explodes = XmlToolbox.GetAttributeBool(element, "explodes", false);
            turns = XmlToolbox.GetAttributeInt(element, "turns", 0);

            foreach (XElement ele in element.Elements())
            {
                switch (ele.Name.LocalName)
                {
                    case "inflictDamage":
                    {
                        minDamagePercentage = XmlToolbox.GetAttributeFloat(element, "minAmount", 0f);
                        maxDamagePercentage = XmlToolbox.GetAttributeFloat(element, "maxAmount", 0f);
                    }
                    break;

                    case "explosion":
                    {
                        explosionTarget = XmlToolbox.GetAttributeEnum(ele, "target", ExplosionType.NONE);
                        explosionDamage = XmlToolbox.GetAttributeFloat(element, "damage", 0f);
                    }
                    break;

                    case "inflictMsg":
                    {
                        inflictMsg = XmlToolbox.ElementInnerText(ele);
                    }
                    break;

                    case "statusMsg":
                    {
                        statusMsg = XmlToolbox.ElementInnerText(ele);
                    }
                    break;

                    case "endMsg":
                    {
                        endMsg = XmlToolbox.ElementInnerText(ele);
                    }
                    break;

                    case "abilities":
                    {
                        Modifiers = new AbilitySet(ele);
                    }
                    break;
                }
            }
        }

        public XElement Save()
        {
            XElement ailment = new XElement("ailment",
                new XAttribute("name", Name),
                new XAttribute("controller", Controller),
                new XAttribute("turns", turns)
                );

            return ailment;
        }

        /// <summary>
        /// Applies a status effect to an entity.
        /// </summary>
        /// <param name="ent">The entity this effect is being applied to.</param>
        /// <param name="tar">The entity the user was targeting or targeted by when the effect was applied.</param>
        /// <param name="strife">The strife object itself.</param>
        /// <returns>The log of the event.</returns>
        public string ApplyStatusEffect(IEntity ent, IEntity tar, Strife strife)
        {
            // Are they immune?
            foreach (string name in ent.Immunities)
            {
                if (this.Name.ToLowerInvariant() == name.ToLowerInvariant())
                {
                    return $"{Syntax.ToCodeLine(ent.Name)} is immune to \"{Syntax.ToCodeLine(name.ToString())}\"!";
                }
            }

            // Check that it is not an immediate explosion or invalid.
            if (turns < 0)
            {
                if (!explodes)
                {
                    System.Console.WriteLine("STRIFE ERROR: invalid turn count!"
                        + " (Name: " + Name + ", TurnCount: " + turns +maxDamagePercentage + ")");

                    return string.Empty;
                }
                else
                {
                    // Boom boom now.
                    return strife.Explosion(inflictMsg, explosionTarget, ent, tar, explosionDamage, explosionFalloffFactor);
                }
            }

            ent.InflictedAilments.Add(this);
            return inflictMsg;
        }

        /// <summary>
        /// Goes through a cycle of the given status effect.
        /// </summary>
        /// <param name="ent">The entity this effect is being applied to.</param>
        /// <param name="tar">The entity the user was targeting or targeted by when the effect was applied.</param>
        /// <param name="strife">The strife object itself.</param>
        /// <returns>A tuple containing a log of the event, whether the effect is removed this turn and whether the player's turn is skipped.</returns>
        public Tuple<string, bool, bool> UpdateStatusEffect(IEntity ent, IEntity tar, Strife strife)
        {
            StringBuilder msg = new StringBuilder();
            if (inflictsDamage)
            {
                // Pick a value to inflict.
                float per = Toolbox.RandFloat(minDamagePercentage, maxDamagePercentage);
                int dmg = (int) Math.Round(ent.MaxHealth * per, MidpointRounding.AwayFromZero);

                msg.AppendLine(EntityUtil.GetEntityMessage(statusMsg, Syntax.ToCodeLine(ent.Name), Syntax.ToCodeLine(dmg.ToString())));
            }

            if (turns == 0 && explodes)
            {
                msg.AppendLine(strife.Explosion(statusMsg, explosionTarget, ent, tar, explosionDamage, explosionFalloffFactor));
            }

            --turns;
            bool endEffect = turns < 0;

            return new Tuple<string, bool, bool>
                (
                msg.ToString() + (endEffect ? "\n" + endMsg : string.Empty),
                endEffect,
                skipsTurn
                );
        }

        public static bool TryParse(string name, out StatusEffect sa, ulong controller, int turns)
        {
            if (Toolbox.StatusEffects.TryGetValue(name, out StatusEffect ail))
            {
                sa = ail.Copy();
                sa.Controller = controller;
                sa.turns = turns;
                return true;
            }

            Console.WriteLine("STRIFE ERROR: Ailment \"" + name + "\" not found!");
            sa = null;
            return false;
        }
    }
}
