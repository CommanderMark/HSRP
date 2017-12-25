using System.Collections.Generic;

namespace HSRP
{
    public interface IEntity
    {
        ulong ID { get; set; }
        string Name { get; set; }

        bool LikesPineappleOnPizza { get; set; }

        AbilitySet BaseAbilities { get; set; }
        AbilitySet AbilitiesWithModifiers { get; }
        List<StatusEffect> inflictedEffects { get; set; }
        List<Move> Moves { get; set; }

        int Health { get; set; }
        int MaxHealth { get; set; }
        bool Dead { get; set; }

        string Specibus { get; set; }
        Item EquippedWeapon { get; }
        int DiceRolls { get; }

        string Display(bool showMods);
    }
}