﻿using System.Collections.Generic;

namespace HSRP
{
    public interface IEntity
    {
        ulong ID { get; set; }
        string Name { get; set; }

        bool LikesPineappleOnPizza { get; set; }

        AbilitySet Abilities { get; set; }
        AbilitySet Modifiers { get; set; }
        Dictionary<int, AbilitySet> TempMods { get; set; }
        AbilitySet TotalMods { get; }
        AbilitySet TotalAbilities { get; }

        int Health { get; set; }
        int MaxHealth { get; set; }
        bool Dead { get; set; }
        string Specibus { get; set; }

        int DiceRolls { get; }
        ulong Controller { get; set; }

        string Display(bool showMods);
    }
}