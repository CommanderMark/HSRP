using System.Xml.Linq;

namespace HSRP
{
    public class Explosion
    {
        public TargetType Target;
        public float Damage;

        /// <summary>
        /// Determines whether or not the value stored in Damage is a fixed number to inflict as damage,
        /// or represents a percentage of damage to be done based on the target's max health.
        /// </summary>
        public bool FixedNumber;

        public Explosion()
        {
            Target = TargetType.Self;
            Damage = 0f;
            FixedNumber = false;
        }

        public Explosion(XElement element) : this()
        {
            TargetType[] enumArr = element.GetAttributeEnumArray("type", new TargetType[0]);
            foreach(TargetType enumi in enumArr)
            {
                Target |= enumi;
            }
            Damage = element.GetAttributeFloat("damage", 0f);
            FixedNumber = element.GetAttributeBool("fixedNumber", false);
        }

        public Explosion(Explosion ex)
        {
            Target = ex.Target;
            Damage = ex.Damage;
            FixedNumber = ex.FixedNumber;
        }
    }
}
