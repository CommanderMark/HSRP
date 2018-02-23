using System;
using System.Xml.Linq;

namespace HSRP
{
    public class InflictDamage
    {
        public static double MarginOfError = 0.001;
        /// <summary>
        /// Amount of damage to inflict when the event is triggered.
        /// </summary>
        public float MinAmount { private set; get; }
        /// <summary>
        /// Amount of damage to inflict when the event is triggered.
        /// </summary>
        public float MaxAmount { private set; get; }

        public bool HasDamage
        {
            get
            {
                return MinAmount > 0;
            }
        }

        public bool Ranged
        {
            get
            {
                return Math.Abs(MinAmount - MaxAmount) >= MarginOfError;
            }
        }

        private float trueDamage
        {
            get
            {
                if (!Ranged)
                {
                    return MinAmount;
                }

                return Toolbox.RandFloat(MinAmount, MaxAmount);
            }
        }

        /// <summary>
        /// The entity(ies) that will be targeted by the damageAmount field.
        /// </summary>
        public TargetType Target;

        /// <summary>
        /// Determines whether the damage applied is a fixed amount or relative to an entity's max health.
        /// </summary>
        public bool FixedAmount;

        public InflictDamage()
        {
            MinAmount = 0f;
            MaxAmount = 0f;
            Target = TargetType.None;
            FixedAmount = false;
        }

        public InflictDamage(InflictDamage copy)
        {
            this.MinAmount = copy.MinAmount;
            this.MaxAmount = copy.MaxAmount;
            this.Target = copy.Target;
            this.FixedAmount = copy.FixedAmount;
        }

        public InflictDamage(XElement element) : this()
        {
            MinAmount = element.GetAttributeFloat("minAmount", 0f);
            MaxAmount = element.GetAttributeFloat("maxAmount", MinAmount);

            TargetType[] enumArr = element.GetAttributeEnumArray("type", new TargetType[2]);
            foreach(TargetType enumi in enumArr)
            {
                Target |= enumi;
            }

            FixedAmount = element.GetAttributeBool("fixed", false);
        }

        public XElement Save()
        {
            XElement inflictDamage = new XElement("inflictDamage");
            
            if (MinAmount != 0)
            {
                inflictDamage.Add(new XAttribute("minAmount", MinAmount));
            }
            
            if (Ranged)
            {
                inflictDamage.Add(new XAttribute("maxAmount", MaxAmount));
            }

            inflictDamage.Add(new XAttribute("type", string.Join(",", Target.GetIndividualFlags())));
            
            if (FixedAmount)
            {
                inflictDamage.Add(new XAttribute("fixed", FixedAmount));
            }
            
            return inflictDamage;
        }

        public void ApplyDamage(Entity ent, Strife strife, int dmg = 0)
        {
            if (dmg == 0)
            {
                dmg = FixedAmount
                    ? (int) trueDamage
                    : (int) (trueDamage * ent.MaxHealth);
            }
            
            ent.Health -= dmg;
            strife.Log.AppendLine($"{Syntax.ToCodeLine(ent.Name)} took {Syntax.ToCodeLine(dmg)} damage.");
        }
    }
}