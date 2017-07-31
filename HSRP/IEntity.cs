using System.Collections.Generic;

namespace HSRP
{
    public interface IEntity
    {
        string Name { get; set; }

        bool LikesPineappleOnPizza { get; set; }

        AbilitySet Abilities { get; set; }
        AbilitySet Modifiers { get; set; }
        Dictionary<int, AbilitySet> TempMods { get; set; }
        AbilitySet TotalAbilities { get; set; }

        int Health { get; set; }
        int MaxHealth { get; set; }
        string Specibus { get; set; }

        int DiceRolls { get; }
    }
}