using System.Xml.Linq;

namespace HSRP
{
    public class Explosion
    {
        public TargetType Target;
        public float Damage;

        public Explosion()
        {
            Target = TargetType.Self;
            Damage = 0f;
        }

        public Explosion(XElement element) : this()
        {
            TargetType[] enumArr = element.GetAttributeEnumArray("type", new TargetType[0]);
            foreach(TargetType enumi in enumArr)
            {
                Target |= enumi;
            }
            Damage = element.GetAttributeFloat("damage", 0f);
        }

        public Explosion(Explosion ex)
        {
            Target = ex.Target;
            Damage = ex.Damage;
        }
    }
}
