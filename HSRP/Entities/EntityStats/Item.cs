using System.Collections.Generic;

namespace HSRP
{
    public class Item
    {
        public string Name;
        public int Quantity;
        public int BaseDamage;

        private List<StatusEffect> statusEffects;

        public Item()
        {
            Name = string.Empty;
            Quantity = 1;
            BaseDamage = 1;

            statusEffects = new List<StatusEffect>();
        }
    }
}
