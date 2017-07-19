namespace HSRP
{
    public class NPC : IEntity
    {
        public string Name { get; set; }
        public string Description { get; set; }

        public bool LikesPineappleOnPizza { get; set; }

        public AbilitySet Abilities { get; set; }

        public int Health { get; set; }
        public int MaxHealth { get; set; }
        public string Specibus { get; set; }
        /// <summary>
        /// The amount of times the NPC gets to roll a die for their attack turn.
        /// </summary>
        public int DiceRolls { get; set; }

        public NPC()
        {
            Abilities = new AbilitySet();

            Name = "";
            Description = "";
            Specibus = "";
        }

        public NPC(string filePath) : this()
        {

        }
    }
}
