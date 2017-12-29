using System.Xml.Linq;

namespace HSRP
{
    public class Explosion
    {
        public TargetType ExplosionTarget;
        public float ExplosionDamage;

        public Explosion()
        {
            ExplosionTarget = TargetType.Self;
            ExplosionDamage = 0f;
        }

        public Explosion(XElement element) : this()
        {
            TargetType[] enumArr = element.GetAttributeEnumArray("type", new TargetType[0]);
            foreach(TargetType enumi in enumArr)
            {
                ExplosionTarget |= enumi;
            }
            ExplosionDamage = element.GetAttributeFloat("damage", 0f);
        }

        public Explosion(Explosion ex)
        {
            ExplosionTarget = ex.ExplosionTarget;
            ExplosionDamage = ex.ExplosionDamage;
        }
    }
}
